using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OberoniaAurea;

public class AstableBullet : Bullet
{
    protected void ProjectileImpact(bool blockedByShield = false)
    {
        GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
        if (!blockedByShield && def.projectile.landedEffecter != null)
        {
            def.projectile.landedEffecter.Spawn(base.Position, base.Map).Cleanup();
        }
        Destroy();
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
            hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
            Pawn hitPawn = hitThing as Pawn;
            StaggerHandler staggerHandler = hitPawn?.stances?.stagger;
            if (staggerHandler != null)
            {
                if (hitPawn.BodySize <= def.projectile.StoppingPower + 0.001f)
                {
                    staggerHandler.StaggerFor(95);
                }
                staggerHandler.Notify_BulletImpact(this);
            }
            if (def.projectile.extraDamages != null)
            {
                foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
                {
                    if (Rand.Chance(extraDamage.chance))
                    {
                        DamageInfo extraDamageinfo = new(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                        hitThing.TakeDamage(extraDamageinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                    }
                }
            }
            if (Rand.Chance(def.projectile.bulletChanceToStartFire) && (hitPawn == null || Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(hitPawn))))
            {
                hitThing.TryAttachFire(def.projectile.bulletFireSizeRange.RandomInRange, launcher);
            }
            return;
        }
        if (!blockedByShield)
        {
            SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(position, map));
            if (position.GetTerrain(map).takeSplashes)
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
            FireUtility.TryStartFireIn(position, map, def.projectile.bulletFireSizeRange.RandomInRange, launcher);
        }
    }

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
