using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public static class PawnUtility
{
    public static bool IsOnBirthday(this Pawn pawn)
    {
        if (pawn is null || pawn.ageTracker is null)
            return false;

        return (float)pawn.ageTracker.AgeBiologicalTicks - pawn.ageTracker.AgeBiologicalYears * 3600000f <= 60000f;
    }

    public static bool IsColonistOnFirstBirthdayPerYear(this Pawn pawn)
    {
        if (pawn is null || pawn.ageTracker is null)
            return false;

        if (!pawn.Faction.IsPlayerSafe())
            return false;

        return SpecialGlobalEventManager.Instance.IsPawnOnFirstBirthdayPerYear(pawn, addIfMiss: true);
    }

    public static bool IsOberoniaAureaPawn(this Pawn pawn)
    {
        if (pawn is null || !pawn.RaceProps.Humanlike || pawn.story is null)
        {
            return false;
        }

        if (pawn.story.Childhood?.HasModExtension<OberoniaAureaBackstoryFlag>() ?? false)
        {
            return true;
        }
        if (pawn.story.Adulthood?.HasModExtension<OberoniaAureaBackstoryFlag>() ?? false)
        {
            return true;
        }

        return false;
    }

    public static IEnumerable<Pawn> GetLendColonistsFromQuest(Quest quest)
    {
        return quest?.PartsListForReading?.OfType<QuestPart_LendColonistsToFaction>()
                                          .FirstOrFallback(null)
                                         ?.LentColonistsListForReading
                                         ?.OfType<Pawn>()
                                         ?? [];
    }
}
