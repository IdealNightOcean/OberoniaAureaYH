using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class CompGravityTrapIED : CompDisplacementExplosive
{
    protected override void PostPawnDisplaced(Pawn pawn)
    {
        base.PostPawnDisplaced(pawn);
        pawn.health.GetOrAddHediff(OARK_HediffDefOf.OARK_Hediff_GravityTrapIEDShock);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        Command_Action command_TriggerStartWick = new()
        {
            defaultLabel = "OARK_Command_TriggerStartWick".Translate(),
            defaultDesc = "OARK_Command_TriggerStartWickDesc".Translate(),
            icon = null,
            action = delegate { StartWick(parent); },
        };

        yield return command_TriggerStartWick;
    }
}
