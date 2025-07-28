using RimWorld;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_UnlockedFile : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!ScienceDepartmentInteractHandler.IsScienceDepartmentInteractAvailable())
        {
            return false;
        }
        return ScienceDepartmentInteractHandler.Instance.GravResearchAssistLendPawn is not null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Quest quest = Find.QuestManager.ActiveQuestsListForReading.FirstOrFallback(q => q.root == OARK_QuestScriptDefOf.OARK_GravResearchAssistance, fallback: null);
        if (quest is null)
        {
            return true;
        }

        Pawn researcher = ScienceDepartmentInteractHandler.Instance.GravResearchAssistLendPawn;
        ChoiceLetter_UnlockedFile letter = (ChoiceLetter_UnlockedFile)LetterMaker.MakeLetter(label: "OARK_LetterLabel_UnlockedFile".Translate(),
                                                                                             text: "OARK_Letter_UnlockedFile".Translate(researcher),
                                                                                             def: OARK_LetterDefOf.OARK_UnlockedFileLetter,
                                                                                             lookTargets: null,
                                                                                             relatedFaction: ModUtility.OAFaction,
                                                                                             quest: null);
        letter.InitLetter(researcher);
        letter.StartTimeout(30000);
        Find.LetterStack.ReceiveLetter(letter);
        return true;
    }
}
