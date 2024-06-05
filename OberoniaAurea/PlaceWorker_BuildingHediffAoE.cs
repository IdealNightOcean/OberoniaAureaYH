using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class PlaceWorker_BuildingHediffAoE : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		TCP_HediffAoE compProperties = def.GetCompProperties<TCP_HediffAoE>();
		if (compProperties != null)
		{
			GenDraw.DrawRadiusRing(center, compProperties.range, Color.white);
		}
	}
}
