using System.Linq;
using RimWorld;
using Verse;

namespace OberoniaAurea_PatrickStar;

internal class QuestPart_RequirementsToAcceptNoOngoingBestowingCeremonys : QuestPart_RequirementsToAccept
{
	public override AcceptanceReport CanAccept()
	{
		if (Find.QuestManager.QuestsListForReading.Where((Quest q) => q.State == QuestState.Ongoing && q.root == OBDefOf.OMBestowingCeremony).Any())
		{
			return new AcceptanceReport("QuestCanNotStartUntilBestowingCeremonyFinished".Translate());
		}
		return true;
	}
}
