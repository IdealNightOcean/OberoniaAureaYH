using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class CompProperties_ComponentBox : CompProperties
{
    public int industrialCount = 15;
    public int spacerCount = 1;

    public CompProperties_ComponentBox()
    {
        compClass = typeof(CompComponentBox);
    }
}

public class CompComponentBox : ThingComp
{
    private CompProperties_ComponentBox Props => (CompProperties_ComponentBox)props;
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = "OARK_OpenOne".Translate(),
            defaultDesc = "OARK_ComponentBoxDesc".Translate(parent.Label),
            icon = parent.def.uiIcon,
            action = OpenBoxOne
        };

        yield return new Command_Action
        {
            defaultLabel = "OARK_OpenAll".Translate(),
            defaultDesc = "OARK_ComponentBoxDesc".Translate(parent.Label),
            icon = parent.def.uiIcon,
            action = OpenBoxAll
        };
    }

    private void OpenBoxOne()
    {
        Thing industrial = ThingMaker.MakeThing(ThingDefOf.ComponentIndustrial);
        industrial.stackCount = Props.industrialCount;
        Thing spacer = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
        spacer.stackCount = Props.spacerCount;
        GenPlace.TryPlaceThing(industrial, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near);
        GenPlace.TryPlaceThing(spacer, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near);
        parent.SplitOff(1).Destroy();
    }

    private void OpenBoxAll()
    {
        Thing industrial = ThingMaker.MakeThing(ThingDefOf.ComponentIndustrial);
        industrial.stackCount = Props.industrialCount * parent.stackCount;
        Thing spacer = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
        spacer.stackCount = Props.spacerCount * parent.stackCount;
        GenPlace.TryPlaceThing(industrial, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near);
        GenPlace.TryPlaceThing(spacer, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near);
        parent.Destroy();
    }
}
