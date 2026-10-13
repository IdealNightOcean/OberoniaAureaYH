using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class WorldObjectCompProperties_SettlementDipComp : WorldObjectCompProperties
{
    public WorldObjectCompProperties_SettlementDipComp() => compClass = typeof(SettlementDipComp);
}

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

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref lastDiplomacyTick, nameof(lastDiplomacyTick), defaultValue: -1);
        Scribe_Values.Look(ref curNegotiatingTeamLevel, nameof(curNegotiatingTeamLevel));
        Scribe_Deep.Look(ref diplomaticSummitHandler, nameof(diplomaticSummitHandler), ctorArgs: Settlement);
    }

    /// <summary>
    /// 据点派系是否有效
    /// </summary>
    private static bool DiplomacyValid(WorldObject worldObject) => worldObject.Faction is not null && worldObject.Faction == ModUtility.OAFaction;

    public override void CompTick()
    {
        base.CompTick();
        diplomaticSummitHandler?.WorkTick();
    }

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (IsWorking || !DiplomacyValid(parent))
            yield break;

        Command_Action command_DipAction = new()
        {
            defaultLabel = "OA_CommandDipAction".Translate(),
            defaultDesc = "OA_CommandDipActionDesc".Translate(),
            icon = IconLibrary.OADipIcon,
            action = () => GizmoFloat_DipAction(caravan)
        };
        AcceptanceReport acceptanceReport = CanVisitNow();
        if (!acceptanceReport.Accepted)
        {
            command_DipAction.Disable(String.IsNullOrEmpty(acceptanceReport.Reason) ? null : acceptanceReport.Reason);
        }
        yield return command_DipAction;
    }

    /// <summary>
    /// 远行队到达时的按钮行为：交互类型菜单
    /// </summary>
    protected virtual void GizmoFloat_DipAction(Caravan caravan)
    {
        List<FloatMenuOption> list = [];
        string actionStr;
        foreach (SettlementDipVisitType visitType in Enum.GetValues(typeof(SettlementDipVisitType)))
        {
            if (visitType == SettlementDipVisitType.None)
                continue;

            actionStr = ($"OA_Settlement{visitType}".Translate(parent.Label));
            FloatMenuAcceptanceReport acceptanceReport = CanVisitNowByType(visitType);
            if (acceptanceReport)
            {
                list.Add(new FloatMenuOption(actionStr, action: () => Notify_CaravanArrived(caravan, visitType)));
            }
            else
            {
                if (acceptanceReport.FailReason.NullOrEmpty())
                {
                    list.Add(new FloatMenuOption(actionStr, action: null));
                }
                else
                {
                    list.Add(new FloatMenuOption(actionStr + ": " + acceptanceReport.FailReason, action: null));
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

    /// <summary>
    /// 触发深入交流
    /// </summary>
    private void DeepExchange(Caravan caravan, Pawn pawn) //
    {
        DeepExchangeUtility.ApplyEffect(caravan, Settlement, pawn);
        diplomacyCoolingDays = 30;
        lastDiplomacyTick = Find.TickManager.TicksGame;
    }

    /// <summary>
    /// 触发外交争锋
    /// </summary>
    protected void DiplomaticSummit(Caravan caravan, Pawn pawn)
    {
        if (!caravan.IsExactTypeCaravan())
            return;

        diplomaticSummitHandler = new(Settlement);
        diplomaticSummitHandler.InitSummit(caravan, pawn);
    }

    public NegotiatingTeamLevel GetSettlementNegotiatingTeamLevel()
    {
        curNegotiatingTeamLevel ??= DiplomaticSummitUtility.NegotiatingTeamLevelWeight.RandomElementByWeight(kv => kv.Item2).Item1;

        return curNegotiatingTeamLevel.Value;
    }

    public void Notify_DiplomaticSummitEnd(bool interrupt)
    {
        if (!interrupt)
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
            return "MustBeAlly".Translate();

        int remainingDiplomacyCoolingTicks = RemainingCoolingTicks;
        if (remainingDiplomacyCoolingTicks > 0)
            return "WaitTime".Translate(remainingDiplomacyCoolingTicks.ToStringTicksToPeriod());

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
                    if (OAInteractHandler.Instance.AllianceDuration() >= 60)
                        return true;

                    return FloatMenuAcceptanceReport.WithFailReason("OA_AllianceDurationShort".Translate(60.ToString("F0")));
                }
            default: return false;
        }
    }
}

public static class DeepExchangeUtility
{
    private const float LearnIntellectualXP = 2000f;
    private const float QuestProbability = 0.5f;
    private const int FlowerCakeCount = 10;
    private const int IntelligenceDraftCount = 10;
    private const int AddAssistPoints = 5;

    private static readonly List<(QuestScriptDef, float)> tmpPossibleOutcomes = [];

    public static void ApplyEffect(Caravan caravan, Settlement settlement, Pawn pawn)
    {
        StringBuilder letterTextBuilder = new(128);
        letterTextBuilder.AppendLine("OA_LetterResearchDeepExchange".Translate(settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT)));

        OAInteractHandler.Instance.AdjustAssistPoints(AddAssistPoints);
        letterTextBuilder.AppendLine("OARK_AssistPointChange".Translate(
            settlement.Faction.Named(KeyLibrary_NamedArgument.FACTION),
            OAFrame_TextUtility.ColoredIntNamedArgument(AddAssistPoints, KeyLibrary_NamedArgument.Change, includeSign: true)));

        if (pawn?.skills is not null)
        {
            pawn.skills.Learn(SkillDefOf.Intellectual, LearnIntellectualXP);
            letterTextBuilder.AppendLine("PeaceTalksSocialXPGain".Translate(pawn.LabelShort, LearnIntellectualXP, pawn.Named(KeyLibrary_NamedArgument.PAWN)));
        }

        List<Thing> gifts = [];
        gifts.AddRange(OAFrame_ThingUtility.GenerateThingsSplitByStack(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AC, FlowerCakeCount));
        gifts.AddRange(OAFrame_ThingUtility.GenerateThingsSplitByStack(OARK_ThingDefOf.OARK_IntelligenceDraft, IntelligenceDraftCount));

        OAFrame_CaravanUtility.GiveThings(caravan, gifts);
        letterTextBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(gifts).Named(KeyLibrary_NamedArgument.ThingsLabel)));

        tmpPossibleOutcomes.Clear();
        if (Rand.Chance(QuestProbability))
        {
            AddQuest(OARK_QuestScriptDefOf.OA_OpportunitySite_MultiPartyTalks, 20f);
            AddQuest(OARK_QuestScriptDefOf.OA_OpportunitySite_PunishmentExecutor, 15f);
            AddQuest(OARK_QuestScriptDefOf.OA_UrbanConstruction, 40f, targetSettle: settlement);
            AddQuest(OARK_QuestScriptDefOf.OA_FestivalOrders, 25f, target: QuestGen_Get.GetMap());
            if (tmpPossibleOutcomes.Count > 0)
            {
                QuestScriptDef questScript = tmpPossibleOutcomes.RandomElementByWeight(q => q.Item2).Item1;
                Slate slate = new();
                slate.Set("settlement", settlement);
                OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out Quest quest, questScript, slate, forced: true);
                letterTextBuilder.AppendLine();
                letterTextBuilder.AppendLine();
                letterTextBuilder.AppendLine("OA_LetterResearchDeepExchangeAdditional".Translate(
                    settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT),
                    quest.name));
            }
            tmpPossibleOutcomes.Clear();
        }
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelDeepExchange".Translate(),
                                       text: letterTextBuilder.ToTaggedString(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: caravan,
                                       relatedFaction: settlement.Faction);
    }
    private static void AddQuest(QuestScriptDef scriptDef, float chance, IIncidentTarget target = null, Settlement targetSettle = null)
    {
        target ??= Find.World;
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