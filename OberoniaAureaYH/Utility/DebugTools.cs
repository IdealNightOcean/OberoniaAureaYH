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
                 requiresOdyssey: false,
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

    [DebugAction(category: "OberoniaAurea",
             name: "DevWin-SDInteractHandler",
             requiresRoyalty: false,
             requiresIdeology: false,
             requiresBiotech: false,
             requiresAnomaly: false,
             requiresOdyssey: false,
             displayPriority: 440,
             hideInSubMenu: false,
             actionType = DebugActionType.Action,
             allowedGameStates = AllowedGameStates.Playing)]
    private static void OpenSDInteractHandlerDevWindow()
    {
        if (ScienceDepartmentInteractHandler.Instance is null)
        {
            Messages.Message("ScienceDepartmentInteractHandler is null", MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        ScienceDepartmentInteractHandler.OpenDevWindow();
    }
}
