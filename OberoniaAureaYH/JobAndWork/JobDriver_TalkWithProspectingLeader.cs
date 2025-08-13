using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea;

public class JobDriver_TalkWithProspectingLeader : JobDriver
{
    private Pawn TalkWith => TargetPawnA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TalkWith, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => TalkWith.DeadOrDowned);
        Toil talk = ToilMaker.MakeToil("MakeNewToils");
        talk.initAction = delegate
        {
            Pawn actor = talk.actor;
            if (!TalkWith.DeadOrDowned)
            {
                Find.WindowStack.Add(TalkTree(actor, TalkWith));
            }
        };
        yield return talk;
    }


    private static Dialog_NodeTree TalkTree(Pawn pawn, Pawn talkWith)
    {
        DiaNode rootNode = new("OARK_TalkWithProspectingLeader".Translate(pawn, talkWith));
        DiaOption acceptOpt = new("OARK_ProspectingLeader_Serve".Translate())
        {
            action = delegate { AcceptResult(talkWith); },
            resolveTree = true
        };

        DiaOption rejectOpt = new("OARK_ProspectingLeader_Reject".Translate())
        {
            action = delegate { IgnoreResult(talkWith); },
            resolveTree = true
        };
        rootNode.options.Add(acceptOpt);
        rootNode.options.Add(rejectOpt);

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
            if (lord.LordJob is LordJob_VisitColonyTalkable talkJob)
            {
                talkJob.SetTalkAvailable(false);
            }
            lord.RemoveAllPawns();
        }

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

    private static void IgnoreResult(Pawn leader)
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
            lord.RemoveAllPawns();
        }
        LordMaker.MakeNewLord(leader.Faction, new LordJob_ExitMapBest(), leader.MapHeld, pawns);
        IntVec3 pos = leader.PositionHeld;
        Map map = leader.MapHeld;

        TaggedString label;
        TaggedString text;
        if (Faction.OfPlayer.def.defName == "OA_RK_PlayerFaction" || Faction.OfPlayer.def.defName == "OA_RK_PlayerFaction_B")
        {
            label = "OARK_LetterLabel_ProspectingTeamReject_OA".Translate();
            text = "OARK_Letter_ProspectingTeamReject_OA".Translate();

            Thing flowerCakes = ThingMaker.MakeThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB);
            flowerCakes.stackCount = 10;
            GenPlace.TryPlaceThing(flowerCakes, pos, map, ThingPlaceMode.Near);
            Thing Silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            Silver.stackCount = 200;
            GenPlace.TryPlaceThing(Silver, pos, map, ThingPlaceMode.Near);

            leader.Faction?.TryAffectGoodwillWith(Faction.OfPlayer, 50, reason: OARK_HistoryEventDefOf.OARK_ProspectingTeam);
        }
        else
        {
            label = "OARK_LetterLabel_ProspectingTeamReject".Translate();
            text = "OARK_Letter_ProspectingTeamReject".Translate();
        }

        Find.LetterStack.ReceiveLetter(label: label,
                                     text: text,
                                     textLetterDef: LetterDefOf.NeutralEvent,
                                     new LookTargets(pos, map),
                                     relatedFaction: leader.Faction);
    }
}
