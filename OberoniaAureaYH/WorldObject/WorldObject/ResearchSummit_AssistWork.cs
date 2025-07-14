using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea;
public class ResearchSummit_AssistWork : WorldObject_InteractWithFixedCarvanBase
{
    private static readonly List<(Action, float)> tmpPossibleOutcomes = [];

    private int cycleRemaining = 5;
    private int allTicksRemaining = 25000;
    public override int TicksNeeded => 5000;

    private int workPoints;
    public int lastWorkTick = -1;
    private int RemainingCoolingTick
    {
        get
        {
            if (lastWorkTick < 0)
            {
                return -1;
            }
            return lastWorkTick + 60000 - Find.TickManager.TicksGame;
        }
    }


    public override IEnumerable<FloatMenuOption> GetSpecificFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this, CanVisit()))
        {
            yield return floatMenuOption;
        }
    }

    public override void Notify_CaravanArrived(Caravan caravan)
    {
        if (RemainingCoolingTick > 0)
        {
            return;
        }
        base.Notify_CaravanArrived(caravan);
    }

    public override bool StartWork(Caravan caravan)
    {
        if (base.StartWork(caravan))
        {
            lastWorkTick = Find.TickManager.TicksGame;
            allTicksRemaining = 25000;
            cycleRemaining = 5;
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void WorkTick()
    {
        allTicksRemaining--;
        if (--ticksRemaining <= 0)
        {
            if (associatedFixedCaravan is null)
            {
                EndWork(interrupt: true, convertToCaravan: false);
                return;
            }
            ticksRemaining = TicksNeeded;
            CheckWork();
        }
    }

    protected override void FinishWork()
    {
        GetReward("OA_LetterLabelRSAssistWork_FinishedWork", "OA_LetterRSAssistWork_FinishedWork", LetterDefOf.PositiveEvent);
    }

    protected override void InterruptWork() { }

    public override void PreConvertToCaravanByPlayer()
    {
        GetReward("OA_LetterLabelRSAssistWork_LeaveHalfway", "OA_LetterRSAssistWork_LeaveHalfway", LetterDefOf.NeutralEvent);
        base.PreConvertToCaravanByPlayer();
    }

    public override string FixedCaravanWorkDesc()
    {
        TaggedString timeLeft = "OA_FixedCaravanRSAssistWork_TimeLeft".Translate(allTicksRemaining.ToStringTicksToPeriod());
        TaggedString workPoints = "OA_FixedCaravanRSAssistWork_workPoints".Translate(workPoints);

        return string.Join(Environment.NewLine, timeLeft, workPoints);
    }

    private void CheckWork()
    {
        ConsumptionNeeds(associatedFixedCaravan);
        cycleRemaining--;
        if (cycleRemaining == 3)
        {
            SupplyFood(associatedFixedCaravan);
        }
        GetPossibleOutcomes();
        if (cycleRemaining == 0 && isWorking)
        {
            EndWork(interrupt: false, convertToCaravan: true);
        }
    }

    private void GetPossibleOutcomes()
    {
        tmpPossibleOutcomes.Clear();
        List<Pawn> pawnsListForReading = associatedFixedCaravan.PawnsListForReading;
        int capableCount = pawnsListForReading.Where(p => p.ageTracker.AgeBiologicalYears > 13).Count();

        float weight = 50f;
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_SmoothWork(capableCount);
        }, weight));

        weight = 20f + Faction.OfPlayer.GoodwillWith(OARatkin_MiscUtility.OAFaction) / 5f;
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_FriendlyCollaboration(capableCount);
        }, weight));

        weight = TradeDisputesSuccessWeight(pawnsListForReading);
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_TradeDisputesSuccess(capableCount);
        }, weight));

        weight = 15f;
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_TradeDisputesFail(capableCount);
        }, weight));

        weight = CausingArgumentWeight(pawnsListForReading);
        tmpPossibleOutcomes.Add((delegate
        {
            Outcome_CausingArgument(capableCount);
        }, weight));

        tmpPossibleOutcomes.RandomElementByWeight(x => x.Item2).Item1();
    }
    private static void ConsumptionNeeds(FixedCaravan assistWorkCaravan) //消耗饥饿/娱乐值
    {
        foreach (Pawn pawn in assistWorkCaravan.PawnsListForReading)
        {
            Need_Food need_Food = pawn.needs?.food;
            if (need_Food is not null)
            {
                need_Food.CurLevel -= 0.1f;
            }
            Need_Joy need_Joy = pawn.needs?.joy;
            if (need_Joy is not null)
            {
                need_Joy.CurLevel -= 0.1f;
            }
        }
    }

    private static void SupplyFood(FixedCaravan assistWorkCaravan) //给予当天的食物
    {
        ThingDef foodDef = Rand.Bool ? ThingDefOf.MealFine : OARatkin_ThingDefOf.Oberonia_Aurea_Chanwu_AB;
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(foodDef, assistWorkCaravan.PawnsCount);
        OAFrame_FixedCaravanUtility.GiveThings(assistWorkCaravan, things);
        foreach (Pawn pawn in assistWorkCaravan.PawnsListForReading)
        {
            Need_Food need_Food = pawn.needs?.food;
            if (need_Food is not null)
            {
                need_Food.CurLevel += 0.5f;
            }
            Need_Joy need_Joy = pawn.needs?.joy;
            if (need_Joy is not null)
            {
                need_Joy.CurLevel += 0.3f;
            }
        }
    }

    private void Outcome_CausingArgument(int capableCount)
    {
        workPoints += capableCount;
        GetReward("OA_LetterLabelRSAssistWork_CausingArgument", "OA_LetterRSAssistWork_CausingArgument", LetterDefOf.NegativeEvent);
        EndWork(interrupt: true, convertToCaravan: true);
    }

    private void Outcome_SmoothWork(int capableCount)
    {
        Messages.Message("OA_FixedCaravanRSAssistWork_SmoothWork".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
        workPoints += capableCount * 2;
    }
    private void Outcome_FriendlyCollaboration(int capableCount)
    {
        Messages.Message("OA_FixedCaravanRSAssistWork_FriendlyCollaboration".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
        workPoints += capableCount * 3;
    }
    private void Outcome_TradeDisputesSuccess(int capableCount)
    {
        Faction oaFaction = OARatkin_MiscUtility.OAFaction;
        if (oaFaction is not null)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(oaFaction, 6, canSendMessage: false, canSendHostilityLetter: false, OARatkin_HistoryEventDefOf.OA_ResearchSummit);
        }
        workPoints += (capableCount * 4 + 6);
        Messages.Message("OA_FixedCaravanRSAssistWork_TradeDisputesSuccess".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
    }
    private void Outcome_TradeDisputesFail(int capableCount)
    {
        workPoints += capableCount;
        Messages.Message("OA_FixedCaravanRSAssistWork_TradeDisputesFail".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
    }

    private void GetReward(string label, string text, LetterDef letterDef) //获得奖励
    {
        int silverNum = Mathf.Max(1, workPoints * 10);
        int APpoints = Mathf.Clamp(Mathf.FloorToInt(workPoints * 0.1f), 0, 10);
        OARatkin_MiscUtility.OAGameComp?.GetAssistPoints(APpoints);
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.Silver, silverNum);
        OAFrame_FixedCaravanUtility.GiveThings(associatedFixedCaravan, things);
        string letterText = text.Translate() + "\n\n" + "OA_RSAssistWork_GetReward".Translate(silverNum, APpoints);
        Find.LetterStack.ReceiveLetter(label.Translate(), letterText, letterDef, this, Faction);
    }

    private FloatMenuAcceptanceReport CanVisit()
    {
        if (!Spawned)
        {
            return false;
        }
        int remainingCoolingTick = RemainingCoolingTick;
        if (remainingCoolingTick > 0)
        {
            return FloatMenuAcceptanceReport.WithFailReason("WaitTime".Translate(remainingCoolingTick.ToStringTicksToPeriod()));
        }
        return true;
    }

    private static float TradeDisputesSuccessWeight(List<Pawn> allPawns) //买卖争执成功权重
    {
        int totalSkill = 0;
        int curSkill;
        int bestSkill = 0;
        foreach (Pawn p in allPawns)
        {
            curSkill = p.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
            totalSkill += curSkill;
            if (curSkill > bestSkill)
            {
                bestSkill = curSkill;
            }
        }
        return 8f + totalSkill * 0.25f + bestSkill;
    }

    private static float CausingArgumentWeight(List<Pawn> allPawns) //引发口角权重
    {
        int totalNum = 0;
        int curSkill;
        foreach (Pawn p in allPawns)
        {
            curSkill = p.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
            if (curSkill < 5)
            {
                totalNum++;
            }
        }
        return 5f + totalNum * 5f;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref allTicksRemaining, "allTicksRemaining", 25000);
        Scribe_Values.Look(ref cycleRemaining, "cycleRemaining", 5);
        Scribe_Values.Look(ref workPoints, "workPoints", 0);
        Scribe_Values.Look(ref lastWorkTick, "lastWorkTick", -1);
    }
}
