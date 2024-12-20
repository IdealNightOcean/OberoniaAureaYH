using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class PlaceWorker_GiveHediffInRange_Building : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        CompProperties_GiveHediffInRange_Building compProperties = def.GetCompProperties<CompProperties_GiveHediffInRange_Building>();
        if (compProperties != null)
        {
            GenDraw.DrawRadiusRing(center, compProperties.range, Color.white);
        }
    }
}
