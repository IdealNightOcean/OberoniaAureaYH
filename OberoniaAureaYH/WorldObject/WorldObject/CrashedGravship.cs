using OberoniaAurea_Frame;
using RimWorld;
using System;
using Verse;

namespace OberoniaAurea;

public class CrashedGravship : MapParent_Enterable
{
    public override void PostMapGenerate()
    {
        base.PostMapGenerate();
        try
        {
            Map map = Map;
            if (map is null || map.listerThings is null)
            {
                return;
            }

            Thing firefoamPopper = map.listerThings.ThingsOfDef(ThingDefOf.FirefoamPopper)?.FirstOrFallback(null);
            if (firefoamPopper is not null)
            {
                IntVec3 firefoamPopperPos = firefoamPopper.Position;

                GenExplosion.DoExplosion(firefoamPopperPos, map, 10f, DamageDefOf.Bomb, null, damAmount: 10);
                GenExplosion.DoExplosion(firefoamPopperPos, map, 3f, DamageDefOf.Flame, null, damAmount: 5);
            }

            Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_EnterCrashedGravship".Translate(),
                                           text: "OARK_Letter_EnterCrashedGravship".Translate(),
                                           textLetterDef: LetterDefOf.NeutralEvent,
                                           lookTargets: null,
                                           relatedFaction: ModUtility.OAFaction);
        }
        catch (Exception ex)
        {
            Log.Error($"Exception occurred in {nameof(PostMapGenerate)}: {ex}");
        }
    }
}
