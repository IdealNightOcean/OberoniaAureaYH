using HarmonyLib;
using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

//通讯台界面Patch

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(FactionDialogMaker), "FactionDialogFor")]
public static class FactionDialogFor_Patch
{
    private static int amountSendableSilver = -1;
    [HarmonyPostfix]
    public static void Postfix(ref DiaNode __result, Pawn negotiator, Faction faction)
    {
        if (!faction.IsOAFaction())
        {

            return;
        }

        if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {

            return;
        }

        FactionDialogCache dialogCache = new(negotiator, faction);
        amountSendableSilver = OAFrame_MapUtility.AmountSendableSilver(dialogCache.Map);

        List<DiaOption> opts =
        [
            CallForLargeScaleTrader(dialogCache),
            SponsorOberoniaAurea(dialogCache),
            BuyTechPrint(dialogCache)
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
        if (!dialogCache.IsAlly)
        {
            diaOption.Disable("MustBeAlly".Translate());
            return diaOption;
        }

        int cooldownTicksLeft = OAInteractHandler.Instance.GetCooldownTicksLeft("LargeScaleTrader");
        if (dialogCache.AllianceDuration < 75)
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
            OAInteractHandler.Instance.RegisterCDRecord("LargeScaleTrader", cdTicks: 60 * 60000);
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
    /// 赞助金鼠
    /// </summary>
    private static DiaOption SponsorOberoniaAurea(FactionDialogCache dialogCache)
    {
        TaggedString taggedString = "OA_SponsorOberoniaAurea".Translate();
        DiaOption diaOption = new(taggedString);

        if (OAInteractHandler.Instance.IsInCooldown("SponsorOA")) //冷却时不需要更多的帮助
        {
            string cdConfirmStr = "OA_SponsorThanksAgain".Translate(dialogCache.Faction.leader).CapitalizeFirst();
            diaOption.link = new DiaNode(cdConfirmStr)
            {
                options = { FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator) }
            };
            return diaOption;
        }

        int amountSendableSilver = OAFrame_MapUtility.AmountSendableSilver(dialogCache.Map);
        //以下为二级界面按钮
        DiaNode diaNode = new(taggedString);
        AddDiaOption(500);
        AddDiaOption(1000);
        AddDiaOption(2000);
        AddDiaOption(5000);
        AddDiaOption(10000);
        AddDiaOption(20000);
        //返回按钮
        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator)
        });
        diaOption.link = diaNode;
        return diaOption;

        void AddDiaOption(int needSilver)
        {
            DiaOption diaOption = new("OA_DoSponsorOberoniaAurea".Translate(needSilver));
            if (amountSendableSilver < needSilver)
            {
                diaOption.Disable("NeedSilverLaunchable".Translate(needSilver));
            }
            else
            {
                diaOption.action = delegate
                {
                    DoSponsorOberoniaAurea(dialogCache, needSilver);
                };
                diaOption.linkLateBind = () => SponsorOberoniaAureaConfirmNode(dialogCache, needSilver);
            }

            diaNode.options.Add(diaOption);
        }
    }

    private static void DoSponsorOberoniaAurea(FactionDialogCache dialogCache, int silverCount)
    {
        int gainAP = silverCount / 250;
        int gainGTP = silverCount / 10;
        int gainFavor = silverCount / 1000;

        TradeUtility.LaunchSilver(dialogCache.Map, silverCount);
        OAInteractHandler.Instance.AdjustAssistPoints(gainAP);

        if (ModsConfig.OdysseyActive)
        {
            ScienceDepartmentInteractHandler.Instance?.AddGravTechPoints(gainGTP, byPlayer: true);
        }

        dialogCache.Negotiator.royalty?.GainFavor(dialogCache.Faction, gainFavor);
        OAInteractHandler.Instance.RegisterCDRecord("SponsorOA", cdTicks: 30 * 60000);
    }

    private static DiaNode SponsorOberoniaAureaConfirmNode(FactionDialogCache dialogCache, int silverCount)
    {
        int gainAP = silverCount / 250;
        int gainGTP = silverCount / 10;
        int gainFavor = silverCount / 1000;

        StringBuilder confirmSb = new("OA_SponsorThanks".Translate(dialogCache.Faction.leader).CapitalizeFirst());
        confirmSb.AppendLine();
        confirmSb.AppendInNewLine("OARK_AssistPointsAdded".Translate(gainAP));
        confirmSb.AppendInNewLine("OA_SponsorRoyaltyFavorGain".Translate(dialogCache.Negotiator.Named("NAME"), gainFavor));
        if (ModsConfig.OdysseyActive)
        {
            confirmSb.AppendInNewLine("OARK_ScienceDepartment_GravTechPointsAdded".Translate(gainGTP));
        }

        return FactionDialogUtility.FinallyConfirmNode(confirmSb.ToString(),
                                                       dialogCache.Faction,
                                                       dialogCache.Negotiator);
    }

    /// <summary>
    /// 购买蓝图
    /// </summary>
    private static DiaOption BuyTechPrint(FactionDialogCache dialogCache)
    {
        TaggedString taggedString = "OA_BuyTechPrint".Translate();
        DiaOption diaOption = new(taggedString);
        if (!dialogCache.IsAlly)
        {
            diaOption.Disable("MustBeAlly".Translate());
            return diaOption;
        }
        if (dialogCache.AllianceDuration < 45)
        {
            diaOption.Disable("OA_AllianceDurationShort".Translate(45.ToString("F0")));
            return diaOption;
        }
        int cooldownTicksLeft = OAInteractHandler.Instance.GetCooldownTicksLeft("BuyTechPrint");
        if (cooldownTicksLeft > 0)
        {
            diaOption.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
            return diaOption;
        }

        diaOption.linkLateBind = () => BuyTechPrintNode(dialogCache);

        return diaOption;
    }
    private static DiaNode BuyTechPrintNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OA_BuyTechPrintNode".Translate(dialogCache.Faction.leader).CapitalizeFirst());

        int negotiatorTitle = dialogCache.Negotiator.GetCurrentTitleSeniorityIn(dialogCache.Faction);

        IOrderedEnumerable<DiaBuyableTechPrintDef> allBuyableTechPrintDefs = DefDatabase<DiaBuyableTechPrintDef>.AllDefsListForReading.Where(d1 => !d1.onlyScienceDepartment).OrderBy(d2 => d2.price);
        foreach (DiaBuyableTechPrintDef item in allBuyableTechPrintDefs)
        {
            AddDiaOption(item);
        }

        //返回按钮
        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator)
        });

        return diaNode;


        void AddDiaOption(DiaBuyableTechPrintDef dbtpDef)
        {
            StringBuilder diaSb = new("OA_TechPrintChoice".Translate(dbtpDef.TechPrintDef.label, dbtpDef.price));
            if (dbtpDef.NeedAllianceDuration)
            {
                diaSb.AppendWithSeparator("OA_TechPrintNeedAllianceDuration".Translate(dbtpDef.allianceDuration), "丨");
            }
            if (dbtpDef.NeedRoyalTitle)
            {
                diaSb.AppendWithSeparator("OA_TechPrintNeedRoyalTitle".Translate(dbtpDef.royalTitleDef), "丨");
            }

            DiaOption diaOption = new(diaSb.ToString());
            if (amountSendableSilver < dbtpDef.price)
            {
                diaOption.linkLateBind = () => DeniedTechPrintDiaOption("NeedSilverLaunchable".Translate(dbtpDef.price), dialogCache);
            }
            else if (dbtpDef.NeedAllianceDuration && dbtpDef.allianceDuration > dialogCache.AllianceDuration)
            {
                diaOption.linkLateBind = () => DeniedTechPrintDiaOption("OA_AllianceDurationShort".Translate(dbtpDef.allianceDuration.ToString("F0")), dialogCache);
            }
            else if (dbtpDef.NeedRoyalTitle && dbtpDef.royalTitleDef.seniority > negotiatorTitle)
            {
                diaOption.linkLateBind = () => DeniedTechPrintDiaOption("OA_BuyTechPrintDeniedDueTitle".Translate(dialogCache.Negotiator.Named("NEGOTIATOR"), dbtpDef.royalTitleDef.GetLabelCapFor(dialogCache.Negotiator).Named("TITLE"), dialogCache.Faction.Named("FACTION")),
                                                                        dialogCache);
            }
            else
            {
                diaOption.action = delegate
                {
                    GetTechPrint(dialogCache.Map, dialogCache.Faction, dbtpDef.TechPrintDef, dbtpDef.price);
                    OAInteractHandler.Instance.RegisterCDRecord("BuyTechPrint", cdTicks: 30 * 60000);
                };
                diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode(text: "OA_TechPrintChoiceConfirm".Translate(dialogCache.Faction.leader).CapitalizeFirst(),
                                                                                       faction: dialogCache.Faction,
                                                                                       negotiator: dialogCache.Negotiator);
            }

            diaNode.options.Add(diaOption);
        }
    }

    private static void GetTechPrint(Map map, Faction faction, ThingDef tDef, int price)
    {
        TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, price, map, null);
        IntVec3 dropCell = OARK_DropPodUtility.DefaultDropSingleThingOfDef(tDef, map, faction);
        Messages.Message("OA_TechPrintArrive".Translate(faction.Named("FACTION"), tDef.label), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
    }

    private static DiaNode DeniedTechPrintDiaOption(string denStr, FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new(denStr);
        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => BuyTechPrintNode(dialogCache)
        });
        return diaNode;
    }
}