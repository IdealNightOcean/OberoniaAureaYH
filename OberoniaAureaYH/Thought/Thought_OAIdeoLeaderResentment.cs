using RimWorld;

namespace OberoniaAurea;

public class Thought_OAIdeoLeaderResentment : Thought_IdeoLeaderResentment
{
    public override float MoodOffset()
    {
        float moodOffset = base.MoodOffset();
        Ideo ideo = pawn.Ideo;
        if (ideo is not null)
        {
            if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_LeaderAttitude_Respect))
            {
                moodOffset -= 1f;
            }
            else if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_LeaderAttitude_Worship))
            {
                moodOffset -= 2f;
            }
        }
        return moodOffset;
    }
}
