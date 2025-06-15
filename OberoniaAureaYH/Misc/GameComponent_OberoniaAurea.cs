using OberoniaAurea_Frame;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class GameComponent_OberoniaAurea : GameComponent
{
    protected const int BaseAssistPointsCap = 40;
    protected const int MaxAssistPointsCap = 300;

    protected int ticksRemaining;
    public int initAllianceTick = -1; //本次结盟的起始时刻
    protected float oldAllianceDuration; //记录旧结盟时间
    public float OldAllianceDuration
    {
        get { return oldAllianceDuration; }
        set { oldAllianceDuration = Mathf.Max(0f, value); }
    }

    public int lastTradeTick = -1; //最后一次大规模贸易时刻
    public int lastSponsorTick = -1; //最后一次赞助时刻
    public int lastTechPrintTick = -1; //最后一次科技蓝图购买时刻

    public int tradeCoolingDays = -1; //大规模贸易冷却天数
    public int sponsorCoolingDays = -1; //赞助冷却天数
    public int techPrintCoolingDays = -1;  //科技蓝图冷却天数

    protected int assistPoints;
    public int AssistPoints => assistPoints;

    public int assistPointsStoppageDays = 0;


    public bool newYearEventTriggeredOnce;

    public GameComponent_OberoniaAurea(Game game) 
    {
        OARatkin_MiscUtility.OAGameComp = this;
    }

    public override void StartedNewGame()
    {
        GameStart();
    }
    public override void LoadedGame()
    {
        GameStart();
    }

    public override void GameComponentTick()
    {
        ticksRemaining--;
        if (ticksRemaining < 0)
        {
            AdjustAssistPoints();
            ticksRemaining = 60000;
        }
    }
    protected void AdjustAssistPoints()
    {
        Faction oaFaction = OARatkin_MiscUtility.OAFaction;
        if (oaFaction != null)
        {
            assistPointsStoppageDays--;
            if (oaFaction.PlayerRelationKind == FactionRelationKind.Hostile)
            {
                assistPoints = Mathf.Max(assistPoints - 25, 0);
            }
            else if (oaFaction.PlayerRelationKind == FactionRelationKind.Ally)
            {
                if (assistPointsStoppageDays <= 0 && assistPoints < CurAssistPointsCap)
                {
                    assistPoints++;
                }
            }
        }
    }
    public void GetAssistPoints(int points)
    {
        if (points > 0)
        {
            assistPoints += points;
        }
    }
    public void UseAssistPoints(int points)
    {
        if (points > 0)
        {
            assistPoints = Mathf.Max(assistPoints - points, 0);
        }
    }
  
    public float AllianceDuration
    {
        get
        {
            if (initAllianceTick < 0)
                return -1f;
            else
                return oldAllianceDuration + (Find.TickManager.TicksGame - initAllianceTick).TicksToDays();
        }
    }
    public int CurAssistPointsCap
    {
        get
        {
            float allianceDuration = AllianceDuration;
            if (allianceDuration < 0)
            {
                return BaseAssistPointsCap;
            }
            else
            {
                int num = BaseAssistPointsCap + 20 * Mathf.FloorToInt(allianceDuration / 60f);
                return Mathf.Min(num, MaxAssistPointsCap);
            }
        }
    }

    public int RemainingTradeCoolingTick
    {
        get
        {
            if (lastTradeTick < 0)
                return -1;
            else
                return lastTradeTick + (tradeCoolingDays * 60000) - Find.TickManager.TicksGame;
        }
    }

    public int RemainingSponsorCoolingTick
    {
        get
        {
            if (lastSponsorTick < 0)
                return -1;
            else
                return lastSponsorTick + (sponsorCoolingDays * 60000) - Find.TickManager.TicksGame;
        }
    }

    public int RemainingTechPrintCoolingTick
    {
        get
        {
            if (lastTechPrintTick < 0)
                return -1;
            else
                return lastTechPrintTick + (techPrintCoolingDays * 60000) - Find.TickManager.TicksGame;
        }
    }

    private void GameStart()
    {
        OARatkin_MiscUtility.Notify_GameStart();
        TryAddNewYearEnent();
    }

    private void TryAddNewYearEnent()
    {
        if (!newYearEventTriggeredOnce)
        {
            DateTime date = DateTime.Now;
            if (date.Month == 1 && date.Day <= 3)
            {
                Map map = Find.AnyPlayerHomeMap;
                if (map != null && OARatkin_MiscUtility.OAFaction != null)
                {
                    if (OARatkin_MiscUtility.OAFaction.PlayerRelationKind == FactionRelationKind.Ally)
                    {
                        IncidentParms parms = new()
                        {
                            target = map,
                            faction = OARatkin_MiscUtility.OAFaction
                        };
                        OAFrame_MiscUtility.AddNewQueuedIncident(OARatkin_MiscDefOf.OARatkin_NewYearEvent, 2500, parms);
                    }
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);
        Scribe_Values.Look(ref oldAllianceDuration, "oldAllianceDuration", 0f);

        Scribe_Values.Look(ref initAllianceTick, "initAllianceTick", -1);
        Scribe_Values.Look(ref lastTradeTick, "lastTradeTick", -1);
        Scribe_Values.Look(ref lastSponsorTick, "lastSponsorTick", -1);
        Scribe_Values.Look(ref lastTechPrintTick, "lastTechPrintTick", -1);

        Scribe_Values.Look(ref tradeCoolingDays, "tradeCoolingDays", -1);
        Scribe_Values.Look(ref sponsorCoolingDays, "sponsorCoolingDays", -1);
        Scribe_Values.Look(ref techPrintCoolingDays, "techPrintCoolingDays", -1);
        Scribe_Values.Look(ref assistPoints, "assistPoints", 0);
        Scribe_Values.Look(ref assistPointsStoppageDays, "assistPointsStoppageDays", 0);

        Scribe_Values.Look(ref newYearEventTriggeredOnce, "newYearEventTriggeredOnce", defaultValue: false);

    }
}
