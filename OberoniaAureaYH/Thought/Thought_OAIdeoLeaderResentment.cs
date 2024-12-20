using RimWorld;

namespace OberoniaAurea;

public class Thought_OAIdeoLeaderResentment : Thought_IdeoLeaderResentment
{
    public override float MoodOffset()
    {
        float moodOffset = base.MoodOffset();
        Ideo ideo = pawn.Ideo;
        if (ideo != null)
        {
            if (ideo.HasPrecept(OA_PreceptDefOf.OA_RK_LeaderAttitude_Respect))
            {
                moodOffset -= 1f;
            }
            else if (ideo.HasPrecept(OA_PreceptDefOf.OA_RK_LeaderAttitude_Worship))
            {
                moodOffset -= 2f;
            }
        }
        return moodOffset;
    }
}
