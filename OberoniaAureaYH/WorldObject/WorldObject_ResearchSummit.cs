using OberoniaAurea_Frame;
using OberoniaAurea_Frame.Utility;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace OberoniaAurea;

/// <summary>
/// 大地图事件点：科研峰会
/// </summary>
public class WorldObject_ResearchSummit : WorldObject_InteractiveBase
{
    /// <summary>
    /// 研究能力 - 失败
    /// </summary>
    private const float BackfireNeedSpeed = 1.25f;
    /// <summary>
    /// 研究能力 - 小失败
    /// </summary>
    private const float FlounderNeedSpeed = 1.75f;
    /// <summary>
    /// 研究能力 - 中立
    /// </summary>
    private const float NormalNeedSpeed = 2.4f;
    /// <summary>
    /// 研究能力 - 成功
    /// </summary>
    private const float SuccessNeedSpeed = 3.0f;
    /// <summary>
    /// 研究能力 - 大成功
    /// </summary>
    private const float TriumphNeedSpeed = 4.0f;

    private static readonly IntRange ScholarGiftValue = new(1000, 3000);

    private static readonly List<Pair<Action, float>> tmpPossibleOutcomesI = [];
    private static readonly List<Pair<Action, float>> tmpPossibleOutcomesII = [];
    private static readonly List<Pair<Action, float>> tmpPossibleOutcomesIII = [];

    private Settlement associateSettlement;
    public Settlement AssociateSettlement => associateSettlement;

    public void SetAssociateSettlement(Settlement settlement) => associateSettlement = settlement;

    public override void Notify_CaravanArrived(Caravan caravan) //判定谈判结果
    {
        Pawn pawn = BestCaravanPawnUtility.FindPawnWithBestStat(caravan, StatDefOf.ResearchSpeed);
        if (pawn is null)
        {
            Messages.Message("OA_MessageNoResearcher".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }
        float researchSpeed = pawn.GetStatValue(StatDefOf.ResearchSpeed);
        MainOutcome(researchSpeed, caravan);
        EatChanwu(caravan);
        AffectFactionGoodwill(participantFactions);
        GetPossibleOutcomesI(researchSpeed, caravan);
        GetPossibleOutcomesII();
        GetPossibleOutcomesIII();
        pawn.skills?.Learn(SkillDefOf.Intellectual, 6000f, direct: true);
        Fair(Tile);
        QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named(KeyLibrary_NamedArgument.SUBJECT));
        Destroy();
    }

    private void MainOutcome(float researchSpeed, Caravan caravan)
    {
        if (researchSpeed > TriumphNeedSpeed)
        {
            Outcome_Triumph(caravan);
        }
        else if (researchSpeed > SuccessNeedSpeed)
        {
            Outcome_Success(caravan);
        }
        else if (researchSpeed > NormalNeedSpeed)
        {
            Outcome_Normal(caravan);
        }
        else if (researchSpeed > FlounderNeedSpeed)
        {
            Outcome_Flounder(caravan);
        }
        else if (researchSpeed > BackfireNeedSpeed)
        {
            Outcome_Backfire(caravan);
        }
        else
        {
            Outcome_Disaster(caravan);
        }
    }

    /// <summary>
    /// 补满饥饿，获得吃花糕心情
    /// </summary>
    private static void EatChanwu(Caravan caravan)
    {
        if (caravan is null)
        {
            return;
        }
        else
        {
            ThoughtDef thoughtDef = OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB.ingestible.tasteThought;
            foreach (Pawn pawn in caravan.PawnsListForReading)
            {
                if (pawn.needs.food is not null)
                {
                    pawn.needs.food.CurLevelPercentage = 1f;
                }
                pawn.needs.mood?.thoughts?.memories?.TryGainMemory(thoughtDef);
            }
        }
    }

    /// <summary>
    /// 集市
    /// </summary>
    private static void Fair(PlanetTile parentTile)
    {
        if (!OAFrame_TileFinderUtility.GetAvailableNeighborTile(parentTile, out PlanetTile tile) && !TileFinder.TryFindNewSiteTile(out tile))
        {
            tile = parentTile;
        }
        ResearchSummit_Fair worldObject = (ResearchSummit_Fair)WorldObjectMaker.MakeWorldObject(OARK_WorldObjectDefOf.OA_ResearchSummit_Fair);
        worldObject.Tile = tile;
        worldObject.InitInnerTrader();
        TimeoutComp timeComp = worldObject.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(150000);
        Find.WorldObjects.Add(worldObject);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Fair".Translate(),
                                       text: "OA_LetterResearchSummit_Fair".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: worldObject);

    }
    private void GetPossibleOutcomesI(float researchSpeed, Caravan caravan)
    {
        tmpPossibleOutcomesI.Clear();
        tmpPossibleOutcomesI.Add(new Pair<Action, float>(delegate
        { }, 60f));
        if (researchSpeed < BackfireNeedSpeed)
        {
            Faction fVar = participantFactions.RandomElement();
            if (fVar is not null)
            {
                tmpPossibleOutcomesI.Add(new Pair<Action, float>(delegate
                {
                    SamePeople(fVar);
                }, 45f));
            }
        }
        tmpPossibleOutcomesI.Add(new Pair<Action, float>(delegate
        {
            Bonus(caravan, Faction);
        }, 20f));
        tmpPossibleOutcomesI.Add(new Pair<Action, float>(delegate
        {
            ResearcherGift(caravan, associateSettlement, Faction);
        }, 30f));
        tmpPossibleOutcomesI.Add(new Pair<Action, float>(delegate
        {
            ResearcherSite(Tile);
        }, 20f));
        if (researchSpeed > NormalNeedSpeed)
        {
            tmpPossibleOutcomesI.Add(new Pair<Action, float>(ResearcherVisit, 30f));
        }
        if (researchSpeed > SuccessNeedSpeed)
        {
            Faction fVar = participantFactions.Where(f => f.HostileTo(Faction.OfPlayer)).RandomElementWithFallback(null);
            if (fVar is not null)
            {
                tmpPossibleOutcomesI.Add(new Pair<Action, float>(delegate
                {
                    ResearcherPeacetalk(fVar);
                }, 30f));
            }
            tmpPossibleOutcomesI.Add(new Pair<Action, float>(delegate
            {
                ScholarGift(caravan);
            }, 30f));
        }
        tmpPossibleOutcomesI.RandomElementByWeight(x => x.Second).First();
    }
    private void GetPossibleOutcomesII()
    {
        tmpPossibleOutcomesII.Clear();
        tmpPossibleOutcomesII.Add(new Pair<Action, float>(delegate
        {
            MysteriousTrader(Tile);
        }, 35f));
        tmpPossibleOutcomesII.Add(new Pair<Action, float>(delegate
        {
            AssistWork(Tile);
        }, 35f));
        tmpPossibleOutcomesII.RandomElementByWeight(x => x.Second).First();
    }
    private void GetPossibleOutcomesIII()
    {
        tmpPossibleOutcomesIII.Clear();
        tmpPossibleOutcomesIII.Add(new Pair<Action, float>(delegate { }, 45f));
        tmpPossibleOutcomesIII.Add(new Pair<Action, float>(delegate
        {
            AcademicDispute(Tile);
        }, 35f));
        tmpPossibleOutcomesIII.Add(new Pair<Action, float>(delegate
        {
            EccentricScholar(Tile);
        }, 25f));
        tmpPossibleOutcomesIII.RandomElementByWeight(x => x.Second).First();
    }

    /// <summary>
    /// 事件 - 同道中人
    /// </summary>
    private static void SamePeople(Faction faction)
    {
        Slate slate = new();
        Map map = Find.AnyPlayerHomeMap;
        slate.Set("faction", faction);
        OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OA_ResearchSummitSamePeopleJoin, slate, forced: false, target: map);
    }

    /// <summary>
    /// 额外收获
    /// </summary>
    private static void Bonus(Caravan caravan, Faction faction)
    {
        int num = new IntRange(12, 80).RandomInRange;
        List<Thing> things = OAFrame_ThingUtility.GenerateThingListSplitByStack(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB, num);
        OAFrame_CaravanUtility.GiveThings(caravan, things);

        Thing book = ThingMaker.MakeThing(ThingDefOf.Novel);
        if (book is not null)
        {
            CompQuality comp = book.TryGetComp<CompQuality>();
            comp.SetQuality(QualityCategory.Good, ArtGenerationContext.Outsider);
            CaravanInventoryUtility.GiveThing(caravan, book);
        }
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Bonus".Translate(),
                                       text: "OA_LetterResearchSummit_Bonus".Translate(num, book.Named("BOOK")),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: caravan,
                                       relatedFaction: faction);
    }

    /// <summary>
    /// 研究员赠礼
    /// </summary>
    private static void ResearcherGift(Caravan caravan, WorldObject worldObject, Faction faction)
    {
        Settlement settlement = worldObject as Settlement;
        List<Thing> things = OAFrame_ThingUtility.GenerateThingListSplitByStack(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB, 10);
        things.Add(ThingMaker.MakeThing(OARK_ThingDefOf.Oberonia_Aurea_Tea));
        OAFrame_CaravanUtility.GiveThings(caravan, things);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_ResearcherGift".Translate(),
                                       text: "OA_LetterResearchSummit_ResearcherGift".Translate(settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT)),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: caravan,
                                       relatedFaction: faction);
    }

    /// <summary>
    /// 落脚点
    /// </summary>
    private static void ResearcherSite(PlanetTile parentTile)
    {
        WorldObjectsHolder worldObjects = Find.WorldObjects;
        if (!TileFinder.TryFindPassableTileWithTraversalDistance(parentTile, 2, 3, out PlanetTile tile, t => !worldObjects.AnyWorldObjectAt(t)))
        {
            tile = parentTile;
        }
        Faction faction = ResearcherCampComp.GenerateTempCampFaction();
        Site camp = SiteMaker.MakeSite(OARK_WorldObjectDefOf.OA_RK_ResearcherCamp, tile, faction);
        camp.GetComponent<ResearcherCampComp>()?.SetActivate(true);
        TimeoutComp timeComp = camp.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(60000);
        worldObjects.Add(camp);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_ResearcherSite".Translate(),
                                       text: "OA_LetterResearchSummit_ResearcherSite".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: camp);
    }

    /// <summary>
    /// 无（小）势力学者的访问
    /// </summary>
    private static void ResearcherVisit()
    {
        Slate slate = new();
        Map map = QuestGen_Get.GetMap();
        slate.Set("map", map);
        OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OA_ResearcherVisit, slate, forced: false, target: map);
    }

    /// <summary>
    /// 敌对势力和平谈判
    /// </summary>
    private static void ResearcherPeacetalk(Faction faction)
    {
        Slate slate = new();
        slate.Set("faction", faction);

        if (OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_RimWorldDefOf.OpportunitySite_PeaceTalks, slate, forced: false, target: Find.World))
        {
            PeaceTalks peaceTalks = slate.Get<PeaceTalks>("peaceTalks");
            Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_ResearcherPeacetalk".Translate(),
                                           text: "OA_LetterResearchSummit_ResearcherPeacetalk".Translate(faction.NameColored),
                                           textLetterDef: LetterDefOf.PositiveEvent,
                                           lookTargets: peaceTalks,
                                           relatedFaction: faction);
        }
    }

    /// <summary>
    /// 学者赠礼
    /// </summary>
    private static void ScholarGift(Caravan caravan)
    {
        int rewardValue = ScholarGiftValue.RandomInRange;
        RewardsGeneratorParams rewardsGeneratorParams = new()
        {
            rewardValue = rewardValue,
            thingRewardItemsOnly = true,
        };
        Reward_Items reward = new();
        reward.InitFromValue(rewardValue, rewardsGeneratorParams, out _);
        OAFrame_CaravanUtility.GiveThings(caravan, reward.items);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_ScholarGift".Translate(),
                                       text: "OA_LetterResearchSummit_ScholarGift".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: caravan);
    }

    /// <summary>
    /// 神秘商人
    /// </summary>
    private static void MysteriousTrader(PlanetTile parentTile)
    {
        WorldObjectsHolder worldObjects = Find.WorldObjects;
        if (!TileFinder.TryFindPassableTileWithTraversalDistance(parentTile, 2, 3, out PlanetTile tile, t => !worldObjects.AnyWorldObjectAt(t)))
        {
            tile = parentTile;
        }
        ResearchSummit_MysteriousTrader worldObject = (ResearchSummit_MysteriousTrader)WorldObjectMaker.MakeWorldObject(OARK_WorldObjectDefOf.OA_ResearchSummit_MysteriousTrader);
        worldObject.Tile = tile;
        TimeoutComp timeComp = worldObject.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(60000);
        Find.WorldObjects.Add(worldObject);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_MysteriousTrader".Translate(),
                                       text: "OA_LetterResearchSummit_MysteriousTrader".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: worldObject);
    }

    /// <summary>
    /// 学术约架
    /// </summary>
    private static void AcademicDispute(PlanetTile parentTile)
    {
        if (!OAFrame_TileFinderUtility.GetAvailableNeighborTile(parentTile, out PlanetTile tile) && !TileFinder.TryFindNewSiteTile(out tile))
        {
            tile = parentTile;
        }
        ResearchSummit_AcademicDispute worldObject = (ResearchSummit_AcademicDispute)WorldObjectMaker.MakeWorldObject(OARK_WorldObjectDefOf.OA_ResearchSummit_AcademicDispute);
        worldObject.Tile = tile;
        TimeoutComp timeComp = worldObject.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(120000);
        Find.WorldObjects.Add(worldObject);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_AcademicDispute".Translate(),
                                       text: "OA_LetterResearchSummit_AcademicDispute".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: worldObject);
    }

    /// <summary>
    /// 废品旁的学者
    /// </summary>
    private static void EccentricScholar(PlanetTile parentTile)
    {
        if (!OAFrame_TileFinderUtility.GetAvailableNeighborTile(parentTile, out PlanetTile tile) && !TileFinder.TryFindNewSiteTile(out tile))
        {
            tile = parentTile;
        }

        ResearchSummit_EccentricScholar worldObject = (ResearchSummit_EccentricScholar)WorldObjectMaker.MakeWorldObject(OARK_WorldObjectDefOf.OA_ResearchSummitEccentricScholar);
        worldObject.Tile = tile;
        TimeoutComp timeComp = worldObject.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(120000);
        Find.WorldObjects.Add(worldObject);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_EccentricScholar".Translate(),
                                       text: "OA_LetterResearchSummit_EccentricScholar".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: worldObject);
    }

    /// <summary>
    /// 协助者募集点
    /// </summary>
    private static void AssistWork(PlanetTile tile)
    {
        ResearchSummit_AssistWork worldObject = (ResearchSummit_AssistWork)WorldObjectMaker.MakeWorldObject(OARK_WorldObjectDefOf.OA_ResearchSummit_AssistWork);
        worldObject.Tile = tile;
        TimeoutComp timeComp = worldObject.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(180000);
        Find.WorldObjects.Add(worldObject);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_AssistWork".Translate(),
                                       text: "OA_LetterResearchSummit_AssistWork".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: worldObject);
    }

    /// <summary>
    /// 结果 - 大失败
    /// </summary>
    private void Outcome_Disaster(Caravan caravan)
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(1, 200);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Disaster".Translate(),
                                       text: GetLetterText("OA_LetterResearchSummit_Disaster".Translate(Faction.NameColored), caravan, 200, availableResearch),
                                       textLetterDef: LetterDefOf.NeutralEvent,
                                       lookTargets: caravan,
                                       relatedFaction: Faction);
    }

    /// <summary>
    /// 结果 - 失败
    /// </summary>
    private void Outcome_Backfire(Caravan caravan)
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(2, 300);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Backfire".Translate(),
                                       text: GetLetterText("OA_LetterResearchSummit_Backfire".Translate(Faction.NameColored), caravan, 300, availableResearch),
                                       textLetterDef: LetterDefOf.NeutralEvent,
                                       lookTargets: caravan,
                                       relatedFaction: Faction);
    }

    /// <summary>
    /// 结果 - 小失败
    /// </summary>
    private void Outcome_Flounder(Caravan caravan)
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(3, 500);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Flounder".Translate(),
                                       text: GetLetterText("OA_LetterResearchSummit_Flounder".Translate(Faction.NameColored), caravan, 500, availableResearch),
                                       textLetterDef: LetterDefOf.NeutralEvent,
                                       lookTargets: caravan,
                                       relatedFaction: Faction);
    }

    /// <summary>
    /// 结果 - 普通
    /// </summary>
    private void Outcome_Normal(Caravan caravan)
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(4, 800);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Normal".Translate(),
                                       text: GetLetterText("OA_LetterResearchSummit_Normal".Translate(Faction.NameColored), caravan, 800, availableResearch),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: caravan,
                                       relatedFaction: Faction);
    }

    /// <summary>
    /// 结果 - 成功
    /// </summary>
    private void Outcome_Success(Caravan caravan)
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(5, 1200);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Success".Translate(),
                                       text: GetLetterText("OA_LetterResearchSummit_Success".Translate(Faction.NameColored), caravan, 1200, availableResearch),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: caravan,
                                       relatedFaction: Faction);
    }

    /// <summary>
    /// 结果 - 大成功
    /// </summary>
    private void Outcome_Triumph(Caravan caravan)
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(6, 1500);
        Find.LetterStack.ReceiveLetter(label: "OA_LetterLabelResearchSummit_Triumph".Translate(),
                                       text: GetLetterText("OA_LetterResearchSummit_Triumph".Translate(Faction.NameColored), caravan, 1500, availableResearch),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: caravan,
                                       relatedFaction: Faction);
    }

    private static void AffectFactionGoodwill(IEnumerable<Faction> factions)
    {
        Faction oaFaction = ModUtility.OAFaction;
        Faction ofPlayer = Faction.OfPlayer;
        if (oaFaction is not null)
        {
            ofPlayer.TryAffectGoodwillWith(
                other: oaFaction,
                goodwillChange: 15,
                canSendMessage: false,
                canSendHostilityLetter: false,
                reason: OARK_HistoryEventDefOf.OA_ResearchSummit);
        }

        if (factions is not null)
        {
            foreach (Faction faction in factions)
            {
                if (faction != oaFaction)
                {
                    ofPlayer.TryAffectGoodwillWith(
                        other: faction,
                        goodwillChange: 15,
                        canSendMessage: false,
                        canSendHostilityLetter: false,
                        reason: OARK_HistoryEventDefOf.OA_ResearchSummit);
                }
            }
        }
    }

    private static List<ResearchProjectDef> AddResearchProgress(int conut, float amount)
    {
        List<ResearchProjectDef> availableResearch = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(x => x.baseCost > 0f && x.CanStartNow).InRandomOrder().Take(conut).ToList();
        ResearchManager manager = Find.ResearchManager;
        foreach (ResearchProjectDef projectDef in availableResearch)
        {
            manager.AddProgress(projectDef, amount);
        }
        return availableResearch;
    }
    private string GetLetterText(string baseText, Caravan caravan, float amout, List<ResearchProjectDef> availableResearch)
    {
        StringBuilder sb = new(baseText);
        if (availableResearch is not null)
        {
            sb.AppendLine();
            sb.AppendLine();
            foreach (ResearchProjectDef r in availableResearch)
            {
                sb.AppendInNewLine("OA_ResearchProgressChange".Translate(r.label, amout));
            }
        }
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendInNewLine("OA_GoodWillChange".Translate(Faction.NameColored, 15));
        if (participantFactions is not null)
        {
            foreach (Faction f in participantFactions)
            {
                sb.AppendInNewLine("OA_GoodWillChange".Translate(f.NameColored, 15));
            }
        }
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn is not null)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendInNewLine("OA_ResearchSummitIntellectualXPGain".Translate(pawn.LabelShort, 6000, pawn.Named(KeyLibrary_NamedArgument.PAWN)));
        }
        return sb.ToString();
    }
}