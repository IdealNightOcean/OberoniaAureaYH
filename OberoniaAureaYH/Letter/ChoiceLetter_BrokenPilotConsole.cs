using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ChoiceLetter_BrokenPilotConsole : ChoiceLetter
{
    public Thing brokenPilot;

    private DiaOption Option_Accept => new("Accept".Translate())
    {
        action = delegate
        {
            brokenPilot?.TryGetComp<CompBrokenPilotConsole>()?.AcceptOberoniaAureaRequest();
            Find.LetterStack.RemoveLetter(this);
        },
        resolveTree = true
    };

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly || quest is null || quest.State != QuestState.Ongoing)
            {
                yield return Option_Close;
                yield break;
            }

            yield return Option_Accept;
            yield return Option_Reject;
            yield return Option_Postpone;

            if (lookTargets.IsValid())
            {
                yield return Option_JumpToLocationAndPostpone;
            }
            if (!quest.hidden)
            {
                yield return Option_ViewInQuestsTab("ViewRelatedQuest", postpone: true);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref brokenPilot, "brokenPilot");
    }
}

