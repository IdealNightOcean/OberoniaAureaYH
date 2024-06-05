using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class PlaceWorker_BuildingHediffAoE : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		TCP_HediffAoE compProperties = def.GetCompProperties<TCP_HediffAoE>();
		if (compProperties != null)
		{
			GenDraw.DrawRadiusRing(center, compProperties.range, Color.white);
		}
	}
}
