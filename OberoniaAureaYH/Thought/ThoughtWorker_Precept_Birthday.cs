using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Precept_Birthday : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p) => ThoughtState.ActiveDefault;

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!p.IsOnBirthday())
        {
            return ThoughtState.Inactive;
        }
        if (p.Faction.IsPlayerSafe() && !p.IsColonistOnFirstBirthdayPerYear())
        {
            return ThoughtState.Inactive;
        }
        return base.CurrentStateInternal(p);
    }
}
