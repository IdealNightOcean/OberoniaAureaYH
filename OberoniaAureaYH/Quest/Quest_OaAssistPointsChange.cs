using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class QuestNode_OaAssistPointsChange : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<int> change;

    public SlateRef<bool> isReward;
    public SlateRef<bool> isSingleReward;

    protected override bool TestRunInt(Slate slate)
    {
        return ModUtility.OAFaction is not null;
    }
    protected override void RunInt()
    {
        if (ModUtility.OAFaction is null)
        {
            return;
        }
        Slate slate = QuestGen.slate;
        Quest quest = QuestGen.quest;

        QuestPart_OaAssistPointsChange questPart_OaAssistPointsChange = new(inSignalTrigger: QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
                                                                            change: change.GetValue(slate));
        quest.AddPart(questPart_OaAssistPointsChange);

        if (isReward.GetValue(slate))
        {
            QuestPart_Choice questPart_Choice;
            Reward_AssistPoint reward = new() { amount = change.GetValue(slate) };
            if (isSingleReward.GetValue(slate))
            {
                questPart_Choice = new QuestPart_Choice()
                {
                    inSignalChoiceUsed = questPart_OaAssistPointsChange.inSignalTrigger,
                };

                questPart_Choice.choices.Add(new QuestPart_Choice.Choice() { rewards = [reward] });
                quest.AddPart(questPart_Choice);
            }
            else
            {
                questPart_Choice = quest.PartsListForReading.OfType<QuestPart_Choice>().FirstOrFallback(null);
                if (questPart_Choice is not null)
                {
                    foreach (QuestPart_Choice.Choice singelChoice in questPart_Choice.choices)
                    {
                        singelChoice.rewards.Add(reward);
                    }
                }
            }
        }
    }
}

public class QuestPart_OaAssistPointsChange : QuestPart
{
    public string inSignalTrigger;
    public int change;
    private QuestPart_OaAssistPointsChange() { }

    public QuestPart_OaAssistPointsChange(string inSignalTrigger, int change)
    {
        this.inSignalTrigger = inSignalTrigger;
        this.change = change;
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (OAInteractHandler.Instance is not null && signal.tag == inSignalTrigger)
        {
            OAInteractHandler.Instance.AdjustAssistPoints(change);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignalTrigger, "inSignalTrigger");
        Scribe_Values.Look(ref change, "change", 0);
    }
}