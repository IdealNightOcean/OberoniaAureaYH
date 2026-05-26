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

    private int curAllianceInitTick = -1;
    /// <summary>
    /// 本次结盟的起始时刻
    /// </summary>
    public int CurAllianceInitTick
    {
        get => curAllianceInitTick;
        set => curAllianceInitTick = value;
    }

    private float oldAllianceDuration;
    /// <summary>
    /// 记录旧结盟时间
    /// </summary>
    public float OldAllianceDuration
    {
        get { return oldAllianceDuration; }
        set { oldAllianceDuration = Mathf.Max(0f, value); }
    }

    private int assistPoints;
    public int AssistPoints => assistPoints;

    private float diplomaticSummitFactor = 0f;
    /// <summary>
    /// 利益谈判难度修正系数
    /// </summary>
    public float DiplomaticSummitFactor
    {
        get => diplomaticSummitFactor;
        set => diplomaticSummitFactor = Mathf.Max(0f, value);
    }

    public int CurAssistPointsCap => InteractUtility.GetCurAssistPointsCap(AllianceDuration());

    public OAInteractHandler()
    {
        OAFrame_MiscUtility.ValidateSingleton(Instance, nameof(Instance));
        Instance = this;
    }
    public static void ClearStaticCache() => Instance = null;
    public static void OpenDevWindow() => Find.WindowStack.Add(new DevWin_OAInteractHandler());

    public void ExposeData()
    {
        Scribe_Values.Look(ref curAllianceInitTick, nameof(curAllianceInitTick), defaultValue: -1);
        Scribe_Values.Look(ref oldAllianceDuration, nameof(oldAllianceDuration), defaultValue: 0f);
        Scribe_Values.Look(ref diplomaticSummitFactor, nameof(diplomaticSummitFactor), defaultValue: 1f);

        Scribe_Values.Look(ref assistPoints, nameof(assistPoints), defaultValue: 0);
    }

    internal void TickDay()
    {
        DailyAssistPointChange();
        if (ModUtility.OAFaction.PlayerRelationKind == FactionRelationKind.Ally)
        {
            DiplomaticSummitFactor -= 0.005f;
        }
    }

    public void OnDiplomaticSummitTriggered()
    {
        DiplomaticSummitFactor = Mathf.Max(diplomaticSummitFactor, 1f);
        DiplomaticSummitFactor += 0.2f;
    }

    /// <summary>
    /// 金鼠结盟时长（天）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float AllianceDuration()
    {
        return CurAllianceInitTick < 0 ? -1f : (oldAllianceDuration + (Find.TickManager.TicksGame - CurAllianceInitTick).TicksToDays());
    }

    private void DailyAssistPointChange()
    {
        bool assistPointsStoppage = ModUtility.CooldownManager.IsInCooldown(KeyLibrary_CDRecord.AssistStoppage);
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
}