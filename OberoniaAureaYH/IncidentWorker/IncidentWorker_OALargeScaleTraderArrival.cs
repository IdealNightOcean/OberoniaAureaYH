﻿using RimWorld;

namespace OberoniaAurea;

//重型商队到达事件
public class IncidentWorker_OALargeScaleTraderArrival : IncidentWorker_TraderCaravanArrival
{
    protected override PawnGroupKindDef PawnGroupKindDef => OARatkin_PawnGenerateDefOf.OA_Rk_LargeScaleTrader;
}
