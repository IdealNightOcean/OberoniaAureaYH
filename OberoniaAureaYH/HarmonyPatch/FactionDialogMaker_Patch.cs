using HarmonyLib;
using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
        if (!faction.IsOAFaction())
            return;

        if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            return;

        FactionDialogCache dialogCache = new(negotiator, faction);

        List<DiaOption> opts =
        [
            CallForLargeScaleTrader(dialogCache),
            SponsorOADialogMaker.SponsorOberoniaAurea(dialogCache),
            BuyTechDialogMaker.BuyTechPrint(dialogCache)
        ];

        if (ModsConfig.OdysseyActive)
        {
            opts.Add(ScienceDepartmentDialogUtility.AccessScienceDepartment(negotiator, faction));
        }

        int index = Mathf.Max(0, __result.options.Count - 3);
        __result.options.InsertRange(index, opts);
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


    /// <summary>
    /// 外交相关界面
    /// </summary>
    private static class DiplomacyDialogMaker
    {
        private static DiaNode FactionSelectionNode(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
        {
            string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
            DiaNode diaNode = new("OARK_MultiPartyTalks_FactionSelectionNode".Translate(selFactionsNames.Named("FactionNames")).CapitalizeFirst());
            Faction ofPlayer = Faction.OfPlayer;
            Faction oaFaction = ModUtility.OAFaction;

            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                if (faction.Hidden || faction.temporary || faction.def.permanentEnemy || faction.IsPlayerSafe())
                    continue;
                if (faction == ofPlayer || faction == oaFaction)
                    continue;


                DiaOption diaOption = new(faction.NameColored.ToString())
                {
                    linkLateBind = () => FactionSelectionNode(dialogCache, selFactions)
                };
                if (selFactions.Contains(faction))
                {
                    diaOption.action = () => selFactions.Remove(faction);
                }
                else
                {
                    diaOption.action = () => selFactions.Add(faction);
                    if (selFactions.Count >= 5)
                    {
                        diaOption.Disable("");
                    }
                }

                diaNode.options.Add(diaOption);
            }

            if (selFactions.Count >= 3)
            {
                DiaOption confirmOption = new("Confirm".Translate())
                {

                };
                diaNode.options.Add(confirmOption);
            }

            return diaNode;
        }

        private static DiaNode FactionConfirmNode(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
        {
            string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
            DiaNode diaNode = new("OARK_MultiPartyTalks_FactionConfirmNode".Translate(selFactionsNames.Named("FactionNames")).CapitalizeFirst());

            throw new NotImplementedException();

        }
    }
}