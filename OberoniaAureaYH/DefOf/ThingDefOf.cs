using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ThingDefOf
{
    public static ThingDef OA_RK_CircuitRegulator;
    public static ThingDef OA_RK_Cloth_Processing_B;
    public static ThingDef Oberonia_Aurea_Tea; //花茶
    public static ThingDef Oberonia_Aurea_Chanwu_AB; //花糕
    public static ThingDef Oberonia_Aurea_Chanwu_AC; //流心花糕
    public static ThingDef Oberonia_Aurea_Chanwu_F; //浓缩液
    public static ThingDef Oberonia_Aurea_Chanwu_G; //提取液

    public static ThingDef OARatkin_ResearchAnalyzer; //科研分析仪
    public static ThingDef OA_RK_EMPInst;
    static OARK_ThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ThingDefOf));
    }
}