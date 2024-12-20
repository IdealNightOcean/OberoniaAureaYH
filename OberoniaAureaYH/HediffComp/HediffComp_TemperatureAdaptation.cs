using Verse;

namespace OberoniaAurea;

public class HediffCompProperties_TemperatureAdaptation : HediffCompProperties
{
    public int maxTemperature = 9999;
    public int minTemperature = -9999;

    public HediffCompProperties_TemperatureAdaptation()
    {
        compClass = typeof(HediffComp_TemperatureAdaptation);
    }
}

public class HediffComp_TemperatureAdaptation : HediffComp
{
    private HediffCompProperties_TemperatureAdaptation Props => (HediffCompProperties_TemperatureAdaptation)props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        if (Pawn.IsHashIntervalTick(2500))
        {
            CheckTemperature();
        }
    }
    private void CheckTemperature()
    {
        float ambientTemperature = Pawn.AmbientTemperature;
        if (ambientTemperature < Props.minTemperature || ambientTemperature > Props.maxTemperature)
        {
            Pawn.health.RemoveHediff(parent);
        }
    }
}
