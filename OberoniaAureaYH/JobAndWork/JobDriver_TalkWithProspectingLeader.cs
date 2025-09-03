using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;

public class JobDriver_TalkWithProspectingLeader : JobDriver_TalkWithAtOnce
{
    protected override void TalkAction(Pawn talker, Pawn talkWith)
    {
        Find.WindowStack.Add(TalkTree(talker, talkWith));
    }

    private static Dialog_NodeTree TalkTree(Pawn pawn, Pawn leader)
    {
        DiaNode rootNode = new("OARK_TalkWithProspectingLeader".Translate(pawn, leader));
        DiaOption acceptOpt = new("OARK_ProspectingLeader_Serve".Translate())
        {
            action = delegate { AcceptResult(leader); },
            resolveTree = true
        };

        DiaOption rejectOpt = new("OARK_ProspectingLeader_Reject".Translate())
        {
            action = delegate { IgnoreResult(leader); },
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
            if (lord.LordJob is ILordJobWithTalk talkLordJob)
            {
                talkLordJob.DisableTalk();
            }
            lord.RemoveAllPawns();
        }

        int index = pawns.IndexOf(leader);
        pawns.Swap(0, index);
        Slate slate = new();
        slate.Set("pawns", pawns);
        if (!OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OARK_ProspectingTeam, slate, forced: true))
        {
            LordMaker.MakeNewLord(leader.Faction, new LordJob_ExitMapBest(), leader.MapHeld, pawns);
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
        if (Faction.OfPlayer.def.defName == "OA_RK_PlayerFaction")
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