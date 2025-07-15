using RimWorld;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class OAInteractHandler : IExposable
{
    private const int BaseAssistPointCap = 40;
    private const int MaxAssistPointCap = 300;

    public static OAInteractHandler Instance { get; private set; }

    public int CurAllianceInitTick = -1; //本次结盟的起始时刻
    private float oldAllianceDuration; //记录旧结盟时间
    public float OldAllianceDuration
    {
        get { return oldAllianceDuration; }
        set { oldAllianceDuration = Mathf.Max(0f, value); }
    }

    private int assistPoints;
    public int AssistPoints => assistPoints;

    private int assistStoppageDays = 0;
    public int AssistStoppageDays { get { return assistStoppageDays; } set { assistStoppageDays = Mathf.Max(0, value); } }

    public int CurAssistPointsCap => InteractUtility.GetCurAssistPointsCap(AllianceDuration());

    private Dictionary<string, OAInteractionCDRecord> interactionCDRecords = [];

    public OAInteractHandler()
    {
        Instance = this;
    }

    public void TickDay()
    {
        DailyAssistPointChange();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float AllianceDuration()
    {
        return CurAllianceInitTick < 0 ? -1f : (oldAllianceDuration + (Find.TickManager.TicksGame - CurAllianceInitTick).TicksToDays());
    }

    private void DailyAssistPointChange()
    {
        bool assistPointsStoppage = assistStoppageDays > 0;
        bool overCap = assistPoints > CurAssistPointsCap;
        AdjustAssistPoints(InteractUtility.GetDailyAssistPointChange(assistPointsStoppage, overCap));
    }

    public void AdjustAssistPoints(int change, bool showMessage = true)
    {
        if (change == 0)
        {
            return;
        }

        int trueChange = assistPoints;
        assistPoints = Mathf.Clamp(assistPoints + change, 0, MaxAssistPointCap);
        trueChange = assistPoints - trueChange;

        if (showMessage && trueChange != 0)
        {
            if (trueChange > 0)
            {
                Messages.Message("OARK_AssistPointsAdded".Translate(trueChange), MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("OARK_AssistPointsReduced".Translate(-trueChange), MessageTypeDefOf.NeutralEvent);
            }
        }
    }

    public bool IsInCooldown(string key)
    {
        if (interactionCDRecords.TryGetValue(key, out OAInteractionCDRecord record))
        {
            return record.IsInCooldown;
        }
        return false;
    }

    public int GetCooldownTicksLeft(string key)
    {
        if (interactionCDRecords.TryGetValue(key, out OAInteractionCDRecord record))
        {
            return record.CooldownTicksLeft;
        }
        return -1;
    }

    public int GetTicksSinceLastActive(string key)
    {
        if (interactionCDRecords.TryGetValue(key, out OAInteractionCDRecord record))
        {
            return record.TicksSinceLastActive;
        }
        return -1;
    }

    public void RegisterCDRecord(string key, int cdTicks)
    {
        interactionCDRecords[key] = new OAInteractionCDRecord(cdTicks);
    }
    public void DeregisterCDRecord(string key)
    {
        interactionCDRecords.Remove(key);
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref CurAllianceInitTick, "curAllianceInitTick", -1);
        Scribe_Values.Look(ref oldAllianceDuration, "oldAllianceDuration", 0f);

        Scribe_Values.Look(ref assistPoints, "assistPoints", 0);
        Scribe_Values.Look(ref assistStoppageDays, "assistStoppageDays", 0);

        Scribe_Collections.Look(ref interactionCDRecords, "interactionCDRecords", LookMode.Value, LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            interactionCDRecords.RemoveAll(kv => kv.Key is null || kv.Value.lastActiveTick < 0);
        }
    }
}