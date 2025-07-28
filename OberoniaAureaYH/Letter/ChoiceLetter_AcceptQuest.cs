using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ChoiceLetter_OptionalQuest : ChoiceLetter
{
    public QuestScriptDef questScriptDef;
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
                    GiveQuest(questScriptDef);
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

    public void GiveQuest(QuestScriptDef questDef)
    {
        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, new Slate());
        if (!quest.hidden && questDef.sendAvailableLetter)
        {
            QuestUtility.SendLetterQuestAvailable(quest);
        }
        this.quest = quest;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref questScriptDef, "questScriptDef");
    }
}