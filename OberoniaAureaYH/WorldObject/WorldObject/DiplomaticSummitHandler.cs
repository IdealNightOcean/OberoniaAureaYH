using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
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

    public string FixedCaravanWorkDesc() => "OA_FixedCaravanDipSummit_TimeLeft".Translate(ticksRemaining.ToStringTicksToPeriod());

    public void InitSummit(Caravan caravan, Pawn negotiant)
    {
        settlementDipComp = settlement.GetComponent<SettlementDipComp>();
        this.negotiant = negotiant;
        Find.WindowStack.Add
        (
            new Dialog_MessageBox("OA_ConfirmDiplomaticSummit".Translate(settlement.Named("SETTLEMENT")),
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
        TaggedString text = "OA_ConfirmDiplomaticSummitInfo".Translate(settlement.Named("SETTLEMENT"));
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
        settlementDipComp?.Notify_DiplomaticSummitEnd(cancel: false);
    }

    public void PreConvertToCaravanByPlayer()
    {
        DiplomaticSummitUtility.Outcome_LeaveHalfway(settlement);
        settlementDipComp?.Notify_DiplomaticSummitEnd(cancel: false);
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
                settlementDipComp?.Notify_DiplomaticSummitEnd(cancel: false);
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
            settlementDipComp?.Notify_DiplomaticSummitEnd(cancel: true);
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
    private static readonly Dictionary<NegotiatingTeamLevel, float> NegotiatingLevelInfluenceFactor = new()
    {
        {NegotiatingTeamLevel.Beginner,1.3f},
        {NegotiatingTeamLevel.Standard,1.0f},
        {NegotiatingTeamLevel.Sophistication,0.65f},
        {NegotiatingTeamLevel.Excellence,0.25f}
    };

    private const int LeaveHalfwayGoodwill = -50;

    private const int DisasterGoodwill = -50;
    private const int DisasterStoppageDays = 30;

    private const int FlounderGoodwill = -15;
    private const int FlounderAssistPoints = 5;
    private const float FlounderXP = 3000f;

    private const int SuccessAssistPoints = 25;
    private const float SuccessXP = 6000f;

    private const int TriumphGoodwill = 10;
    private const int TriumphAssistPoints = 60;
    private const float TriumphXP = 8000f;

    private static readonly List<(Action, float)> tmpPossibleOutcomes = [];

    public static void ApplyEffect(NegotiatingTeamLevel negotiatingTeamLevel, FixedCaravan fixedCaravan, Settlement settlement, Pawn pawn)
    {
        float influenceFactor = NegotiatingLevelInfluenceFactor[negotiatingTeamLevel];
        float negotiationAbility = pawn.GetStatValue(StatDefOf.NegotiationAbility);
        tmpPossibleOutcomes.Clear();
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_Disaster(settlement);
        }, 40f));
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_Flounder(settlement, pawn);
        }, 40f + negotiationAbility * 20f * influenceFactor));
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_Success(settlement, pawn);
        }, 10f + negotiationAbility * 40f * influenceFactor));
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_Triumph(fixedCaravan, settlement, pawn);
        }, negotiationAbility * 20f * influenceFactor));
        tmpPossibleOutcomes.RandomElementByWeight(x => x.Item2).Item1();
    }
    public static void Outcome_LeaveHalfway(Settlement settlement)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, LeaveHalfwayGoodwill, canSendMessage: false, canSendHostilityLetter: false, OARK_HistoryEventDefOf.OA_DiplomaticSummit_LeaveHalfway);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_LeaveHalfway".Translate(), "OA_LetterDiplomaticSummit_LeaveHalfway".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, LeaveHalfwayGoodwill), LetterDefOf.NegativeEvent, settlement, faction);
    }
    private static void Outcome_Disaster(Settlement settlement)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, DisasterGoodwill, canSendMessage: false, canSendHostilityLetter: false, OARK_HistoryEventDefOf.OA_DiplomaticSummit_Disaster);
        OAInteractHandler.Instance.CooldownManager.RegisterRecord("AssistStoppage", cdTicks: DisasterStoppageDays * 60000, removeWhenExpired: true);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Disaster".Translate(), "OA_LetterDiplomaticSummit_Disaster".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, DisasterGoodwill, DisasterStoppageDays), LetterDefOf.NegativeEvent, settlement, faction);
    }
    private static void Outcome_Flounder(Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, FlounderGoodwill, canSendMessage: false, canSendHostilityLetter: false, OARK_HistoryEventDefOf.OA_DiplomaticSummit_Flounder);
        OAInteractHandler.Instance.AdjustAssistPoints(FlounderAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Flounder".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, FlounderGoodwill, FlounderAssistPoints);
        if (pawn is not null)
        {
            pawn.skills?.Learn(SkillDefOf.Intellectual, FlounderXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, FlounderXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Flounder".Translate(), text, LetterDefOf.NeutralEvent, settlement, faction);
    }
    private static void Outcome_Success(Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        OAInteractHandler.Instance.AdjustAssistPoints(SuccessAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Success".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, SuccessAssistPoints);
        if (pawn is not null)
        {
            pawn.skills?.Learn(SkillDefOf.Intellectual, SuccessXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, SuccessXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Success".Translate(), text, LetterDefOf.PositiveEvent, settlement, faction);

    }
    private static void Outcome_Triumph(FixedCaravan fixedCaravan, Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.Silver, 1500);
        OAFrame_FixedCaravanUtility.GiveThings(fixedCaravan, things);
        Faction.OfPlayer.TryAffectGoodwillWith(faction, TriumphGoodwill, canSendMessage: false, canSendHostilityLetter: false, OARK_HistoryEventDefOf.OA_DiplomaticSummit_Triumph);
        OAInteractHandler.Instance.AdjustAssistPoints(TriumphAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Triumph".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, TriumphGoodwill, TriumphAssistPoints, 1500);
        if (pawn is not null)
        {
            pawn.skills?.Learn(SkillDefOf.Intellectual, TriumphXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, TriumphXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Triumph".Translate(), text, LetterDefOf.PositiveEvent, settlement, faction);
    }

}