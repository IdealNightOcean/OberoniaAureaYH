using OberoniaAurea_Frame;
using RimWorld;
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

    public int CurAssistPointsCap => InteractUtility.GetCurAssistPointsCap(AllianceDuration());

    private CooldownRecordManager cooldownManager;
    public CooldownRecordManager CooldownManager => cooldownManager;

    public OAInteractHandler()
    {
        OAFrame_MiscUtility.ValidateSingleton(Instance, nameof(Instance));
        Instance = this;
        cooldownManager ??= new();
    }
    public static void ClearStaticCache() => Instance = null;
    public static void OpenDevWindow() => Find.WindowStack.Add(new DevWin_OAInteractHandler());

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
        bool assistPointsStoppage = cooldownManager.IsInCooldown("AssistStoppage");
        bool overCap = assistPoints > CurAssistPointsCap;
        AdjustAssistPoints(InteractUtility.GetDailyAssistPointChange(assistPointsStoppage, overCap), showMessage: false);
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

    public void ExposeData()
    {
        Scribe_Values.Look(ref CurAllianceInitTick, "curAllianceInitTick", -1);
        Scribe_Values.Look(ref oldAllianceDuration, "oldAllianceDuration", 0f);

        Scribe_Values.Look(ref assistPoints, "assistPoints", 0);

        Scribe_Deep.Look(ref cooldownManager, "cooldownManager");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            cooldownManager ??= new();
        }
    }
}