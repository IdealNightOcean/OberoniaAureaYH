using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

//科研峰会 - 同道中人
public class QuestNode_Root_ResearchSummitSamePeopleJoin : QuestNode_Root_SinglePawnJoin
{
    private static readonly int TimeoutTicks = 60000;

    public static readonly float RelationWithColonistWeight = 20f;
    protected override void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
    {
        signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
        signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
        quest.Signal(signalAccept, delegate
        {
            quest.SetFaction(Gen.YieldSingle(pawn), Faction.OfPlayer);
            quest.Delay(120000, delegate
            {
                quest.PawnsArrive(Gen.YieldSingle(pawn), null, map.Parent);
                QuestGen_End.End(quest, QuestEndOutcome.Success);
            });
        });
        quest.Signal(signalReject, delegate
        {
            QuestGen_End.End(quest, QuestEndOutcome.Fail);
        });
    }
    public override Pawn GeneratePawn()
    {
        Slate slate = QuestGen.slate;
        slate.TryGet<Faction>("faction", out Faction fVar);
        if (!slate.TryGet<PawnGenerationRequest>("overridePawnGenParams", out var var))
        {
            var = new PawnGenerationRequest(PawnKindDefOf.Villager, fVar, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true);
        }
        Pawn pawn = PawnGenerator.GeneratePawn(var);

        if (!pawn.IsWorldPawn())
        {
            Find.WorldPawns.PassToWorld(pawn);
        }
        if (pawn.skills != null)
        {
            pawn.skills.GetSkill(SkillDefOf.Intellectual).Level = 0;
        }
        TraitSet pawnTrait = pawn.story?.traits;
        if (pawnTrait != null)
        {
            Trait trait = pawnTrait.GetTrait(OA_RimWorldDefOf.NaturalMood);
            if (trait != null)
            {
                pawn.story.traits.RemoveTrait(trait);
            }
            pawnTrait.GainTrait(new Trait(OA_RimWorldDefOf.NaturalMood, 2));
        }
        return pawn;
    }
    public override void SendLetter(Quest quest, Pawn pawn)
    {
        TaggedString title = "OA_LetterLabelResearchSummitSamePeopleJoin".Translate();
        TaggedString letterText = "OA_LetterResearchSummitSamePeopleJoin".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
        PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
        ChoiceLetter_AcceptJoinerViewInfo choiceLetter_AcceptJoinerViewInfo = (ChoiceLetter_AcceptJoinerViewInfo)LetterMaker.MakeLetter(title, letterText, OA_MiscDefOf.OA_RK_AcceptJoinerViewInfo);
        choiceLetter_AcceptJoinerViewInfo.signalAccept = signalAccept;
        choiceLetter_AcceptJoinerViewInfo.signalReject = signalReject;
        choiceLetter_AcceptJoinerViewInfo.associatedPawn = pawn;
        choiceLetter_AcceptJoinerViewInfo.quest = quest;
        choiceLetter_AcceptJoinerViewInfo.StartTimeout(TimeoutTicks);
        Find.LetterStack.ReceiveLetter(choiceLetter_AcceptJoinerViewInfo);
    }
    protected override void RunInt()
    {
        base.RunInt();
        Quest quest = QuestGen.quest;
        quest.Delay(60000, delegate
        {
            QuestGen_End.End(quest, QuestEndOutcome.Fail);
        }, null, signalAccept);
    }
}