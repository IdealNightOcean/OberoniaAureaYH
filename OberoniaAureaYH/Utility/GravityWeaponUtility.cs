using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class GravityWeaponUtility
{
    public static void GetPawnsInRadius(IntVec3 ctrPosition, Map map, Thing caster, float radius, List<Pawn> targetPawns, bool onlyTargetHostile = false)
    {
        targetPawns.Clear();
        Faction faction = caster?.Faction;
        onlyTargetHostile = onlyTargetHostile && faction is not null;
        float radiusSquared = radius * radius;

        IReadOnlyList<Pawn> potentialPawns = map.mapPawns.AllPawnsSpawned;

        for (int i = 0; i < potentialPawns.Count; i++)
        {
            if (PawnValidator(potentialPawns[i]))
            {
                targetPawns.Add(potentialPawns[i]);
            }
        }

        bool PawnValidator(Pawn p)
        {
            if (p.Position.DistanceToSquared(ctrPosition) > radiusSquared)
            {
                return false;
            }
            if (onlyTargetHostile && !p.HostileTo(faction))
            {
                return false;
            }
            return true;
        }
    }
    public static void GetPawnsInRadiusForPawnCaster(IntVec3 ctrPosition, Map map, Pawn caster, float radius, List<Pawn> targetPawns, bool onlyTargetHostile = false, bool canApplyOnSelf = true)
    {
        targetPawns.Clear();
        Faction faction = caster?.Faction;
        onlyTargetHostile = onlyTargetHostile && faction is not null;
        float radiusSquared = radius * radius;

        IReadOnlyList<Pawn> potentialPawns = map.mapPawns.AllPawnsSpawned;

        for (int i = 0; i < potentialPawns.Count; i++)
        {
            if (PawnValidator(potentialPawns[i]))
            {
                targetPawns.Add(potentialPawns[i]);
            }
        }

        bool PawnValidator(Pawn p)
        {
            if (p.Position.DistanceToSquared(ctrPosition) > radiusSquared)
            {
                return false;
            }
            if (onlyTargetHostile && !p.HostileTo(faction))
            {
                return false;
            }
            if (!canApplyOnSelf && p == caster) //canApplyOnSelf或caster不是Pawn时跳过检测
            {
                return false;
            }
            return true;
        }
    }
    public static void TryDisplacement(IntVec3 ctrPosition, Map map, DisplacementExplosionExtension modEx_DE, List<Pawn> targetPawns, Action<Pawn> PostPawnDisplaced = null)
    {
        if (modEx_DE is null)
        {
            Log.Error("Tried to cause displacement at" + ctrPosition.ToString() + ", But got a null DisplacedExplosionExtension.");
            return;
        }

        Vector3 ctrDrawPos = ctrPosition.ToVector3Shifted();
        ctrDrawPos.y = 0f;
        foreach (Pawn pawn in targetPawns)
        {
            IntVec3 displacedDestination = DisplacementDestination(pawn, ctrDrawPos, map, modEx_DE);
            pawn.Position = displacedDestination;
            pawn.pather.StopDead();
            pawn.jobs.StopAll();
            PostPawnDisplaced?.Invoke(pawn);
        }

        /*
        Effecter effecter;
        if (modEx_DE.isRepel)
        {
            effecter = GravityWeapon_DefOf.YHG_Effecter_Repulsive.Spawn();
        }
        else
        {
            effecter = GravityWeapon_DefOf.YHG_Effecter_Attractive.Spawn();
            effecter.scale = modEx_DE.displacementRadius / 5f;
        }
        effecter.Trigger(new TargetInfo(ctrPosition, map), new TargetInfo(ctrPosition, map));
        effecter.Cleanup();
        */
    }
    private static IntVec3 DisplacementDestination(Pawn pawn, Vector3 ctrDrawPos, Map map, DisplacementExplosionExtension modEx_DE)
    {
        Vector3 displacedOrigin = pawn.DrawPos;
        displacedOrigin.y = 0f;
        float distance = Vector3.Distance(ctrDrawPos, displacedOrigin);

        float displacedDistance = modEx_DE.defaultDisplacement;
        if (modEx_DE.affectedByDistance)
        {
            displacedDistance *= modEx_DE.displacementCurveFromDistance.Evaluate(distance);
        }
        if (modEx_DE.affectedByMass)
        {
            float mass = pawn.GetStatValue(StatDefOf.Mass) + MassUtility.GearAndInventoryMass(pawn);
            displacedDistance *= modEx_DE.displacementCurveFromMass.Evaluate(mass);
        }
        Vector3 displacedOne;
        if (modEx_DE.isRepel)
        {
            displacedOne = (displacedOrigin - ctrDrawPos).normalized;
        }
        else
        {
            displacedDistance = Mathf.Min(displacedDistance, distance);
            displacedOne = (ctrDrawPos - displacedOrigin).normalized;
        }
        Vector3 destination = displacedOrigin + displacedOne * displacedDistance;
        IntVec3 displacedDestination = LastPointOnLineOfSight(displacedOrigin.ToIntVec3(), destination.ToIntVec3(), map);

        return displacedDestination.IsValid ? displacedDestination : pawn.Position;
    }
    public static IntVec3 LastPointOnLineOfSight(IntVec3 start, IntVec3 end, Map map)
    {
        IntVec3 lastValidCell = start;
        foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(start, end))
        {
            if (!ValidDisplacementTarget(map, cell))
            {
                return lastValidCell;
            }
            lastValidCell = cell;
        }

        return lastValidCell;
    }
    public static bool ValidDisplacementTarget(Map map, IntVec3 cell)
    {
        if (!cell.IsValid || !cell.InBounds(map))
        {
            return false;
        }

        if (cell.Impassable(map) || !cell.WalkableByNormal(map) || cell.Fogged(map))
        {
            return false;
        }

        Building edifice = cell.GetEdifice(map);
        if (edifice is not null && edifice is Building_Door building_Door && !building_Door.Open)
        {
            return false;
        }

        return true;
    }

}