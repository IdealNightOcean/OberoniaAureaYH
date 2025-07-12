using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;
public class QuestNode_Root_WoundedTraveler : QuestNode_Root_RefugeeBase
{
    private static readonly IntRange InjuryCount = new(2, 3);

    private const int AssistPoints = 15;

    private int curPawnIndex;

    protected override bool TestRunInt(Slate slate)
    {
        Faction faction = OARatkin_MiscUtility.OAFaction;
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        return base.TestRunInt(slate);
    }

    protected override Faction GetOrGenerateFaction()
    {
        return OARatkin_MiscUtility.OAFaction;
    }

    protected override QuestParameter InitQuestParameter(Faction faction)
    {
        curPawnIndex = 0;
        int lodgerCount = Rand.RangeInclusive(2, 3);
        return new QuestParameter(faction, QuestGen_Get.GetMap())
        {
            allowAssaultColony = false,
            allowLeave = true,
            allowBadThought = true,
            LodgerCount = lodgerCount,
            ChildCount = 0,

            goodwillSuccess = lodgerCount + 8,
            goodwillFailure = -lodgerCount - 8,
            rewardValueRange = new FloatRange(1200f, 1800f) * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor,
            questDurationTicks = Rand.RangeInclusive(6, 8) * 60000,

            fixedPawnKind = OARatkin_PawnGenerateDefOf.OA_RK_Traveller
        };
    }

    protected override void SetPawnsLeaveComp(QuestParameter questParameter, List<Pawn> pawns, string inSignalEnable, string inSignalRemovePawn)
    {
        Quest quest = questParameter.quest;

        string travelerCanLeaveNowSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.AllHealthy");
        QuestPart_WoundedTravelerCanLeaveNow questPart_WoundedTravelerCanLeaveNow = new()
        {
            inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
            outSignal = travelerCanLeaveNowSignal,
            map = questParameter.map,
        };
        questPart_WoundedTravelerCanLeaveNow.pawns.AddRange(pawns);
        quest.AddPart(questPart_WoundedTravelerCanLeaveNow);
        //提前离开
        quest.SignalPassWithFaction(questParameter.faction, null, delegate
        {
            quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersLeavingEarlyLetterText]", null, "[lodgersLeavingEarlyLetterLabel]");
        }, inSignal: travelerCanLeaveNowSignal);
        quest.Leave(pawns, travelerCanLeaveNowSignal, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn, wakeUp: true);

        base.SetPawnsLeaveComp(questParameter, pawns, inSignalEnable, inSignalRemovePawn);
    }

    protected override void SetQuestEndComp(QuestParameter questParameter, QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string bigFailSignal, string successSignal)
    {
        questParameter.quest.AddPart(new QuestPart_OaAssistPointsChange(successSignal, AssistPoints));
    }

    protected override void PostPawnGenerated(Pawn pawn)
    {
        if (curPawnIndex++ == 1)
        {
            pawn.health.AddHediff(OARatkin_PawnInfoDefOf.OA_RK_SeriousInjury);
            TakeNonLethalDamage(pawn, DamageDefOf.Blunt);
        }
    }

    private static void TakeNonLethalDamage(Pawn p, DamageDef fixedDamageDef = null) //造成不致命、不残疾的伤害
    {
        if (p.Downed)
        {
            return;
        }
        p.health.forceDowned = true;
        IEnumerable<BodyPartRecord> source = HittablePartsViolence(p);
        int num = 0;
        while (num < InjuryCount.RandomInRange)
        {
            num++;
            if (!source.Any())
            {
                break;
            }
            BodyPartRecord bodyPartRecord = source.RandomElement();
            float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
            float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
            int min = Mathf.Min(Mathf.RoundToInt(maxHealth * 0.3f), (int)partHealth - 1);
            int max = Mathf.Min(Mathf.RoundToInt(maxHealth * 0.8f), (int)partHealth - 1);
            int num2 = Rand.RangeInclusive(min, max);
            DamageDef damageDef = fixedDamageDef ?? HealthUtility.RandomViolenceDamageType();
            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
            if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, num2))
            {
                break;
            }
            DamageInfo dinfo = new(damageDef, num2, 999f, -1f, null, bodyPartRecord);
            dinfo.SetAllowDamagePropagation(val: false);
            p.TakeDamage(dinfo);
        }
        if (p.Dead)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(string.Concat(p, " died during GiveInjuriesToForceDowned"));
            for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
            {
                stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
            }
            Log.Error(stringBuilder.ToString());
        }
        p.health.forceDowned = false;
    }
    private static IEnumerable<BodyPartRecord> HittablePartsViolence(Pawn pawn)
    {
        HediffSet hediffSet = pawn.health.hediffSet;
        return from x in hediffSet.GetNotMissingParts()
               where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, hediffSet.hediffs))
               where !pawn.health.hediffSet.hediffs.Any(y => y.Part == x && y.CurStage is not null && y.CurStage.partEfficiencyOffset < 0f)
               select x;
    }

}
