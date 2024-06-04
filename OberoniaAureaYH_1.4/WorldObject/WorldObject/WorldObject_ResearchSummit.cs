using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

//大地图事件点：科研峰会
public class WorldObject_ResearchSummit : WorldObject_WithMutiFactions
{
    private static readonly float BackfireNeedSpeed = 1.25f; //失败所需要的研究能力
    private static readonly float FlounderNeedSpeed = 1.75f; //小失败所需要的研究能力
    private static readonly float NormalNeedSpeed = 2.4f; //中立所需要的研究能力
    private static readonly float SuccessNeedSpeed = 3.0f; //成功所需要的研究能力
    private static readonly float TriumphNeedSpeed = 4.0f; //大成功所需要的研究能力

    private static readonly float AcademicDisputeFactor = 0.4f; //学术约架概率

    private static readonly IntRange ScholarGiftValue = new(1000, 3000);

    private readonly static List<Pair<Action, float>> tmpPossibleOutcomes = [];

    protected static ResearchManager Manager => Find.ResearchManager;
    private static readonly BindingFlags BindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public override void Notify_CaravanArrived(Caravan caravan) //判定谈判结果
    {
        Pawn pawn = BestCaravanPawnUtility.FindPawnWithBestStat(caravan, StatDefOf.ResearchSpeed);
        if (pawn == null)
        {
            Messages.Message("OA_MessageNoResearcher".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }
        float researchSpeed = pawn.GetStatValue(StatDefOf.ResearchSpeed);

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
        EatChanwu(caravan);
        GetPossibleOutcomes(researchSpeed, caravan);
        AffectFactionGoodwill(base.Faction, participantFactions);
        pawn.skills.Learn(SkillDefOf.Intellectual, 6000f, direct: true);
        if (Rand.Chance(AcademicDisputeFactor))
        {
            AcademicDispute(this.Tile);
        }
        QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named("SUBJECT"));
        Destroy();
    }
    private static void EatChanwu(Caravan caravan) //补满饥饿，获得吃花糕心情
    {
        if (caravan == null)
        {
            return;
        }
        else
        {
            ThoughtDef thoughtDef = OberoniaAureaYHDefOf.Oberonia_Aurea_Chanwu_AB.ingestible.tasteThought;
            foreach (Pawn pawn in caravan.PawnsListForReading)
            {
                if (pawn.needs.food != null)
                {
                    pawn.needs.food.CurLevelPercentage = 1f;
                }
                pawn.needs.mood?.thoughts?.memories?.TryGainMemory(thoughtDef);
            }
        }
    }
    private void GetPossibleOutcomes(float researchSpeed, Caravan caravan)
    {
        tmpPossibleOutcomes.Clear();
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        { }, 60f));
        if (researchSpeed < BackfireNeedSpeed)
        {
            Faction fVar = participantFactions.RandomElement();
            if (fVar != null)
            {
                tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
                {
                    SamePeople(fVar);
                }, 45f));
            }
        }
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Bonus(caravan, base.Faction);
        }, 20f));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            ResearcherGift(caravan, associateWorldObject, base.Faction);
        }, 30f));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            ResearcherSite(this.Tile);
        }, 20f));
        if (researchSpeed > NormalNeedSpeed)
        {
            tmpPossibleOutcomes.Add(new Pair<Action, float>(ResearcherVisit, 30f));
        }
        if (researchSpeed > SuccessNeedSpeed)
        {
            Faction fVar = participantFactions.Where(f => f.HostileTo(Faction.OfPlayer)).RandomElementWithFallback(null);
            if (fVar != null)
            {
                tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
                {
                    ResearcherPeacetalk(fVar);
                }, 30f));
            }
            tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
            {
                ScholarGift(caravan);
            }, 30f));
        }
        tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First();
    }
    private static void SamePeople(Faction faction) //同道中人
    {
        Slate slate = new();
        slate.Set("faction", faction);
        if (OberoniaAureaYHDefOf.OA_ResearchSummitSamePeopleJoin.CanRun(slate))
        {
            QuestUtility.GenerateQuestAndMakeAvailable(OberoniaAureaYHDefOf.OA_ResearchSummitSamePeopleJoin, slate);
        }
    }
    private static void Bonus(Caravan caravan, Faction faction) //额外收获
    {
        int num = new IntRange(12, 80).RandomInRange;
        List<Thing> things = OberoniaAureaYHUtility.TryGenerateThing(OberoniaAureaYHDefOf.Oberonia_Aurea_Chanwu_AB, num);

        foreach (Thing thing in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_Bonus".Translate(), "OA_LetterResearchSummit_Bonus".Translate(num), LetterDefOf.PositiveEvent, caravan, faction);
    }
    private static void ResearcherGift(Caravan caravan, WorldObject worldObject, Faction faction) //研究员赠礼
    {
        Settlement settlement = worldObject as Settlement;
        List<Thing> things = OberoniaAureaYHUtility.TryGenerateThing(OberoniaAureaYHDefOf.Oberonia_Aurea_Chanwu_AB, 10);
        things.Add(ThingMaker.MakeThing(OberoniaAureaYHDefOf.Oberonia_Aurea_Tea));
        foreach (Thing thing in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_ResearcherGift".Translate(), "OA_LetterResearchSummit_ResearcherGift".Translate(settlement.Named("SETTLEMENT")), LetterDefOf.PositiveEvent, caravan, faction);
    }
    private static void ResearcherSite(int parentTile) //无势力学者们的住宿点
    {

        int tile = GetTile(parentTile);
        Faction faction = ResearcherCampComp.GenerateTempCampFaction();
        Site camp = SiteMaker.MakeSite(OberoniaAureaYHDefOf.OA_RK_ResearcherCamp, tile, faction);
        camp.GetComponent<ResearcherCampComp>()?.SetActivate(true);
        TimeoutComp timeComp = camp.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(60000);
        Find.WorldObjects.Add(camp);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_ResearcherSite".Translate(), "OA_LetterResearchSummit_ResearcherSite".Translate(), LetterDefOf.PositiveEvent, camp, faction);

        static int GetTile(int pTile)
        {
            List<int> parentNeighborTiles = [];
            List<int> neighborTiles = [];
            List<int> validNeighborTiles = [];
            Find.WorldGrid.GetTileNeighbors(pTile, neighborTiles);
            if (neighborTiles.Any())
            {
                WorldObjectsHolder worldObjects = Find.WorldObjects;
                foreach (int tile1 in neighborTiles)
                {
                    parentNeighborTiles.Clear();
                    Find.WorldGrid.GetTileNeighbors(tile1, parentNeighborTiles);
                    foreach (int tile2 in parentNeighborTiles)
                    {
                        if (!worldObjects.AnyWorldObjectAt(tile2))
                        {
                            return tile2;
                        }
                    }
                    if (!worldObjects.AnyWorldObjectAt(tile1))
                    {
                        validNeighborTiles.Add(tile1);
                    }
                }

                return validNeighborTiles.Any() ? validNeighborTiles.RandomElement() : pTile;
            }
            else
            {
                return pTile;
            }
        }
    }
    private static void ResearcherVisit() //无势力学者的访问
    {
        Slate slate = new();
        if (OberoniaAureaYHDefOf.OA_ResearcherVisit.CanRun(slate))
        {
            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(OberoniaAureaYHDefOf.OA_ResearcherVisit, slate);
            if (!quest.hidden && quest.root.sendAvailableLetter)
            {
                QuestUtility.SendLetterQuestAvailable(quest);
            }
        }
    }
    private static void ResearcherPeacetalk(Faction faction) //敌对势力和平谈判
    {
        Slate slate = new();
        slate.Set("faction", faction);
        if (OA_RimWorldDefOf.OpportunitySite_PeaceTalks.CanRun(slate))
        {
            QuestUtility.GenerateQuestAndMakeAvailable(OA_RimWorldDefOf.OpportunitySite_PeaceTalks, slate);
            PeaceTalks peaceTalks = slate.Get<PeaceTalks>("peaceTalks");
            Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_ResearcherPeacetalk".Translate(), "OA_LetterResearchSummit_ResearcherPeacetalk".Translate(faction.NameColored), LetterDefOf.PositiveEvent, peaceTalks, faction);
        }
    }
    private static void ScholarGift(Caravan caravan) //学者赠礼
    {
        int rewardValue = ScholarGiftValue.RandomInRange;
        RewardsGeneratorParams rewardsGeneratorParams = new()
        {
            rewardValue = rewardValue,
            thingRewardItemsOnly = true,
        };
        Reward_Items reward = new();
        reward.InitFromValue(rewardValue, rewardsGeneratorParams, out _);
        foreach (Thing item in reward.items)
        {
            CaravanInventoryUtility.GiveThing(caravan, item);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_ScholarGift".Translate(), "OA_LetterResearchSummit_ScholarGift".Translate(), LetterDefOf.PositiveEvent, caravan);
    }
    private static void AcademicDispute(int parentTile)
    {
        List<int> neighborTiles = [];
        int tile = -1;
        Find.WorldGrid.GetTileNeighbors(parentTile, neighborTiles);
        if (neighborTiles.Any())
        {
            WorldObjectsHolder worldObjects = Find.WorldObjects;
            foreach (int item in neighborTiles)
            {
                if (!worldObjects.AnyWorldObjectAt(item))
                {
                    tile = item;
                    break;
                }
            }
        }
        if(tile == -1)
        {
            tile = parentTile;
        }
        ResearchSummit_AcademicDispute worldObject = (ResearchSummit_AcademicDispute)WorldObjectMaker.MakeWorldObject(OberoniaAureaYHDefOf.OA_RK_AcademicDispute);
        worldObject.Tile = tile;
        TimeoutComp timeComp = worldObject.GetComponent<TimeoutComp>();
        timeComp?.StartTimeout(120000);
        Find.WorldObjects.Add(worldObject);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelAcademicDispute".Translate(), "OA_LetterAcademicDispute".Translate(), LetterDefOf.PositiveEvent, worldObject);
    }
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
        {
            yield return floatMenuOption;
        }
        foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this))
        {
            yield return floatMenuOption2;
        }
    }
    private void Outcome_Disaster(Caravan caravan) //大失败事件
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(1, 200);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_Disaster".Translate(), GetLetterText("OA_LetterResearchSummit_Disaster".Translate(base.Faction.NameColored), caravan, 200, availableResearch), LetterDefOf.NeutralEvent, caravan, base.Faction);
    }
    private void Outcome_Backfire(Caravan caravan) //失败事件
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(2, 300);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_Backfire".Translate(), GetLetterText("OA_LetterResearchSummit_Backfire".Translate(base.Faction.NameColored), caravan, 300, availableResearch), LetterDefOf.NeutralEvent, caravan, base.Faction);
    }
    private void Outcome_Flounder(Caravan caravan) //小失败事件
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(3, 500);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_Flounder".Translate(), GetLetterText("OA_LetterResearchSummit_Flounder".Translate(base.Faction.NameColored), caravan, 500, availableResearch), LetterDefOf.NeutralEvent, caravan, base.Faction);
    }
    private void Outcome_Normal(Caravan caravan) //中立事件
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(4, 800);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_Normal".Translate(), GetLetterText("OA_LetterResearchSummit_Normal".Translate(base.Faction.NameColored), caravan, 800, availableResearch), LetterDefOf.PositiveEvent, caravan, base.Faction);
    }
    private void Outcome_Success(Caravan caravan) //成功谈判的结果
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(5, 1200);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_Success".Translate(), GetLetterText("OA_LetterResearchSummit_Success".Translate(base.Faction.NameColored), caravan, 1200, availableResearch), LetterDefOf.PositiveEvent, caravan, base.Faction);
    }

    private void Outcome_Triumph(Caravan caravan) //大成功谈判的结果
    {
        List<ResearchProjectDef> availableResearch = AddResearchProgress(6, 1500);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelResearchSummit_Triumph".Translate(), GetLetterText("OA_LetterResearchSummit_Triumph".Translate(base.Faction.NameColored), caravan, 1500, availableResearch), LetterDefOf.PositiveEvent, caravan, base.Faction);
    }

    private static void AffectFactionGoodwill(Faction oaFaction, List<Faction> factions)
    {
        if (oaFaction != null)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(oaFaction, 15, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_ResearchSummit);
        }

        if (!factions.NullOrEmpty())
        {
            foreach (Faction faction in factions)
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, 15, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_ResearchSummit);
            }
        }
    }

    private static List<ResearchProjectDef> AddResearchProgress(int conut, float amount)
    {
        List<ResearchProjectDef> availableResearch = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(x => x.baseCost > 0f && x.CanStartNow).Take(conut).ToList();
        Dictionary<ResearchProjectDef, float> progress = GetProjectProgress();
        foreach (ResearchProjectDef projectDef in availableResearch)
        {
            AddProgress(progress, projectDef, amount);
        }
        SetProjectProgress(progress);
        return availableResearch;
    }
    public static Dictionary<ResearchProjectDef, float> GetProjectProgress()
    {
        ResearchManager manager = Manager;
        object obj = manager?.GetType().GetField("progress", BindingAttr)?.GetValue(manager);
        if (obj != null)
        {
            return obj as Dictionary<ResearchProjectDef, float>;
        }
        return null;
    }
    public static void SetProjectProgress(Dictionary<ResearchProjectDef, float> progress)
    {
        if (progress.NullOrEmpty())
        {
            return;
        }
        ResearchManager manager = Manager;
        manager?.GetType().GetField("progress", BindingAttr)?.SetValue(manager, progress);
    }
    public static void AddProgress(Dictionary<ResearchProjectDef, float> progress, ResearchProjectDef proj, float amount, Pawn source = null)
    {
        if (progress == null)
        {
            return;
        }
        if (progress.TryGetValue(proj, out var value))
        {
            progress[proj] = value + amount;
        }
        else
        {
            progress.Add(proj, amount);
        }

        progress[proj] = Mathf.Min(progress[proj], proj.baseCost);
        if (proj.PrerequisitesCompleted && proj.IsFinished)
        {
            Manager.FinishProject(proj, doCompletionDialog: true, source);
        }
        SetProjectProgress(progress);
    }
    private string GetLetterText(string baseText, Caravan caravan, float amout, List<ResearchProjectDef> availableResearch)
    {
        StringBuilder sb = new(baseText);
        if (availableResearch != null)
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
        sb.AppendInNewLine("OA_GoodWillChange".Translate(base.Faction.NameColored, 15));
        if (participantFactions != null)
        {
            foreach (Faction f in participantFactions)
            {
                sb.AppendInNewLine("OA_GoodWillChange".Translate(f.NameColored, 15));
            }
        }
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn != null)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendInNewLine("OA_ResearchSummitIntellectualXPGain".Translate(pawn.LabelShort, 6000f.ToString("F0"), pawn.Named("PAWN")));
        }
        return sb.ToString();
    }

    [DebugOutput("Incidents", false)]
    private static void ResearchSummitChances()
    {
        StringBuilder sb = new();
        sb.AppendLine();
        sb.AppendInNewLine("Disaster: " + 1f.ToStringPercent());
        sb.AppendInNewLine("Backfire: " + 1.25f.ToStringPercent());
        sb.AppendInNewLine("Flounder: " + 1.75f.ToStringPercent());
        sb.AppendInNewLine("Normal: " + 2.4f.ToStringPercent());
        sb.AppendInNewLine("Success: " + 3.0f.ToStringPercent());
        sb.AppendInNewLine("Triumph: " + 4.0f.ToStringPercent());
        Log.Message(sb.ToString());
    }


}
