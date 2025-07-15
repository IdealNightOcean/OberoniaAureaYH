using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class MapComponent_OberoniaAurea(Map map) : MapComponent(map)
{
    public readonly List<CompCircuitRegulator> circuitRegulators = [];

    protected int nextOAGeneCheckTick = -1;
    protected int cachedOAGenePawnsCount;

    public int OAGenePawnsCount
    {
        get
        {
            if (Find.TickManager.TicksGame > nextOAGeneCheckTick)
            {
                cachedOAGenePawnsCount = PawnsCountWithOAGene(map);
                nextOAGeneCheckTick = Find.TickManager.TicksGame + Rand.RangeInclusive(27500, 32500);
            }
            return cachedOAGenePawnsCount;
        }
    }

    protected static int PawnsCountWithOAGene(Map map)
    {
        int validPawns = 0;
        List<Pawn> allHumans = map.mapPawns.AllHumanlikeSpawned;
        for (int i = 0; i < allHumans.Count; i++)
        {
            Pawn pawn = allHumans[i];
            if (pawn.genes.HasActiveGene(OARK_ModDefOf.RK_OA_gene_Social))
            {
                validPawns++;
            }
        }
        return validPawns;
    }

    //电路稳定检测
    public int ValidCircuitRegulatorCount(PowerNet powerNet)
    {
        return circuitRegulators.Where(c => c.PowerNet == powerNet && c.CurCircuitStability > 0).Count();
    }
    public float AverageCircuitStability(PowerNet powerNet)
    {
        if (powerNet is null)
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

