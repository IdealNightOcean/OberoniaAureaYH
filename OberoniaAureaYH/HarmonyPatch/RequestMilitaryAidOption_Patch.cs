using HarmonyLib;
using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(FactionDialogMaker), "RequestMilitaryAidOption")]
public static class RequestMilitaryAidOption_Patch //我们遇到了麻烦
{
    [HarmonyPrefix]
    public static bool Prefix(ref DiaOption __result, Map map, Faction faction, Pawn negotiator)
    {
        if (!faction.IsOAFaction())
        {
            return true;
        }
        __result = RequestOberoniaAurea(map, faction, negotiator);
        return false;
    }

    private static DiaOption RequestOberoniaAurea(Map map, Faction faction, Pawn negotiator)
    {
        DiaOption diaOption = new("OA_AssistWithTrouble".Translate());

        if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            diaOption.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
        }
        else if (faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            diaOption.Disable("MustBeAlly".Translate());
        }
        else if (faction.PlayerGoodwill < 50)
        {
            diaOption.Disable("NeedGoodwill".Translate(50.ToString("F0")));
        }
        else
        {
            diaOption.linkLateBind = () => RequestOberoniaAureaNode(map, faction, negotiator);
        }

        return diaOption;
    }

    private static DiaNode RequestOberoniaAureaNode(Map map, Faction faction, Pawn negotiator)
    {
        OAInteractHandler interactHandler = OAInteractHandler.Instance;

        int assistPoints = interactHandler.AssistPoints;
        int curAssistPointsCap = interactHandler.CurAssistPointsCap;
        float allianceDuration = interactHandler.AllianceDuration();
        int assistStoppageDays = interactHandler.AssistStoppageDays;

        StringBuilder taggedStringInfo = new("OA_AssistWithTrouble".Translate());
        taggedStringInfo.AppendLine();
        taggedStringInfo.AppendLine();
        taggedStringInfo.AppendInNewLine("OA_AssistWithTroubleInfo".Translate(assistPoints, curAssistPointsCap));
        if (assistStoppageDays > 0)
        {
            taggedStringInfo.AppendLine();
            taggedStringInfo.AppendLine();
            taggedStringInfo.AppendInNewLine("OA_AssistPointsStoppage".Translate(assistStoppageDays.ToString().Colorize(Color.red)));
        }
        DiaNode diaNode = new(taggedStringInfo.ToString());

        //挨饿
        AddDiaOption("OA_WeAreStarving",
                    delegate
                    {
                        CallForMealSurvivalPack(map, faction, 5, 80);
                        OAInteractHandler.Instance.AdjustAssistPoints(-35);
                    },
                    needAP: 35);
        //粮荒
        AddDiaOption("OA_WeHadAFamine",
                    delegate
                    {
                        CallForMealSurvivalPack(map, faction, 20, 320);
                        OAInteractHandler.Instance.AdjustAssistPoints(-80);
                    },
                    needAD: 60,
                    needAP: 80);
        //应急药品
        AddDiaOption("OA_WeNeedEmergencyMedicine",
                    delegate
                    {
                        CallEmergencyMedicine(map, faction);
                        OAInteractHandler.Instance.AdjustAssistPoints(-35);
                    },
                    needAP: 35);
        //资金困难
        AddDiaOption("OA_WeAreInFinancialTrouble",
                    delegate
                    {
                        CallForSilver(map, faction);
                        OAInteractHandler.Instance.AdjustAssistPoints(-35);
                    }, needAP: 35);
        //长期协助
        AddDiaOption("OA_WeNeedLongTermAssistance",
                    delegate
                    {
                        PawnKindDef pawnKind = DefDatabase<PawnKindDef>.GetNamed("OA_RK_Court_Member");
                        CallForLongTermAssist(map, faction, pawnKind, 2);
                        OAInteractHandler.Instance.AdjustAssistPoints(-80);
                    },
                    needAD: 60,
                    needAP: 80);
        //物质征募
        AddDiaOption("OA_CallForTributeCollector",
                    delegate
                    {
                        CallForTributeCollector(map, faction);
                        OAInteractHandler.Instance.AdjustAssistPoints(-35);
                    },
                    needAP: 35);
        //突袭小组
        AddDiaOption("OA_WeAreBeingAttacked",
                    delegate
                    {
                        ModUtility.CallForAidFixedPoints(map, faction, 1800);
                        OAInteractHandler.Instance.AdjustAssistPoints(-45);
                    },
                    needAD: 60,
                    needAP: 45);
        //突袭分队
        AddDiaOption("OA_WeNeedUrgentBackup",
                    delegate
                    {
                        ModUtility.CallForAidFixedPoints(map, faction, 6300);
                        OAInteractHandler.Instance.AdjustAssistPoints(-100);
                    },
                    needAD: 120,
                    needAP: 100);
        //军事部署
        AddDiaOption("OA_WeNeedMilitaryDeployment",
                    delegate
                    {
                        CallForMilitaryDeployment(map, faction);
                        OAInteractHandler.Instance.AdjustAssistPoints(-200);
                    },
                    needAD: 360,
                    needAP: 200);

        if (ModsConfig.OdysseyActive && ScienceDepartmentInteractHandler.Instance is not null)
        {
            diaNode.options.Add(GravityDistortionBombOption(map, faction, negotiator));
        }

        if (Prefs.DevMode)
        {
            diaNode.options.Add(new DiaOption("(Debug) AssistPoints +100")
            {
                action = delegate { OAInteractHandler.Instance.AdjustAssistPoints(100); }
            });
            diaNode.options.Add(new DiaOption("(Debug) AssistPoints -100")
            {
                action = delegate { OAInteractHandler.Instance.AdjustAssistPoints(-100); }
            });
            diaNode.options.Add(new DiaOption("(Debug) Refresh")
            {
                linkLateBind = () => RequestOberoniaAureaNode(map, faction, negotiator)
            });
        }
        //返回按钮
        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
        });

        return diaNode;

        //内部函数 - 添加通讯台按钮
        void AddDiaOption(string diaStr, Action diaAaction, int needAD = -1, int needAP = -1)
        {
            string confirmsStr = diaStr + "Confirm";
            DiaOption diaOption = new(diaStr.Translate());
            if (allianceDuration < needAD)
            {
                diaOption.Disable("OA_AllianceDurationShort".Translate(needAD.ToString("F0")));
            }
            else if (assistPoints < needAP)
            {
                diaOption.Disable("OA_AlliancePointsNotEnough".Translate(needAP.ToString("F0")));
            }
            else
            {
                diaOption.action = diaAaction;
                diaOption.linkLateBind = () => FactionDialogUtility.FinallyConfirmNode(confirmsStr.Translate(faction.leader).CapitalizeFirst(), faction, negotiator);
            }
            diaNode.options.Add(diaOption);
        }
    }

    private static DiaOption GravityDistortionBombOption(Map map, Faction faction, Pawn negotiator)
    {
        DiaOption diaOption = new("OARK_GravityDistortionBomb".Translate());

        if (ScienceDepartmentInteractHandler.Instance.CurGravTechStage < 2)
        {
            diaOption.Disable("OARK_GravTechStageNotEnough".Translate(2));
        }
        else if (OAInteractHandler.Instance.AssistPoints < 55)
        {
            diaOption.Disable("OA_AlliancePointsNotEnough".Translate(55));
        }

        else
        {
            diaOption.linkLateBind = () => GravityDistortionBombConfimNode(map, faction, negotiator);
        }
        return diaOption;
    }

    private static DiaNode GravityDistortionBombConfimNode(Map map, Faction faction, Pawn negotiator)
    {
        DiaNode diaNode = new("OARK_GravityDistortionBombConfirmInfo".Translate().Colorize(Color.yellow));

        diaNode.options.Add(new DiaOption("Confirm".Translate().Colorize(Color.red))
        {
            action = delegate
            {
                OAInteractHandler.Instance.AdjustAssistPoints(-55);
                CallGravityDistortionBomb(map);
            },
            linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("OARK_GravityDistortionBombConfirm".Translate(faction.leader), faction, negotiator)
        });

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => RequestOberoniaAureaNode(map, faction, negotiator)
        });

        return diaNode;
    }

    private static void CallForMealSurvivalPack(Map map, Faction faction, int preCount, int maxCount)
    {
        int pawnNum = map.mapPawns.FreeColonistsSpawnedCount;
        int count = Mathf.Min(pawnNum * preCount, maxCount);
        if (count > 0)
        {
            IntVec3 dropCell = OARK_DropPodUtility.DefaultDropThingOfDef(ThingDefOf.MealSurvivalPack, count, map, faction);
            Messages.Message("OA_MealSurvivalPackArrive".Translate(faction.Named("FACTION"), count), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
        }
    }
    private static void CallEmergencyMedicine(Map map, Faction faction)
    {
        List<List<Thing>> dropThings = [];
        dropThings.Add(OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_F, 10));
        dropThings.Add(OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_G, 10));
        dropThings.Add(OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.MedicineIndustrial, 20));
        IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingGroupsNear(dropCell, map, dropThings, forbid: false, allowFogged: false, faction: faction);
        Messages.Message("OA_EmergencyMedicineArrive".Translate(faction.Named("FACTION")), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
    }
    private static void CallForSilver(Map map, Faction faction, int count = 2500)
    {
        if (count > 0)
        {
            IntVec3 dropCell = OARK_DropPodUtility.DefaultDropThingOfDef(ThingDefOf.Silver, count, map, faction);
            Messages.Message("OA_SilverArrive".Translate(faction.Named("FACTION"), count), new LookTargets(dropCell, map), MessageTypeDefOf.PositiveEvent);
        }
    }
    private static void CallForLongTermAssist(Map map, Faction faction, PawnKindDef pawnKind, int pawnCount)
    {
        StringBuilder letterStr = new();
        letterStr.Append("OA_AssistPawnArrivalDetail".Translate(faction.Named("FACTION"), pawnCount, pawnKind.race.LabelCap));
        letterStr.AppendLine();
        IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
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
            traderKind = OARK_PawnGenerateDefOf.OA_RK_Caravan_TributeCollector,
            forced = true
        };
        OAFrame_MiscUtility.AddNewQueuedIncident(incidentDef, 120000, parms, 240000);
    }

    private static void CallForMilitaryDeployment(Map map, Faction faction)
    {
        if (OAFrame_MapUtility.ThreatsCountOfPlayerOnMap(map) > 0)
        {
            ModUtility.CallForAidFixedPoints(map, faction, 4500);
        }
        GameCondition gameCondition = GameConditionMaker.MakeCondition(OARK_ModDefOf.OA_MilitaryDeployment, 300000);
        map.gameConditionManager.RegisterCondition(gameCondition);
    }

    private static void CallGravityDistortionBomb(Map map)
    {
        GameCondition gameCondition = GameConditionMaker.MakeCondition(OARK_ModDefOf.OARK_GravityDistortionBomb, 2700);
        map.gameConditionManager.RegisterCondition(gameCondition);
    }
}