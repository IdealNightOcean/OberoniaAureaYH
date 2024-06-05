using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class FactionDialogUtility
{
    public static DiaOption OKToRoot(Faction faction, Pawn negotiator) //返回通讯台派系通讯初始界面
    {
        return new DiaOption("OK".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
        };
    }
}
