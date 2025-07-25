using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class ThoughtWorker_HaveOutstandingLeader : ThoughtWorker
{
    public static List<SkillDef> LeaderSkill =
    [
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
    ];

    protected override ThoughtState CurrentStateInternal(Pawn pawn)
    {
        if (!pawn.Spawned)
        {
            return ThoughtState.Inactive;
        }
        Ideo ideo = pawn.Ideo;
        if (ideo is not null && HasOutstandingLeader(pawn.Faction))
        {
            if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_LeaderAttitude_Respect))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_LeaderAttitude_Worship))
            {
                return ThoughtState.ActiveAtStage(1);
            }
        }
        return ThoughtState.Inactive;
    }
    protected static bool HasOutstandingLeader(Faction faction)
    {
        if (faction is null || faction.leader is null)
        {
            return false;
        }
        Pawn leader = faction.leader;
        int num = 0;
        for (int i = 0; i < LeaderSkill.Count(); i++)
        {
            if (leader.skills.GetSkill(LeaderSkill[i]).Level > 15)
            {
                num++;
            }
        }
        return num >= 2;
    }
}
