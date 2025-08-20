using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_ScienceShipRecycle : IncidentWorker
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

        ScienceDepartmentInteractHandler.Instance.AddGravTechPoints(2000, byPlayer: false);
        bool questValid = true;
        if (ModUtility.OAFaction is null
            || ModUtility.OAFaction.PlayerRelationKind != FactionRelationKind.Ally
            || !ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.HasValue)
        {
            questValid = false;
        }

        questValid = questValid && OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OARK_ScienceShipRecycle, new Slate());

        if (!questValid)
        {
            Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_ScienceShipRecycleTriggerFail".Translate(),
                                           text: "OARK_Letter_ScienceShipRecycleTriggerFail".Translate(),
                                           textLetterDef: LetterDefOf.NeutralEvent);
        }

        return true;
    }
}
