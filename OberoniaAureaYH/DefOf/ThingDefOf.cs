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
    public static ThingDef OARK_ComponentBox; //零部件盒

    [MayRequireOdyssey]
    public static ThingDef OARK_SDCommunicationEquipment; //科技部通讯设备
    [MayRequireOdyssey]
    public static ThingDef OARK_AirportClearanceBeaconDeactive; //失效的空港许可信标
    [MayRequireOdyssey]
    public static ThingDef OARK_BrokenPilotConsole; // 损坏的飞船控制台
    [MayRequireOdyssey]
    public static ThingDef OARK_UnlockedFile_EncryptedFile; // 机密文件副本
    [MayRequireOdyssey]
    public static ThingDef OARK_SpaceportAccessPermit; // 太空港通行许可
    [MayRequireOdyssey]
    public static ThingDef OARK_CrashedScienceShip; // 坠毁的科学舱

    public static ThingDef OA_RK_EMPInst;
    static OARK_ThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ThingDefOf));
    }
}