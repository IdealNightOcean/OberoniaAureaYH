﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OberoniaAurea;

//霰弹枪Bullet
public class ShotgunBullet : Bullet
{
    private ShotgunExtension modEx_SG;
    public ShotgunExtension ModEx_SG => modEx_SG ??= equipmentDef.GetModExtension<ShotgunExtension>();

    public float firingDistance;
    public override bool AnimalsFleeImpact => true;

    public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
    {
        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        firingDistance = Vector3.Distance(origin, intendedTarget.CenterVector3);
    }
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        Map map = base.Map;
        IntVec3 position = base.Position;
        ProjectileImpact(blockedByShield);
        BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
        Find.BattleLog.Add(battleLogEntry_RangedImpact);
        NotifyImpact(hitThing, map, position);
        if (hitThing != null)
        {
            bool instigatorGuilty = launcher is not Pawn pawn || !pawn.Drafted;
            DamageInfo dinfo = new(def.projectile.damageDef, DamageAmount, ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            dinfo.SetWeaponQuality(equipmentQuality);
            Pawn pawn2 = hitThing as Pawn;
            HitThing(hitThing, dinfo, battleLogEntry_RangedImpact, pawn2);
            if (def.projectile.extraDamages != null)
            {
                foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
                {
                    if (Rand.Chance(extraDamage.chance))
                    {
                        DamageInfo dinfo2 = new(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                        hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                    }
                }
            }
            if (Rand.Chance(def.projectile.bulletChanceToStartFire) && (pawn2 == null || Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(pawn2))))
            {
                hitThing.TryAttachFire(def.projectile.bulletFireSizeRange.RandomInRange, launcher);
            }
            return;
        }
        if (!blockedByShield)
        {
            SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map));
            if (base.Position.GetTerrain(map).takeSplashes)
            {
                FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(DamageAmount) * 1f, 4f);
            }
            else
            {
                FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
            }
        }
        if (Rand.Chance(def.projectile.bulletChanceToStartFire))
        {
            FireUtility.TryStartFireIn(base.Position, map, def.projectile.bulletFireSizeRange.RandomInRange, launcher);
        }
    }

    protected void HitThing(Thing hitThing, DamageInfo dinfo, BattleLogEntry_RangedImpact battleLogEntry_RangedImpact, Pawn hitPawn)
    {
        string logMessage = $"开火距离：{firingDistance} | ";
        ShotgunExtension modEx_SG = ModEx_SG;
        int damageCount = GetDamageCount(modEx_SG, firingDistance);
        logMessage += $"伤害总数：{damageCount} | ";
        if (modEx_SG.splash)
        {
            IntRange splashDamageCount = GetSplashRadiusAndDamageCount(modEx_SG, firingDistance, out float splashRadius);
            if (splashRadius > 0)
            {
                List<Pawn> splashPawns = [];
                OberoniaAureaYHUtility.GetPawnsInRadius(hitThing.Position, hitThing.Map, splashRadius, splashPawns);
                logMessage += $"溅射人数：{splashPawns.Count} | 溅射数量：";
                int splashCount;
                foreach (Pawn p in splashPawns)
                {
                    splashCount = splashDamageCount.RandomInRange;
                    logMessage += $"{splashCount} ";
                    BattleLogEntry_RangedImpact battleLogEntry_SplashImpact = new(launcher, p, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
                    Find.BattleLog.Add(battleLogEntry_SplashImpact);
                    for (int i = 0; i < splashCount; i++)
                    {
                        p.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_SplashImpact);
                    }
                    p.stances?.stagger.Notify_BulletImpact(this);
                    damageCount -= (splashCount == 0 ? 0 : 1);
                }
            }
        }
        damageCount = Mathf.Max(damageCount, 1);
        for (int i = 0; i < damageCount; i++)
        {
            hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
        }
        hitPawn?.stances?.stagger.Notify_BulletImpact(this);
        logMessage += $" | 直接伤害数量：{damageCount}";
        Log.Message(logMessage);
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
        return IntRange.zero;

    }

    //祖父类Projectile的Impact方法
    protected void ProjectileImpact(bool blockedByShield = false)
    {
        GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
        if (!blockedByShield && def.projectile.landedEffecter != null)
        {
            def.projectile.landedEffecter.Spawn(base.Position, base.Map).Cleanup();
        }
        Destroy();
    }

    //父类Bullet的NotifyImpact方法
    private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
    {
        BulletImpactData bulletImpactData = default;
        bulletImpactData.bullet = this;
        bulletImpactData.hitThing = hitThing;
        bulletImpactData.impactPosition = position;
        BulletImpactData impactData = bulletImpactData;
        hitThing?.Notify_BulletImpactNearby(impactData);
        int num = 9;
        for (int i = 0; i < num; i++)
        {
            IntVec3 c = position + GenRadial.RadialPattern[i];
            if (!c.InBounds(map))
            {
                continue;
            }
            List<Thing> thingList = c.GetThingList(map);
            for (int j = 0; j < thingList.Count; j++)
            {
                if (thingList[j] != hitThing)
                {
                    thingList[j].Notify_BulletImpactNearby(impactData);
                }
            }
        }
    }

}
