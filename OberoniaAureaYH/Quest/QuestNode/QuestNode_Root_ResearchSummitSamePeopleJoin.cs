using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_Root_ResearchSummitSamePeopleJoin : QuestNode
{
    protected string acceptSignal;
    protected string rejectSignal;

    public Pawn GeneratePawn(Slate slate)
    {
        slate.TryGet("faction", out Faction faction);

        PawnGenerationRequest request = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(PawnKindDefOf.Villager, faction, forceNew: true);
        request.ForceAddFreeWarmLayerIfNeeded = true;
        Pawn pawn = PawnGenerator.GeneratePawn(request);

        if (!pawn.IsWorldPawn())
        {
            Find.WorldPawns.PassToWorld(pawn);
        }
        if (pawn.skills is not null)
        {
            pawn.skills.GetSkill(SkillDefOf.Intellectual).Level = 0;
        }
        TraitSet pawnTrait = pawn.story?.traits;
        if (pawnTrait is not null)
        {
            Trait trait = pawnTrait.GetTrait(OARK_RimWorldDefOf.NaturalMood);
            if (trait is not null)
            {
                pawn.story.traits.RemoveTrait(trait);
            }
            pawnTrait.GainTrait(new Trait(OARK_RimWorldDefOf.NaturalMood, 2));
        }
        return pawn;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!slate.TryGet("map", out Map map))
        {
            map = QuestGen_Get.GetMap();
        }

        Quest quest = QuestGen.quest;
        Pawn pawn = GeneratePawn(slate);
        slate.Set("pawn", pawn);

        acceptSignal = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
        rejectSignal = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
        quest.Signal(acceptSignal, delegate
        {
            quest.SetFaction([pawn], Faction.OfPlayer);
            quest.Delay(120000, delegate
            {
                quest.PawnsArrive([pawn], null, map.Parent);
                QuestGen_End.End(quest, QuestEndOutcome.Fail);
            });
        });

        quest.End(QuestEndOutcome.Fail, inSignal: rejectSignal);

        SendLetter(quest, pawn);
        string killedSignal = QuestGenUtility.HardcodedSignalWithQuestID("pawn.Killed");
        string playerTended = QuestGenUtility.HardcodedSignalWithQuestID("pawn.PlayerTended");
        string leftMapSignal = QuestGenUtility.HardcodedSignalWithQuestID("pawn.LeftMap");
        string recruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("pawn.Recruited");
        quest.End(QuestEndOutcome.Fail, goodwillChangeAmount: 0, goodwillChangeFactionOf: null, killedSignal);
        quest.End(QuestEndOutcome.Success, goodwillChangeAmount: 0, goodwillChangeFactionOf: null, playerTended);
        quest.End(QuestEndOutcome.Success, goodwillChangeAmount: 0, goodwillChangeFactionOf: null, recruitedSignal);
        quest.Signal(leftMapSignal, delegate
        {
            quest.AnyPawnUnhealthy(pawns: [pawn],
                                   action: delegate
                                   {
                                       QuestGen_End.End(quest, QuestEndOutcome.Fail);
                                   },
                                   elseAction: delegate
                                   {
                                       QuestGen_End.End(quest, QuestEndOutcome.Success);
                                   });
        });

        quest.Delay(60000, delegate
        {
            QuestGen_End.End(quest, QuestEndOutcome.Fail);
        }, inSignalEnable: null, inSignalDisable: acceptSignal);
    }

    private void SendLetter(Quest quest, Pawn pawn)
    {
        TaggedString title = "OA_LetterLabelResearchSummitSamePeopleJoin".Translate();
        TaggedString letterText = "OA_LetterResearchSummitSamePeopleJoin".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
        PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
        ChoiceLetter_AcceptJoinerViewInfo choiceLetter_AcceptJoinerViewInfo = (ChoiceLetter_AcceptJoinerViewInfo)LetterMaker.MakeLetter(title, letterText, OARK_ModDefOf.OA_RK_AcceptJoinerViewInfo);
        choiceLetter_AcceptJoinerViewInfo.signalAccept = acceptSignal;
        choiceLetter_AcceptJoinerViewInfo.signalReject = rejectSignal;
        choiceLetter_AcceptJoinerViewInfo.associatedPawn = pawn;
        choiceLetter_AcceptJoinerViewInfo.quest = quest;
        choiceLetter_AcceptJoinerViewInfo.StartTimeout(60000);
        Find.LetterStack.ReceiveLetter(choiceLetter_AcceptJoinerViewInfo);
    }

    protected override bool TestRunInt(Slate slate)
    {
        return QuestGen_Get.GetMap() is not null;
    }
}