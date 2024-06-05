using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;
//研究峰会 - 参会旅行者到达
[StaticConstructorOnStartup]
public class IncidentWorker_ResearchSummitTraveller : IncidentWorker_NeutralGroup
{
    protected override PawnGroupKindDef PawnGroupKindDef => base.PawnGroupKindDef;
    protected static readonly TraderKindDef TraderKindDef = OA_PawnGenerateDefOf.OA_ResearchSummit_TravellerTrader;
    protected static readonly StandalonePawnGroupMakerDef PawnGroupMakerDef = OA_PawnGenerateDefOf.OA_ResearchSummit_TravellerMaker;

    private static readonly SimpleCurve PointsCurve =
    [
        new CurvePoint(45f, 0f),
        new CurvePoint(50f, 1f),
        new CurvePoint(100f, 1f),
        new CurvePoint(200f, 0.25f),
        new CurvePoint(300f, 0.1f),
        new CurvePoint(500f, 0f)
    ];

    protected virtual LordJob_VisitColony CreateLordJob(IncidentParms parms, List<Pawn> pawns)
    {
        RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out var result);
        return new LordJob_VisitColony(parms.faction, result);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        if (!TryResolveParms(parms))
        {
            return false;
        }
        PawnGroupMakerParms groupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true);
        if (!PawnGroupUtility.TryGetRandomPawnGroupMaker(groupMakerParms, PawnGroupMakerDef, out PawnGroupMaker groupMaker))
        {
            return false;
        }
        List<Pawn> list = SpawnPawns(parms, groupMakerParms, groupMaker);
        if (list.Count == 0)
        {
            return false;
        }
        LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, list), map, list);
        bool traderExists = false;
        if (Rand.Value < 0.75f)
        {
            traderExists = TryConvertOnePawnToSmallTrader(list, map, parms);
        }
        Pawn leader = list.Find((Pawn x) => parms.faction.leader == x);
        SendLetter(parms, list, leader, traderExists);
        return true;
    }

    protected virtual void SendLetter(IncidentParms parms, List<Pawn> pawns, Pawn leader, bool traderExists)
    {
        TaggedString letterLabel;
        TaggedString letterText;
        if (pawns.Count == 1)
        {
            TaggedString taggedString = (traderExists ? ("\n\n" + "SingleVisitorArrivesTraderInfo".Translate(pawns[0].Named("PAWN")).AdjustedFor(pawns[0])) : ((TaggedString)""));
            TaggedString taggedString2 = ((leader != null) ? ("\n\n" + "SingleVisitorArrivesLeaderInfo".Translate(pawns[0].Named("PAWN")).AdjustedFor(pawns[0])) : ((TaggedString)""));
            letterLabel = "LetterLabelSingleVisitorArrives".Translate();
            letterText = "SingleVisitorArrives".Translate(pawns[0].story.Title, parms.faction.NameColored, pawns[0].Name.ToStringFull, taggedString, taggedString2, pawns[0].Named("PAWN")).AdjustedFor(pawns[0]);
        }
        else
        {
            TaggedString taggedString3 = (traderExists ? ("\n\n" + "GroupVisitorsArriveTraderInfo".Translate()) : TaggedString.Empty);
            TaggedString taggedString4 = ((leader != null) ? ("\n\n" + "GroupVisitorsArriveLeaderInfo".Translate(leader.LabelShort, leader)) : TaggedString.Empty);
            letterLabel = "LetterLabelGroupVisitorsArrive".Translate();
            letterText = "GroupVisitorsArrive".Translate(parms.faction.NameColored, taggedString3, taggedString4);
        }
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
        SendStandardLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, parms, pawns[0]);
    }

    protected override void ResolveParmsPoints(IncidentParms parms)
    {
        if (!(parms.points >= 0f))
        {
            parms.points = Rand.ByCurve(PointsCurve);
        }
    }
    protected List<Pawn> SpawnPawns(IncidentParms parms, PawnGroupMakerParms groupMakerParms, PawnGroupMaker groupMaker)
    {
        Map map = (Map)parms.target;
        List<Pawn> list = PawnGroupUtility.GeneratePawns(groupMakerParms, groupMaker, warnOnZeroResults: false).ToList();
        foreach (Pawn item in list)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
            GenSpawn.Spawn(item, loc, map);
            parms.storeGeneratedNeutralPawns?.Add(item);
        }
        return list;
    }
    private bool TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Map map, IncidentParms parms)
    {
        Faction faction = parms.faction;
        IEnumerable<Pawn> source = pawns.Where((Pawn p) => p.DevelopmentalStage.Adult());
        if (!source.Any())
        {
            return false;
        }
        Pawn pawn = source.RandomElement();
        Lord lord = pawn.GetLord();
        pawn.mindState.wantsToTradeWithColony = true;
        PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
        pawn.trader.traderKind = TraderKindDef;
        pawn.inventory.DestroyAll();

        ThingSetMakerParams parms2 = default;
        parms2.traderDef = TraderKindDef;
        parms2.tile = map.Tile;
        parms2.makingFaction = faction;
        foreach (Thing item in ThingSetMakerDefOf.TraderStock.root.Generate(parms2))
        {
            if (item is Pawn pawn2)
            {
                if (pawn2.Faction != pawn.Faction)
                {
                    pawn2.SetFaction(pawn.Faction);
                }
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 5);
                GenSpawn.Spawn(pawn2, loc, map);
                lord.AddPawn(pawn2);
            }
            else if (!pawn.inventory.innerContainer.TryAdd(item))
            {
                item.Destroy();
            }
        }
        PawnInventoryGenerator.GiveRandomFood(pawn);
        return true;
    }
    private static bool TryFindResearchSummit(out WorldObject_ResearchSummit researchSummit)
    {
        var obj = Find.WorldObjects.AllWorldObjects.Where(w => w.def == OA_WorldObjectDefOf.OA_RK_ResearchSummit).RandomElementWithFallback(null);
        if (obj == null)
        {
            researchSummit = null;
            return false;
        }
        else
        {
            researchSummit = (WorldObject_ResearchSummit)obj;
            return true;
        }
    }

}
