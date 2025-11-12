using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ChoiceLetter_AcceptJoinerViewInfo : ChoiceLetter
{
    public string signalAccept;

    public string signalReject;

    public Pawn associatedPawn;

    public override bool CanDismissWithRightClick => false;

    public override bool CanShowInLetterStack
    {
        get
        {
            if (base.CanShowInLetterStack && quest is not null)
            {
                if (quest.State != QuestState.Ongoing)
                {
                    return quest.State == QuestState.NotYetAccepted;
                }
                return true;
            }
            return false;
        }
    }

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
                    Find.SignalManager.SendSignal(new Signal(signalAccept));
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true
            };
            DiaOption optionReject = new("RejectLetter".Translate())
            {
                action = delegate
                {
                    Find.SignalManager.SendSignal(new Signal(signalReject));
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true
            };
            yield return optionAccept;
            yield return optionReject;
            if (associatedPawn is not null)
            {
                DiaOption optionViewInfo = new("OA_ViewPawnInfoButton".Translate())
                {
                    action = delegate
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(associatedPawn));
                    }
                };
                yield return optionViewInfo;
            }
            if (lookTargets.IsValid())
            {
                yield return Option_JumpToLocationAndPostpone;
            }
            yield return Option_Postpone;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref signalAccept, "signalAccept");
        Scribe_Values.Look(ref signalReject, "signalReject");
        Scribe_References.Look(ref associatedPawn, "associatedPawn");
    }
}
