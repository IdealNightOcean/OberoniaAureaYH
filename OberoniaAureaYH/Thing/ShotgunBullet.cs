using OberoniaAurea_Frame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

//霰弹枪Bullet
public class ShotgunBullet : BulletBase
{
    private ShotgunExtension modEx_SG;
    public ShotgunExtension ModEx_SG => modEx_SG ??= equipmentDef.GetModExtension<ShotgunExtension>();

    private float firingDistance;
    private int splashCount;
    public override bool AnimalsFleeImpact => true;

    public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
    {
        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        firingDistance = Vector3.Distance(origin, intendedTarget.CenterVector3);
        splashCount = 0;
    }

    protected override void ImpactCell(IntVec3 hitCell, BattleLogEntry_RangedImpact battleLogEntry_RangedImpact)
    {
        TryTakeSplashDamage(hitCell, Map, out splashCount);
    }

    protected override void ImpactThing(Thing hitThing, Quaternion exactRotation, bool instigatorGuilty, BattleLogEntry_RangedImpact battleLogEntry_RangedImpact)
    {
        DamageDef damageDef = DamageDef;
        float amount = DamageAmount;
        float armorPenetration = ArmorPenetration;
        DamageInfo dinfo = new(damageDef, amount, armorPenetration, exactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
        dinfo.SetWeaponQuality(equipmentQuality);
        HitThingTakeDamage(hitThing, dinfo, battleLogEntry_RangedImpact, splashCount);
    }

    protected void HitThingTakeDamage(Thing hitThing, DamageInfo dinfo, BattleLogEntry_RangedImpact battleLogEntry_RangedImpact, int splashCount = 0)
    {
        ShotgunExtension modEx_SG = ModEx_SG;
        if (modEx_SG is null)
        {
            return;
        }
        int damageCount = GetDamageCount(modEx_SG, firingDistance);
        damageCount = Mathf.Max(damageCount - splashCount, 1);
        for (int i = 0; i < damageCount; i++)
        {
            hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
        }
    }

    protected void TryTakeSplashDamage(IntVec3 hitPos, Map hitMap, out int splashCount)
    {
        splashCount = 0;
        ShotgunExtension modEx_SG = ModEx_SG;
        if (modEx_SG is not null && modEx_SG.splash)
        {
            IntRange splashDamageCount = GetSplashRadiusAndDamageCount(modEx_SG, firingDistance, out float splashRadius);
            if (splashRadius > 0)
            {
                List<Pawn> splashPawns = [];
                GetPawnsInRadius(hitPos, hitMap, splashRadius, splashPawns);
                if (splashPawns.Any())
                {
                    int preSplashCount;
                    bool flag = launcher is not Pawn pawn || !pawn.Drafted;
                    DamageInfo dinfo = new(def.projectile.damageDef, (float)DamageAmount, ArmorPenetration, ExactRotation.eulerAngles.y, launcher, (BodyPartRecord)null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, flag, true);
                    foreach (Pawn p in splashPawns)
                    {
                        preSplashCount = splashDamageCount.RandomInRange;
                        BattleLogEntry_RangedImpact battleLogEntry_SplashImpact = new(launcher, p, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
                        Find.BattleLog.Add(battleLogEntry_SplashImpact);
                        for (int i = 0; i < preSplashCount; i++)
                        {
                            p.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_SplashImpact);
                        }
                        p.stances?.stagger.Notify_BulletImpact(this);
                        splashCount += preSplashCount;
                    }
                }
            }
        }
    }
    protected static int GetDamageCount(ShotgunExtension modEx_SG, float firingDistance)
    {
        foreach (ShotgunDirectDamage sdd in modEx_SG.shotgunDirectDamage)
        {
            if (sdd.distanceRange.Includes(firingDistance))
                return sdd.damageCount.RandomInRange;
        }
        return 1;
    }
    protected static IntRange GetSplashRadiusAndDamageCount(ShotgunExtension modEx_SG, float firingDistance, out float splashRadius)
    {
        foreach (ShotgunSplashDamage sdd in modEx_SG.shotgunSplashDamage)
        {
            if (sdd.distanceRange.Includes(firingDistance))
            {
                splashRadius = sdd.splashRadius;
                return sdd.damageCount;
            }
        }
        splashRadius = 0;
        return IntRange.Zero;

    }
    protected static void GetPawnsInRadius(IntVec3 ctrPosition, Map map, float radius, List<Pawn> targetPawns) //获取以ctrPosition为圆心半径radius内的pawn
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
}
