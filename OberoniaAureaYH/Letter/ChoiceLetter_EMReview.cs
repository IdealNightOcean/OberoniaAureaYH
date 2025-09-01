using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

internal class ChoiceLetter_EMReview : ChoiceLetter
{
    public override bool CanDismissWithRightClick => false;

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly)
            {
                yield return Option_Close;
            }
            else
            {
                yield return Option_AcceptReview;
                yield return Option_RejectReview;
            }
        }
    }

    private DiaOption Option_AcceptReview => new("OARK_EMReview_Accept".Translate())
    {
        action = AcceptResult,
        resolveTree = true
    };

    private DiaOption Option_RejectReview => new("OARK_EMReview_Reject".Translate())
    {
        action = RejectResult,
        resolveTree = true
    };

    private void AcceptResult()
    {
        Find.LetterStack.RemoveLetter(this);

        Slate slate = new();
        if (OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OARK_EconomyMinistryReview, new Slate()))
        {
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo("OARK_EMReviewAcceptInfo".Translate(), ModUtility.OAFaction));
        }
        else
        {
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo("OARK_EMReviewAcceptFailInfo".Translate(), ModUtility.OAFaction));
        }
    }

    private void RejectResult()
    {
        Find.LetterStack.RemoveLetter(this);
        ChoiceLetter_EMReview_Reject letter = (ChoiceLetter_EMReview_Reject)LetterMaker.MakeLetter(label: "OARK_LetterLabel_EMReviewReject".Translate(),
                                                                                                   text: "OARK_Letter_EMReviewReject".Translate(),
                                                                                                   def: OARK_LetterDefOf.OARK_EMReview_RejectLetter,
                                                                                                   relatedFaction: ModUtility.OAFaction);
        int delayTicks = Rand.RangeInclusive(2 * 60000, 3 * 60000);
        letter.StartTimeout(delayTicks + 30000);
        Find.LetterStack.ReceiveLetter(letter, delayTicks: delayTicks);
    }
}

internal class ChoiceLetter_EMReview_Reject : ChoiceLetter
{
    public override bool ShouldAutomaticallyOpenLetter => true;
    public override bool CanDismissWithRightClick => false;

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly)
            {
                yield return Option_Close;
            }
            else
            {
                yield return Option_Confirm;
            }
        }
    }

    private DiaOption Option_Confirm => new("Confirm".Translate())
    {
        action = delegate
        {
            Find.LetterStack.RemoveLetter(this);
            ScienceDepartmentInteractHandler.Instance?.StartEMReviewBlock();
        },
        resolveTree = true
    };
}
