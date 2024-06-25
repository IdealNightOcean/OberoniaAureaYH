﻿using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Gene_HotAdaptation : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!ModsConfig.BiotechActive)
        {
            return ThoughtState.Inactive;
        }
        float ambientTemperature = p.AmbientTemperature;
        if (ambientTemperature > 45f)
        {
            return ThoughtState.ActiveDefault;
        }

        return ThoughtState.Inactive;
    }
}