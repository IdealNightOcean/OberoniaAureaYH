using UnityEngine;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class IconUtility
{
    public static readonly Texture2D OADipIcon = ContentFinder<Texture2D>.Get("World/OA_RK_SettlementDip");
    public static readonly Texture2D FlashIcon = ContentFinder<Texture2D>.Get("Ui/OA_RK_EMP_Jineng");
}
