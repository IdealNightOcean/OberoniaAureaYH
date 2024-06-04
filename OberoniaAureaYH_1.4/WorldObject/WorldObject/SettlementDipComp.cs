using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public enum NegotiatingTeamLevel
{
    Beginner,
    Standard,
    Sophistication,
    Excellence
}
public enum SettlementDipVisitType
{
    None,
    DeepExchange,
    DiplomaticSummit
}
public class WorldObjectCompProperties_SettlementDipComp : WorldObjectCompProperties
{
    public WorldObjectCompProperties_SettlementDipComp()
    {
        compClass = typeof(SettlementDipComp);
    }
}

[StaticConstructorOnStartup]
public class SettlementDipComp : WorldObjectComp
{
    protected static readonly Texture2D SettlementDipIcon = ContentFinder<Texture2D>.Get("World/OA_RK_SettlementDip");
    protected int diplomacyCoolingDays = 30;
    protected bool tickActive = false;
    protected Settlement Settlement => parent as Settlement;

    protected Caravan expectCaravan;
    protected Pawn expectNegotiant;
    protected SettlementDipVisitType curVisitType = SettlementDipVisitType.None;
    protected NegotiatingTeamLevel curNegotiatingTeamLevel = NegotiatingTeamLevel.Standard;

    protected int ticksRemaining = -1;

    protected int lastDiplomacyTick = -1;
    protected int RemainingDiplomacyCoolingTicks
    {
        get
        {
            if (lastDiplomacyTick < 0)
            {
                return 0;
            }
            return lastDiplomacyTick + diplomacyCoolingDays * 60000 - Find.TickManager.TicksGame;
        }
    }

    protected static bool DiplomacyValid(WorldObject worldObject)
    {
        if (worldObject == null)
        {
            return false;
        }
        Faction OAFaction = OberoniaAureaYHUtility.OAFaction;
        if (OAFaction == null || worldObject.Faction != OAFaction)
        {
            return false;
        }
        return true;
    }

    public override void CompTick()
    {
        if (tickActive)
        {
            ticksRemaining--;
            if (ticksRemaining < 0)
            {
                CheckOutCome();
                ResetCompTick();
            }
        }
    }

    protected void CheckOutCome()
    {
        switch (curVisitType)
        {
            case SettlementDipVisitType.DiplomaticSummit:
                DiplomaticSummitOutcome();
                break;
            default: break;
        }
    }

    protected void ResetCompTick()
    {
        tickActive = false;
        ticksRemaining = -1;
        curVisitType = SettlementDipVisitType.None;
        curNegotiatingTeamLevel = NegotiatingTeamLevel.Standard;
        expectCaravan = null;
        expectNegotiant = null;
    }

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (!DiplomacyValid(parent))
        {
            yield break;
        }
        Command_Action command_DipAction = new()
        {
            defaultLabel = "OA_CommandDipAction".Translate(),
            defaultDesc = "OA_CommandDipActionDesc".Translate(),
            icon = SettlementDipIcon,
            action = delegate
            {
                GizmoFloat_DipAction(caravan);
            }
        };
        AcceptanceReport acceptanceReport = CanVisitNow();
        if (!acceptanceReport.Accepted)
        {
            command_DipAction.Disable(acceptanceReport.Reason.NullOrEmpty() ? null : acceptanceReport.Reason);
        }
        yield return command_DipAction;
    }
    protected virtual void GizmoFloat_DipAction(Caravan caravan) //生成按钮行为：产品列表菜单
    {
        List<FloatMenuOption> list = [];
        string actionStr;
        foreach (SettlementDipVisitType visitType in Enum.GetValues(typeof(SettlementDipVisitType)))
        {
            if(visitType == SettlementDipVisitType.None)
            {
                continue;
            }
            actionStr = ("OA_Settlement" + visitType.ToString()).Translate(parent.Label);
            FloatMenuAcceptanceReport acceptanceReport = CanVisitNowByType(visitType);
            if (acceptanceReport)
            {
                list.Add(new FloatMenuOption(actionStr, delegate
                {
                    Notify_CaravanArrived(caravan, visitType);
                }));
            }
            else
            {
                if (acceptanceReport.FailReason.NullOrEmpty())
                {
                    list.Add(new FloatMenuOption(actionStr, null));
                }
                else
                {
                    list.Add(new FloatMenuOption(actionStr + ": " + acceptanceReport.FailReason, null));          
                }
            }
        }
        Find.WindowStack.Add(new FloatMenu(list));
    }

    /*
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption1 in base.GetFloatMenuOptions(caravan))
        {
            yield return floatMenuOption1;
        }
        if (!DiplomacyValid(parent))
        {
            yield break;
        }
        foreach (FloatMenuOption floatMenuOption2 in GetFloatMenuOptionsByType(caravan, SettlementDipVisitType.DeepExchange, "OA_SettlementDeepExchange".Translate()))
        {
            yield return floatMenuOption2;
        }
        foreach (FloatMenuOption floatMenuOption3 in GetFloatMenuOptionsByType(caravan, SettlementDipVisitType.DiplomaticSummit, "OA_SettlementDiplomaticSummit".Translate()))
        {
            yield return floatMenuOption3;
        }

    }
    

    public IEnumerable<FloatMenuOption> GetFloatMenuOptionsByType(Caravan caravan, SettlementDipVisitType SettlementDipVisitType, string label = null)
    {
        FloatMenuAcceptanceReport report = CanVisitNow(SettlementDipVisitType);
        Settlement settlement = this.Settlement;
        foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitSettlementDip.GetFloatMenuOptions(caravan, settlement, report, label, SettlementDipVisitType))
        {
            yield return floatMenuOption2;
        }
    }
    */
    public void Notify_CaravanArrived(Caravan caravan, SettlementDipVisitType SettlementDipVisitType)
    {
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn == null)
        {
            Messages.Message("OA_MessageNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }
        switch (SettlementDipVisitType)
        {
            case SettlementDipVisitType.None: return;
            case SettlementDipVisitType.DeepExchange:
                DeepExchange(caravan, pawn);
                return;
            case SettlementDipVisitType.DiplomaticSummit:
                DiplomaticSummit(caravan, pawn);
                return;
            default: return;
        }
    }
    private void DeepExchange(Caravan caravan, Pawn pawn)
    {
        DeepExchangeUtility.ApplyEffect(caravan, this.Settlement, pawn);
        diplomacyCoolingDays = 30;
        lastDiplomacyTick = Find.TickManager.TicksGame;
    }
    private void DiplomaticSummit(Caravan caravan, Pawn pawn)
    {
        Settlement settlement = this.Settlement;
        Find.WindowStack.Add
        (
            new Dialog_MessageBox
            (
                "OA_ConfirmDiplomaticSummit".Translate(settlement.Named("SETTLEMENT")),
                "Confirm".Translate(),
                delegate
                {
                    ConfirmDiplomaticSummit(caravan, pawn);
                },
                "GoBack".Translate(),
                buttonADestructive: false
            )
        );

        void ConfirmDiplomaticSummit(Caravan caravan, Pawn pawn)
        {
            Settlement settlement = this.Settlement;
            NegotiatingTeamLevel negotiatingTeamLevel = DiplomaticSummitUtility.NegotiatingTeamLevelWeight.RandomElementByWeight((KeyValuePair<NegotiatingTeamLevel, float> x) => x.Value).Key;
            TaggedString text = "OA_ConfirmDiplomaticSummitInfo".Translate(settlement.Named("SETTLEMENT"));
            text += "\n\n";
            text += ("OA_DiplomaticSummitNegotiatingTeamLevel" + negotiatingTeamLevel.ToString()).Translate();
            Find.WindowStack.Add
            (
                new Dialog_MessageBox(text, "Confirm".Translate(), delegate
                {
                    InitDiplomaticSummit(negotiatingTeamLevel);
                },
                    buttonADestructive: false
                )
            );
        }

        void InitDiplomaticSummit(NegotiatingTeamLevel teamLevel)
        {
            expectCaravan = caravan;
            expectNegotiant = pawn;
            curNegotiatingTeamLevel = teamLevel;
            curVisitType = SettlementDipVisitType.DiplomaticSummit;
            lastDiplomacyTick = Find.TickManager.TicksGame;
            ticksRemaining = 5000;
            tickActive = true;
        }

    }
    private void DiplomaticSummitOutcome()
    {
        if (expectCaravan == null || expectCaravan.Tile != this.parent.Tile)
        {
            DiplomaticSummitUtility.Outcome_LeaveHalfway(this.Settlement);
        }
        else
        {
            Pawn pawn = expectNegotiant ?? BestCaravanPawnUtility.FindBestDiplomat(expectCaravan);
            DiplomaticSummitUtility.ApplyEffect(curNegotiatingTeamLevel, expectCaravan, this.Settlement, pawn);
        }
        diplomacyCoolingDays = 30;
    }

    public AcceptanceReport CanVisitNow()
    {
        if (parent.Faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return "MustBeAlly".Translate();
        }
        int remainingDiplomacyCoolingTicks = RemainingDiplomacyCoolingTicks;
        if (remainingDiplomacyCoolingTicks > 0)
        {
            return "WaitTime".Translate(remainingDiplomacyCoolingTicks.ToStringTicksToPeriod());
        }
        return true;
    }
    public FloatMenuAcceptanceReport CanVisitNowByType(SettlementDipVisitType SettlementDipVisitType)
    {
        switch (SettlementDipVisitType)
        {
            case SettlementDipVisitType.None: return false;
            case SettlementDipVisitType.DeepExchange: return true;
            case SettlementDipVisitType.DiplomaticSummit:
                {
                    float allianceDuration = OberoniaAureaYHUtility.OA_GCOA.AllianceDuration;
                    if (allianceDuration < 60)
                    {
                        return FloatMenuAcceptanceReport.WithFailReason("OA_AllianceDurationShort".Translate(60.ToString("F0")));
                    }
                    return true;
                }
            default: return false;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref lastDiplomacyTick, "lastDiplomacyTick", -1);
        Scribe_Values.Look(ref tickActive, "tickActive", defaultValue: false);
        Scribe_References.Look(ref expectCaravan, "expectCaravan");
        Scribe_References.Look(ref expectNegotiant, "expectNegotiant");
        Scribe_Values.Look(ref curVisitType, "curVisitType", SettlementDipVisitType.None);
        Scribe_Values.Look(ref curNegotiatingTeamLevel, "curNegotiatingTeamLevel", NegotiatingTeamLevel.Standard);
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);
    }

}

public static class DeepExchangeUtility
{
    private static readonly float LearnIntellectualXP = 2000f;
    private static readonly float QuestProbability = 0.3f;
    private static readonly int ChanwuNum = 10;
    private static readonly int AddAssistPoints = 5;

    private readonly static List<QuestScriptDef> tmpPossibleOutcomes = [];

    public static void ApplyEffect(Caravan caravan, Settlement settlement, Pawn pawn)
    {
        OberoniaAureaYHUtility.OA_GCOA?.GetAssistPoints(AddAssistPoints);
        pawn.skills.Learn(SkillDefOf.Intellectual, LearnIntellectualXP, direct: true);
        List<Thing> things = OberoniaAureaYHUtility.TryGenerateThing(OberoniaAureaYHDefOf.Oberonia_Aurea_Chanwu_AC, ChanwuNum);
        foreach (Thing thing in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
        }
        TaggedString text = "OA_LetterResearchDeepExchange".Translate(settlement.Named("SETTLEMENT"));
        tmpPossibleOutcomes.Clear();
        if (Rand.Chance(QuestProbability))
        {
            Slate slate = new();
            if (OberoniaAureaYHDefOf.OA_OpportunitySite_MultiPartyTalks.CanRun(slate))
            {
                tmpPossibleOutcomes.Add(OberoniaAureaYHDefOf.OA_OpportunitySite_MultiPartyTalks);
            }
            slate = new();
            if (OberoniaAureaYHDefOf.OA_OpportunitySite_PunishmentExecutor.CanRun(slate))
            {
                tmpPossibleOutcomes.Add(OberoniaAureaYHDefOf.OA_OpportunitySite_PunishmentExecutor);
            }
            if (tmpPossibleOutcomes.Any())
            {
                slate = new();
                QuestScriptDef questScript = tmpPossibleOutcomes.RandomElement();
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questScript, slate);
                if (!quest.hidden && quest.root.sendAvailableLetter)
                {
                    QuestUtility.SendLetterQuestAvailable(quest);
                }
                text += "\n\n" + "OA_LetterResearchDeepExchangeAdditional".Translate(settlement.Named("SETTLEMENT"), quest.name);
            }
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDeepExchange".Translate(), text, LetterDefOf.PositiveEvent, caravan, settlement.Faction);
    }
}

public static class DiplomaticSummitUtility
{


    public static readonly Dictionary<NegotiatingTeamLevel, float> NegotiatingTeamLevelWeight = new()
    {
        {NegotiatingTeamLevel.Beginner,0.15f},
        {NegotiatingTeamLevel.Standard,0.5f},
        {NegotiatingTeamLevel.Sophistication,0.25f},
        {NegotiatingTeamLevel.Excellence,0.1f}
    };
    private static readonly Dictionary<NegotiatingTeamLevel, float> NegotiatingLevelInfluenceFactor = new()
    {
        {NegotiatingTeamLevel.Beginner,1.3f},
        {NegotiatingTeamLevel.Standard,1.0f},
        {NegotiatingTeamLevel.Sophistication,0.65f},
        {NegotiatingTeamLevel.Excellence,0.25f}
    };

    private static readonly int LeaveHalfwayGoodwill = -50;

    private static readonly int DisasterGoodwill = -50;
    private static readonly int DisasterStoppageDays = 30;

    private static readonly int FlounderGoodwill = -15;
    private static readonly int FlounderAssistPoints = 5;
    private static readonly float FlounderXP = 3000f;

    private static readonly int SuccessAssistPoints = 25;
    private static readonly float SuccessXP = 6000f;

    private static readonly int TriumphGoodwill = 10;
    private static readonly int TriumphAssistPoints = 60;
    private static readonly float TriumphXP = 8000f;

    private readonly static List<Pair<Action, float>> tmpPossibleOutcomes = [];

    public static void ApplyEffect(NegotiatingTeamLevel negotiatingTeamLevel, Caravan caravan, Settlement settlement, Pawn pawn)
    {
        float influenceFactor = NegotiatingLevelInfluenceFactor[negotiatingTeamLevel];
        float negotiationAbility = pawn.GetStatValue(StatDefOf.NegotiationAbility);
        tmpPossibleOutcomes.Clear();
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Disaster(settlement);
        }, 40f));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Flounder(settlement, pawn);
        }, 40f + negotiationAbility * 20f * influenceFactor));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Success(settlement, pawn);
        }, 10f + negotiationAbility * 40f * influenceFactor));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Triumph(caravan, settlement, pawn);
        }, negotiationAbility * 20f * influenceFactor));
        tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First();
    }
    public static void Outcome_LeaveHalfway(Settlement settlement)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, LeaveHalfwayGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_LeaveHalfway);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_LeaveHalfway".Translate(), "OA_LetterDiplomaticSummit_LeaveHalfway".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, LeaveHalfwayGoodwill), LetterDefOf.NegativeEvent, settlement, faction);
    }
    private static void Outcome_Disaster(Settlement settlement)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, DisasterGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_Disaster);
        OberoniaAureaYHUtility.OA_GCOA.assistPointsStoppageDays = DisasterStoppageDays;
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Disaster".Translate(), "OA_LetterDiplomaticSummit_Disaster".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, DisasterGoodwill, DisasterStoppageDays), LetterDefOf.NegativeEvent, settlement, faction);
    }
    private static void Outcome_Flounder(Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, FlounderGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_Flounder);
        OberoniaAureaYHUtility.OA_GCOA?.GetAssistPoints(FlounderAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Flounder".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, FlounderGoodwill, FlounderAssistPoints);
        if (pawn != null)
        {
            pawn.skills.Learn(SkillDefOf.Intellectual, FlounderXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, FlounderXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Flounder".Translate(), text, LetterDefOf.NeutralEvent, settlement, faction);
    }
    private static void Outcome_Success(Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        OberoniaAureaYHUtility.OA_GCOA?.GetAssistPoints(SuccessAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Success".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, SuccessAssistPoints);
        if (pawn != null)
        {
            pawn.skills.Learn(SkillDefOf.Intellectual, SuccessXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, SuccessXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Success".Translate(), text, LetterDefOf.PositiveEvent, settlement, faction);

    }
    private static void Outcome_Triumph(Caravan caravan, Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        List<Thing> things = OberoniaAureaYHUtility.TryGenerateThing(RimWorld.ThingDefOf.Silver, 1500);
        foreach (Thing thing in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
        }
        Faction.OfPlayer.TryAffectGoodwillWith(faction, TriumphGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_Triumph);
        OberoniaAureaYHUtility.OA_GCOA?.GetAssistPoints(TriumphAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Triumph".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, TriumphGoodwill, TriumphAssistPoints, 1500);
        if (pawn != null)
        {
            pawn.skills.Learn(SkillDefOf.Intellectual, TriumphXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, TriumphXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Triumph".Translate(), text, LetterDefOf.PositiveEvent, settlement, faction);

    }

}