using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_ScienceDepartmentAnnualInteraction : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!ScienceDepartmentInteractHandler.IsInteractAvailable())
        {
            return false;
        }
        return ScienceDepartmentInteractHandler.Instance.IsInitGravQuestCompleted;
    }
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!CanFireNowSub(parms))
        {
            return false;
        }

        QuestScriptDef questScriptDef = Rand.Bool ? OARK_QuestScriptDefOf.OARK_GravResearchAssistance : OARK_QuestScriptDefOf.OARK_CrashedGravship;

        IncidentParms questIncidentParm = new()
        {
            target = Find.World,
            faction = ModUtility.OAFaction,
            questScriptDef = questScriptDef
        };

        return OAFrame_MiscUtility.TryFireIncidentNow(OAFrameDefOf.OAFrame_GiveQuest, questIncidentParm);
    }
}