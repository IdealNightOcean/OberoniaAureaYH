using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class MapComponent_OberoniaAurea(Map map) : MapComponent(map)
{
    protected List<CompCircuitRegulator> circuitRegulators = [];

    public void RegisterCircuitRegulator(CompCircuitRegulator comp)
    {
        if (!circuitRegulators.Contains(comp))
        {
            circuitRegulators.Add(comp);
        }
    }
    public void DeregisterCircuitRegulator(CompCircuitRegulator comp)
    {
        circuitRegulators.Remove(comp);
    }

    public int ValidCircuitRegulatorCount(PowerNet powerNet)
    {
        return circuitRegulators.Where(c => c.PowerNet == powerNet && c.CurCircuitStability > 0).Count();
    }
    public float AverageCircuitStability(PowerNet powerNet)
    {
        if (powerNet == null)
        {
            return 0f;
        }
        List<float> circuitStabilities = circuitRegulators.Where(c => c.PowerNet == powerNet).Select(s => s.CurCircuitStability).ToList();
        if (circuitStabilities.NullOrEmpty())
        {
            return 0f;
        }
        return circuitStabilities.Average();
    }
}

