using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Social : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!p.Spawned)
		{
			return ThoughtState.Inactive;
		}
		int count = PawnWithSameGene(p).Count;
		if (count <= 2)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		if (count <= 5)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		if (count <= 10)
		{
			return ThoughtState.Inactive;
		}
		if (count <= 25)
		{
			return ThoughtState.ActiveAtStage(2);
		}
		return ThoughtState.ActiveAtStage(3);
	}

	public List<Pawn> PawnWithSameGene(Pawn pawn)
	{
		List<Pawn> list = new List<Pawn>();
		List<Pawn> list2 = pawn.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(pawn.Faction);
		for (int i = 0; i < list2.Count; i++)
		{
			if (list2[i].Faction == pawn.Faction && list2[i].genes.HasGene(OADefOf.RK_OA_gene_Social))
			{
				list.Add(list2[i]);
			}
		}
		return list;
	}
}
