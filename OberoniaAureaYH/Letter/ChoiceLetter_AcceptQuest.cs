using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ChoiceLetter_OptionalQuest : ChoiceLetter
{

    public QuestScriptDef questScriptDef;
    public Slate slate;
    public override bool CanDismissWithRightClick => false;

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly)
            {
                yield return Option_Close;
                yield break;
            }
            DiaOption optionAccept = new("AcceptButton".Translate())
            {
                action = delegate
                {
                    GiveQuest(questScriptDef, slate);
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true
            };

            DiaOption optionReject = new("RejectLetter".Translate())
            {
                action = delegate
                {
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true
            };

            yield return optionAccept;
            yield return optionReject;
            if (lookTargets.IsValid())
            {
                yield return Option_JumpToLocationAndPostpone;
            }
            yield return Option_Postpone;
        }
    }

    public void GiveQuest(QuestScriptDef questDef, Slate vars)
    {
        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, vars ?? new Slate());
        if (!quest.hidden && questDef.sendAvailableLetter)
        {
            QuestUtility.SendLetterQuestAvailable(quest);
        }
        this.quest = quest;
    }
}