using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class CompProperties_GravityTrapIED : CompProperties_DisplacementExplosive
{
    public HediffDef giveHediff;
    public HediffDef immuneHediff;

    public CompProperties_GravityTrapIED()
    {
        compClass = typeof(CompGravityTrapIED);
    }
}

public class CompGravityTrapIED : CompDisplacementExplosive
{
    private new CompProperties_GravityTrapIED Props => (CompProperties_GravityTrapIED)props;

    protected override void PostPawnDisplaced(Pawn pawn)
    {
        base.PostPawnDisplaced(pawn);
        if (!pawn.health.hediffSet.HasHediff(Props.immuneHediff))
        {
            pawn.health.GetOrAddHediff(Props.giveHediff);
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        if (!wickStarted)
        {
            Command_Action command_TriggerStartWick = new()
            {
                defaultLabel = "OARK_Command_TriggerStartWick".Translate(),
                defaultDesc = "OARK_Command_TriggerStartWickDesc".Translate(),
                icon = IconUtility.FlashIcon,
                action = delegate { StartWick(parent); },
            };

            yield return command_TriggerStartWick;
        }
    }
}
