using RimWorld;

namespace OberoniaAurea;

public class Thought_SkyWarmth : Thought_Situational
{
    public override float MoodOffset()
    {
        float moodOffset = base.MoodOffset();
        Ideo ideo = pawn.Ideo;
        if (ideo != null && ideo.HasMeme(OARatkin_MiscDefOf.OA_RK_meme_Friendly))
        {
            moodOffset += 2f;
        }
        return moodOffset;
    }
}
