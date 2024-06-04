using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class PlaceWorker_ShowFacilityRadius : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        CompProperties_Facility compProperties = def.GetCompProperties<CompProperties_Facility>();
        if (compProperties != null)
        {
            GenDraw.DrawRadiusRing(center, compProperties.maxDistance, Color.white);
        }
    }
}
