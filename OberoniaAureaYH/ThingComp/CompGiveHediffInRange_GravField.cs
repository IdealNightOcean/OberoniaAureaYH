using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OberoniaAurea;

public class CompGiveHediffInRange_GravField : CompGiveHediffInRange_Building
{
    private int suppressTickLeft;
    [Unsaved] private int tickToNextFleck = 120;

    public override void CompTickInterval(int delta)
    {
        if (!parent.Spawned) { return; }

        if (suppressTickLeft > 0)
        {
            suppressTickLeft -= delta;
            if ((tickToNextFleck -= delta) < 0)
            {
                tickToNextFleck = 180;
                FleckMaker.Static(parent.Position, parent.Map, OARK_ModDefOf.OARK_Fleck_GravBomb, 0.5f);
                OARK_ModDefOf.OARK_Sound_GravBomb.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent)));
            }
        }

        if ((ticksToNextCheck -= delta) <= 0)
        {
            ticksToNextCheck = Props.checkInterval;

            if (powerTrader is not null && !powerTrader.PowerOn)
            {
                return;
            }

            HediffDef giveHediff = Props.hediff;
            int overrideDisappearTicks = Props.checkInterval + 120;
            Faction faction = parent.Faction;

            foreach (Pawn pawn in parent.Map.mapPawns.AllPawnsSpawned.Where(PawnValidator))
            {
                GiveHediff(pawn, giveHediff, overrideDisappearTicks);
                if (suppressTickLeft > 0)
                {
                    GiveHediff(pawn, OARK_HediffDefOf.OARK_Hediff_GravFieldSuppress, overrideDisappearTicks);
                }
            }
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        suppressTickLeft = -1;
        tickToNextFleck = 180;
        ticksToNextCheck = Props.checkInterval;
    }

    private void GiveHediff(Pawn pawn, HediffDef giveHediff, int overrideDisappearTicks)
    {
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(giveHediff);
        if (hediff is null)
        {
            hediff = pawn.health.AddHediff(giveHediff);
            hediff.Severity = 1f;
            HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link is not null)
            {
                hediffComp_Link.drawConnection = false;
                hediffComp_Link.other = parent;
            }
        }
        HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
        if (hediffComp_Disappears is null)
        {
            Log.Error($"{nameof(parent)} has a hediff in props which does not have a HediffComp_Disappears");
        }
        else
        {
            hediffComp_Disappears.ticksToDisappear = overrideDisappearTicks;
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        if (suppressTickLeft < 0)
        {
            Command_Action command_Suppress = new()
            {
                defaultLabel = "OARK_Command_GravSuppress".Translate(),
                defaultDesc = "OARK_Command_GravSuppressDesc".Translate(),
                icon = IconUtility.FlashIcon,
                action = StartSuppress
            };

            if (powerTrader.PowerNet.CurrentStoredEnergy() < 10000f)
            {
                command_Suppress.Disable("OARK_LowStoredEnergy".Translate(10000));
            }
            yield return command_Suppress;
        }
    }

    private void StartSuppress()
    {
        TryConsumeEnergy(powerTrader.PowerNet, 10000f);
        suppressTickLeft = 7500;
        tickToNextFleck = 180;
        FleckMaker.Static(parent.Position, parent.Map, OARK_ModDefOf.OARK_Fleck_GravBomb, 0.5f);
        OARK_ModDefOf.OARK_Sound_GravBomb.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent)));
    }

    protected static void TryConsumeEnergy(PowerNet powerNet, float consume)
    {
        List<CompPowerBattery> batteryComps = powerNet.batteryComps;
        if (batteryComps.NullOrEmpty())
        {
            return;
        }

        float remainConsume = consume;
        float curConsume;
        float batteryPower;
        for (int i = 0; i < batteryComps.Count; i++)
        {
            CompPowerBattery battery = batteryComps[i];
            batteryPower = battery.StunnedByEMP ? 0f : battery.StoredEnergy;
            curConsume = Mathf.Min(remainConsume, batteryPower);

            float pct = (batteryPower - curConsume) / battery.Props.storedEnergyMax;
            battery.SetStoredEnergyPct(pct);
            remainConsume -= curConsume;

            if (remainConsume <= 0f)
            {
                break;
            }
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref suppressTickLeft, "suppressTickLeft", 0);
    }
}
