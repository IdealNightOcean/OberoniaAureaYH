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
        listing_Rect.Label($"CurGravTechStageIndex: {sdInteractHandler.CurGravTechStage - 1}");
        listing_Rect.Label($"CurGravTechStage: {sdInteractHandler.CurGravTechStage}");

        listing_Rect.Gap(3f);
        listing_Rect.Label($"GravTechPoints: {sdInteractHandler.GravTechPoints}");
        listing_Rect.Label($"PlayerTechPoints: {sdInteractHandler.PlayerTechPoints}");
        listing_Rect.Label($"GravTechAssistPoints: {sdInteractHandler.GravTechAssistPoints}");

        listing_Rect.Gap(3f);
        listing_Rect.Label($"IsInitGravQuestCompleted: {sdInteractHandler.IsInitGravQuestCompleted}");
        if (sdInteractHandler.GravResearchAssistLendPawn is null)
        {
            listing_Rect.Label("GravResearchAssistLendPawn: None");
        }
        else
        {
            listing_Rect.Label($"GravResearchAssistLendPawn: {sdInteractHandler.GravResearchAssistLendPawn}");
        }

        listing_Rect.Gap(3f);
        if (shipName is null)
        {
            listing_Rect.Label("ScienceShipRecord: None");
        }
        else
        {
            listing_Rect.Label($"ScienceShipRecord: {shipName}");
        }
        listing_Rect.Gap(3f);
        listing_Rect.Label($"ToDayDutyText: {sdInteractHandler.ToDayDutyText}");
        listing_Rect.Label($"NextShiftWorkDay: {sdInteractHandler.NextShiftWorkDay}");

        if (Event.current.type == EventType.Layout)
        {
            viewRectHeight = listing_Rect.MaxColumnHeightSeen + 50f;
        }

        listing_Rect.End();
        Widgets.EndScrollView();
    }
}