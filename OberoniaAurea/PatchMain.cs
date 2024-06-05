using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace OberoniaAurea;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class PatchMain
{
	private static Harmony harmonyInstance;

	internal static Harmony HarmonyInstance
	{
		get
		{
			if (harmonyInstance == null)
			{
				harmonyInstance = new Harmony("OberoniaAurea_Harmony");
			}
			return harmonyInstance;
		}
	}

	static PatchMain()
	{
		HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
	}
}
