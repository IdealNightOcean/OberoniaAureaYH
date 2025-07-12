using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class FixedCaravan_ResearchSummitAssistWork : FixedCaravan
{
    protected static readonly List<Pair<Action, float>> tmpPossibleOutcomes = [];
    public const int CheckInterval = 5000;

    protected int allTicksRemaining = 25000;
    protected int checkRemaining = 5;
    public ResearchSummit_AssistWork assistWork;
    protected int workPoints;
    protected bool workEnd;

    public override void Tick()
    {
        base.Tick();
        allTicksRemaining--;
        ticksRemaining--;
        if (ticksRemaining <= 0)
        {
            if (assistWork is null)
            {
                FinishedWork();
                return;
            }
            checkRemaining--;
            CheckWork();
            ticksRemaining = CheckInterval;
        }
    }
    protected void CheckWork()
    {
        ConsumptionNeeds(PawnsListForReading);
        if (checkRemaining == 3)
        {
            SupplyFood(this);
        }
        GetPossibleOutcomes();
        if (checkRemaining <= 0 && !workEnd)
        {
            FinishedWork();
        }
    }
    protected void GetPossibleOutcomes()
    {
        tmpPossibleOutcomes.Clear();
        int capableCount = PawnsListForReading.Where(p => p.ageTracker.AgeBiologicalYears > 13).Count();

        float weight = 50f;
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_SmoothWork(capableCount);
        }, weight));

        weight = 20f + Faction.OfPlayer.GoodwillWith(OARatkin_MiscUtility.OAFaction) / 5f;
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_FriendlyCollaboration(capableCount);
        }, weight));

        weight = TradeDisputesSuccessWeight(PawnsListForReading);
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_TradeDisputesSuccess(capableCount);
        }, weight));

        weight = 15f;
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_TradeDisputesFail(capableCount);
        }, weight));

        weight = CausingArgumentWeight(PawnsListForReading);
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_CausingArgument(capableCount);
        }, weight));

        tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First();
    }
    protected override void PreConvertToCaravanByPlayer()
    {
        LeaveHalfway();
    }
    protected void GetReward(string label, string text, LetterDef letterDef) //获得奖励
    {
        int silverNum = Mathf.Max(1, workPoints * 10);
        int APpoints = Mathf.Clamp(Mathf.FloorToInt(workPoints * 0.1f), 0, 10);
        OARatkin_MiscUtility.OAGameComp?.GetAssistPoints(APpoints);
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(RimWorld.ThingDefOf.Silver, silverNum);
        foreach (Thing t in things)
        {
            OAFrame_FixedCaravanUtility.GiveThing(this, t);
        }
        string letterText = text.Translate() + "\n\n" + "OA_RSAssistWork_GetReward".Translate(silverNum, APpoints);
        Find.LetterStack.ReceiveLetter(label.Translate(), letterText, letterDef, assistWork, assistWork?.Faction);
    }
    public override void Notify_ConvertToCaravan()
    {
        workEnd = true;
        assistWork?.EndWork();
    }
    protected static void ConsumptionNeeds(List<Pawn> allPawns) //消耗饥饿/娱乐值
    {
        if (allPawns.NullOrEmpty())
        {
            return;
        }
        foreach (Pawn pawn in allPawns)
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
    protected static void SupplyFood(FixedCaravan_ResearchSummitAssistWork assistWorkCaravan) //给予当天的食物
    {
        ThingDef foodDef = Rand.Bool ? ThingDefOf.MealFine : OARatkin_ThingDefOf.Oberonia_Aurea_Chanwu_AB;
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(foodDef, assistWorkCaravan.PawnsCount);
        foreach (Thing t in things)
        {
            OAFrame_FixedCaravanUtility.GiveThing(assistWorkCaravan, t);
        }
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

    public void LeaveHalfway() //主动触发ConvertToCaravan()前触发
    {
        GetReward("OA_LetterLabelRSAssistWork_LeaveHalfway", "OA_LetterRSAssistWork_LeaveHalfway", LetterDefOf.NeutralEvent);
    }
    public void FinishedWork()
    {
        GetReward("OA_LetterLabelRSAssistWork_FinishedWork", "OA_LetterRSAssistWork_FinishedWork", LetterDefOf.PositiveEvent);
        OAFrame_FixedCaravanUtility.ConvertToCaravan(this);
    }
    protected void Outcome_CausingArgument(int capableCount)
    {
        workPoints += capableCount;
        GetReward("OA_LetterLabelRSAssistWork_CausingArgument", "OA_LetterRSAssistWork_CausingArgument", LetterDefOf.NegativeEvent);
        OAFrame_FixedCaravanUtility.ConvertToCaravan(this);
    }

    protected void Outcome_SmoothWork(int capableCount)
    {
        Messages.Message("OA_FixedCaravanRSAssistWork_SmoothWork".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
        workPoints += capableCount * 2;
    }
    protected void Outcome_FriendlyCollaboration(int capableCount)
    {
        Messages.Message("OA_FixedCaravanRSAssistWork_FriendlyCollaboration".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
        workPoints += capableCount * 3;
    }
    protected void Outcome_TradeDisputesSuccess(int capableCount)
    {
        Faction oaFaction = OARatkin_MiscUtility.OAFaction;
        if (oaFaction is not null)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(oaFaction, 6, canSendMessage: false, canSendHostilityLetter: false, OARatkin_HistoryEventDefOf.OA_ResearchSummit);
        }
        workPoints += (capableCount * 4 + 6);
        Messages.Message("OA_FixedCaravanRSAssistWork_TradeDisputesSuccess".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
    }
    protected void Outcome_TradeDisputesFail(int capableCount)
    {
        workPoints += capableCount;
        Messages.Message("OA_FixedCaravanRSAssistWork_TradeDisputesFail".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
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

    public override string GetInspectString()
    {
        StringBuilder stringBuilder = new(base.GetInspectString());
        stringBuilder.AppendInNewLine("OA_FixedCaravanRSAssistWork_TimeLeft".Translate(allTicksRemaining.ToStringTicksToPeriod()));
        stringBuilder.AppendInNewLine("OA_FixedCaravanRSAssistWork_workPoints".Translate(workPoints));
        return stringBuilder.ToString();
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref allTicksRemaining, "allTicksRemaining", 25000);
        Scribe_Values.Look(ref checkRemaining, "checkRemaining", 5);
        Scribe_Values.Look(ref workPoints, "workPoints", 0);
        Scribe_References.Look(ref assistWork, "assistWork");
    }
}