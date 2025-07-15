using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_CheckInitGravQuestState : QuestNode
{
    public SlateRef<bool> questState;
    protected override bool TestRunInt(Slate slate)
    {
        return ModsConfig.OdysseyActive && (ScienceDepartmentInteractHandler.Instance.IsInitGravQuestCompleted == questState.GetValue(slate));
    }

    protected override void RunInt() { }
}
