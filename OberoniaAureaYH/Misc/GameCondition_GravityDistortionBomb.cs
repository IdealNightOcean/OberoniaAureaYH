using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace OberoniaAurea;

public class GameCondition_GravityDistortionBomb : GameCondition
{
    private int ticksToNextBomb = 120;

    public override void PostMake()
    {
        base.PostMake();
        ticksToNextBomb = 300;
    }

    public override void GameConditionTick()
    {
        base.GameConditionTick();
        if (--ticksToNextBomb < 0)
        {
            ticksToNextBomb = 120;
            ApplyGravityDistortionBomb(SingleMap);
        }
    }

    private static void ApplyGravityDistortionBomb(Map map)
    {
        if (map is null)
        {
            return;
        }

        Find.CameraDriver.shaker.DoShake(6f);
        FleckMaker.Static(map.Center, map, OARK_ModDefOf.OARK_Fleck_GravBomb);
        OARK_ModDefOf.OARK_Sound_GravBomb.PlayOneShotOnCamera(map);

        int damageCount;

        IReadOnlyList<Pawn> potentialPawns = map.mapPawns.AllPawnsSpawned;
        for (int i = 0; i < potentialPawns.Count; i++)
        {
            damageCount = Rand.RangeInclusive(-1, 2);
            if (damageCount > 0)
            {
                ApplyGravityDistortionToPawn(potentialPawns[i], map, damageCount == 2);
            }
        }
        potentialPawns = null;

        List<Building> potentialBuildings = map.listerBuildings.allBuildingsColonist;
        for (int i = 0; i < potentialBuildings.Count; i++)
        {
            damageCount = Rand.RangeInclusive(-1, 2);
            if (damageCount > 0)
            {
                ApplyGravityDistortionToBuilding(potentialBuildings[i], map, damageCount == 2);
            }
        }

        potentialBuildings = map.listerBuildings.allBuildingsNonColonist;
        for (int i = 0; i < potentialBuildings.Count; i++)
        {
            damageCount = Rand.RangeInclusive(-1, 2);
            if (damageCount > 0)
            {
                ApplyGravityDistortionToBuilding(potentialBuildings[i], map, damageCount == 2);
            }
        }
    }

    private static void ApplyGravityDistortionToPawn(Pawn pawn, Map map, bool doubleDamage)
    {
        float damageAmount = 16;

        if (pawn.BodySize >= 2f)
        {
            damageAmount *= 2f;
        }

        if (pawn.Position.GetRoof(map) is not null)
        {
            damageAmount *= 0.25f;
        }

        pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, damageAmount, armorPenetration: 20f));
        if (doubleDamage)
        {
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, damageAmount, armorPenetration: 20f));
        }
    }

    private static void ApplyGravityDistortionToBuilding(Building building, Map map, bool doubleDamage)
    {
        if (building.Position.GetRoof(map) is not null)
        {
            return;
        }

        building.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 16f, armorPenetration: 20f));
        if (doubleDamage)
        {
            building.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 16f, armorPenetration: 20f));
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksToNextBomb, "ticksToNextBomb", 0);
    }
}

