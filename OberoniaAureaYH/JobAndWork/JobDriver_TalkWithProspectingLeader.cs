using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea;

public class JobDriver_TalkWithProspectingLeader : JobDriver
{
    private Pawn Leader => TargetPawnA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Leader, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => Leader != OAInteractHandler.Instance.ProspectingLeader);
        Toil talk = ToilMaker.MakeToil("MakeNewToils");
        talk.initAction = delegate
        {
            Pawn actor = talk.actor;
            if (Leader == OAInteractHandler.Instance.ProspectingLeader)
            {
                Find.WindowStack.Add(TalkTree(actor, Leader));
            }
        };
        yield return talk;
    }


    private static Dialog_NodeTree TalkTree(Pawn pawn, Pawn leader)
    {
        DiaNode rootNode = new("OARK_TalkWithProspectingLeader".Translate(pawn, leader));
        DiaOption acceptOpt = new("OARK_Serve".Translate())
        {
            action = delegate { AcceptResult(leader); },
            resolveTree = true
        };

        DiaOption ignoreOpt = new("OARK_TalkWithProspectingLeader_Ignore".Translate())
        {
            resolveTree = true
        };
        rootNode.options.Add(acceptOpt);
        rootNode.options.Add(ignoreOpt);

        return new Dialog_NodeTree(rootNode);
    }

    private static void AcceptResult(Pawn leader)
    {
        Lord lord = leader.GetLord();
        List<Pawn> pawns = [];
        if (lord is null)
        {
            pawns.Add(leader);
        }
        else
        {
            pawns.AddRange(lord.ownedPawns);
            lord.Cleanup();
            lord.RemoveAllPawns();
        }

        OAInteractHandler.Instance.ProspectingLeader = null;

        int index = pawns.IndexOf(leader);
        pawns.Swap(0, index);
        Slate slate = new();
        slate.Set("pawns", pawns);
        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(OARK_QuestScriptDefOf.OARK_ProspectingTeam, slate);

        if (quest is null)
        {
            LordMaker.MakeNewLord(leader.Faction, new LordJob_ExitMapBest(), leader.MapHeld, pawns);
        }
        else if (!quest.hidden && quest.root.sendAvailableLetter)
        {
            QuestUtility.SendLetterQuestAvailable(quest);
        }
    }
}
