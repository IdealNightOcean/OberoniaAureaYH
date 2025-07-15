using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ChoiceLetter_OptionalQuest : ChoiceLetter
{
    public QuestScriptDef questScriptDef;
    [Unsaved] public Slate slate;
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
                    GiveQuest();
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

    public void GiveQuest()
    {
        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questScriptDef, slate ?? new Slate());
        if (!quest.hidden && questScriptDef.sendAvailableLetter)
        {
            QuestUtility.SendLetterQuestAvailable(quest);
        }
        this.quest = quest;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref questScriptDef, "questScriptDef");
        if (Scribe.mode == LoadSaveMode.Saving && slate is not null)
        {
            Log.Error("Try saving a ChoiceLetter_OptionalQuest with a non-null slate.");
        }
    }
}