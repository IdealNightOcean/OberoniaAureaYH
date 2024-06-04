using RimWorld;

namespace OberoniaAurea;

public class Thought_MoodwithIdeo : Thought_Situational
{
	public override float MoodOffset()
	{
		float num = BaseMoodOffset;
		Ideo ideo = pawn.Ideo;
		if (ideo != null && ideo.HasMeme(OADefOf.OA_RK_meme_Friendly))
		{
			num += 2f;
		}
		return num;
	}
}
