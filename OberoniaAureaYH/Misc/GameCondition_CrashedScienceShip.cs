using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class GameCondition_CrashedScienceShip : GameCondition
{
    public override bool ElectricityDisabled => disableElectricity;

    private ScienceShipRecord.EnvirAffectType curEnvirType;
    private HediffDef curAffectHediff;
    private bool disableElectricity;

    private int ticksToNextHediffApply = 600;
    private string curLabel = string.Empty;
    private string curDescription = string.Empty;

    public override string Label => curLabel;
    public override string Description => curDescription;

    public void SetCondition(ScienceShipRecord.EnvirAffectType curEnvirType, HediffDef affectHediff)
    {
        this.curEnvirType = curEnvirType;
        curAffectHediff = affectHediff;
        disableElectricity = curEnvirType == ScienceShipRecord.EnvirAffectType.DisorderedMagnetism;

        curLabel = ScienceShipRecord.GetShipEnvirLabel(curEnvirType);
        curDescription = ScienceShipRecord.GetShipEnvirText(curEnvirType);
    }

    public override void GameConditionTick()
    {
        base.GameConditionTick();

        if (--ticksToNextHediffApply < 0)
        {
            ticksToNextHediffApply = 600;
            if (curAffectHediff is null || SingleMap is null)
            {
                ticksToNextHediffApply = 5000;
                return;
            }
            IReadOnlyList<Pawn> mapPawns = SingleMap.mapPawns.AllPawnsSpawned;
            if (disableElectricity)
            {
                for (int i = mapPawns.Count - 1; i >= 0; i--)
                {
                    if (mapPawns[i].RaceProps.IsMechanoid)
                    {
                        mapPawns[i].health.AddHediff(curAffectHediff);
                    }
                }
            }
            else
            {
                for (int i = mapPawns.Count - 1; i >= 0; i--)
                {
                    mapPawns[i].health.AddHediff(curAffectHediff);
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref curAffectHediff, "curAffectHediff");
        Scribe_Values.Look(ref disableElectricity, "disableElectricity", defaultValue: false);
        Scribe_Values.Look(ref curEnvirType, "curEnvirType");
        Scribe_Values.Look(ref curLabel, "curLabel");
        Scribe_Values.Look(ref curDescription, "curDescription");
    }
}
