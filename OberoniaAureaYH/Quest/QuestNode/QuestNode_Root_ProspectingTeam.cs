using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class QuestNode_Root_ProspectingTeam : QuestNode_Root_RefugeeBase
{
    protected override bool TestRunInt(Slate slate)
    {
        Faction faction = ModUtility.OAFaction;
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        return base.TestRunInt(slate);
    }

    protected override Faction GetOrGenerateFaction()
    {
        return ModUtility.OAFaction;
    }

    protected override QuestParameter InitQuestParameter(Faction faction)
    {
        return new QuestParameter(faction, QuestGen_Get.GetMap())
        {
            allowAssaultColony = false,
            allowLeave = true,
            allowBadThought = false,
            LodgerCount = 4,
            ChildCount = 0,

            goodwillSuccess = 12,
            goodwillFailure = -12,
            rewardValueRange = new FloatRange(300f, 500f) * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor,
            questDurationTicks = 60000,

            fixedPawnKind = OARK_PawnGenerateDefOf.OA_RK_Court_Member
        };
    }

    protected override void RunInt()
    {
        questParameter = null;
        Faction faction = GetOrGenerateFaction();
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            return;
        }

        questParameter = InitQuestParameter(faction);
        Quest quest = questParameter.quest;

        string lodgerArrestedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrested");
        string lodgerRecruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
        string lodgerBecameMutantSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.BecameMutant");
        string lodgerArrestedOrRecruited = QuestGen.GenerateNewSignal("Lodger_ArrestedOrRecruited");
        quest.AnySignal(inSignals: [lodgerRecruitedSignal, lodgerArrestedSignal], outSignals: [lodgerArrestedOrRecruited]);

        List<Pawn> pawns = questParameter.slate.Get<List<Pawn>>("pawns");
        if (pawns.NullOrEmpty())
        {
            quest.End(QuestEndOutcome.Unknown, sendLetter: true, playSound: false);
            return;
        }
        questParameter.pawns = pawns;

        quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, [lodgerRecruitedSignal, lodgerBecameMutantSignal]);

        string lodgerArrivalSignal = null;

        quest.JoinPlayer(questParameter.map.Parent, pawns, joinPlayer: true);

        SetQuestAward();

        QuestPart_OARefugeeInteractions questPart_RefugeeInteractions = new()
        {
            inSignalEnable = questParameter.slate.Get<string>("inSignal"),
            faction = faction,
            mapParent = questParameter.map.Parent,
            inSignalArrested = lodgerArrestedSignal,
            inSignalRecruited = lodgerRecruitedSignal,
            signalListenMode = QuestPart.SignalListenMode.Always
        };
        questPart_RefugeeInteractions.InitWithDefaultSingals(questParameter.allowAssaultColony, questParameter.allowLeave, questParameter.allowBadThought);
        questPart_RefugeeInteractions.pawns.AddRange(pawns);
        quest.AddPart(questPart_RefugeeInteractions);

        SetQuestEndLetters(questPart_RefugeeInteractions);

        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_RefugeeInteractions.outSignalArrested_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony);

        SetQuestEndCompCommon(questPart_RefugeeInteractions);
        SetPawnsLeaveComp(lodgerArrivalSignal, lodgerArrestedOrRecruited);
        SetSlateValue();
    }

    protected override void SetQuestEndComp(QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string bigFailSignal, string successSignal)
    {
        base.SetQuestEndComp(questPart_Interactions, failSignal, bigFailSignal, successSignal);
        Quest quest = questParameter.quest;
        quest.AnySignal(inSignals: [successSignal],
                        action: delegate
                        {
                            GiveSpecialReward(questParameter.map, questParameter.faction, questParameter.quest);
                        });
    }

    private static void GiveSpecialReward(Map map, Faction faction, Quest quest = null)
    {
        if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10, c => CanScatterAt(c, map), map, out IntVec3 centerCell))
        {
            Log.Error("Could not find a center cell for deep scanning lump generation!");
        }
        ThingDef thingDef = ThingDefOf.Uranium;
        int numCells = Mathf.CeilToInt(thingDef.deepLumpSizeRange.RandomInRange);
        Thing deepDrill = ThingMaker.MakeThing(ThingDefOf.DeepDrill);
        if (deepDrill.def.CanHaveFaction)
        {
            deepDrill.SetFaction(Faction.OfPlayer);
        }
        Thing deepMini = MinifyUtility.MakeMinified(deepDrill);
        GenPlace.TryPlaceThing(deepMini, centerCell, map, ThingPlaceMode.Near);

        foreach (IntVec3 cell in GridShapeMaker.IrregularLump(centerCell, map, numCells))
        {
            if (CanScatterAt(cell, map) && !cell.InNoBuildEdgeArea(map))
            {
                map.deepResourceGrid.SetAt(cell, thingDef, thingDef.deepCountPerCell);
            }
        }

        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_ProspectingTeamReward".Translate(),
                                       text: "OARK_Letter_ProspectingTeamReward".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       new LookTargets(deepMini),
                                       relatedFaction: faction,
                                       quest: quest);
    }

    private static bool CanScatterAt(IntVec3 pos, Map map)
    {
        int index = CellIndicesUtility.CellToIndex(pos, map.Size.x);
        TerrainDef terrainDef = map.terrainGrid.BaseTerrainAt(pos);
        if ((terrainDef is not null && terrainDef.IsWater && terrainDef.passability == Traversability.Impassable)
            || !pos.GetAffordances(map).Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded))
        {
            return false;
        }
        return !map.deepResourceGrid.GetCellBool(index);
    }
}
