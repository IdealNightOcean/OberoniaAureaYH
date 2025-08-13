using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class QuestNode_Root_ProspectingTeam : QuestNode_Root_RefugeeBase
{
    protected override bool TestRunInt(Slate slate)
    {
        Faction faction = ModUtility.OAFaction;
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        return base.TestRunInt(slate);
    }

    protected override Faction GetOrGenerateFaction()
    {
        return ModUtility.OAFaction;
    }

    protected override QuestParameter InitQuestParameter(Faction faction)
    {
        return new QuestParameter(faction, QuestGen_Get.GetMap())
        {
            allowAssaultColony = false,
            allowLeave = true,
            allowBadThought = false,
            LodgerCount = 4,
            ChildCount = 0,

            goodwillSuccess = 12,
            goodwillFailure = -12,
            rewardValueRange = new FloatRange(300f, 500f) * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor,
            questDurationTicks = 60000,

            fixedPawnKind = OARK_PawnGenerateDefOf.OA_RK_Court_Member_Exploration
        };
    }

    protected override List<Pawn> GeneratePawns(string lodgerRecruitedSignal = null)
    {
        return questParameter.slate.Get<List<Pawn>>("pawns");
    }

    protected override void RunInt()
    {
        questParameter = null;
        Faction faction = GetOrGenerateFaction();
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            return;
        }

        questParameter = InitQuestParameter(faction);
        Quest quest = questParameter.quest;

        string lodgerArrestedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrested");
        string lodgerRecruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
        string lodgerBecameMutantSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.BecameMutant");
        string lodgerArrestedOrRecruited = QuestGen.GenerateNewSignal("Lodger_ArrestedOrRecruited");
        quest.AnySignal(inSignals: [lodgerRecruitedSignal, lodgerArrestedSignal], outSignals: [lodgerArrestedOrRecruited]);

        List<Pawn> pawns = GeneratePawns(lodgerRecruitedSignal);
        if (pawns.NullOrEmpty())
        {
            quest.End(QuestEndOutcome.Unknown, sendLetter: true, playSound: false);
            return;
        }
        questParameter.pawns = pawns;

        quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, [lodgerRecruitedSignal, lodgerBecameMutantSignal]);
        quest.SetAllApparelLocked(pawns);
        quest.JoinPlayer(questParameter.map.Parent, pawns, joinPlayer: true);

        SetQuestAward();

        QuestPart_OARefugeeInteractions questPart_RefugeeInteractions = new()
        {
            inSignalEnable = questParameter.slate.Get<string>("inSignal"),
            faction = faction,
            mapParent = questParameter.map.Parent,
            inSignalArrested = lodgerArrestedSignal,
            inSignalRecruited = lodgerRecruitedSignal,
            signalListenMode = QuestPart.SignalListenMode.Always
        };
        questPart_RefugeeInteractions.InitWithDefaultSingals(questParameter.allowAssaultColony, questParameter.allowLeave, questParameter.allowBadThought);
        questPart_RefugeeInteractions.pawns.AddRange(pawns);
        quest.AddPart(questPart_RefugeeInteractions);

        SetQuestEndLetters(questPart_RefugeeInteractions);

        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_RefugeeInteractions.outSignalArrested_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony);

        SetQuestEndCompCommon(questPart_RefugeeInteractions);
        SetPawnsLeaveComp(inSignalEnable: null, lodgerArrestedOrRecruited);
        SetSlateValue();
    }

    protected override void SetPawnsLeaveComp(string inSignalEnable, string inSignalRemovePawn)
    {
        base.SetPawnsLeaveComp(inSignalEnable, inSignalRemovePawn);

        Quest quest = questParameter.quest;

        string specialRewardSignal = QuestGen.GenerateNewSignal("Lodger_GiveSpecialReward");
        quest.Delay(questParameter.questDurationTicks, inner: null, inSignalEnable: inSignalEnable, outSignalComplete: specialRewardSignal);

        QuestPart_ProspectingTeamReward questPart_ProspectingTeamReward = new()
        {
            insSignal = specialRewardSignal,
            inSignalRemovePawn = inSignalRemovePawn,
            leader = questParameter.pawns[0],
            mapParent = questParameter.map.Parent,
            faction = questParameter.faction,
            pawns = []
        };
        questPart_ProspectingTeamReward.pawns.AddRange(questParameter.pawns);
        quest.AddPart(questPart_ProspectingTeamReward);
    }
}
