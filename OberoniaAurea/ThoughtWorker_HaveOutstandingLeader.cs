using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_HaveOutstandingLeader : ThoughtWorker
{
	public static List<SkillDef> LeaderSkill = new List<SkillDef>
	{
		SkillDefOf.Shooting,
		SkillDefOf.Melee,
		SkillDefOf.Construction,
		SkillDefOf.Mining,
		SkillDefOf.Cooking,
		SkillDefOf.Plants,
		SkillDefOf.Animals,
		SkillDefOf.Crafting,
		SkillDefOf.Artistic,
		SkillDefOf.Medicine,
		SkillDefOf.Social,
		SkillDefOf.Intellectual
	};

	protected override ThoughtState CurrentStateInternal(Pawn pawn)
	{
		Ideo ideo = pawn.Ideo;
		Faction faction = pawn.Faction;
		if (pawn.Spawned && ideo != null && faction != null && faction.leader != null)
		{
			int num = 0;
			for (int i = 0; i < LeaderSkill.Count(); i++)
			{
				if (faction.leader.skills.GetSkill(LeaderSkill[i]).Level > 15)
				{
					num++;
				}
			}
			if (num >= 2 && ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Respect))
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (num >= 2 && ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Worship))
			{
				return ThoughtState.ActiveAtStage(1);
			}
		}
		return ThoughtState.Inactive;
	}
}
