using OberoniaAurea_Frame;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class FixedCaravan_ScienceShipLaunch : FixedCaravan
{
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        Command_Action command_Quiz = (associatedWorldObject as ScienceShipLaunchSite).QuizCommand();
        if (command_Quiz is not null)
        {
            yield return command_Quiz;
        }
    }
}
