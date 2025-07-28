using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;

public abstract class IncidentWorker_VisitorGroupBase : IncidentWorker_NeutralGroup
{
    protected virtual TraderKindDef FixedTraderKind => OARK_RimWorldDefOf.Visitor_Outlander_Standard;
    protected virtual float TraderChance => 0.75f;

    protected virtual int? DurationTicks => null;

    protected static readonly SimpleCurve PointsCurve =
        [
            new CurvePoint(45f, 0f),
            new CurvePoint(50f, 1f),
            new CurvePoint(100f, 1f),
            new CurvePoint(200f, 0.25f),
            new CurvePoint(300f, 0.1f),
            new CurvePoint(500f, 0f)
        ];

    public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
    {
        if (!BaseFactionCanBeGroupSource(f, parms, MustHaveSettlementOnLayer, desperate))
        {
            return false;
        }

        Map map = (Map)parms.target;
        if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f))
        {
            return false;
        }

        if (!f.def.neutralArrivalLayerWhitelist.NullOrEmpty() && !f.def.neutralArrivalLayerWhitelist.Contains(map.Tile.LayerDef))
        {
            return false;
        }

        if (!f.def.neutralArrivalLayerBlacklist.NullOrEmpty() && f.def.neutralArrivalLayerBlacklist.Contains(map.Tile.LayerDef))
        {
            return false;
        }

        return true;
    }

    protected virtual LordJob_VisitColonyBase CreateLordJob(IncidentParms parms, List<Pawn> pawns)
    {
        RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out IntVec3 result);
        return new LordJob_VisitColonyBase(parms.faction, result, DurationTicks);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        Log.Message("0");
        if (!TryResolveParms(parms))
        {
            return false;
        }
        Log.Message("1");
        List<Pawn> pawns = GenerateAndSpawnPawns(parms);
        if (pawns.NullOrEmpty())
        {
            return false;
        }
        Log.Message("2");
        LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, pawns), map, pawns);
        bool traderExists = false;
        Pawn trader = null;
        if (Rand.Value < TraderChance)
        {
            (traderExists, trader) = TryConvertOnePawnToSmallTrader(pawns, parms.faction, map);
        }
        Log.Message("3");
        PostTraderResolved(parms, pawns, trader, traderExists);
        return true;
    }

    protected virtual List<Pawn> GenerateAndSpawnPawns(IncidentParms parms)
    {
        List<Pawn> pawns = GeneratePawns(parms);
        if (pawns.NullOrEmpty())
        {
            return null;
        }

        Map map = (Map)parms.target;
        foreach (Pawn pawn in pawns)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
            GenSpawn.Spawn(pawn, loc, map);
            parms.storeGeneratedNeutralPawns?.Add(pawn);
        }
        return pawns;
    }

    protected abstract List<Pawn> GeneratePawns(IncidentParms parms);

    protected virtual void PostTraderResolved(IncidentParms parms, List<Pawn> pawns, Pawn trader, bool traderExists)
    {
        TaggedString letterLabel;
        TaggedString letterText;
        TaggedString taggedString;
        if (pawns.Count == 1)
        {
            taggedString = (traderExists ? ("\n\n" + "SingleVisitorArrivesTraderInfo".Translate(pawns[0].Named("PAWN")).AdjustedFor(pawns[0])) : ((TaggedString)""));
            letterLabel = "LetterLabelSingleVisitorArrives".Translate();
            letterText = "SingleVisitorArrives".Translate(pawns[0].story.Title, parms.faction.NameColored, pawns[0].Name.ToStringFull, taggedString, (TaggedString)"", pawns[0].Named("PAWN")).AdjustedFor(pawns[0]);
        }
        else
        {
            taggedString = (traderExists ? ("\n\n" + "GroupVisitorsArriveTraderInfo".Translate()) : TaggedString.Empty);
            letterLabel = "LetterLabelGroupVisitorsArrive".Translate();
            letterText = "GroupVisitorsArrive".Translate(parms.faction.NameColored, taggedString, (TaggedString)"");
        }
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
        SendStandardLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, parms, pawns[0]);
    }

    protected override void ResolveParmsPoints(IncidentParms parms)
    {
        if (parms.points < 0f)
        {
            parms.points = Rand.ByCurve(PointsCurve);
        }
    }

    protected virtual (bool, Pawn) TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Faction faction, Map map, bool force = false)
    {
        if (!force && faction.def.visitorTraderKinds.NullOrEmpty())
        {
            return (false, null);
        }

        Pawn trader = pawns.Where(p => p.DevelopmentalStage.Adult())?.RandomElementWithFallback(null);
        if (trader is null)
        {
            return (false, null);
        }

        trader.mindState.wantsToTradeWithColony = true;
        PawnComponentsUtility.AddAndRemoveDynamicComponents(trader, actAsIfSpawned: true);
        TraderKindDef traderKindDef;
        if (!faction.def.visitorTraderKinds.NullOrEmpty())
        {
            traderKindDef = faction.def.visitorTraderKinds.RandomElementByWeight(traderDef => traderDef.CalculatedCommonality);
        }
        else
        {
            traderKindDef = FixedTraderKind;
        }
        trader.trader.traderKind = traderKindDef;
        trader.inventory.DestroyAll();

        ThingSetMakerParams parms = new()
        {
            traderDef = traderKindDef,
            tile = map.Tile,
            makingFaction = faction
        };

        Lord lord = trader.GetLord();
        foreach (Thing item in ThingSetMakerDefOf.TraderStock.root.Generate(parms))
        {
            if (item is Pawn pawn)
            {
                if (pawn.Faction != trader.Faction)
                {
                    pawn.SetFaction(trader.Faction);
                }
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(trader.Position, map, 5);
                GenSpawn.Spawn(pawn, loc, map);
                lord.AddPawn(pawn);
            }
            else if (!trader.inventory.innerContainer.TryAdd(item))
            {
                item.Destroy();
            }
        }
        PawnInventoryGenerator.GiveRandomFood(trader);
        TradeUtility.CheckGiveTraderQuest(trader);
        return (false, trader);
    }


    protected static bool BaseFactionCanBeGroupSource(Faction f, IncidentParms parms, bool mustHaveSettlementOnLayer, bool desperate = false)
    {
        Map map = (Map)parms.target;
        if (f.IsPlayer)
        {
            return false;
        }
        if (f.defeated)
        {
            return false;
        }
        if (f.temporary)
        {
            return false;
        }
        if (!desperate && (!f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) || !f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp)))
        {
            return false;
        }
        if (mustHaveSettlementOnLayer && !f.Hidden && map.Tile.Valid && !Find.WorldObjects.AnyFactionSettlementOnLayer(f, map.Tile.Layer))
        {
            return false;
        }
        if (!f.def.arrivalLayerWhitelist.NullOrEmpty() && !f.def.arrivalLayerWhitelist.Contains(map.Tile.LayerDef))
        {
            return false;
        }
        if (!f.def.arrivalLayerBlacklist.NullOrEmpty() && f.def.arrivalLayerBlacklist.Contains(map.Tile.LayerDef))
        {
            return false;
        }
        if (map.Tile.LayerDef.onlyAllowWhitelistedArrivals && (f.def.arrivalLayerWhitelist.NullOrEmpty() || !f.def.arrivalLayerWhitelist.Contains(map.Tile.LayerDef)))
        {
            return false;
        }
        return true;
    }
}
