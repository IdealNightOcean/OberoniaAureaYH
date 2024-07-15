using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Verse;


namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class OberoniaAureaYHUtility
{
    public static Faction OAFaction => Find.FactionManager.FirstFactionOfDef(OberoniaAureaYHDefOf.OA_RK_Faction);

    public static GameComponent_OberoniaAurea OA_GCOA => Current.Game.GetComponent<GameComponent_OberoniaAurea>();

    public static List<Thing> TryGenerateThing(ThingDef def, int count)
    {
        List<Thing> list = [];
        int stackLimit = def.stackLimit;
        int remaining = count;
        while (remaining > 0)
        {
            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = Mathf.Min(remaining, stackLimit);
            list.Add(thing);
            remaining -= stackLimit;
        }
        return list;
    }

    public static List<List<Thing>> TryGengrateThingGroup(ThingDef def, int count)
    {
        List<List<Thing>> lists = [];
        int perPodCount = Mathf.Max(1, Mathf.FloorToInt(150 / def.GetStatValueAbstract(StatDefOf.Mass)));
        int remaining = count;
        while (remaining > 0)
        {
            lists.Add(TryGenerateThing(def, Mathf.Min(remaining, perPodCount)));
            remaining -= perPodCount;
        }
        return lists;
    }

    public static int AmountSendable(Map map, ThingDef def) //获取信标附近def物品数
    {
        return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
                where t.def == def
                select t).Sum((Thing t) => t.stackCount);
    }

    public static void CallForAidFixedPoints(Map map, Faction faction, float points, PawnsArrivalModeDef raidArrivalMode = null) //呼叫固定点数支援，固定方式的工作支援
    {
        IncidentParms incidentParms = new()
        {
            target = map,
            faction = faction,
            raidArrivalModeForQuickMilitaryAid = true,
            points = points,
            raidArrivalMode = raidArrivalMode ?? PawnsArrivalModeDefOf.EdgeDrop
        };
        IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
    }

    public static bool AnyEnemiesOfFactionOnMap(Map map, Faction faction) //map上是否有faction派系的敌人
    {
        var potentiallyDangerous = map.mapPawns.AllPawnsSpawned.Where(p => !p.Dead && !p.IsPrisoner && !p.Downed && !p.InContainerEnclosed).ToArray();
        var hostileFactions = potentiallyDangerous.Where(p => p.Faction != null).Select(p => p.Faction).Where(f => f.HostileTo(faction)).ToArray();
        return hostileFactions.Any();
    }

    public static bool GetAvailableNeighborTile(int rootTile, out int tile, bool exclusion = true)
    {
        List<int> allNeighborTiles = [];
        tile = -1;
        Find.WorldGrid.GetTileNeighbors(rootTile, allNeighborTiles);
        var neighborTiles = allNeighborTiles.Where(t => !Find.World.Impassable(t));
        if (neighborTiles.Any())
        {
            if (exclusion)
            {
                WorldObjectsHolder worldObjects = Find.WorldObjects;
                foreach (int item in neighborTiles)
                {
                    if (!worldObjects.AnyWorldObjectAt(item))
                    {
                        tile = item;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                tile = neighborTiles.RandomElement();
                return true;
            }
        }
        return false;
    }

    public static Hediff GetOrAddHediff(Pawn pawn, HediffDef hediffDef)
    {
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
        if (hediff == null)
        {
            return pawn.health.AddHediff(hediffDef);
        }
        return hediff;
    }
    public static void AddHediff(Pawn pawn, HediffDef hediffDef, float severity = -1, int overrideDisappearTicks = -1)
    {
        Hediff hediff = GetOrAddHediff(pawn, hediffDef);
        if (hediff == null)
        {
            return;
        }
        if (severity > 0)
        {
            hediff.Severity = severity;
        }
        if (overrideDisappearTicks > 0)
        {
            HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
            if (comp != null)
            {
                comp.ticksToDisappear = overrideDisappearTicks;
            }
        }

    }
}