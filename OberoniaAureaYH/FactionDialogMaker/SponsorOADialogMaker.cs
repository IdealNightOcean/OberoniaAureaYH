using OberoniaAurea_Frame.Utility;
using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

/// <summary>
/// 赞助金鼠通讯台界面
/// </summary>
internal static class SponsorOADialogMaker
{
    /// <summary>
    /// 赞助金鼠
    /// </summary>
    internal static DiaOption SponsorOberoniaAurea(FactionDialogCache dialogCache)
    {
        TaggedString taggedString = "OA_SponsorOberoniaAurea".Translate();
        DiaOption diaOption = new(taggedString);

        if (ModUtility.CooldownManager.IsInCooldown(KeyLibrary_CDRecord.SponsorOA)) //冷却时不需要更多的帮助
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
        ModUtility.CooldownManager.RegisterRecord(KeyLibrary_CDRecord.SponsorOA, cdTicks: 30 * 60000, removeWhenExpired: true);
    }

    private static DiaNode SponsorOberoniaAureaConfirmNode(FactionDialogCache dialogCache, int silverCount)
    {
        int gainAP = silverCount / 250;
        int gainGTP = silverCount / 10;
        int gainFavor = silverCount / 1000;

        StringBuilder confirmSb = new("OA_SponsorThanks".Translate(dialogCache.Faction.leader).CapitalizeFirst());
        confirmSb.AppendLine();
        confirmSb.AppendInNewLine("OARK_AssistPointsAdded".Translate(gainAP));
        confirmSb.AppendInNewLine("OA_SponsorRoyaltyFavorGain".Translate(dialogCache.Negotiator.Named(KeyLibrary_NamedArgument.PAWN), gainFavor.Named(KeyLibrary_NamedArgument.Count)));
        if (ModsConfig.OdysseyActive)
        {
            confirmSb.AppendInNewLine("OARK_ScienceDepartment_GravTechPointsAdded".Translate(gainGTP));
        }

        return FactionDialogUtility.FinallyConfirmNode(confirmSb.ToString(),
                                                       dialogCache.Faction,
                                                       dialogCache.Negotiator);
    }
}
