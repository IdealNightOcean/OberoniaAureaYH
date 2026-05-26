using HarmonyLib;
using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

//通讯台界面Patch

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(FactionDialogMaker), nameof(FactionDialogMaker.FactionDialogFor))]
public static class FactionDialogFor_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref DiaNode __result, Pawn negotiator, Faction faction)
    {
        if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            return;

        FactionDialogCache dialogCache = new(negotiator, faction);
        List<DiaOption> opts = [];
        if (faction.IsOAFaction())
        {
            opts.AddRange(OAFactionPatch(dialogCache));
        }

        opts.AddRange(CommonFactionPatch(dialogCache));

        if (opts.Count <= 0)
            return;

        int index = Mathf.Max(0, __result.options.Count - 3);
        __result.options.InsertRange(index, opts);
    }


    private static IEnumerable<DiaOption> OAFactionPatch(FactionDialogCache dialogCache)
    {
        yield return CallForLargeScaleTrader(dialogCache);
        yield return SponsorOADialogMaker.SponsorOberoniaAurea(dialogCache);
        yield return BuyTechDialogMaker.BuyTechPrint(dialogCache);
        yield return DiplomacyDialogMaker.Diplomacy(dialogCache);

        if (ModsConfig.OdysseyActive)
        {
            yield return ScienceDepartmentDialogUtility.AccessScienceDepartment(dialogCache.Negotiator, dialogCache.Faction);
        }
    }

    private static IEnumerable<DiaOption> CommonFactionPatch(FactionDialogCache dialogCache)
    {
        DiaOption intelligenceOpt = IntelligenceDialogMaker.IntelligenceOpt(dialogCache);
        if (intelligenceOpt is not null)
            yield return intelligenceOpt;
    }

    /// <summary>
    /// 大规模贸易
    /// </summary>
    private static DiaOption CallForLargeScaleTrader(FactionDialogCache dialogCache)
    {
        TaggedString taggedString = "OA_CallForLargeScaleTrade".Translate();
        DiaOption diaOption = new(taggedString);
        if (dialogCache.PlayerRelationKind != FactionRelationKind.Ally)
        {
            diaOption.Disable("MustBeAlly".Translate());
            return diaOption;
        }

        int cooldownTicksLeft = ModUtility.CooldownManager.GetCooldownTicksLeft(KeyLibrary_CDRecord.LargeScaleTrader);
        if (dialogCache.TotalAllianceDuration < 75)
        {
            diaOption.Disable("OA_AllianceDurationShort".Translate(75.ToString("F0")));
            return diaOption;
        }
        if (cooldownTicksLeft > 0)
        {
            diaOption.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
            return diaOption;
        }

        if (OAFrame_MapUtility.AmountSendableSilver(dialogCache.Map) < 2000)
        {
            diaOption.Disable("NeedSilverLaunchable".Translate(2000));
            return diaOption;
        }

        diaOption.action = CallTrader;
        diaOption.link = new DiaNode("OA_CallForLargeScaleTradeConfirm".Translate(dialogCache.Faction.leader).CapitalizeFirst())
        {
            options = { FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator) }
        };

        return diaOption;

        void CallTrader()
        {
            ModUtility.CooldownManager.RegisterRecord(KeyLibrary_CDRecord.LargeScaleTrader, cdTicks: 60 * 60000, removeWhenExpired: true);
            TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed("OA_RK_Caravan_TraderGeneral_B");
            IncidentParms parms = new()
            {
                target = dialogCache.Map,
                faction = dialogCache.Faction,
                traderKind = traderKind,
                forced = true
            };
            OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_LargeScaleTraderArrival, 120000, parms, 12000);
            TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, 2000, dialogCache.Map, null);
        }
    }
}