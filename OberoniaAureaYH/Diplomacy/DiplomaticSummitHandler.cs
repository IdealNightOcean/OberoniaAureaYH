using OberoniaAurea_Frame;
using OberoniaAurea_Frame.Utility;

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class DiplomaticSummitHandler(Settlement settlement) : IExposable, IFixedCaravanAssociate
{
    public const int TicksNeeded = 5000;
    public string FixedCaravanName => null;

    private readonly Settlement settlement = settlement ?? throw new ArgumentNullException(nameof(settlement));

    [Unsaved] private SettlementDipComp settlementDipComp;
    private FixedCaravan associatedFixedCaravan;
    private Pawn negotiant;
    private NegotiatingTeamLevel negotiatingTeamLevel;

    public FixedCaravan AssociatedFixedCaravan => associatedFixedCaravan;

    private bool isWorking;
    public bool IsWorking => isWorking;

    private int ticksRemaining;

    public string FixedCaravanWorkDesc()
    {
        float baselineNegotiationAbility = DiplomaticSummitUtility.GetBaselineNegotiationAbility(negotiatingTeamLevel);
        float diplomaticSummitFactor = OAInteractHandler.Instance.DiplomaticSummitFactor;
        Color diplomaticSummitFactorColor = diplomaticSummitFactor > 1f ? ColorLibrary.RedReadable : diplomaticSummitFactor < 0.8f ? Color.green : Color.white;

        return "OARK_FixedCaravanDipSummit_TimeLeft".Translate(
            ticksRemaining.ToStringTicksToPeriod(),
            baselineNegotiationAbility.ToStringPercent("0.##"),
            diplomaticSummitFactor.ToStringPercent("0.##").Colorize(diplomaticSummitFactorColor)
        );
    }

    public void InitSummit(Caravan caravan, Pawn negotiant)
    {
        settlementDipComp = settlement.GetComponent<SettlementDipComp>();
        this.negotiant = negotiant;
        Find.WindowStack.Add
        (
            new Dialog_MessageBox("OA_ConfirmDiplomaticSummit".Translate(settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT)),
                                  "Confirm".Translate(),
                                  delegate { ConfirmSummit(caravan); },
                                  "Cancel".Translate(),
                                  CancelWork,
                                  buttonADestructive: false
            )
        );
    }

    public void ConfirmSummit(Caravan caravan)
    {
        negotiatingTeamLevel = settlementDipComp?.GetSettlementNegotiatingTeamLevel() ?? NegotiatingTeamLevel.Standard;
        TaggedString text = "OA_ConfirmDiplomaticSummitInfo".Translate(settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT));
        text += "\n\n";
        text += ("OA_DiplomaticSummitNegotiatingTeamLevel" + negotiatingTeamLevel.ToString()).Translate();
        Find.WindowStack.Add
        (
            new Dialog_MessageBox(text,
                                 "Confirm".Translate(),
                                 delegate { TryStartSummit(caravan); },
                                 "Cancel".Translate(),
                                 CancelWork,
                                 buttonADestructive: false
            )
        );
    }

    private void TryStartSummit(Caravan caravan)
    {
        associatedFixedCaravan = (FixedCaravan_DiplomaticSummit)OAFrame_FixedCaravanUtility.CreateFixedCaravan(caravan, OARK_WorldObjectDefOf.OA_FixedCaravan_DiplomaticSummit, settlement);

        if (associatedFixedCaravan is null)
        {
            CancelWork();
            return;
        }
        Find.WorldObjects.Add(associatedFixedCaravan);
        Find.WorldSelector.Select(associatedFixedCaravan);
        ticksRemaining = TicksNeeded;
        isWorking = true;
    }

    public void WorkTick()
    {
        if (isWorking && ticksRemaining-- == 0)
        {
            EndWork(interrupt: false, convertToCaravan: true);
        }
    }

    private void FinishWork()
    {
        DiplomaticSummitUtility.ApplyEffect(negotiatingTeamLevel, associatedFixedCaravan, settlement, negotiant);
        settlementDipComp?.Notify_DiplomaticSummitEnd(interrupt: false);
    }

    public void PreConvertToCaravanByPlayer()
    {
        DiplomaticSummitUtility.Outcome_LeaveHalfway(settlement);
        settlementDipComp?.Notify_DiplomaticSummitEnd(interrupt: false);
        EndWork(interrupt: true, convertToCaravan: false);
    }

    public void PostConvertToCaravan(Caravan caravan) { }

    private void EndWork(bool interrupt, bool convertToCaravan)
    {
        if (isWorking)
        {
            isWorking = false;
            if (interrupt)
            {
                settlementDipComp?.Notify_DiplomaticSummitEnd(interrupt: false);
            }
            else
            {
                FinishWork();
            }
        }

        if (convertToCaravan && associatedFixedCaravan is not null)
        {
            OAFrame_FixedCaravanUtility.ConvertToCaravan(associatedFixedCaravan);
        }

        Reset();
    }

    private void CancelWork()
    {
        if (isWorking)
        {
            EndWork(interrupt: true, convertToCaravan: true);
        }
        else
        {
            if (associatedFixedCaravan is not null)
            {
                OAFrame_FixedCaravanUtility.ConvertToCaravan(associatedFixedCaravan);
            }
            settlementDipComp?.Notify_DiplomaticSummitEnd(interrupt: true);
            Reset();
        }
    }

    private void Reset()
    {
        isWorking = false;
        negotiatingTeamLevel = NegotiatingTeamLevel.Standard;
        ticksRemaining = TicksNeeded;
        associatedFixedCaravan = null;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref associatedFixedCaravan, "associatedFixedCaravan");
        Scribe_References.Look(ref negotiant, "negotiant");

        Scribe_Values.Look(ref isWorking, "workStart", defaultValue: false);
        Scribe_Values.Look(ref negotiatingTeamLevel, "negotiatingTeamLevel", NegotiatingTeamLevel.Standard);
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            settlementDipComp = settlement.GetComponent<SettlementDipComp>();
        }
    }
}

[StaticConstructorOnStartup]
public static class DiplomaticSummitUtility
{
    public static readonly List<(NegotiatingTeamLevel, float)> NegotiatingTeamLevelWeight =
    [
        (NegotiatingTeamLevel.Beginner,0.15f),
        (NegotiatingTeamLevel.Standard,0.5f),
        (NegotiatingTeamLevel.Sophistication,0.25f),
        (NegotiatingTeamLevel.Excellence,0.1f)
    ];

    private const int LeaveHalfwayGoodwill = -50;
    private const int DisasterStoppageDays = 30;

    public static float GetBaselineNegotiationAbility(NegotiatingTeamLevel negotiatingTeamLevel)
    {
        return negotiatingTeamLevel switch
        {
            NegotiatingTeamLevel.Beginner => 1f,
            NegotiatingTeamLevel.Standard => 1.33f,
            NegotiatingTeamLevel.Sophistication => 1.75f,
            NegotiatingTeamLevel.Excellence => 2.5f,
            _ => 1.33f
        };
    }

    public static void ApplyEffect(NegotiatingTeamLevel negotiatingTeamLevel, FixedCaravan fixedCaravan, Settlement settlement, Pawn negotiator)
    {
        float usedBaseline = GetBaselineNegotiationAbility(negotiatingTeamLevel);
        usedBaseline *= OAInteractHandler.Instance.DiplomaticSummitFactor;
        if (fixedCaravan.PawnsListForReading.Contains(Faction.OfPlayer.leader))
        {
            usedBaseline *= 0.95f;
        }

        float negotiationAbility = negotiator.GetStatValue(StatDefOf.NegotiationAbility);
        InteractionOutcome outcome = DiplomacyUtility.DetermineOutcomeCommon(usedBaseline, negotiationAbility);
        if (outcome == InteractionOutcome.Neutral) outcome = InteractionOutcome.Success;

        Faction faction = settlement.Faction;

        DiplomacyRewardData rewardData = new();
        StringBuilder letterTextBuilder = new(128);
        letterTextBuilder.AppendLine($"OARK_DiplomaticSummitOutcomeText_{outcome}".Translate(settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT)));
        letterTextBuilder.AppendLine();
        letterTextBuilder.AppendLine();

        switch (outcome)
        {
            case InteractionOutcome.CriticalSuccess:
                {
                    rewardData.Gifts.AddRange(OAFrame_ThingUtility.GenerateThingListSplitByStack(ThingDefOf.Silver, 1500));
                    rewardData.OrganizerFactionGoodwill = 30;
                    rewardData.OrganizerFactionGoodwill = 60;
                    rewardData.HistoryEvent = OARK_HistoryEventDefOf.OA_DiplomaticSummit_Triumph;
                    rewardData.LetterDef = LetterDefOf.PositiveEvent;
                    break;
                }
            case InteractionOutcome.Neutral or InteractionOutcome.Success:
                {
                    rewardData.AssistPoints = 25;
                    rewardData.NegotiatorXpGain = 6000;
                    rewardData.LetterDef = LetterDefOf.PositiveEvent;
                    break;
                }
            case InteractionOutcome.Failure:
                {
                    rewardData.OrganizerFactionGoodwill = -15;
                    rewardData.AssistPoints = 5;
                    rewardData.NegotiatorXpGain = 3000;
                    rewardData.LetterDef = LetterDefOf.NegativeEvent;
                    rewardData.HistoryEvent = OARK_HistoryEventDefOf.OA_DiplomaticSummit_Flounder;
                    break;
                }
            case InteractionOutcome.CriticalFailure:
                {
                    rewardData.OrganizerFactionGoodwill = -50;
                    rewardData.LetterDef = LetterDefOf.NegativeEvent;
                    rewardData.HistoryEvent = OARK_HistoryEventDefOf.OA_DiplomaticSummit_Disaster;

                    ModUtility.CooldownManager.RegisterRecord(key: KeyLibrary_CDRecord.AssistStoppage,
                                                              cdTicks: DisasterStoppageDays * 60000,
                                                              removeWhenExpired: true);

                    letterTextBuilder.AppendLine("OARK_AssistStoppage".Translate(
                        faction.Named(KeyLibrary_NamedArgument.FACTION),
                        DisasterStoppageDays.Named(KeyLibrary_NamedArgument.Value)));
                    break;
                }
        }

        if (rewardData.OrganizerFactionGoodwill != 0)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(other: faction,
                                                   goodwillChange: rewardData.OrganizerFactionGoodwill,
                                                   reason: rewardData.HistoryEvent,
                                                   lookTarget: settlement);

            letterTextBuilder.AppendLine("OAFrame_FactionGoodwillChange".Translate(
                faction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(rewardData.OrganizerFactionGoodwill, KeyLibrary_NamedArgument.Change, includeSign: true)));
        }
        if (rewardData.AssistPoints != 0)
        {
            OAInteractHandler.Instance.AdjustAssistPoints(rewardData.AssistPoints);
            letterTextBuilder.AppendLine("OARK_AssistPointChange".Translate(
                faction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(rewardData.AssistPoints, KeyLibrary_NamedArgument.Change, includeSign: true)));
        }
        if (!rewardData.Gifts.NullOrEmpty())
        {
            OAInteractHandler.Instance.AdjustAssistPoints(rewardData.AssistPoints);
            letterTextBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(rewardData.Gifts).Named(KeyLibrary_NamedArgument.ThingsLabel)));
        }
        if (rewardData.NegotiatorXpGain != 0 && negotiator?.skills is not null)
        {
            negotiator.skills.Learn(SkillDefOf.Intellectual, rewardData.NegotiatorXpGain);
            letterTextBuilder.AppendLine("PeaceTalksSocialXPGain".Translate(negotiator.LabelShort, rewardData.NegotiatorXpGain, negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
        }

        letterTextBuilder.AppendLine();
        letterTextBuilder.AppendLine();
        letterTextBuilder.Append(DiplomacyUtility.OutcomeProbabilityInfo(usedBaseline, negotiationAbility));

        Find.LetterStack.ReceiveLetter(
            label: $"OARK_DiplomaticSummitOutcome_{outcome}".Translate(),
            text: letterTextBuilder.ToTaggedString(),
            textLetterDef: rewardData.LetterDef,
            lookTargets: settlement,
            relatedFaction: faction);

        OAInteractHandler.Instance.OnDiplomaticSummitTriggered();
    }
    public static void Outcome_LeaveHalfway(Settlement settlement)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(other: faction,
                                               goodwillChange: LeaveHalfwayGoodwill,
                                               canSendMessage: false,
                                               canSendHostilityLetter: false,
                                               reason: OARK_HistoryEventDefOf.OA_DiplomaticSummit_LeaveHalfway);

        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabelDiplomaticSummit_LeaveHalfway".Translate(),
                                       text: "OARK_LetterDiplomaticSummit_LeaveHalfway".Translate(
                                           settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT),
                                           faction.Named(KeyLibrary_NamedArgument.FACTION),
                                           OAFrame_TextUtility.ColoredIntNamedArgument(LeaveHalfwayGoodwill, KeyLibrary_NamedArgument.Goodwill, includeSign: true)),
                                       textLetterDef: LetterDefOf.NegativeEvent,
                                       lookTargets: settlement,
                                       relatedFaction: faction);

        OAInteractHandler.Instance.OnDiplomaticSummitTriggered();
    }

}