using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OberoniaAurea;

public class CompProperties_EquipmentShield : CompProperties_Shield
{
    public bool breakByEMP = true;

    public CompProperties_EquipmentShield()
    {
        compClass = typeof(CompEquipmentShield);
    }
}

[StaticConstructorOnStartup]
public class CompEquipmentShield : ThingComp
{
    protected const float MaxDamagedJitterDist = 0.05f;
    protected const int JitterDurationTicks = 8;
    protected static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

    protected float energy;
    protected int ticksToReset = -1;
    protected int lastKeepDisplayTick = -9999;
    protected Vector3 impactAngleVect;
    protected int lastAbsorbDamageTick = -9999;
    protected int KeepDisplayingTicks = 1000;
    protected float ApparelScorePerEnergyMax = 0.25f;

    protected Pawn pawnOwner;

    public CompProperties_EquipmentShield Props => (CompProperties_EquipmentShield)props;

    protected float EnergyMax => parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
    protected float EnergyGainPerTick => parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;
    public float Energy => energy;
    public ShieldState ShieldState => ticksToReset <= 0 ? ShieldState.Active : ShieldState.Resetting;

    protected bool ShouldDisplay
    {
        get
        {
            if (pawnOwner is null)
            {
                return false;
            }
            if (!pawnOwner.Spawned || pawnOwner.Dead || pawnOwner.Downed)
            {
                return false;
            }
            if (pawnOwner.InAggroMentalState || pawnOwner.Drafted)
            {
                return true;
            }
            if (pawnOwner.Faction.HostileTo(Faction.OfPlayer) && !pawnOwner.IsPrisoner)
            {
                return true;
            }
            if (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks)
            {
                return true;
            }
            return false;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref energy, "energy", 0f);
        Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0);
        Scribe_References.Look(ref pawnOwner, "pawnOwner");
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        pawnOwner = pawn;
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        pawnOwner = null;
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
        {
            yield return gizmo;
        }
        if (pawnOwner.Faction.IsPlayerSafe())
        {
            Gizmo_EquipmentEnergyShieldStatus gizmo_EnergyShieldStatus = new()
            {
                shieldComp = this
            };
            yield return gizmo_EnergyShieldStatus;
        }

        if (DebugSettings.ShowDevGizmos)
        {
            Command_Action command_Break = new()
            {
                defaultLabel = "DEV: Break",
                action = Break
            };
            yield return command_Break;
            if (ticksToReset > 0)
            {
                Command_Action command_ClearReset = new()
                {
                    defaultLabel = "DEV: Clear reset",
                    action = delegate { ticksToReset = 0; }
                };
                yield return command_ClearReset;
            }
        }
    }

    public override float CompGetSpecialApparelScoreOffset()
    {
        return EnergyMax * ApparelScorePerEnergyMax;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (pawnOwner is null)
        {
            energy = 0f;
        }
        else if (ShieldState == ShieldState.Resetting)
        {
            if (--ticksToReset <= 0)
            {
                Reset();
            }
        }
        else if (ShieldState == ShieldState.Active)
        {
            energy += EnergyGainPerTick;
            if (energy > EnergyMax)
            {
                energy = EnergyMax;
            }
        }
    }

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        absorbed = false;
        if (ShieldState != ShieldState.Active || pawnOwner is null)
        {
            return;
        }

        if (Props.breakByEMP && dinfo.Def == DamageDefOf.EMP)
        {
            energy = 0f;
            Break();
        }
        else if (!dinfo.Def.ignoreShields && (dinfo.Def.isRanged || dinfo.Def.isExplosive))
        {
            energy -= dinfo.Amount * Props.energyLossPerDamage;
            if (energy < 0f)
            {
                Break();
            }
            else
            {
                AbsorbedDamage(dinfo);
            }
            absorbed = true;
        }
    }

    public void KeepDisplaying()
    {
        lastKeepDisplayTick = Find.TickManager.TicksGame;
    }

    protected virtual void AbsorbedDamage(DamageInfo dinfo)
    {
        SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawnOwner.Position, pawnOwner.Map));
        impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
        Vector3 loc = pawnOwner.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
        float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
        FleckMaker.Static(loc, pawnOwner.Map, FleckDefOf.ExplosionFlash, num);
        int num2 = (int)num;
        for (int i = 0; i < num2; i++)
        {
            FleckMaker.ThrowDustPuff(loc, pawnOwner.Map, Rand.Range(0.8f, 1.2f));
        }
        lastAbsorbDamageTick = Find.TickManager.TicksGame;
        KeepDisplaying();
    }

    protected virtual void Break()
    {
        if (pawnOwner.Spawned)
        {
            float scale = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, energy);
            EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
            FleckMaker.Static(pawnOwner.TrueCenter(), pawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(pawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), pawnOwner.Map, Rand.Range(0.8f, 1.2f));
            }
        }
        energy = 0f;
        ticksToReset = Props.startingTicksToReset;
    }

    protected virtual void Reset()
    {
        if (pawnOwner.Spawned)
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(pawnOwner.Position, pawnOwner.Map));
            FleckMaker.ThrowLightningGlow(pawnOwner.TrueCenter(), pawnOwner.Map, 3f);
        }
        ticksToReset = -1;
        energy = Props.energyOnReset;
    }

    public override void CompDrawWornExtras()
    {
        base.CompDrawWornExtras();
        Draw();
    }

    protected void Draw()
    {
        if (ShieldState == ShieldState.Active && ShouldDisplay)
        {
            float drawSize = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, energy);
            Vector3 drawPos = pawnOwner.DrawPos;
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            int jitterTick = Find.TickManager.TicksGame - lastAbsorbDamageTick;
            if (jitterTick < 8)
            {
                float sizeOffset = (8 - jitterTick) / 8f * 0.05f;
                drawPos += impactAngleVect * sizeOffset;
                drawSize -= sizeOffset;
            }
            float angle = Rand.Range(0, 360);
            Vector3 s = new(drawSize, 1f, drawSize);
            Matrix4x4 matrix = default;
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
        }
    }

    public override bool CompAllowVerbCast(Verb verb)
    {
        return !Props.blocksRangedWeapons || verb is not Verb_LaunchProjectile;
    }
}
