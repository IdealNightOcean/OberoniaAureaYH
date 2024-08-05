using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(WorldPawnGC), "GetCriticalPawnReason")]
public static class GetCriticalPawnReason_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, Pawn pawn)
    {
        if (__result == null || !pawn.Discarded)
        {
            if (pawn.IsFixedCaravanMember())
            {
                __result = "OA_FixedCaravanMember";
            }
        }
    }
}
