using OberoniaAurea_Frame.Utility;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;
//研究峰会 - 参会旅行者到达
[StaticConstructorOnStartup]
public class IncidentWorker_ResearchSummitTraveller : IncidentWorker_NeutralGroup
{
    protected static readonly TraderKindDef TraderKindDef = OARatkin_PawnGenerateDefOf.OA_ResearchSummit_TravellerTrader;

    private static readonly SimpleCurve PointsCurve =
    [
        new CurvePoint(0f, 200f),
        new CurvePoint(10000f, 500f)
    ];

    protected LordJob_VisitColony CreateLordJob(IncidentParms parms, List<Pawn> pawns)
    {
        RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out var result);
        return new LordJob_VisitColony(parms.faction, result);
    }
    public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
    {
        if (f.IsPlayerSafe() || f.defeated || f.temporary || f.Hidden)
        {
            return false;
        }

        if (f.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        Map map = (Map)parms.target;
        if (map is null)
        {
            return false;
        }
        if (!desperate && (!f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) || !f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp)))
        {
            return false;
        }

        return !NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f);
    }
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        if (!TryResolveParms(parms))
        {
            return false;
        }
        if (!TryFindResearchSummit(out WorldObject_ResearchSummit researchSummit))
        {
            return false;
        }
        Settlement settlement = (Settlement)researchSummit.AssociateWorldObject;
        PawnGroupMakerParms groupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true);
        if (!OAFrame_PawnGenerateUtility.TryGetRandomPawnGroupMaker(groupMakerParms, OARatkin_PawnGenerateDefOf.OA_ResearchSummit_TravellerMaker, out PawnGroupMaker groupMaker))
        {
            return false;
        }
        List<Pawn> list = SpawnPawns(parms, groupMakerParms, groupMaker);
        if (list.Count == 0)
        {
            return false;
        }
        LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, list), map, list);
        if (TryConvertOnePawnToSmallTrader(list, map, parms, out Pawn trader))
        {
            Messages.Message("OA_ResearchSummitTraveller_Arrival".Translate(parms.faction.NameColored, settlement.Named("SETTLEMENT")), trader, MessageTypeDefOf.NeutralEvent);
        }
        return true;
    }
    protected override void ResolveParmsPoints(IncidentParms parms)
    {
        if (parms.points < 0f)
        {
            parms.points = 5000f;
        }
        parms.points = PointsCurve.Evaluate(parms.points);
    }
    protected List<Pawn> SpawnPawns(IncidentParms parms, PawnGroupMakerParms groupMakerParms, PawnGroupMaker groupMaker)
    {
        Map map = (Map)parms.target;
        List<Pawn> list = OAFrame_PawnGenerateUtility.GeneratePawns(groupMakerParms, groupMaker, warnOnZeroResults: false).ToList();
        foreach (Pawn item in list)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
            GenSpawn.Spawn(item, loc, map);
            parms.storeGeneratedNeutralPawns?.Add(item);
        }
        return list;
    }
    private bool TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Map map, IncidentParms parms, out Pawn trader)
    {
        Faction faction = parms.faction;
        IEnumerable<Pawn> source = pawns.Where((Pawn p) => p.DevelopmentalStage.Adult());
        if (!source.Any())
        {
            trader = null;
            return false;
        }
        Pawn pawn = source.RandomElement();
        Lord lord = pawn.GetLord();
        pawn.mindState.wantsToTradeWithColony = true;
        PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
        pawn.trader.traderKind = TraderKindDef;
        pawn.inventory.DestroyAll();
        trader = pawn;

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
        var obj = Find.WorldObjects.AllWorldObjects.Where(w => w.def == OARatkin_WorldObjectDefOf.OA_RK_ResearchSummit).RandomElementWithFallback(null);
        if (obj is null)
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
