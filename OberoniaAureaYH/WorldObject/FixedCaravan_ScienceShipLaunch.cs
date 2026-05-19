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
        if (associatedWorldObject is ScienceShipLaunchSite launchSite)
        {
            foreach (Gizmo interactGizmo in launchSite.InteractGizmos())
            {
                yield return interactGizmo;
            }
        }
    }
}
