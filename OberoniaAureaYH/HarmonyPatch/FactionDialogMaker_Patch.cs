using HarmonyLib;
using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

//通讯台界面Patch

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(FactionDialogMaker), "FactionDialogFor")]
public static class FactionDialogFor_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref DiaNode __result, Pawn negotiator, Faction faction)
    {
        if (faction.def != OARatkin_MiscDefOf.OA_RK_Faction)
        {
            return;
        }
        List<DiaOption> opts =
            [
                CallForLargeScaleTrader(negotiator, faction, negotiator.Map),
                SponsorOberoniaAurea(negotiator, faction, negotiator.Map),
                BuyTechPrint(negotiator, faction, negotiator.Map)
            ];
        if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            string disStr = "WorkTypeDisablesOption".Translate(SkillDefOf.Social.label);
            foreach (DiaOption opt in opts)
            {
                opt.Disable(disStr);
            }
        }
        int index = Mathf.Max(0, __result.options.Count - 3);
        __result.options.InsertRange(index, opts);
    }
    private static DiaOption CallForLargeScaleTrader(Pawn negotiator, Faction faction, Map map) //进行一次大规模贸易
    {
        TaggedString taggedString = "OA_CallForLargeScaleTrade".Translate();
        DiaOption diaOption = new(taggedString);
        if (faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            diaOption.Disable("MustBeAlly".Translate());
            return diaOption;
        }
        GameComponent_OberoniaAurea oaGameComp = OARatkin_MiscUtility.OAGameComp;
        float allianceDuration = oaGameComp.AllianceDuration;
        int remainingTradeCoolingTick = oaGameComp.RemainingTradeCoolingTick;
        if (allianceDuration < 75)
        {
            diaOption.Disable("OA_AllianceDurationShort".Translate(75.ToString("F0")));
            return diaOption;
        }
        if (remainingTradeCoolingTick > 0)
        {
            diaOption.Disable("WaitTime".Translate(remainingTradeCoolingTick.ToStringTicksToPeriod()));
            return diaOption;
        }
        if (OARatkin_MiscUtility.AmountSendable(map, ThingDefOf.Silver) < 2000)
        {
            diaOption.Disable("NeedSilverLaunchable".Translate(2000));
            return diaOption;
        }

        diaOption.action = CallTrader;
        diaOption.link = new DiaNode("OA_CallForLargeScaleTradeConfirm".Translate(faction.leader).CapitalizeFirst())
        {
            options = { FactionDialogUtility.OKToRoot(faction, negotiator) }
        };

        return diaOption;

        void CallTrader()
        {
            oaGameComp.lastTradeTick = Find.TickManager.TicksGame;
            oaGameComp.tradeCoolingDays = 60;
            TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed("OA_RK_Caravan_TraderGeneral_B");
            IncidentParms parms = new()
            {
                target = map,
                faction = faction,
                traderKind = traderKind,
                forced = true
            };
            OAFrame_MiscUtility.AddNewQueuedIncident(OARatkin_MiscDefOf.OA_RK_LargeScaleTraderArrival, 120000, parms, 12000);
            TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, 2000, map, null);
        }
    }

    private static DiaOption SponsorOberoniaAurea(Pawn negotiator, Faction faction, Map map)
    {
        TaggedString taggedString = "OA_SponsorOberoniaAurea".Translate();
        DiaOption diaOption = new(taggedString);

        GameComponent_OberoniaAurea oaGameComp = OARatkin_MiscUtility.OAGameComp;

        if (oaGameComp.RemainingSponsorCoolingTick > 0) //冷却时不需要更多的帮助
        {
            string cdConfirmStr = "OA_SponsorThanksAgain".Translate(faction.leader).CapitalizeFirst();
            diaOption.link = new DiaNode(cdConfirmStr)
            {
                options = { FactionDialogUtility.OKToRoot(faction, negotiator) }
            };
            return diaOption;
        }

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
            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
        });
        diaOption.link = diaNode;
        return diaOption;

        void AddDiaOption(int needSilver)
        {
            int gainAP = needSilver / 250;
            int gainFavor = needSilver / 1000;
            string diaStr = "OA_DoSponsorOberoniaAurea".Translate(needSilver);
            StringBuilder confirmSb = new("OA_SponsorThanks".Translate(faction.leader).CapitalizeFirst());
            confirmSb.AppendLine();
            confirmSb.AppendInNewLine("OA_SponsorAssistPointsGet".Translate(gainAP));
            confirmSb.AppendInNewLine("OA_SponsorRoyaltyFavorGain".Translate(negotiator.Named("NAME"), gainFavor));

            DiaOption diaOption = new(diaStr)
            {
                action = delegate
                {
                    DoSponsorOberoniaAurea(needSilver, gainAP, gainFavor);
                    oaGameComp.lastSponsorTick = Find.TickManager.TicksGame;
                    oaGameComp.sponsorCoolingDays = 30;
                },
                link = new DiaNode(confirmSb.ToString())
                {
                    options = { FactionDialogUtility.OKToRoot(faction, negotiator) }
                }
            };
            if (OARatkin_MiscUtility.AmountSendable(map, ThingDefOf.Silver) < needSilver)
            {
                diaOption.Disable("NeedSilverLaunchable".Translate(needSilver));
            }
            diaNode.options.Add(diaOption);
        }
        void DoSponsorOberoniaAurea(int silverCount, int gainAP, int gainFavor)
        {
            TradeUtility.LaunchThingsOfType(RimWorld.ThingDefOf.Silver, silverCount, map, null);
            oaGameComp.GetAssistPoints(gainAP);
            negotiator.royalty?.GainFavor(faction, gainFavor);
        }
    }
    private static DiaOption BuyTechPrint(Pawn negotiator, Faction faction, Map map)
    {
        TaggedString taggedString = "OA_BuyTechPrint".Translate();
        DiaOption diaOption = new(taggedString);
        if (faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            diaOption.Disable("MustBeAlly".Translate());
            return diaOption;
        }
        GameComponent_OberoniaAurea oaGameComp = OARatkin_MiscUtility.OAGameComp;
        float allianceDuration = oaGameComp.AllianceDuration;
        int remainingTechPrintCoolingTick = oaGameComp.RemainingTechPrintCoolingTick;
        if (allianceDuration < 45)
        {
            diaOption.Disable("OA_AllianceDurationShort".Translate(45.ToString("F0")));
            return diaOption;
        }
        if (remainingTechPrintCoolingTick > 0)
        {
            diaOption.Disable(null);
            return diaOption;
        }
        TaggedString nodeTaggedString = "OA_BuyTechPrintNode".Translate(faction.leader).CapitalizeFirst();
        DiaNode diaNode = new(nodeTaggedString);
        List<DiaBuyableTechPrintDef> defsList = DefDatabase<DiaBuyableTechPrintDef>.AllDefsListForReading;
        foreach (DiaBuyableTechPrintDef item in defsList)
        {
            AddDiaOption(item);
        }
        //返回按钮
        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
        });
        diaOption.link = diaNode;
        return diaOption;

        void AddDiaOption(DiaBuyableTechPrintDef dbtpDef)
        {
            string confirmStr = "OA_TechPrintChoiceConfirm".Translate(faction.leader).CapitalizeFirst();
            StringBuilder diaSb = new("OA_TechPrintChoice".Translate(dbtpDef.TechPrintDef.label, dbtpDef.price));
            if (dbtpDef.NeedAllianceDuration)
            {
                diaSb.AppendWithSeparator("OA_TechPrintNeedAllianceDuration".Translate(dbtpDef.allianceDuration), "丨");
            }
            if (dbtpDef.NeedRoyalTitle)
            {
                diaSb.AppendWithSeparator("OA_TechPrintNeedRoyalTitle".Translate(dbtpDef.royalTitleDef), "丨");
            }
            DiaOption diaOption = new(diaSb.ToString())
            {
                action = delegate
                {
                    GetTechPrint(map, faction, dbtpDef.TechPrintDef, dbtpDef.price);
                    oaGameComp.lastTechPrintTick = Find.TickManager.TicksGame;
                    oaGameComp.techPrintCoolingDays = 30;
                },
                link = new DiaNode(confirmStr)
                {
                    options = { FactionDialogUtility.OKToRoot(faction, negotiator) }
                }
            };
            if (OARatkin_MiscUtility.AmountSendable(map, ThingDefOf.Silver) < dbtpDef.price)
            {
                DeniedTechPrintDiaOption("NeedSilverLaunchable".Translate(dbtpDef.price), diaNode, diaOption);
            }
            else if (dbtpDef.NeedAllianceDuration && dbtpDef.allianceDuration > allianceDuration)
            {
                string denStr = "OA_AllianceDurationShort".Translate(dbtpDef.allianceDuration.ToString("F0"));
                DeniedTechPrintDiaOption(denStr, diaNode, diaOption);
            }
            else if (dbtpDef.NeedRoyalTitle && dbtpDef.royalTitleDef.seniority > negotiator.GetCurrentTitleSeniorityIn(faction))
            {
                string denStr = "OA_BuyTechPrintDeniedDueTitle".Translate(negotiator.Named("NEGOTIATOR"), dbtpDef.royalTitleDef.GetLabelCapFor(negotiator).Named("TITLE"), faction.Named("FACTION"));
                DeniedTechPrintDiaOption(denStr, diaNode, diaOption);
            }
            diaNode.options.Add(diaOption);
        }
    }
    private static void GetTechPrint(Map map, Faction faction, ThingDef tDef, int price)
    {
        TradeUtility.LaunchThingsOfType(RimWorld.ThingDefOf.Silver, price, map, null);
        Thing dropThing = ThingMaker.MakeThing(tDef);
        IntVec3 dropCell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.One);
        DropPodUtility.DropThingsNear(dropCell, map, [dropThing], forbid: false, allowFogged: false, faction: faction);
        Messages.Message("OA_TechPrintArrive".Translate(faction.Named("FACTION"), tDef.label), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);

    }
    private static void DeniedTechPrintDiaOption(string denStr, DiaNode diaNode, DiaOption diaOption)
    {
        DiaNode diaNode2 = new(denStr);
        DiaOption diaOption2 = new("GoBack".Translate());
        diaNode2.options.Add(diaOption2);
        diaOption.link = diaNode2;
        diaOption.action = null;
        diaOption2.link = diaNode;
    }
}


[StaticConstructorOnStartup]
[HarmonyPatch(typeof(FactionDialogMaker), "RequestMilitaryAidOption")]
public static class RequestMilitaryAidOption_Patch //我们遇到了麻烦
{
    [HarmonyPrefix]
    public static bool Prefix(ref DiaOption __result, Map map, Faction faction, Pawn negotiator)
    {
        if (faction.def != OARatkin_MiscDefOf.OA_RK_Faction)
        {
            return true;
        }
        DiaOption opt = RequestOberoniaAurea(map, faction, negotiator);
        if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            opt.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
        }
        __result = opt;
        return false;
    }
    private static DiaOption RequestOberoniaAurea(Map map, Faction faction, Pawn negotiator)
    {
        TaggedString taggedString = "OA_AssistWithTrouble".Translate();
        DiaOption diaOption = new(taggedString);
        if (faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            diaOption.Disable("MustBeAlly".Translate());
            return diaOption;
        }
        if (faction.PlayerGoodwill < 50)
        {
            diaOption.Disable("NeedGoodwill".Translate(50.ToString("F0")));
            return diaOption;
        }
        GameComponent_OberoniaAurea oaGameComp = OARatkin_MiscUtility.OAGameComp;
        int assistPoints = oaGameComp.AssistPoints;
        int curAssistPointsCap = oaGameComp.CurAssistPointsCap;
        float allianceDuration = oaGameComp.AllianceDuration;
        int assistPointsStoppageDays = oaGameComp.assistPointsStoppageDays;
        //以下为二级界面按钮
        TaggedString taggedStringInfo = taggedString + "\n\n" + "OA_AssistWithTroubleInfo".Translate(assistPoints, curAssistPointsCap);
        if (assistPointsStoppageDays > 0)
        {
            taggedStringInfo += "\n\n" + "OA_AssistPointsStoppage".Translate(assistPointsStoppageDays.ToString().Colorize(Color.red));
        }
        DiaNode diaNode = new(taggedStringInfo);
        //挨饿
        AddDiaOption("OA_WeAreStarving", delegate
        {
            CallForMealSurvivalPack(map, faction, 5, 80);
            oaGameComp.UseAssistPoints(35);
        }, needAP: 35);
        //粮荒
        AddDiaOption("OA_WeHadAFamine", delegate
        {
            CallForMealSurvivalPack(map, faction, 20, 320);
            oaGameComp.UseAssistPoints(80);
        }, needAD: 60, needAP: 80);
        //应急药品
        AddDiaOption("OA_WeNeedEmergencyMedicine", delegate
        {
            CallEmergencyMedicine(map, faction);
            oaGameComp.UseAssistPoints(35);
        }, needAP: 35);
        //资金困难
        AddDiaOption("OA_WeAreInFinancialTrouble", delegate
        {
            CallForSilver(map, faction);
            oaGameComp.UseAssistPoints(35);
        }, needAP: 35);
        //长期协助
        AddDiaOption("OA_WeNeedLongTermAssistance", delegate
        {
            PawnKindDef pawnKind = DefDatabase<PawnKindDef>.GetNamed("OA_RK_Court_Member");
            CallForLongTermAssist(map, faction, pawnKind, 2);
            oaGameComp.UseAssistPoints(80);
        }, needAD: 60, needAP: 80);
        //物质征募
        AddDiaOption("OA_CallForTributeCollector", delegate
        {
            CallForTributeCollector(map, faction);
            oaGameComp.UseAssistPoints(35);
        }, needAP: 35);
        //突袭小组
        AddDiaOption("OA_WeAreBeingAttacked", delegate
        {
            OARatkin_MiscUtility.CallForAidFixedPoints(map, faction, 1800);
            oaGameComp.UseAssistPoints(45);
        }, needAD: 60, needAP: 45);
        //突袭分队
        AddDiaOption("OA_WeNeedUrgentBackup", delegate
        {
            OARatkin_MiscUtility.CallForAidFixedPoints(map, faction, 6300);
            oaGameComp.UseAssistPoints(100);
        }, needAD: 120, needAP: 100);
        //军事部署
        AddDiaOption("OA_WeNeedMilitaryDeployment", delegate
        {
            CallForMilitaryDeployment(map, faction);
            oaGameComp.UseAssistPoints(200);
        }, needAD: 360, needAP: 200);
        //返回按钮
        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
        });

        diaOption.link = diaNode;
        return diaOption;
        //内部函数 - 添加通讯台按钮
        void AddDiaOption(string diaStr, Action diaAaction, int needAD = -1, int needAP = -1)
        {
            string confirmsStr = diaStr + "Confirm";
            DiaOption diaOption = new(diaStr.Translate())
            {
                action = diaAaction,
                link = new DiaNode(confirmsStr.Translate(faction.leader).CapitalizeFirst())
                {
                    options = { FactionDialogUtility.OKToRoot(faction, negotiator) }
                }
            };
            if (allianceDuration < needAD)
            {
                diaOption.Disable("OA_AllianceDurationShort".Translate(needAD.ToString("F0")));
            }
            else if (assistPoints < needAP)
            {
                diaOption.Disable("OA_AlliancePointsNotEnough".Translate(needAP.ToString("F0")));
            }
            diaNode.options.Add(diaOption);
        }
    }
    private static void CallForMealSurvivalPack(Map map, Faction faction, int preCount, int maxCount)
    {
        int pawnNum = map.mapPawns.FreeColonistsSpawnedCount;
        int count = Mathf.Min(pawnNum * preCount, maxCount);
        if (count > 0)
        {
            List<Thing> dropThings = OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.MealSurvivalPack, count);
            IntVec3 dropCell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.MealSurvivalPack.size);
            DropPodUtility.DropThingGroupsNear(dropCell, map, [dropThings], forbid: false, allowFogged: false, faction: faction);
            Messages.Message("OA_MealSurvivalPackArrive".Translate(faction.Named("FACTION"), count), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
        }
    }
    private static void CallEmergencyMedicine(Map map, Faction faction)
    {
        List<List<Thing>> dropThings = [];
        dropThings.Add(OAFrame_MiscUtility.TryGenerateThing(OARatkin_ThingDefOf.Oberonia_Aurea_Chanwu_F, 10));
        dropThings.Add(OAFrame_MiscUtility.TryGenerateThing(OARatkin_ThingDefOf.Oberonia_Aurea_Chanwu_G, 10));
        dropThings.Add(OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.MedicineIndustrial, 20));
        IntVec3 dropCell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, new IntVec2(2, 2));
        DropPodUtility.DropThingGroupsNear(dropCell, map, dropThings, forbid: false, allowFogged: false, faction: faction);
        Messages.Message("OA_EmergencyMedicineArrive".Translate(faction.Named("FACTION")), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
    }
    private static void CallForSilver(Map map, Faction faction, int count = 2500)
    {
        if (count > 0)
        {
            List<Thing> dropThings = OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.Silver, count);
            IntVec3 dropCell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.Silver.size);
            DropPodUtility.DropThingGroupsNear(dropCell, map, [dropThings], forbid: false, allowFogged: false, faction: faction);
            Messages.Message("OA_SilverArrive".Translate(faction.Named("FACTION"), count), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
        }
    }
    private static void CallForLongTermAssist(Map map, Faction faction, PawnKindDef pawnKind, int pawnCount)
    {
        StringBuilder letterStr = new();
        letterStr.Append("OA_AssistPawnArrivalDetail".Translate(faction.Named("FACTION"), pawnCount, pawnKind.race.LabelCap));
        letterStr.AppendLine();
        IntVec3 dropCell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.One);
        List<Pawn> DropPawns = [];
        for (int i = 0; i < pawnCount; i++)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, Faction.OfPlayer);
            pawn.needs.SetInitialLevels();
            pawn.mindState?.SetupLastHumanMeatTick();
            letterStr.AppendInNewLine($"- {pawnKind.LabelCap} {pawn.Name.ToStringShort}");
            DropPawns.Add(pawn);
        }
        DropPodUtility.DropThingsNear(dropCell, map, DropPawns, 110, allowFogged: false, faction: faction);
        letterStr.AppendLine();
        letterStr.AppendInNewLine("OA_AssistPawnJoin".Translate());
        ChoiceLetter letter = LetterMaker.MakeLetter("OA_AssistPawnArrival".Translate(), letterStr.ToString(), LetterDefOf.PositiveEvent, faction);
        letter.lookTargets = new LookTargets(dropCell, map);
        Find.LetterStack.ReceiveLetter(letter);
    }
    private static void CallForTributeCollector(Map map, Faction faction)
    {
        IncidentDef incidentDef = DefDatabase<IncidentDef>.GetNamed("OA_RK_CaravanArrivalTributeCollector");
        IncidentParms parms = new()
        {
            target = map,
            faction = faction,
            traderKind = OARatkin_PawnGenerateDefOf.OA_RK_Caravan_TributeCollector,
            forced = true
        };
        OAFrame_MiscUtility.AddNewQueuedIncident(incidentDef, 120000, parms, 240000);
    }

    private static void CallForMilitaryDeployment(Map map, Faction faction)
    {
        if (OAFrame_MapUtility.ThreatsCountOfPlayerOnMap(map) > 0)
        {
            OARatkin_MiscUtility.CallForAidFixedPoints(map, faction, 4500);
        }
        GameCondition gameCondition = GameConditionMaker.MakeCondition(OARatkin_MiscDefOf.OA_MilitaryDeployment, 300000);
        map.gameConditionManager.RegisterCondition(gameCondition);
    }
}