using LudeonTK;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public static class DebugTools
{
    [DebugAction(category: "OberoniaAurea",
                 name: "DevWin-OAInteractHandler",
                 requiresRoyalty: false,
                 requiresIdeology: false,
                 requiresBiotech: false,
                 requiresAnomaly: false,
                 displayPriority: 450,
                 hideInSubMenu: false,
                 actionType = DebugActionType.Action,
                 allowedGameStates = AllowedGameStates.Playing)]
    private static void OpenOAInteractHandlerDevWindow()
    {
        if (OAInteractHandler.Instance is null)
        {
            Messages.Message("OAInteractHandler is null", MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        OAInteractHandler.OpenDevWindow();
    }
}
