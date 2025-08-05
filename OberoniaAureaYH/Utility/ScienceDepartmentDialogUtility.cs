using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Linq;
using System.Text;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public static class ScienceDepartmentDialogUtility
{
    public delegate void GrammarRequestAction(ref GrammarRequest request);

    public static Action<DiaNode, FactionDialogCache> ExtendMainNode;
    public static GrammarRequestAction SalutationRequestProcesser;
    public static GrammarRequestAction ProgressRequestProcesser;

    private static int mapGravlitePanelCount = -1;
    private static int totalGTAP = -1;

    public static void ClearStaticCache()
    {
        ExtendMainNode = null;
        SalutationRequestProcesser = null;
        ProgressRequestProcesser = null;
        mapGravlitePanelCount = -1;
        totalGTAP = -1;
    }

    /// <summary>
    /// 科学部交互
    /// </summary>
    public static DiaOption AccessScienceDepartment(Pawn negotiator, Faction faction)
    {
        TaggedString taggedString = "OARK_AccessScienceDepartment".Translate();
        DiaOption diaOption = new(taggedString);
        if (!ModsConfig.OdysseyActive)
        {
            diaOption.Disable(null);
            return diaOption;
        }

        FactionDialogCache dialogCache = new(negotiator, faction);
        if (!dialogCache.IsAlly)
        {
            diaOption.Disable("MustBeAlly".Translate());
        }
        else
        {
            diaOption.linkLateBind = () => AccessScienceDepartmentNode(dialogCache);
        }

        return diaOption;
    }

    public static DiaNode AccessScienceDepartmentNode(FactionDialogCache dialogCache)
    {
        ScienceDepartmentInteractHandler sdInteractHandler = ScienceDepartmentInteractHandler.Instance;
        mapGravlitePanelCount = OAFrame_MapUtility.AmountSendableThing(dialogCache.Map, ThingDefOf.GravlitePanel);
        totalGTAP = ScienceDepartmentInteractHandler.Instance.GravTechAssistPoints;

        TaggedString nodeTaggedString = sdInteractHandler.IsInitGravQuestCompleted ? GetScienceDepartmentSalutation()
                                                                                   : "OARK_AccessScienceDepartmentInfo_Empty".Translate(dialogCache.Faction.leader).CapitalizeFirst();
        DiaNode diaNode = new(nodeTaggedString);

        if (sdInteractHandler.IsInitGravQuestCompleted)
        {
            diaNode.options.Add(AcquireDataBeacon(dialogCache));

            diaNode.options.Add(DonateGravcore(dialogCache));

            diaNode.options.Add(FactionDialogUtility.DiaOptionWithCooldown(text: "OARK_DonateGravlitePanel".Translate(),
                                                                           key: "DonateGravlitePanel",
                                                                           linkLateBind: () => DonateGravlitePanelNode(dialogCache)));

            if (sdInteractHandler.GravResearchAssistLendPawn is not null)
            {
                diaNode.options.Add(QuestDonateGravlitePanel(dialogCache));
            }

            diaNode.options.Add(SailorAssistance(dialogCache));

            diaNode.options.Add(ExchangeForSDAssistance(dialogCache));

            diaNode.options.Add(new DiaOption("OARK_CovertGTAP".Translate())
            {
                linkLateBind = () => CovertGTAPNode(dialogCache)
            });

            ExtendMainNode?.Invoke(diaNode, dialogCache);

            if (Prefs.DevMode)
            {
                diaNode.options.Add(new DiaOption("(Debug) GravTechPoints +10000")
                {
                    action = delegate { ScienceDepartmentInteractHandler.Instance.AddGravTechPoints(10000, byPlayer: false); }
                });
                diaNode.options.Add(new DiaOption("(Debug) GravTechAssistPoints +100")
                {
                    action = delegate { ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(100); }
                });
                diaNode.options.Add(new DiaOption("(Debug) GravTechPoints -100")
                {
                    action = delegate { ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(-100); }
                });
                diaNode.options.Add(new DiaOption("(Debug) Refresh")
                {
                    linkLateBind = () => AccessScienceDepartmentNode(dialogCache)
                });
            }
        }

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator)
        });

        return diaNode;
    }

    private static DiaOption AcquireDataBeacon(FactionDialogCache dialogCache)
    {
        int cooldownTicksLeft = OAInteractHandler.Instance.GetCooldownTicksLeft("AcquireDataBeacon");

        DiaOption diaOption = new("OARK_AcquireDataBeacon".Translate());
        if (cooldownTicksLeft > 0)
        {
            diaOption.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
        }
        else
        {
            diaOption.action = delegate
            {
                Thing beacon = ThingMaker.MakeThing(OARK_ThingDefOf.OARK_GravDataBeacon);
                if (beacon.def.CanHaveFaction)
                {
                    beacon.SetFaction(Faction.OfPlayer);
                }
                Thing beaconMini = MinifyUtility.TryMakeMinified(beacon);
                OARK_DropPodUtility.DefaultDropSingleThing(beaconMini, dialogCache.Map, dialogCache.Faction);
                OAInteractHandler.Instance.RegisterCDRecord("AcquireDataBeacon", cdTicks: 120 * 60000);
            };
            diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode(text: "OARK_AcquireDataBeaconConfirm".Translate(),
                                                                                   faction: dialogCache.Faction,
                                                                                   negotiator: dialogCache.Negotiator);
        }

        return diaOption;
    }

    private static DiaOption DonateGravcore(FactionDialogCache dialogCache)
    {
        int cooldownTicksLeft = OAInteractHandler.Instance.GetCooldownTicksLeft("DonateGravcore");
        int count = dialogCache.Map.listerThings.ThingsOfDef(ThingDefOf.Gravcore).Count;

        DiaOption diaOption = new("OARK_DonateGravcore".Translate());
        if (cooldownTicksLeft > 0)
        {
            diaOption.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
        }
        else if (count < 1)
        {
            diaOption.Disable("OAFrame_NeedCount".Translate(1));
        }
        else
        {
            diaOption.action = delegate
            {
                dialogCache.Map.listerThings.ThingsOfDef(ThingDefOf.Gravcore)?.FirstOrFallback(null)?.SplitOff(1).Destroy();
                ScienceDepartmentInteractHandler sdInteractHandler = ScienceDepartmentInteractHandler.Instance;
                sdInteractHandler.AddGravTechPoints(500, byPlayer: true);
                sdInteractHandler.AdjustGravTechAssistPoint(100);
                OAInteractHandler.Instance.RegisterCDRecord("DonateGravcore", cdTicks: 10 * 60000);
            };
            diaOption.linkLateBind = () => new DiaNode("OARK_DonateGravcoreConfirm".Translate(500, 100))
            {
                options = { FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator) }
            };
        }

        return diaOption;
    }

    private static DiaOption SailorAssistance(FactionDialogCache dialogCache)
    {
        int cooldownTicksLeft = OAInteractHandler.Instance.GetCooldownTicksLeft("SailorAssistance");
        DiaOption diaOption = new("OARK_SailorAssistance".Translate());
        if (cooldownTicksLeft > 0)
        {
            diaOption.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
        }
        else if (totalGTAP < 200)
        {
            diaOption.Disable("OARK_GravTechAssistPointsNotEnough".Translate(200));
        }
        else
        {
            diaOption.action = delegate
            {
                Slate slate = new();
                slate.Set("map", dialogCache.Map);
                QuestUtility.GenerateQuestAndMakeAvailable(OARK_QuestScriptDefOf.OARK_ScienceDepartment_SailorsAssistance, slate);

                ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(-200);
            };
            diaOption.linkLateBind = () => new DiaNode("OARK_SailorAssistanceConfirm".Translate(200))
            {
                options = { FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator) }
            };
        }
        return diaOption;
    }

    private static DiaNode DonateGravlitePanelNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_DonateGravlitePanelInfo".Translate());

        diaNode.options.Add(DonateOption(20));
        diaNode.options.Add(DonateOption(100));
        diaNode.options.Add(DonateOption(500));

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => AccessScienceDepartmentNode(dialogCache)
        });

        return diaNode;

        DiaOption DonateOption(int count)
        {
            int gainGTP = count * 5;
            int gainGTAP = count / 2;
            DiaOption diaOption = new("OARK_DonateGravlitePanelDetail".Translate(count));
            if (mapGravlitePanelCount < count)
            {
                diaOption.Disable(null);
            }
            else
            {
                diaOption.action = delegate
                {
                    TradeUtility.LaunchThingsOfType(ThingDefOf.GravlitePanel, count, dialogCache.Map, null);
                    ScienceDepartmentInteractHandler sdInteractHandler = ScienceDepartmentInteractHandler.Instance;
                    sdInteractHandler.AddGravTechPoints(gainGTP, byPlayer: true);
                    sdInteractHandler.AdjustGravTechAssistPoint(gainGTAP);
                    OAInteractHandler.Instance.RegisterCDRecord("DonateGravlitePanel", cdTicks: 10 * 60000);
                };
                diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("OARK_DonateGravlitePanelDetailConfirm".Translate(count, gainGTP, gainGTAP),
                                                                                       dialogCache.Faction,
                                                                                       dialogCache.Negotiator);
            }

            return diaOption;
        }
    }

    private static DiaOption QuestDonateGravlitePanel(FactionDialogCache dialogCache)
    {
        Pawn researcher = ScienceDepartmentInteractHandler.Instance.GravResearchAssistLendPawn;
        int cooldownTicksLeft = OAInteractHandler.Instance.GetCooldownTicksLeft("QuestGravlitePanel");

        DiaOption diaOption = new("OARK_QuestDonateGravlitePanel".Translate(researcher));
        if (cooldownTicksLeft > 0)
        {
            diaOption.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
            return diaOption;
        }

        if (mapGravlitePanelCount < 20)
        {
            diaOption.Disable("OAFrame_NeedCount".Translate(20));
            return diaOption;
        }

        diaOption.action = Donate;
        diaOption.link = FactionDialogUtility.FinallyConfirmNode("OARK_QuestDonateGravlitePanelConfirm".Translate(250, 10),
                                                                 dialogCache.Faction,
                                                                 dialogCache.Negotiator);

        void Donate()
        {
            TradeUtility.LaunchThingsOfType(ThingDefOf.GravlitePanel, 20, dialogCache.Map, null);
            ScienceDepartmentInteractHandler sdInteractHandler = ScienceDepartmentInteractHandler.Instance;
            sdInteractHandler.AddGravTechPoints(250, byPlayer: true);
            sdInteractHandler.AdjustGravTechAssistPoint(10);
            OAInteractHandler.Instance.RegisterCDRecord("QuestGravlitePanel", cdTicks: 5 * 60000);

            Quest quest = Find.QuestManager.QuestsListForReading.Where(q => q.root == OARK_QuestScriptDefOf.OARK_GravResearchAssistance).FirstOrFallback(null);
            if (quest is not null)
            {
                QuestPart_GravTechAssistWatcher assistWatcher = quest.PartsListForReading.OfType<QuestPart_GravTechAssistWatcher>()?.FirstOrFallback(null);
                if (assistWatcher is not null)
                {
                    assistWatcher.TotalGTP += 250;
                    assistWatcher.ExternalGTAP += 10;
                }
            }
        }

        return diaOption;
    }

    private static DiaNode CovertGTAPNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CovertGTAPInfo".Translate());

        diaNode.options.Add(new DiaOption("OARK_CovertGTAPToAP".Translate())
        {
            linkLateBind = () => CovertGTAPToAP(dialogCache)
        });

        diaNode.options.Add(new DiaOption("OARK_CovertGTAPToSliver".Translate())
        {
            linkLateBind = () => CovertGTAPToSliver(dialogCache)
        });


        diaNode.options.Add(new DiaOption("OARK_ExchangeForTechPrint".Translate())
        {
            linkLateBind = () => ExchangeForTechPrint(dialogCache)
        });

        diaNode.options.Add(FactionDialogUtility.DiaOptionWithCooldown(text: "OARK_ExchangeForSpecialEquipment".Translate(),
                                                                       key: "ExchangeGravEquipment",
                                                                       linkLateBind: () => ExchangeForSpecialEquipmentNode(dialogCache)));

        DiaOption backOpt = new("GoBack".Translate())
        {
            linkLateBind = () => AccessScienceDepartmentNode(dialogCache)
        };
        diaNode.options.Add(backOpt);

        return diaNode;
    }

    private static DiaNode CovertGTAPToAP(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CovertGTAPToAPInfo".Translate() + "\n\n" + "OARK_GravTechAssistPoints".Translate(totalGTAP));

        CovertOption(10);
        CovertOption(50);
        CovertOption(100);
        CovertOption(200);
        CovertOption(500);
        CovertOption(1000);
        CovertOption(2000);

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => CovertGTAPNode(dialogCache)
        });

        return diaNode;

        void CovertOption(int gtap)
        {
            int gainAP = gtap / 10;
            DiaOption diaOption = new("OARK_CovertGTAPToAPDetail".Translate(gtap, gainAP));
            if (totalGTAP < gtap)
            {
                diaOption.Disable(null);
            }
            else
            {
                diaOption.action = delegate
                {
                    OAInteractHandler.Instance.AdjustAssistPoints(gainAP);
                    ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(-gtap);
                };
                diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("OARK_CovertGTAPToAPConfirm".Translate(gtap, gainAP),
                                                                                       dialogCache.Faction,
                                                                                       dialogCache.Negotiator);
            }

            diaNode.options.Add(diaOption);
        }
    }

    private static DiaNode CovertGTAPToSliver(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CovertGTAPToSliverInfo".Translate() + "\n\n" + "OARK_GravTechAssistPoints".Translate(totalGTAP));

        CovertOption(10);
        CovertOption(50);
        CovertOption(100);
        CovertOption(200);
        CovertOption(500);
        CovertOption(1000);
        CovertOption(2000);

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => CovertGTAPNode(dialogCache)
        });

        return diaNode;

        void CovertOption(int gtap)
        {
            int gainSliver = gtap * 15;
            DiaOption diaOption = new("OARK_CovertGTAPToSliverDetail".Translate(gtap, gainSliver));
            if (totalGTAP < gtap)
            {
                diaOption.Disable(null);
            }
            else
            {
                diaOption.action = delegate
                {
                    OARK_DropPodUtility.DefaultDropThingOfDef(ThingDefOf.Silver, gainSliver, dialogCache.Map, dialogCache.Faction);
                    ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(-gtap);
                };
                diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("OARK_CovertGTAPToSliverConfirm".Translate(gtap, gainSliver),
                                                                                       dialogCache.Faction,
                                                                                       dialogCache.Negotiator);
            }

            diaNode.options.Add(diaOption);
        }
    }

    private static DiaNode ExchangeForTechPrint(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_ExchangeForTechPrintInfo".Translate() + "\n\n" + "OARK_GravTechAssistPoints".Translate(totalGTAP));
        int curGravTechStage = ScienceDepartmentInteractHandler.Instance.CurGravTechStage;

        IOrderedEnumerable<DiaBuyableTechPrintDef> allBuyableTechPrintDefs = DefDatabase<DiaBuyableTechPrintDef>.AllDefsListForReading.OrderBy(d => d.gravTechAssistPoints);
        foreach (DiaBuyableTechPrintDef item in allBuyableTechPrintDefs)
        {
            TechPrintOption(item);
        }

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => CovertGTAPNode(dialogCache)
        });
        return diaNode;

        void TechPrintOption(DiaBuyableTechPrintDef dbtpDef)
        {
            ThingDef techPrint = dbtpDef.TechPrintDef;
            int gtapNeeded = dbtpDef.gravTechAssistPoints;

            DiaOption diaOption = new("OARK_ExchangeForTechPrintDetail".Translate(techPrint.label, gtapNeeded));
            if (curGravTechStage < dbtpDef.gravTechStage)
            {
                diaOption.Disable("OARK_GravTechStageNeedAbove".Translate(dbtpDef.gravTechStage - 1));
            }
            else if (totalGTAP < gtapNeeded)
            {
                diaOption.Disable(null);
            }
            else
            {
                diaOption.action = delegate
                {
                    OARK_DropPodUtility.DefaultDropSingleThingOfDef(techPrint, dialogCache.Map, dialogCache.Faction);
                    ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(-gtapNeeded);
                };
                diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("OARK_ExchangeForTechPrintConfirm".Translate(techPrint.LabelCap, gtapNeeded),
                                                                                       dialogCache.Faction,
                                                                                       dialogCache.Negotiator);
            }

            diaNode.options.Add(diaOption);
        }
    }

    private static DiaNode ExchangeForSpecialEquipmentNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_ExchangeForSpecialEquipmentInfo".Translate() + "\n\n" + "OARK_GravTechAssistPoints".Translate(totalGTAP));

        if (ModsConfig.BiotechActive)
        {
            EquipmentOption(OARK_RimWorldDefOf.DeathrestCapacitySerum, 100);
            EquipmentOption(ThingDefOf.ArchiteCapsule, 100);
        }

        EquipmentOption(OARK_RimWorldDefOf.SentienceCatalyst, 100);
        EquipmentOption(OARK_RimWorldDefOf.BroadshieldCore, 200);
        EquipmentOption(OARK_RimWorldDefOf.VanometricPowerCell, 300);
        EquipmentOption(ThingDefOf.PsychicAmplifier, 500);

        if (ModsConfig.BiotechActive)
        {
            EquipmentOption(ThingDefOf.Mechlink, 1500);
        }

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => CovertGTAPNode(dialogCache)
        });

        return diaNode;

        void EquipmentOption(ThingDef thingDef, int gtap)
        {
            DiaOption diaOption = new("OARK_ExchangeForSpecialEquipmentDetail".Translate(thingDef.LabelCap, gtap));
            if (totalGTAP < gtap)
            {
                diaOption.Disable(null);
            }
            else
            {
                diaOption.action = delegate
                {
                    OARK_DropPodUtility.DefaultDropSingleThingOfDef(thingDef, dialogCache.Map, dialogCache.Faction);
                    ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(-gtap);
                    OAInteractHandler.Instance.RegisterCDRecord("ExchangeGravEquipment", cdTicks: 15 * 60000);
                };
                diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("OARK_ExchangeForSpecialEquipmentConfirm".Translate(thingDef.LabelCap),
                                                                                       dialogCache.Faction,
                                                                                       dialogCache.Negotiator);
            }
            diaNode.options.Add(diaOption);
        }
    }

    private static DiaOption ExchangeForSDAssistance(FactionDialogCache dialogCache)
    {
        DiaOption diaOption = new("OARK_ExchangeForSDAssistance".Translate());

        if (totalGTAP < 100)
        {
            diaOption.Disable("OARK_GravTechAssistPointsNotEnough".Translate(100));
        }
        else
        {
            diaOption.action = delegate
            {
                OARK_DropPodUtility.DefaultDropSingleThingOfDef(OARK_ThingDefOf.OARK_SDCommunicationEquipment, dialogCache.Map, dialogCache.Faction);
                ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(-100);
            };
            diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("OARK_ExchangeForSDAssistanceConfirm".Translate(OARK_ThingDefOf.OARK_SDCommunicationEquipment.LabelCap, 100),
                                                                                   dialogCache.Faction,
                                                                                   dialogCache.Negotiator);
        }

        return diaOption;
    }

    private static readonly string[] gravTechPointsStageArr =
        [
            "OARK_GravTechPointsStage_I",
            "OARK_GravTechPointsStage_II",
            "OARK_GravTechPointsStage_III",
            "OARK_GravTechPointsStage_Max",
        ];

    private static TaggedString GetScienceDepartmentSalutation()
    {
        StringBuilder stringBuilder = new(GetSalutationText());

        stringBuilder.AppendLine();
        stringBuilder.AppendInNewLine(GetProgressText());

        stringBuilder.AppendLine();
        ScienceDepartmentInteractHandler sdInteractHandler = ScienceDepartmentInteractHandler.Instance;
        int gravTechPoints = sdInteractHandler.GravTechPoints;
        int stageStrIndex = sdInteractHandler.CurGravTechStage - 1;
        if (stageStrIndex >= ScienceDepartmentInteractHandler.MaxGravTechStageIndex)
        {
            stringBuilder.AppendInNewLine(gravTechPointsStageArr[gravTechPointsStageArr.Length - 1].Translate(gravTechPoints));
        }
        else
        {
            stringBuilder.AppendInNewLine(gravTechPointsStageArr[stageStrIndex].Translate(gravTechPoints, ScienceDepartmentInteractHandler.GravTechStageBoundary[stageStrIndex + 1]));
        }

        stringBuilder.AppendInNewLine("OARK_PlayerTechPoints".Translate(sdInteractHandler.PlayerTechPoints));
        stringBuilder.AppendInNewLine("OARK_GravTechAssistPoints".Translate(totalGTAP));

        return stringBuilder.ToTaggedString();
    }

    private static string GetSalutationText()
    {
        if (GenDate.DaysPassed >= ScienceDepartmentInteractHandler.Instance.NextShiftWorkDay)
        {
            ScienceDepartmentInteractHandler.Instance.NextShiftWorkDay = GenDate.DaysPassed + 1;
            ScienceDepartmentInteractHandler.Instance.ToDayDutyText = GetNewSalutationText();
        }

        return ScienceDepartmentInteractHandler.Instance.ToDayDutyText;
    }

    private static string GetNewSalutationText()
    {
        GrammarRequest grammarRequest = new();
        grammarRequest.Includes.Add(OARK_ModDefOf.OARK_RulePackSalutationText);

        GrammarRequest backupRequest = grammarRequest;
        try
        {
            SalutationRequestProcesser?.Invoke(ref grammarRequest);
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred while processing the salutation GrammarRequest: " + ex);
            grammarRequest = backupRequest;
        }
        return GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_text", grammarRequest));
    }

    private static string GetProgressText()
    {
        if (Rand.Chance(0.2f) && ScienceDepartmentInteractHandler.Instance.GravResearchAssistLendPawn is not null)
        {
            return "OARK_ScienceDepartmentProgress_Assist".Translate(ScienceDepartmentInteractHandler.Instance.GravResearchAssistLendPawn);
        }

        GrammarRequest grammarRequest = new();
        grammarRequest.Includes.Add(OARK_ModDefOf.OARK_RulePackProgressText);

        int ticksSinceLastActive = OAInteractHandler.Instance.GetTicksSinceLastActive("DonateGravlitePanel");
        if (ticksSinceLastActive > 0 && ticksSinceLastActive.TicksToDays() < 10f)
        {
            grammarRequest.Constants.Add("gravlitePanel", true.ToString());
        }
        else
        {
            ticksSinceLastActive = OAInteractHandler.Instance.GetTicksSinceLastActive("QuestGravlitePanel");
            if (ticksSinceLastActive > 0 && ticksSinceLastActive.TicksToDays() < 10f)
            {
                grammarRequest.Constants.Add("gravlitePanel", true.ToString());
            }
        }

        ticksSinceLastActive = OAInteractHandler.Instance.GetTicksSinceLastActive("GravDataBeacon");
        if (ticksSinceLastActive > 0 && ticksSinceLastActive.TicksToDays() < 10f)
        {
            grammarRequest.Constants.Add("gravDataBeacon", true.ToString());
        }

        GrammarRequest backupRequest = grammarRequest;
        try
        {
            ProgressRequestProcesser?.Invoke(ref grammarRequest);
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred while processing the progress GrammarRequest: " + ex);
            grammarRequest = backupRequest;
        }

        return GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_text", grammarRequest));

    }
}