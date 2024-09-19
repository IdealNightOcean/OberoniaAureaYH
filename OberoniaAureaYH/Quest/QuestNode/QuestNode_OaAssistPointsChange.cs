using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;
public class QuestNode_OaAssistPointsChange : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<int> changePoints;

    public SlateRef<bool> canSendLetter;
    public SlateRef<LetterDef> letterDef;
    public SlateRef<string> letterLabel;
    public SlateRef<string> letterText;

    protected override bool TestRunInt(Slate slate)
    {
        return OberoniaAureaYHUtility.OAFaction != null;
    }
    protected override void RunInt()
    {
        if (OberoniaAureaYHUtility.OAFaction == null)
        {
            return;
        }
        Slate slate = QuestGen.slate;
        Quest quest = QuestGen.quest;
        if (storeAs.GetValue(slate) != null)
        {
            slate.Set(storeAs.GetValue(slate), changePoints.GetValue(slate));
        }
        QuestPart_OaAssistPointsChange questPart_OaAssistPointsChange = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            changePoints = changePoints.GetValue(slate)
        };
        quest.AddPart(questPart_OaAssistPointsChange);

        if (canSendLetter.GetValue(slate))
        {
            LetterDef letterDef = this.letterDef.GetValue(slate) ?? LetterDefOf.NeutralEvent;
            QuestPart_Letter questPart_Letter = new()
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
                letter = LetterMaker.MakeLetter(letterLabel.GetValue(slate), letterText.GetValue(slate), letterDef, OberoniaAureaYHUtility.OAFaction, quest)
            };
            quest.AddPart(questPart_Letter);
        }
    }
}