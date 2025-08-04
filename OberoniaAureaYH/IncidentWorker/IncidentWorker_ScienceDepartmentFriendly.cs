using RimWorld;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_ScienceDepartmentFriendly : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!ScienceDepartmentInteractHandler.IsInteractAvailable())
        {
            return false;
        }
        return ModUtility.OAFaction.PlayerRelationKind != FactionRelationKind.Hostile;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!CanFireNowSub(parms))
        {
            return false;
        }

        ChoiceLetter_ScienceDepartmentFriendly letter = (ChoiceLetter_ScienceDepartmentFriendly)LetterMaker.MakeLetter(label: "OARK_LetterLabel_ScienceDepartmentFriendly".Translate(),
                                                                                                                       text: "OARK_Letter_ScienceDepartmentFriendly".Translate(),
                                                                                                                       def: OARK_LetterDefOf.OARK_ScienceDepartmentFriendlyLetter,
                                                                                                                       lookTargets: null,
                                                                                                                       relatedFaction: ModUtility.OAFaction);
        letter.InitLetter();
        letter.StartTimeout(60000);
        Find.LetterStack.ReceiveLetter(letter);
        return true;
    }
}
