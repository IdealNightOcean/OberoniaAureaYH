using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class QuestNode_PawnGravResearchExchange : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignalComplete;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_PawnGravResearchExchange questPart_PawnGravResearchExchange = new()
        {
            inSignalComplete = inSignalComplete.GetValue(slate) ?? slate.Get<string>("inSignal"),
        };
        QuestGen.quest.AddPart(questPart_PawnGravResearchExchange);
    }
}


public class QuestPart_PawnGravResearchExchange : QuestPart
{
    public string inSignalComplete;
    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignalComplete)
        {
            ApplyExchangeResults();
        }
    }

    private void ApplyExchangeResults()
    {
        IEnumerable<Pawn> LentColonists = ModUtility.GetLendColonistsFromQuest(quest);

        foreach (Pawn pawn in LentColonists)
        {
            pawn.skills?.Learn(SkillDefOf.Intellectual, 20000f);
            pawn.needs.mood?.thoughts.memories.TryGainMemory(OARK_ThoughtDefOf.OARK_GraveResearchExchange);
            pawn.mindState?.inspirationHandler?.TryStartInspiration(InspirationDefOf.Inspired_Creativity, "OARK_Message_GravResearchExchange".Translate());
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignalComplete, "inSignalComplete");
    }
}