using OberoniaAurea_Frame;
using RimWorld;
using System.Linq;
using System.Text;
using Verse;

namespace OberoniaAurea;

/// <summary>
/// 购买蓝图通讯台界面
/// </summary>
internal static class BuyTechDialogMaker
{
    /// <summary>
    /// 购买蓝图
    /// </summary>
    internal static DiaOption BuyTechPrint(FactionDialogCache dialogCache)
    {
        TaggedString taggedString = "OA_BuyTechPrint".Translate();
        DiaOption diaOption = new(taggedString);
        if (dialogCache.PlayerRelationKind != FactionRelationKind.Ally)
        {
            diaOption.Disable("MustBeAlly".Translate());
            return diaOption;
        }
        if (dialogCache.TotalAllianceDuration < 45)
        {
            diaOption.Disable("OA_AllianceDurationShort".Translate(45.ToString("F0")));
            return diaOption;
        }
        int cooldownTicksLeft = ModUtility.CooldownManager.GetCooldownTicksLeft(KeyLibrary_CDRecord.BuyTechPrint);
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
            if (dialogCache.AmountSendableSilver < dbtpDef.price)
            {
                diaOption.linkLateBind = () => DeniedTechPrintDiaOption("NeedSilverLaunchable".Translate(dbtpDef.price), dialogCache);
            }
            else if (dbtpDef.NeedAllianceDuration && dbtpDef.allianceDuration > dialogCache.TotalAllianceDuration)
            {
                diaOption.linkLateBind = () => DeniedTechPrintDiaOption("OA_AllianceDurationShort".Translate(dbtpDef.allianceDuration.ToString("F0")), dialogCache);
            }
            else if (dbtpDef.NeedRoyalTitle && dbtpDef.royalTitleDef.seniority > negotiatorTitle)
            {
                diaOption.linkLateBind = () => DeniedTechPrintDiaOption("OA_BuyTechPrintDeniedDueTitle".Translate(dialogCache.Negotiator.Named("NEGOTIATOR"), dbtpDef.royalTitleDef.GetLabelCapFor(dialogCache.Negotiator).Named("TITLE"), dialogCache.Faction.Named(KeyLibrary_NamedArgument.FACTION)),
                                                                        dialogCache);
            }
            else
            {
                diaOption.action = delegate
                {
                    GetTechPrint(dialogCache.Map, dialogCache.Faction, dbtpDef.TechPrintDef, dbtpDef.price);
                    ModUtility.CooldownManager.RegisterRecord(KeyLibrary_CDRecord.BuyTechPrint, cdTicks: 30 * 60000, removeWhenExpired: true);
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
        IntVec3 dropCell = OAFrame_DropPodUtility.DefaultDropSingleThingOfDef(tDef, map, faction);
        Messages.Message("OA_TechPrintArrive".Translate(faction.Named(KeyLibrary_NamedArgument.FACTION), tDef.label), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
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
