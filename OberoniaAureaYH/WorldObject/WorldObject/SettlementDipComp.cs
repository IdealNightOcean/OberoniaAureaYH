using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

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

    protected Settlement Settlement => parent as Settlement;

    protected bool workStart;
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

    protected static bool DiplomacyValid(WorldObject worldObject) //基地派系是否可用
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

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (workStart || !DiplomacyValid(parent))
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
    protected virtual void GizmoFloat_DipAction(Caravan caravan) //远行队到达时的按钮行为：交互类型菜单
    {
        List<FloatMenuOption> list = [];
        string actionStr;
        foreach (SettlementDipVisitType visitType in Enum.GetValues(typeof(SettlementDipVisitType)))
        {
            if (visitType == SettlementDipVisitType.None)
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
    public void Notify_CaravanArrived(Caravan caravan, SettlementDipVisitType SettlementDipVisitType)//触发交互事件
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
    private void DeepExchange(Caravan caravan, Pawn pawn) //触发深入交流
    {
        DeepExchangeUtility.ApplyEffect(caravan, this.Settlement, pawn);
        diplomacyCoolingDays = 30;
        lastDiplomacyTick = Find.TickManager.TicksGame;
    }
    private void DiplomaticSummit(Caravan caravan, Pawn pawn) //触发外交争锋
    {
        Settlement settlement = this.Settlement;
        Find.WindowStack.Add
        (
            Dialog_MessageBox.CreateConfirmation
            (
                "OA_ConfirmDiplomaticSummit".Translate(settlement.Named("SETTLEMENT")),
                delegate
                {
                    ConfirmDiplomaticSummit(caravan, pawn);
                },
                destructive: false
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
            FixedCaravan_DiplomaticSummit fixedCaravan = (FixedCaravan_DiplomaticSummit)FixedCaravan.CreateFixedCaravan(caravan, FixedCaravan_DiplomaticSummit.AssociateObjectDef, FixedCaravan_DiplomaticSummit.CheckInterval);
            fixedCaravan.negotiant = pawn;
            fixedCaravan.associateSettlement = settlement;
            fixedCaravan.curNegotiatingTeamLevel = teamLevel;
            Find.WorldObjects.Add(fixedCaravan);
            Find.WorldSelector.Select(fixedCaravan);
            workStart = true;
        }

    }
    public void DiplomaticSummitEnd()
    {
        lastDiplomacyTick = Find.TickManager.TicksGame;
        diplomacyCoolingDays = 30;
        workStart = false;
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
        Scribe_Values.Look(ref workStart, "workStart", defaultValue: false);
        Scribe_Values.Look(ref lastDiplomacyTick, "lastDiplomacyTick", -1);
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