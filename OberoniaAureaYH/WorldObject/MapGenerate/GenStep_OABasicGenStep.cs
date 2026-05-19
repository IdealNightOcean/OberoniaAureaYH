using RimWorld.BaseGen;
using Verse;

namespace OberoniaAurea;

public class GenStep_OABasicGenStep : GenStep
{
    protected CellRect adventureRegion;

    protected ResolveParams baseResolveParams;

    public override int SeedPart => 87846193;

    public override void Generate(Map map, GenStepParams parms)
    {
        int minX = map.Size.x / 10;
        int width = 8 * map.Size.x / 10;
        int minZ = map.Size.z / 10;
        int height = 8 * map.Size.z / 10;
        adventureRegion = new CellRect(minX, minZ, width, height);
        adventureRegion.ClipInsideMap(map);
        BaseGen.globalSettings.map = map;
        CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map), map, 0f, out IntVec3 cell);
        MapGenerator.PlayerStartSpot = cell;
        baseResolveParams = default;
        baseResolveParams.rect = adventureRegion;
    }
}