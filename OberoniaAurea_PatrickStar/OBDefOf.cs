using RimWorld;
using Verse;

namespace OberoniaAurea_PatrickStar;

[DefOf]
public static class OBDefOf
{
	public static FactionDef OA_RK_Faction;

	public static PawnKindDef OA_RK_Court_Member;

	public static PawnKindDef OA_RK_Elite_Court_Member;

	public static QuestScriptDef OMBestowingCeremony;

	public static PawnKindDef OA_RK_Noble_C;

	public static PawnKindDef OA_RK_Guard_Member;

	public static PawnKindDef OA_RK_Assault_B;

	static OBDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
	}
}
