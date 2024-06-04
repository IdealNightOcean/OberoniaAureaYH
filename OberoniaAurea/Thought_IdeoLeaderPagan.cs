using RimWorld;

namespace OberoniaAurea;

public class Thought_IdeoLeaderPagan : Thought_IdeoLeaderResentment
{
	public override float MoodOffset()
	{
		float num = BaseMoodOffset;
		Ideo ideo = pawn.Ideo;
		if (ideo != null && ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Respect))
		{
			num -= 1f;
		}
		if (ideo != null && ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Worship))
		{
			num -= 2f;
		}
		return num;
	}
}
