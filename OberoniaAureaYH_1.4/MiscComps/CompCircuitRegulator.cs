﻿using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class CompProperties_CircuitRegulator : CompProperties_Power
{
    public int checkInterval = 2500;

    public CompProperties_CircuitRegulator()
    {
        compClass = typeof(CompCircuitRegulator);
    }
}
public class CompCircuitRegulator : CompPowerTransmitter
{
    protected static readonly float BaseCircuitStabilityLoss = 0.0005f;
    private new CompProperties_CircuitRegulator Props => (CompProperties_CircuitRegulator)props;
    protected MapComponent_OberoniaAurea mc_oa;
    public MapComponent_OberoniaAurea MC_OA => mc_oa ??= parent.Map.GetComponent<MapComponent_OberoniaAurea>();

    protected int ticksRemaining;
    public bool repairmentEnabled = true; //是否允许自动维修
    private float targetCircuitStability = 0.4f;
    public float TargetCircuitStability //目标稳定度
    {
        get { return targetCircuitStability; }
        set { targetCircuitStability = Mathf.Clamp(value, 0f, 1f); }
    }
    private float curCircuitStability = 1.0f;
    public float CurCircuitStability //当前稳定度
    {
        get { return curCircuitStability; }
        set { curCircuitStability = Mathf.Clamp(value, 0f, 1f); }
    }
    public bool NeedRepairment => targetCircuitStability > curCircuitStability; //是否需要维修
    private float curCircuitStabilityLoss;
    public float CurCircuitStabilityLoss //当前损失率
    {
        get
        {
            float curLoadElectricity = CurLoadElectricity(PowerNet);
            float loss = BaseCircuitStabilityLoss * Mathf.FloorToInt(curLoadElectricity / 2000f);
            float factory = 1f;
            if (curLoadElectricity >= 30000f)
            {
                factory = 20f;
            }
            else if (curLoadElectricity >= 20000f)
            {
                factory = 5f;
            }
            else if (curLoadElectricity >= 10000f)
            {
                factory = 2f;
            }
            curCircuitStabilityLoss = loss * factory;
            return curCircuitStabilityLoss;
        }
    }
    private float curLoadElectricity;
    protected float CurLoadElectricity(PowerNet powerNet) //当前负载
    {
        if (powerNet == null)
        {
            curLoadElectricity = 0f;
        }
        else if (curCircuitStability > 0)
        {
            curLoadElectricity = CurrentPowerExport(powerNet) / MC_OA.ValidCircuitRegulatorCount(powerNet);
        }
        else
        {
            curLoadElectricity = 0f;
        }
        return curLoadElectricity;
    }
    public override void CompTickRare()
    {
        ticksRemaining -= 250;
        if (ticksRemaining <= 0)
        {
            CurCircuitStability -= CurCircuitStabilityLoss;
            ticksRemaining = Props.checkInterval;
        }
    }
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        MC_OA.RegisterCircuitRegulator(this);
        ticksRemaining = 0;
    }
    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        MC_OA.DeregisterCircuitRegulator(this);
        curLoadElectricity = 0f;
        curCircuitStabilityLoss = 0f;
    }
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }
        if (DebugSettings.ShowDevGizmos)
        {
            Command_Action command_AddStability = new()
            {
                defaultLabel = "DEV: Add Stability",
                action = delegate
                {
                    CurCircuitStability += 0.1f;
                }
            };
            Command_Action command_ReduceStability = new()
            {
                defaultLabel = "DEV: Reduce Stability",
                action = delegate
                {
                    CurCircuitStability -= 0.1f;
                }
            };
            yield return command_AddStability;
            yield return command_ReduceStability;
        }
    }
    public override string CompInspectStringExtra()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(base.CompInspectStringExtra());
        stringBuilder.AppendInNewLine("OA_curCircuitStability".Translate(curCircuitStability.ToStringPercent()));
        stringBuilder.AppendInNewLine("OA_curLoadElectricity".Translate(curLoadElectricity));
        stringBuilder.AppendInNewLine("OA_curCircuitStabilityLoss".Translate(curCircuitStabilityLoss.ToStringPercent()));
        return stringBuilder.ToString();
    }
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
        Scribe_Values.Look(ref repairmentEnabled, "repairmentEnabled", defaultValue: true);
        Scribe_Values.Look(ref targetCircuitStability, "targetCircuitStability", 0.4f);
        Scribe_Values.Look(ref curCircuitStability, "curCircuitStability", 1f);
    }

    public static float CurrentPowerExport(PowerNet powerNet)
    {
        if (powerNet == null)
        {
            return 0f;
        }
        List<CompPowerTrader> powerComps = powerNet.powerComps;
        float powerExport = 0f;
        foreach (CompPowerTrader comp in powerComps)
        {
            if (comp.PowerOn && comp.powerOutputInt > 0)
            {
                powerExport += comp.PowerOutput;
            }
        }
        return powerExport;
    }
}
