using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class QuestNode_Root_SDBirthdayVisit : QuestNode_Root_RefugeeBase
{
    protected override PawnKindDef FixedPawnKind => OARK_PawnGenerateDefOf.OA_RK_Court_Member;
    protected override ThoughtDef ThoughtToAdd => base.ThoughtToAdd;

    protected override bool InitQuestParameter()
    {
        questParameter = new QuestParameter()
        {
            allowBadThought = false,
            allowLeave = false,

            LodgerCount = Rand.RangeInclusive(2, 3),
            ChildCount = 0,

            questDurationTicks = 30000,
            goodwillFailure = -12,
            goodwillSuccess = 0
        };

        Slate slate = QuestGen.slate;

        List<Pawn> birthdayPawns = slate.Get<List<Pawn>>("birthdayPawns");
        if (birthdayPawns.NullOrEmpty())
        {
            return false;
        }

        slate.Set("birthdayPawnsInfo", GenLabel.ThingsLabel(birthdayPawns));

        slate.Set(UniqueQuestDescSlate, true);
        slate.Set(UniqueLeavingLetterSlate, true);

        return true;
    }

    protected override Faction GetOrGenerateFaction()
    {
        QuestGen.slate.Set(IsMainFactionSlate, true);
        return QuestGen.slate.Get<Faction>("faction") ?? ModUtility.OAFaction;
    }

    protected override void PawnArrival(string lodgerArrivalSignal)
    {
        List<Thing> pawnsAndGifts = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB, questParameter.pawns.Count * 10);
        pawnsAndGifts.AddRange(questParameter.pawns);

        QuestGen.quest.DropPods(mapParent: questParameter.map.Parent,
            pawnsAndGifts,
            customLetterLabel: "[birthdayVisitorsArriveLetterLabel]",
            customLetterText: "[birthdayVisitorsArriveLetterText]",
            useTradeDropSpot: true,
            joinPlayer: true,
            faction: questParameter.faction);
        QuestGen.quest.SendSignals([lodgerArrivalSignal]);
    }

    protected override void PostPawnGenerated(Pawn pawn, string lodgerRecruitedSignal)
    {
        base.PostPawnGenerated(pawn, lodgerRecruitedSignal);
        Thing thing = ThingMaker.MakeThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB);
        thing.stackCount = 1;
        pawn.inventory.TryAddAndUnforbid(thing);
    }
}