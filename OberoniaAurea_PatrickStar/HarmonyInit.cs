using System.Reflection;
using HarmonyLib;
using Verse;

namespace OberoniaAurea_PatrickStar;

[StaticConstructorOnStartup]
public class HarmonyInit
{
	
    public static Harmony instance;

    static HarmonyInit()
    {
        instance = new Harmony("SR.NoDecreeQuest");
        instance.PatchAll(Assembly.GetExecutingAssembly());
    }
}
