using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompPowerEnergyComplex : CompPowerPlant
{
    MapComponent_OberoniaAurea mc_OA;
    protected int ticksRemaining = 250;
    public override void SetUpPowerVars()
    {
        base.SetUpPowerVars();
        if (mc_OA != null && mc_OA.AverageCircuitStability(PowerNet) > 0.1f)
        {
            Log.Message("ooo");
            PowerOutput *= 1.2f;
        }
    }
    public override void CompTick()
    {
        base.CompTick();
        ticksRemaining--;
        if (ticksRemaining <= 0)
        {
            SetUpPowerVars();
            ticksRemaining = 2500;
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        mc_OA = parent.Map.GetComponent<MapComponent_OberoniaAurea>();
    }
    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        mc_OA = null;
    }
}
