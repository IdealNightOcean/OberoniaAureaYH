using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class JobDriver_SailorInvitation : JobDriver
{
    private const TargetIndex InviteeInd = TargetIndex.A;

    public Pawn Invitee => TargetPawnA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Invitee, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(InviteeInd);
        //this.FailOn(() => !Invitee.mindState.WillJoinColonyIfRescued);

        yield return Toils_Goto.GotoThing(InviteeInd, PathEndMode.Touch);
        Toil toil = Toils_General.WaitWith(InviteeInd, 300);
        toil.WithProgressBarToilDelay(InviteeInd);
        toil.FailOnDespawnedOrNull(InviteeInd);
        toil.FailOnCannotTouch(InviteeInd, PathEndMode.Touch);
        yield return toil;
        yield return Toils_General.Do(delegate
        {
            Invitation(Invitee, pawn);
        });
    }

    private static void Invitation(Pawn invitee, Pawn inviter)
    {
        QuestUtility.SendQuestTargetSignals(invitee.questTags, "Recruited", invitee.Named("SUBJECT"));
        QuestUtility.SendQuestTargetSignals(invitee.questTags, "Recruited", invitee.Named("SUBJECT"));

        if (invitee.IsColonist || invitee.IsPrisonerOfColony || invitee.IsSlaveOfColony)
        {
            RecruitUtility.Recruit(invitee, Faction.OfPlayer, inviter);
        }
        else if (invitee.Faction != Faction.OfPlayer)
        {
            invitee.SetFaction(Faction.OfPlayer);
        }

        OAFrame_PawnUtility.RemoveFirstHediffOfDef(invitee, OARK_HediffDefOf.OARK_Hediff_SailorAssistance);
    }
}
