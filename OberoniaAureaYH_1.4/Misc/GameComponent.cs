using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class GameComponent_OberoniaAurea : GameComponent
{
    protected static Faction OAFaction => Find.FactionManager.FirstFactionOfDef(OberoniaAureaYHDefOf.OA_RK_Faction);

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
    protected static readonly int BaseAssistPointsCap = 40;
    protected static readonly int MaxAssistPointsCap = 260;

    public GameComponent_OberoniaAurea(Game game)
    {
    }
    public override void LoadedGame()
    {
        ResolveOldSave();
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
        Faction faction = OAFaction;
        if (faction != null)
        {
            assistPointsStoppageDays--;
            if (faction.PlayerRelationKind == FactionRelationKind.Hostile)
            {
                assistPoints = Mathf.Max(assistPoints - 25, 0);
            }
            else if (faction.PlayerRelationKind == FactionRelationKind.Ally)
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
    private void ResolveOldSave()
    {
        Faction faction = OAFaction;
        if (faction != null)
        {
            if (initAllianceTick < 0 && faction.PlayerRelationKind == FactionRelationKind.Ally)
                initAllianceTick = Find.TickManager.TicksGame;
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

    }
}
