using RimWorld;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class QuestPart_OARequirementsToAcceptNoOngoingBestowingCeremony : QuestPart_RequirementsToAccept
{
    public override AcceptanceReport CanAccept()
    {
        if (Find.QuestManager.QuestsListForReading.Where((Quest q) => q.State == QuestState.Ongoing && q.root == OA_QuestScriptDefOf.OA_BestowingCeremony).Any())
        {
            return new AcceptanceReport("QuestCanNotStartUntilBestowingCeremonyFinished".Translate());
        }

        return true;
    }
}
