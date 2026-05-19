using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public static class MultiPartyTalksUtility
{
    /// <summary>
    /// 根据交互难度a和交互能力b，确定一般交互的结果。
    /// </summary>
    /// <param name="a">交互难度</param>
    /// <param name="b">交互能力</param>
    public static InteractionOutcome DetermineOutcomeCommon(float a, float b)
    {
        CalculateOutcomeProbCommon(a, b, out float criticalSuccessProb, out float successProb, out float normalProb, out float failureProb, out float criticalFailureProb);

        float rand = Random.value;
        if (rand < criticalSuccessProb)
        {
            return InteractionOutcome.CriticalSuccess;
        }
        rand -= criticalSuccessProb;
        if (rand < successProb)
        {
            return InteractionOutcome.Success;
        }
        rand -= successProb;
        if (rand < normalProb)
        {
            return InteractionOutcome.Normal;
        }
        rand -= normalProb;
        if (rand < failureProb)
        {
            return InteractionOutcome.Failure;
        }
        return InteractionOutcome.CriticalFailure;
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

        float t1 = 0.60f + delta;
        float t2 = 0.85f + delta;
        float t3 = 1.15f + delta;
        float t4 = 1.60f + delta;

        float lowerBound = 0.5f * a;
        float upperBound = 1.5f * a;

        criticalSuccessProb = Mathf.Max(0f, Mathf.Min(upperBound, t1 * bUsed) - lowerBound) / a;
        successProb = Mathf.Max(0f, Mathf.Min(upperBound, t2 * bUsed) - Mathf.Max(lowerBound, t1 * bUsed)) / a;
        normalProb = Mathf.Max(0f, Mathf.Min(upperBound, t3 * bUsed) - Mathf.Max(lowerBound, t2 * bUsed)) / a;
        failureProb = Mathf.Max(0f, Mathf.Min(upperBound, t4 * bUsed) - Mathf.Max(lowerBound, t3 * bUsed)) / a;
        criticalFailureProb = Mathf.Max(0f, upperBound - Mathf.Max(lowerBound, t4 * bUsed)) / a;
    }

    /// <summary>
    /// 多发会谈任务谈判能力需求基线计算方法。基线值默认为1，参与派系的科技水平、意识形态、与玩家的关系等因素会对基线进行正负调整。
    /// </summary>
    public static float CalculateBaselineNegotiationAbility(IEnumerable<Faction> participantFactions, bool resultOnly, out string explanation)
    {
        explanation = string.Empty;
        StringBuilder expSB = resultOnly ? null : new StringBuilder(512);
        float abilityBaseline = 0.5f;

        Faction oaFaction = ModUtility.OAFaction;

        int ratkinFactionCount = 0;
        foreach (Faction faction in participantFactions)
        {
            if (faction.IsRatkinFaction())
            {
                ratkinFactionCount++;
            }
            PerFactionModification(faction);
        }

        if (ratkinFactionCount > 0 && !resultOnly)
        {
            float ratkinBonus = ratkinFactionCount * 0.05f;
            expSB.AppendLine("OARK_MultiPartyTalks_Baseline_RatkinFactions".Translate(ratkinFactionCount, ratkinBonus.ToStringPercentSigned("0.##").Colorize(Color.green)));
        }

        abilityBaseline -= 0.5f;
        if (!resultOnly)
        {
            expSB.AppendLine("OARK_MultiPartyTalks_Baseline_OABase".Translate(oaFaction.Named(KeyLibrary_NamedArgument.FACTION), (-0.5f).ToStringPercentSigned("0.##").Colorize(Color.green)));
        }
        float allianceDurationModify = CalculateOATotalAllianceDurationModify();
        if (allianceDurationModify != 0f)
        {
            abilityBaseline += allianceDurationModify;
            if (!resultOnly)
            {
                expSB.AppendLine("OARK_MultiPartyTalks_Baseline_AllianceDuration".Translate(oaFaction.Named(KeyLibrary_NamedArgument.FACTION), allianceDurationModify.ToStringPercentSigned("0.##").Colorize(Color.green)));
            }
        }

        abilityBaseline = Mathf.Max(0.5f, abilityBaseline);

        if (!resultOnly)
        {
            expSB.AppendLine("OARK_MultiPartyTalks_Baseline_Final".Translate(abilityBaseline.ToStringPercentSigned("0.##")));
            explanation = expSB.ToString();
        }

        return abilityBaseline;

        void PerFactionModification(Faction f)
        {
            if (f is null || f.IsPlayerSafe())
                return;

            float techLevelModifier = f.def.techLevel switch
            {
                TechLevel.Neolithic => 0.25f,
                TechLevel.Medieval => 0.30f,
                TechLevel.Industrial => 0.35f,
                TechLevel.Spacer => 0.50f,
                TechLevel.Ultra => 0.75f,
                TechLevel.Archotech => 0.75f,
                _ => 0f
            };
            float memeModifier = ModsConfig.IdeologyActive ? GetMemeModifier(f) : 0f;
            float techAndMemeModify = techLevelModifier + memeModifier;

            float permanentEnemyModify = f.def.permanentEnemy ? 0.35f : 0f;
            float relationModify = f.PlayerRelationKind switch
            {
                FactionRelationKind.Ally => -0.15f,
                FactionRelationKind.Hostile => 0.25f,
                _ => 0f
            };

            float totalModify = techAndMemeModify + relationModify + permanentEnemyModify;
            abilityBaseline += totalModify;

            if (!resultOnly)
            {
                GrammarRequest grammarRequest = new();
                grammarRequest.Includes.Add(OARK_RulePackDef.OARK_MultiPartyTalks_BaselineText);

                grammarRequest.Rules.AddRange(GrammarUtility.RulesForFaction(KeyLibrary_NamedArgument.FACTION, f));
                grammarRequest.Rules.Add(new Rule_String("techAndMemeModify", techAndMemeModify.ToStringPercentSigned("0.##").Colorize(techAndMemeModify < 0f ? Color.green : ColorLibrary.RedReadable)));
                grammarRequest.Rules.Add(new Rule_String("relationModify", relationModify.ToStringPercentSigned("0.##").Colorize(relationModify < 0f ? Color.green : ColorLibrary.RedReadable)));
                grammarRequest.Rules.Add(new Rule_String("permanentEnemyModify", permanentEnemyModify.ToStringPercentSigned("0.##").Colorize(permanentEnemyModify < 0f ? Color.green : ColorLibrary.RedReadable)));

                grammarRequest.Constants.Add("techAndMemeModify", techAndMemeModify.ToString("0.###"));
                grammarRequest.Constants.Add("relationModify", relationModify.ToString("0.###"));
                grammarRequest.Constants.Add("permanentEnemyModify", permanentEnemyModify.ToString("0.###"));
                expSB.AppendLine(GrammarResolver.Resolve("r_text", grammarRequest));
            }
        }
    }


    private static readonly (string, float)[] MemeModifiesArr =
    [
        ("Rancher", -0.05f),
        ("Loyalist", 0.05f),
        ("Guilty", -0.05f),
        ("Raider", 0.20f),
        ("PainIsVirtue", -0.05f)
    ];

    private static float GetMemeModifier(Faction faction)
    {
        if (faction is null || faction.ideos is null)
            return 0f;

        Ideo primaryIdeo = faction.ideos.PrimaryIdeo;
        if (primaryIdeo is null)
            return 0f;

        float modifier = 0f;
        HashSet<string> allMemeDefNames = primaryIdeo.memes.Select(m => m.defName).ToHashSet();

        foreach ((string memeDefName, float modify) in MemeModifiesArr)
        {
            if (allMemeDefNames.Contains(memeDefName))
            {
                modifier += modify;
            }
        }

        return modifier;
    }

    private static float CalculateOATotalAllianceDurationModify()
    {
        Faction oaFaction = ModUtility.OAFaction;
        if (oaFaction is null || oaFaction.PlayerRelationKind != FactionRelationKind.Ally)
            return 0f;

        float allianceDurationDays = OAInteractHandler.Instance.AllianceDuration();
        if (allianceDurationDays < 0f)
            return 0f;

        float yearsSinceAlliance = allianceDurationDays / 60f;
        return -yearsSinceAlliance * 0.025f;
    }
}
