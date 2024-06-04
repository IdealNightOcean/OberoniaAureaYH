using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

//复制派系，用于某些需要特定storeAs名称Faction的Node
public class QuestNode_CopyFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<Faction> faction;

    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        SetVars(slate);
    }

    protected void SetVars(Slate slate)
    {
        Faction fVar = faction.GetValue(slate);
        if (fVar != null)
        {
            slate.Set(storeAs.GetValue(slate), fVar);
        }
    }
}
