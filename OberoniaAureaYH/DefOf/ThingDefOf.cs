using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ThingDefOf
{
    public static ThingDef OA_RK_CircuitRegulator;
    public static ThingDef OA_RK_Cloth_Processing_B;
    ///<summary>花茶</summary>
    public static ThingDef Oberonia_Aurea_Tea;
    ///<summary>花糕</summary>
    public static ThingDef Oberonia_Aurea_Chanwu_AB;
    ///<summary>流心花糕</summary>
    public static ThingDef Oberonia_Aurea_Chanwu_AC;
    ///<summary>浓缩液</summary>
    public static ThingDef Oberonia_Aurea_Chanwu_F;
    ///<summary>提取液</summary>
    public static ThingDef Oberonia_Aurea_Chanwu_G;

    ///<summary>科研分析仪</summary>
    public static ThingDef OARatkin_ResearchAnalyzer;
    ///<summary>零部件盒</summary>
    public static ThingDef OARK_ComponentBox;

    ///<summary>金鸢尾兰花环</summary>
    public static ThingDef OA_RK_New_Hat_A;


    [MayRequireOdyssey]
    ///<summary>科研部逆重数据信标</summary>
    public static ThingDef OARK_GravDataBeacon;
    [MayRequireOdyssey]
    ///<summary>科技部通讯设备</summary>
    public static ThingDef OARK_SDCommunicationEquipment;
    [MayRequireOdyssey]
    ///<summary>失效的空港许可信标</summary>
    public static ThingDef OARK_AirportClearanceBeaconDeactive;
    [MayRequireOdyssey]
    ///<summary>损坏的飞船控制台</summary>
    public static ThingDef OARK_BrokenPilotConsole;
    [MayRequireOdyssey]
    ///<summary>机密文件副本</summary>
    public static ThingDef OARK_UnlockedFile_EncryptedFile;
    [MayRequireOdyssey]
    ///<summary>金鸢尾兰空港许可信标</summary>
    public static ThingDef OARK_AirportClearanceBeacon;
    [MayRequireOdyssey]
    ///<summary>坠毁的科学舱</summary>
    public static ThingDef OARK_CrashedScienceShip;
    [MayRequireOdyssey]
    ///<summary>科研部账目资料</summary>
    public static ThingDef OARK_SDAccountingRecord;
    [MayRequireOdyssey]
    ///<summary>统筹官气泡</summary>
    public static ThingDef OARO_Mote_Referendary;

    public static ThingDef OA_RK_EMPInst;
    static OARK_ThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ThingDefOf));
    }
}