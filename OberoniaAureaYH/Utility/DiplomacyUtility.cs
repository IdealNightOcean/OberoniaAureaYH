using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public static class DiplomacyUtility
{
    /// <summary>
    /// 根据交互难度a和交互能力b，确定一般交互的结果。
    /// </summary>
    /// <param name="a">交互难度</param>
    /// <param name="b">交互能力</param>
    public static InteractionOutcome DetermineOutcomeCommon(float a, float b)
    {
        CalculateOutcomeProbCommon(a, b, out float criticalSuccessProb, out float successProb, out float normalProb, out float failureProb, out _);

        float rand = Random.value;
        if (rand < criticalSuccessProb)
            return InteractionOutcome.CriticalSuccess;

        rand -= criticalSuccessProb;
        if (rand < successProb)
            return InteractionOutcome.Success;

        rand -= successProb;
        if (rand < normalProb)
            return InteractionOutcome.Normal;

        rand -= normalProb;
        if (rand < failureProb)
            return InteractionOutcome.Failure;

        return InteractionOutcome.CriticalFailure;
    }

    public static TaggedString OutcomeProbabilityInfo(float a, float b)
    {
        CalculateOutcomeProbCommon(a, b, out float criticalSuccessProb, out float successProb, out float normalProb, out float failureProb, out float criticalFailureProb);
        return "OARK_InteractionOutcome_PropInfo".Translate(
            criticalSuccessProb.ToStringPercent("0.##").Named("CriticalSuccessProb"),
            successProb.ToStringPercent("0.##").Named("SuccessProb"),
            normalProb.ToStringPercent("0.##").Named("NormalProb"),
            failureProb.ToStringPercent("0.##").Named("FailureProb"),
            criticalFailureProb.ToStringPercent("0.##").Named("CriticalFailureProb"));
    }

    /// <summary>
    /// 计算一般交互的结果概率分布。a代表交互难度，b代表交互能力。
    /// </summary>
    /// <param name="a">交互难度</param>
    /// <param name="b">交互能力</param>
    public static void CalculateOutcomeProbCommon(float a, float b, out float criticalSuccessProb, out float successProb, out float normalProb, out float failureProb, out float criticalFailureProb)
    {
        float bUsed = Mathf.Clamp(b, 0.5f * a, 2f * a);

        float delta = Mathf.Clamp((b - a) / a * 0.4f, -0.25f, 0.25f);

        float t1 = 0.6f + delta;
        float t2 = 0.85f + delta;
        float t3 = 1.15f + delta;
        float t4 = 1.6f + delta;

        float lowerBound = 0.5f * a;
        float upperBound = 1.5f * a;
        float range = upperBound - lowerBound;

        criticalSuccessProb = Mathf.Max(0f, Mathf.Min(upperBound, t1 * bUsed) - lowerBound) / range;
        successProb = Mathf.Max(0f, Mathf.Min(upperBound, t2 * bUsed) - Mathf.Max(lowerBound, t1 * bUsed)) / range;
        normalProb = Mathf.Max(0f, Mathf.Min(upperBound, t3 * bUsed) - Mathf.Max(lowerBound, t2 * bUsed)) / range;
        failureProb = Mathf.Max(0f, Mathf.Min(upperBound, t4 * bUsed) - Mathf.Max(lowerBound, t3 * bUsed)) / range;
        criticalFailureProb = Mathf.Max(0f, upperBound - Mathf.Max(lowerBound, t4 * bUsed)) / range;
    }



}
