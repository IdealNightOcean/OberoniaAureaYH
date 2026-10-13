using OberoniaAurea_Frame;

using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public static class MultiPartyTalksUtility
{
    public static bool CanTriggerTopicEvent(MultiPartyTalksTopicEventDef eventDef, MultiPartyTalksTopicEventDef.EventType eventType, bool isSuccessful, MultiPartyTalksTopicEventData eventData)
    {
        if (eventDef is null || (eventDef.eventType & eventType) == 0)
            return false;

        if (isSuccessful && !eventDef.allowSucceedTopic)
            return false;
        if (!isSuccessful && !eventDef.allowFailedTopic)
            return false;

        return eventDef.Worker?.CanTriggerEvent(eventData) ?? false;
    }

    public static IEnumerable<Pawn> GetCaravanPawns(MultiPartyTalksTopicEventData eventData)
    {
        FixedCaravan fixedCaravan = eventData.TalksSite?.AssociatedFixedCaravan;
        if (fixedCaravan is not null && fixedCaravan.Spawned)
        {
            return fixedCaravan.PawnsListForReading;
        }

        Caravan caravan = ModUtility.GetPlayerCaravanOnTile(eventData.TalksSiteTileCached);
        if (caravan is not null && caravan.Spawned)
        {
            return caravan.PawnsListForReading;
        }

        return [eventData.Negotiator];
    }

    public static void TryGiveThingsToPlayer(MultiPartyTalksTopicEventData eventData, IEnumerable<Thing> things)
    {
        OAFrame_FixedCaravanUtility.TryGiveThingsToPlayer(eventData.TalksSite, things, eventData.TalksSiteTileCached);
    }


    /// <summary>
    /// 多发会谈任务谈判能力需求基线计算方法。基线值默认为0，参与派系的科技水平、意识形态、与玩家的关系等因素会对基线进行正负调整。
    /// </summary>
    public static float CalculateBaselineNegotiationAbility(IEnumerable<Faction> participantFactions, bool resultOnly, out string explanation)
    {
        explanation = string.Empty;
        StringBuilder expSB = resultOnly ? null : new StringBuilder(512);
        float abilityBaseline = 0f;

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
            float ratkinBonus = -ratkinFactionCount * 0.05f;
            abilityBaseline += ratkinBonus;
            expSB.AppendLine("OARK_MultiPartyTalks_Baseline_RatkinFactions".Translate(ratkinFactionCount,
                                                                                      OAFrame_TextUtility.ColoredPercentNamedArgument(ratkinBonus, KeyLibrary_NamedArgument.Change, includeSign: true, reverse: true))
                                                                           .Resolve());
        }

        abilityBaseline -= 0.5f;
        if (!resultOnly)
        {
            expSB.AppendLine("OARK_MultiPartyTalks_Baseline_OABase".Translate(oaFaction.Named(KeyLibrary_NamedArgument.FACTION),
                                                                              OAFrame_TextUtility.ColoredPercentNamedArgument(-0.5f, KeyLibrary_NamedArgument.Change, includeSign: true, reverse: true))
                                                                   .Resolve());
        }
        float allianceDurationModify = CalculateOATotalAllianceDurationModify();
        if (allianceDurationModify != 0f)
        {
            abilityBaseline += allianceDurationModify;
            if (!resultOnly)
            {
                expSB.AppendLine("OARK_MultiPartyTalks_Baseline_AllianceDuration".Translate(oaFaction.Named(KeyLibrary_NamedArgument.FACTION),
                                                                                            OAFrame_TextUtility.ColoredPercentNamedArgument(allianceDurationModify, KeyLibrary_NamedArgument.Change, includeSign: true, reverse: true))
                                                                                 .Resolve());
            }
        }

        abilityBaseline = Mathf.Max(0.5f, abilityBaseline);

        if (!resultOnly)
        {
            expSB.AppendLine("OARK_MultiPartyTalks_Baseline_Final".Translate(abilityBaseline.ToStringPercentSigned("0.##").Colorize(Color.yellow))
                                                                  .Resolve());
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
                grammarRequest.Rules.Add(new Rule_String(nameof(techAndMemeModify), techAndMemeModify.ToStringPercentSigned("0.##").Colorize(techAndMemeModify < 0f ? Color.green : ColorLibrary.RedReadable)));
                grammarRequest.Rules.Add(new Rule_String(nameof(relationModify), relationModify.ToStringPercentSigned("0.##").Colorize(relationModify < 0f ? Color.green : ColorLibrary.RedReadable)));
                grammarRequest.Rules.Add(new Rule_String(nameof(permanentEnemyModify), permanentEnemyModify.ToStringPercentSigned("0.##").Colorize(permanentEnemyModify < 0f ? Color.green : ColorLibrary.RedReadable)));

                grammarRequest.Constants.Add(nameof(techAndMemeModify), techAndMemeModify.ToString("0.###"));
                grammarRequest.Constants.Add(nameof(relationModify), relationModify.ToString("0.###"));
                grammarRequest.Constants.Add(nameof(permanentEnemyModify), permanentEnemyModify.ToString("0.###"));
                expSB.AppendLine(GrammarResolver.Resolve("r_text", grammarRequest));
            }
        }
    }


    private static readonly (string, float)[] MemeModifiesArr =
    [
        ("OAGene_TerritorialHegemony", 0.15f), //OA - 领地霸权
        ("OA_RK_meme_Friendly", -0.1f), //OA - 和谐团体
        ("HAR_Xenophilia", -0.05f), //外星人 - 异族亲和
        ("HAR_Xenophobia", 0.1f), //外星人 - 异族摈斥
        ("Rancher", -0.05f), //大牧场主
        ("Loyalist", 0.05f), //赤胆忠心
        ("Guilty", -0.05f), //原罪主义
        ("Raider", 0.20f), //劫掠成性
        ("PainIsVirtue", -0.05f) //苦痛美德
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
