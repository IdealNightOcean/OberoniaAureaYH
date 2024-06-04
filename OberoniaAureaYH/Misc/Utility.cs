using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;


namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class OberoniaAureaYHUtility
{

    public static Faction OAFaction => Find.FactionManager.FirstFactionOfDef(OberoniaAureaYHDefOf.OA_RK_Faction);
    public static GameComponent_OberoniaAurea OA_GCOA => Current.Game.GetComponent<GameComponent_OberoniaAurea>();
    public static DiaOption OKToRoot(Faction faction, Pawn negotiator) //返回通讯台派系通讯初始界面
    {
        return new DiaOption("OK".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
        };
    }
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

    public static void CallForAidFixedPoints(Map map, Faction faction, float points) //呼叫固定点数支援
    {
        IncidentParms incidentParms = new()
        {
            target = map,
            faction = faction,
            raidArrivalModeForQuickMilitaryAid = true,
            points = points
        };
        IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
    }

    public static bool AnyEnemiesOfFactionOnMap(Map map) //map上是否有玩家的敌人
    {
        return AnyEnemiesOfFactionOnMap(map, Faction.OfPlayer);
    }

    public static bool AnyEnemiesOfFactionOnMap(Map map, Faction faction) //map上是否有faction派系的敌人
    {
        var potentiallyDangerous = map.mapPawns.AllPawnsSpawned.Where(p => !p.Dead && !p.IsPrisoner && !p.Downed && !p.InContainerEnclosed).ToArray();
        var hostileFactions = potentiallyDangerous.Where(p => p.Faction != null).Select(p => p.Faction).Where(f => f.HostileTo(faction)).ToArray();
        return hostileFactions.Any();
    }

    public static void GetPawnsInRadius(IntVec3 ctrPosition, Map map, float radius, List<Pawn> targetPawns) //获取以ctrPosition为圆心半径radius内的pawn
    {
        targetPawns.Clear();
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(ctrPosition, radius, useCenter: true).ToList())
        {
            List<Thing> thingList = map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i] is Pawn pawn)
                {
                    targetPawns.Add(pawn);
                }
            }
        }
    }

    public static IEnumerable<BodyPartRecord> HittablePartsViolence(Pawn pawn)
    {
        HediffSet hediffSet = pawn.health.hediffSet;
        return from x in hediffSet.GetNotMissingParts()
               where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, hediffSet.hediffs))
               where !pawn.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
               select x;
    }

    public static bool HealthyPawn(Pawn pawn) //判断一个Pawn是否健康
    {
        if (pawn.Destroyed || pawn.InMentalState)
        {
            return false;
        }
        HediffSet pawnHediffSet = pawn.health.hediffSet;
        if (pawnHediffSet == null) //没有健康状态属性那肯定是健康的（确信）
        {
            return true;
        }
        if (pawnHediffSet.BleedRateTotal > 0.001f)
        {
            return false;
        }
        if (pawnHediffSet.HasHediff(OberoniaAureaYHDefOf.OA_RK_SeriousInjuryI) || pawnHediffSet.HasHediff(OberoniaAureaYHDefOf.OA_RK_SeriousInjuryII))
        {
            return false;
        }
        if (pawnHediffSet.HasNaturallyHealingInjury())
        {
            return false;
        }
        return true;
    }

    public static int GetAvailableNeighborTile(int rootTile, bool exclusion = true)
    {
        List<int> allNeighborTiles = [];
        int tile = -1;
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
                        break;
                    }
                }
            }
            else
            {
                tile = neighborTiles.RandomElement();
            }
        }
        return tile;
    }
}


