using UnityEngine;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class IconUtility
{
    public static readonly Texture2D OADipIcon = ContentFinder<Texture2D>.Get("World/OA_RK_SettlementDip");
    public static readonly Texture2D FlashIcon = ContentFinder<Texture2D>.Get("Ui/OA_RK_EMP_Jineng");

    public static readonly Texture2D IncreaseGravIcon = ContentFinder<Texture2D>.Get("UI/OARK_IncreaseGrav");
    public static readonly Texture2D DecreaseGravIcon = ContentFinder<Texture2D>.Get("UI/OARK_Decrease");
    public static readonly Texture2D ReassignmentGravIcon = ContentFinder<Texture2D>.Get("UI/OARK_ReassignmentGrav");
}
