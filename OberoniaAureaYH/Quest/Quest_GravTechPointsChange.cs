using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class QuestNode_GravTechPointsChange : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<int> change;

    public SlateRef<bool> isReward;
    public SlateRef<bool> isSingleReward;

    protected override bool TestRunInt(Slate slate)
    {
        return ScienceDepartmentInteractHandler.IsScienceDepartmentInteractAvailable();
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Quest quest = QuestGen.quest;

        QuestPart_GravTechPointChange questPart_GravTechPointChange = new(inSignalTrigger: QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
                                                                          change: change.GetValue(slate));
        quest.AddPart(questPart_GravTechPointChange);

        if (isReward.GetValue(slate))
        {
            QuestPart_Choice questPart_Choice;
            Reward_GravTechPoint reward = new() { amount = change.GetValue(slate) };
            if (isSingleReward.GetValue(slate))
            {
                questPart_Choice = new QuestPart_Choice()
                {
                    inSignalChoiceUsed = questPart_GravTechPointChange.inSignalTrigger,
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

public class QuestPart_GravTechPointChange : QuestPart
{
    public string inSignalTrigger;

    public int change;

    private QuestPart_GravTechPointChange() { }
    public QuestPart_GravTechPointChange(string inSignalTrigger, int change)
    {
        this.inSignalTrigger = inSignalTrigger;
        this.change = change;
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (ModsConfig.OdysseyActive && ScienceDepartmentInteractHandler.Instance is not null)
        {
            if (signal.tag == inSignalTrigger)
            {
                ScienceDepartmentInteractHandler.Instance.AddGravTechPoints(change, byPlayer: true);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignalTrigger, "inSignalTrigger");
        Scribe_Values.Look(ref change, "change", 0);
    }
}
