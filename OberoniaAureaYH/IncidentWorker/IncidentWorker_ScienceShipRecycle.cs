using RimWorld;
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

        if (questValid)
        {
            if (OARK_QuestScriptDefOf.OARK_ScienceShipRecycle.CanRun(new RimWorld.QuestGen.Slate(), Find.World))
            {
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(OARK_QuestScriptDefOf.OARK_ScienceShipRecycle, 5000f);
                if (!quest.hidden && OARK_QuestScriptDefOf.OARK_ScienceShipRecycle.sendAvailableLetter)
                {
                    QuestUtility.SendLetterQuestAvailable(quest);
                }
            }
            else
            {
                questValid = false;
            }
        }

        if (!questValid)
        {
            Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_ScienceShipRecycleTriggerFail".Translate(),
                                           text: "OARK_Letter_ScienceShipRecycleTriggerFail".Translate(),
                                           textLetterDef: LetterDefOf.NeutralEvent);
        }

        return true;
    }
}
