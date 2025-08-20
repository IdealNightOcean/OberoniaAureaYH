using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

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
    private int diplomacyCoolingDays = 30;

    private Settlement Settlement => parent as Settlement;

    private DiplomaticSummitHandler diplomaticSummitHandler;
    public DiplomaticSummitHandler DiplomaticSummitHandler => diplomaticSummitHandler;
    private bool IsWorking => diplomaticSummitHandler?.IsWorking ?? false;

    private int lastDiplomacyTick = -1;
    private NegotiatingTeamLevel? curNegotiatingTeamLevel = null;
    private int RemainingCoolingTicks
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

    private static bool DiplomacyValid(WorldObject worldObject) //基地派系是否可用
    {
        Faction oaFaction = ModUtility.OAFaction;
        return oaFaction is not null && worldObject.Faction == oaFaction;
    }

    public override void CompTick()
    {
        base.CompTick();
        diplomaticSummitHandler?.WorkTick();
    }
    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (IsWorking || !DiplomacyValid(parent))
        {
            yield break;
        }
        Command_Action command_DipAction = new()
        {
            defaultLabel = "OA_CommandDipAction".Translate(),
            defaultDesc = "OA_CommandDipActionDesc".Translate(),
            icon = IconUtility.OADipIcon,
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
        if (pawn is null)
        {
            Messages.Message("OAFrame_MessageNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
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
        DeepExchangeUtility.ApplyEffect(caravan, Settlement, pawn);
        diplomacyCoolingDays = 30;
        lastDiplomacyTick = Find.TickManager.TicksGame;
    }

    protected void DiplomaticSummit(Caravan caravan, Pawn pawn) //触发外交争锋
    {
        if (!OAFrame_CaravanUtility.IsExactTypeCaravan(caravan))
        {
            return;
        }

        diplomaticSummitHandler = new(Settlement);
        diplomaticSummitHandler.InitSummit(caravan, pawn);
    }

    public NegotiatingTeamLevel GetSettlementNegotiatingTeamLevel()
    {
        curNegotiatingTeamLevel ??= DiplomaticSummitUtility.NegotiatingTeamLevelWeight.RandomElementByWeight(kv => kv.Item2).Item1;

        return curNegotiatingTeamLevel.Value;
    }

    public void Notify_DiplomaticSummitEnd(bool cancel)
    {
        if (!cancel)
        {
            curNegotiatingTeamLevel = null;
            lastDiplomacyTick = Find.TickManager.TicksGame;
            diplomacyCoolingDays = 30;
        }
        diplomaticSummitHandler = null;
    }

    public AcceptanceReport CanVisitNow()
    {
        if (parent.Faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return "MustBeAlly".Translate();
        }
        int remainingDiplomacyCoolingTicks = RemainingCoolingTicks;
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
                    if (OAInteractHandler.Instance.AllianceDuration() < 60)
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
        Scribe_Values.Look(ref curNegotiatingTeamLevel, "curNegotiatingTeamLevel");
        Scribe_Deep.Look(ref diplomaticSummitHandler, "diplomaticSummitHandler", ctorArgs: Settlement);
    }

}

public static class DeepExchangeUtility
{
    private const float LearnIntellectualXP = 2000f;
    private const float QuestProbability = 0.5f;
    private const int ChanwuNum = 10;
    private const int AddAssistPoints = 5;

    private readonly static List<(QuestScriptDef, float)> tmpPossibleOutcomes = [];

    public static void ApplyEffect(Caravan caravan, Settlement settlement, Pawn pawn)
    {
        OAInteractHandler.Instance.AdjustAssistPoints(AddAssistPoints);
        pawn.skills?.Learn(SkillDefOf.Intellectual, LearnIntellectualXP, direct: true);
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AC, ChanwuNum);
        foreach (Thing thing in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
        }
        TaggedString text = "OA_LetterResearchDeepExchange".Translate(settlement.Named("SETTLEMENT"));
        tmpPossibleOutcomes.Clear();
        if (Rand.Chance(QuestProbability))
        {
            AddQuest(OARK_QuestScriptDefOf.OA_OpportunitySite_MultiPartyTalks, 20f, Find.World);
            AddQuest(OARK_QuestScriptDefOf.OA_OpportunitySite_PunishmentExecutor, 15f, Find.World);
            AddQuest(OARK_QuestScriptDefOf.OA_UrbanConstruction, 40f, Find.World, settlement);
            AddQuest(OARK_QuestScriptDefOf.OA_FestivalOrders, 25f, QuestGen_Get.GetMap());
            if (tmpPossibleOutcomes.Any())
            {
                Slate slate = new();
                QuestScriptDef questScript = tmpPossibleOutcomes.RandomElementByWeight(q => q.Item2).Item1;
                if (questScript == OARK_QuestScriptDefOf.OA_UrbanConstruction)
                {
                    slate.Set("settlement", settlement);
                }
                OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out Quest quest, questScript, slate, forced: true);
                text += "\n\n" + "OA_LetterResearchDeepExchangeAdditional".Translate(settlement.Named("SETTLEMENT"), quest.name);
            }
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDeepExchange".Translate(), text, LetterDefOf.PositiveEvent, caravan, settlement.Faction);
    }
    private static void AddQuest(QuestScriptDef scriptDef, float chance, IIncidentTarget target, Settlement targetSettle = null)
    {
        Slate slate = new();
        if (targetSettle is not null)
        {
            slate.Set("settlement", targetSettle);
        }
        if (scriptDef.CanRun(slate, target))
        {
            tmpPossibleOutcomes.Add((scriptDef, chance));
        }
    }
}