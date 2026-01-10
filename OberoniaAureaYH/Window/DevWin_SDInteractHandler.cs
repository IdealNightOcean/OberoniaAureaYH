using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class DevWin_SDInteractHandler : Window
{
    private Vector2 scrollPosition;
    private float viewRectHeight;
    public override Vector2 InitialSize => new(450, 750);

    private readonly ScienceDepartmentInteractHandler sdInteractHandler;
    private readonly string shipName = null;

    public DevWin_SDInteractHandler()
    {
        doCloseX = true;
        draggable = true;
        sdInteractHandler = ScienceDepartmentInteractHandler.Instance;
        if (sdInteractHandler.ScienceShipRecord.HasValue)
        {
            shipName = sdInteractHandler.ScienceShipRecord.Value.ShipName;
        }
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
        listing_Rect.Label($"当前科研阶段索引: {sdInteractHandler.CurGravTechStage - 1}");
        listing_Rect.Label($"当前科研阶段: {sdInteractHandler.CurGravTechStage}");

        listing_Rect.Gap(6f);
        listing_Rect.Label($"当前科研点数: {sdInteractHandler.GravTechPoints}");
        listing_Rect.Label($"玩家贡献科研点数: {sdInteractHandler.PlayerTechPoints}");
        listing_Rect.Label($"当前科研贡献点: {sdInteractHandler.GravTechAssistPoints}");

        if (listing_Rect.ButtonText("+2000 科技点", widthPct: 0.5f))
        {
            sdInteractHandler.AddGravTechPoints(2000, byPlayer: false);
        }
        if (listing_Rect.ButtonText("-2000 科技点", widthPct: 0.5f))
        {
            sdInteractHandler.AddGravTechPoints(-2000, byPlayer: false);
        }
        if (listing_Rect.ButtonText("+100 科技贡献点", widthPct: 0.5f))
        {
            sdInteractHandler.AdjustGravTechAssistPoint(100);
        }
        if (listing_Rect.ButtonText("-100 科技贡献点", widthPct: 0.5f))
        {
            sdInteractHandler.AdjustGravTechAssistPoint(-100);
        }
        listing_Rect.Gap(6f);
        listing_Rect.Label($"科技部初见任务是否已完成: {sdInteractHandler.IsInitGravQuestCompleted}");
        if (sdInteractHandler.GravResearchAssistLendPawn is null)
        {
            listing_Rect.Label("科技部借调人员: 无");
        }
        else
        {
            listing_Rect.Label($"科技部借调人员: {sdInteractHandler.GravResearchAssistLendPawn}");
        }

        listing_Rect.Gap(6f);
        listing_Rect.Label($"统筹部审查冻结科研点增长（天）: {sdInteractHandler.GravTechPointEMBlockDays}");
        listing_Rect.Label($"上次统筹部审查结果: {sdInteractHandler.LastEconomyReviewOutcome}");
        listing_Rect.Label($"上次统筹部审查结果: {sdInteractHandler.LastEconomyReviewTick}");

        listing_Rect.Gap(6f);
        if (shipName is null)
        {
            listing_Rect.Label("存在的科研船: 无");
        }
        else
        {
            listing_Rect.Label($"存在的科研船: {shipName}");
        }
        listing_Rect.Gap(6f);
        listing_Rect.Label($"今日通讯台值班文本: {sdInteractHandler.TodayDutyText}");
        listing_Rect.Label($"距离通讯台值班文本切换（天）: {sdInteractHandler.NextShiftWorkDay}");

        if (Event.current.type == EventType.Layout)
        {
            viewRectHeight = listing_Rect.MaxColumnHeightSeen + 50f;
        }

        listing_Rect.End();
        Widgets.EndScrollView();
    }
}