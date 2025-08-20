using OberoniaAurea_Frame;
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

        listing_Rect.Label($"CurAllianceInitTick: {interactHandler.CurAllianceInitTick}");
        listing_Rect.Label($"CurAllianceDuration: {allianceDuration:F2}");
        listing_Rect.Label($"OldAllianceDuration: {interactHandler.OldAllianceDuration:F2}");
        listing_Rect.Gap(3f);
        listing_Rect.Label($"AssistPoints: {interactHandler.AssistPoints}");
        listing_Rect.Label($"CurAssistPointsCap: {curAssistPointsCap}");
        listing_Rect.Gap(3f);
        listing_Rect.Label("BaseAssistPointCap: 40");
        listing_Rect.Label("MaxAssistPointCap: 300");
        listing_Rect.Gap(3f);
        if (listing_Rect.ButtonText("Interaction CD Records", null, 0.6f))
        {
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(interactHandler.CooldownManager.GetCDRecordsDetailInfo()));
        }

        if (Event.current.type == EventType.Layout)
        {
            viewRectHeight = listing_Rect.MaxColumnHeightSeen + 50f;
        }

        listing_Rect.End();
        Widgets.EndScrollView();
    }
}