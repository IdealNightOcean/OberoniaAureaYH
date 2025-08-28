﻿using OberoniaAurea_Frame;
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

    protected override void InitQuestParameter()
    {
        questParameter = new QuestParameter()
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

            fixedPawnKind = OARK_PawnGenerateDefOf.OA_RK_Court_Member
        };
    }

    protected override List<Pawn> GeneratePawns(string lodgerRecruitedSignal = null)
    {
        return QuestGen.slate.Get<List<Pawn>>("pawns");
    }

    protected override void PawnArrival(string lodgerArrivalSignal)
    {
        QuestGen.quest.JoinPlayer(questParameter.map.Parent, questParameter.pawns, joinPlayer: true);
        QuestGen.quest.SendSignals([lodgerArrivalSignal]);
    }

    protected override void SetPawnsLeaveComp(string lodgerArrivalSignal, string inSignalRemovePawn)
    {
        base.SetPawnsLeaveComp(lodgerArrivalSignal, inSignalRemovePawn);

        Quest quest = QuestGen.quest;

        string specialRewardSignal = QuestGen.GenerateNewSignal("Lodger_GiveSpecialReward");
        quest.Delay(questParameter.questDurationTicks, inner: null, inSignalEnable: lodgerArrivalSignal, outSignalComplete: specialRewardSignal);

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