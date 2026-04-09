using OberoniaAurea_Frame;
using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class DevWin_OAInteractHandler : Window
{
    private Vector2 scrollPosition;
    private float viewRectHeight;
    public override Vector2 InitialSize => new(450, 750);

    private readonly OAInteractHandler interactHandler;
    private readonly int curAssistPointsCap;
    private readonly float allianceDuration;
    public DevWin_OAInteractHandler()
    {
        doCloseX = true;
        interactHandler = OAInteractHandler.Instance;
        curAssistPointsCap = interactHandler.CurAssistPointsCap;
        allianceDuration = interactHandler.AllianceDuration();
    }

    public override void DoWindowContents(Rect inRect)
    {
        Rect viewRect = inRect.ContractedBy(8f);
        viewRect.height = viewRectHeight;
        Listing_Standard listing_Rect = new(inRect, () => scrollPosition)
        {
            ColumnWidth = viewRect.width
        };
        Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
        listing_Rect.Begin(viewRect);

        listing_Rect.Label($"当前联盟开始Tick: {interactHandler.CurAllianceInitTick}");
        listing_Rect.Label($"当前联盟持续时间 (Day): {allianceDuration:F2}");
        listing_Rect.Label($"前联盟持续时间 (Day): {interactHandler.OldAllianceDuration:F2}");
        listing_Rect.Gap(6f);
        listing_Rect.Label($"贡献点: {interactHandler.AssistPoints}");
        listing_Rect.Label($"当前贡献点上限: {curAssistPointsCap}");
        listing_Rect.Gap(3f);
        listing_Rect.Label("自然贡献点上限: 40");
        listing_Rect.Label("最大贡献点上限: 300");
        listing_Rect.Gap(3f);
        int cooldownTicksLeft = ModUtility.CooldownManager.GetCooldownTicksLeft("AssistStoppage");
        if (cooldownTicksLeft > 0)
        {
            listing_Rect.Label($"贡献点自然增长停止: {cooldownTicksLeft.ToStringTicksToPeriod()}");
        }
        if (listing_Rect.ButtonText("+10 贡献点", widthPct: 0.5f))
        {
            interactHandler.AdjustAssistPoints(10);
        }
        if (listing_Rect.ButtonText("-10 贡献点", widthPct: 0.5f))
        {
            interactHandler.AdjustAssistPoints(-10);
        }
        listing_Rect.Gap(12f);
        if (listing_Rect.ButtonText("交互CD记录"))
        {
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(ModUtility.CooldownManager.GetCDRecordsDetailInfo()));
        }

        if (Event.current.type == EventType.Layout)
        {
            viewRectHeight = listing_Rect.MaxColumnHeightSeen + 50f;
        }

        listing_Rect.End();
        Widgets.EndScrollView();
    }
}