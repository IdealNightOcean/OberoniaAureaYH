using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea;

public class QuestNode_InitGravQuestCompleted : QuestNode
{
    protected override bool TestRunInt(Slate slate)
    {
        return ScienceDepartmentInteractHandler.IsInteractAvailable();
    }

    protected override void RunInt()
    {
        QuestGen.quest.AddPart(new QuestPart_InitGravQuestCompleted());
    }
}

public class QuestPart_InitGravQuestCompleted : QuestPart
{
    public override void Cleanup()
    {
        base.Cleanup();
        if (quest.State == QuestState.EndedSuccess)
        {
            ScienceDepartmentInteractHandler.Instance?.Notify_InitGravQuestCompleted();
        }
    }
}