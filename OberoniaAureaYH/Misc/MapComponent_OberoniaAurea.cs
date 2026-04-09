using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class MapComponent_OberoniaAurea : MapComponent
{
    public readonly List<CompCircuitRegulator> circuitRegulators = [];

    private readonly int birthdayTickHash;
    private int nextOAGeneCheckTick = -1;

    private int cachedOAGenePawnsCount;

    public MapComponent_OberoniaAurea(Map map) : base(map)
    {
        birthdayTickHash = Rand.Range(0, int.MaxValue).HashOffset();
    }

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

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (ModUtility.IsHashIntervalTick(birthdayTickHash, 60000))
        {
            InfoBirthdayOfColonists();
        }
    }

    private void InfoBirthdayOfColonists()
    {
        if (!ModsConfig.IdeologyActive || !map.IsPlayerHome)
            return;

        Ideo primaryIdeo = Faction.OfPlayer.ideos?.PrimaryIdeo;
        if (primaryIdeo is null)
            return;

        if (!primaryIdeo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Appreciate) && !primaryIdeo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Solemn))
            return;

        List<Pawn> birthdayColonists = map.mapPawns.FreeColonists.Where(p => p.IsColonistOnFirstBirthdayPerYear()).ToList();
        if (birthdayColonists.NullOrEmpty())
            return;

        Messages.Message(
            "OARK_Birthday_Message".Translate(GenLabel.ThingsLabel(birthdayColonists).Named("PawnsInfo")),
            birthdayColonists,
            MessageTypeDefOf.PositiveEvent);
    }

    private static int PawnsCountWithOAGene(Map map)
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

