using RimWorld;
using Verse;

namespace OberoniaAurea_Hyacinth;

public class StatPart_RatkinHarmTurret : StatPart
{
    public float factor = 0.05f;

    private bool applied;

    [Unsaved]
    GameComponent_Hyacinth hyacinthGameComp;

    GameComponent_Hyacinth HyacinthGameComp => hyacinthGameComp ??= Hyacinth_Utility.HyacinthGameComp;


    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!req.HasThing)
        {
            return;
        }

        Thing thing = req.Thing;
        if (!thing.Spawned)
        {
            return;
        }
        applied = HyacinthGameComp?.RatkinHarmTurretCheck(thing) ?? false;
        if (applied)
        {
            val *= factor;
        }
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!req.HasThing)
        {
            return null;
        }
        if (applied)
        {
            return "OA_ElectromagneticInterference".Translate(factor.ToStringPercent());
        }
        return null;
    }
}
