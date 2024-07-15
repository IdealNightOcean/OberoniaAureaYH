using Verse;

namespace OberoniaAurea;

public class Gene_TemperatureAdaptation : Gene
{
    public override void PostAdd()
    {
        base.PostAdd();
        if (!pawn.Dead && pawn.RaceProps.Humanlike && pawn.needs.mood != null)
        {
            pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
        }
    }
    public override void PostRemove()
    {
        base.PostRemove();
        if (!pawn.Dead && pawn.RaceProps.Humanlike && pawn.needs.mood != null)
        {
            pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
        }
    }
}
