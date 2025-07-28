using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
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

            Thing gravEngine = map.listerThings.ThingsOfDef(ThingDefOf.GravEngine)?.FirstOrFallback(null);
            if (gravEngine is not null)
            {
                IntVec3 gravEnginePos = gravEngine.Position;
                gravEngine.Kill();
                GenExplosion.DoExplosion(gravEnginePos, map, 10f, DamageDefOf.Bomb, null, damAmount: 25);
                GenExplosion.DoExplosion(gravEnginePos, map, 3f, DamageDefOf.Flame, null, damAmount: 5);
            }

            Thing pilotConsole = map.listerThings.ThingsOfDef(ThingDefOf.PilotConsole)?.FirstOrFallback(null);
            if (pilotConsole is not null)
            {
                IntVec3 pilotConsolePos = pilotConsole.Position;
                Rot4 pilotConsoleRot = pilotConsole.Rotation;
                pilotConsole.Kill();
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(OARK_ThingDefOf.OARK_BrokenPilotConsole), pilotConsolePos, map, ThingPlaceMode.Direct, rot: pilotConsoleRot);
            }

            List<Thing> gravshipHulls = map.listerThings.ThingsOfDef(ThingDefOf.GravshipHull);
            if (gravshipHulls is not null)
            {
                Thing hull;
                for (int i = gravshipHulls.Count - 1; i >= 0; i--)
                {
                    hull = gravshipHulls[i];
                    hull.HitPoints -= Rand.Range(150, 450);
                    if (hull.HitPoints <= 0)
                    {
                        hull.Kill();
                    }
                }
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
