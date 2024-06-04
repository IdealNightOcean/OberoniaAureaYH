using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class GenStep_ResearcherCamp : GenStep_OABasicGenStep
{
    private static readonly IntRange ResearcherCampSizeWidth = new(12, 14);

    private static readonly IntRange ResearcherCampSizeHeight = new(12, 14);

    public override void Generate(Map map, GenStepParams parms)
    {
        base.Generate(map, parms);
        int width = ResearcherCampSizeWidth.RandomInRange;
        int height = ResearcherCampSizeHeight.RandomInRange;
        CellRect rect = new(adventureRegion.minX + adventureRegion.Width / 2 - width / 2, adventureRegion.minZ + adventureRegion.Height / 2 - height / 2, width, height);
        rect.ClipInsideMap(map);
        foreach (IntVec3 item in rect)
        {
            CompTerrainPumpDry.AffectCell(map, item);
            for (int i = 0; i < 8; i++)
            {
                Vector3 vector = Rand.InsideUnitCircleVec3 * 3f;
                IntVec3 c = IntVec3.FromVector3(item.ToVector3Shifted() + vector);
                if (c.InBounds(map))
                {
                    CompTerrainPumpDry.AffectCell(map, c);
                }
            }
        }
        ResolveParams resolveParams = default;
        resolveParams.rect = rect;
        BaseGen.globalSettings.map = map;
        BaseGen.symbolStack.Push("oa_researcherCamp", resolveParams);
        BaseGen.Generate();
    }
}