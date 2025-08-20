using RimWorld;
using System;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class FactionDialogUtility
{
    public static DiaNode FinallyConfirmNode(TaggedString text, Faction faction, Pawn negotiator)
    {
        return new DiaNode(text)
        {
            options = { OKToRoot(faction, negotiator) }
        };
    }

    public static DiaOption OKToRoot(Faction faction, Pawn negotiator) //返回通讯台派系通讯初始界面
    {
        return new DiaOption("OK".Translate())
        {
            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
        };
    }

    public static DiaOption DiaOptionWithCooldown(TaggedString text, string key, Func<DiaNode> linkLateBind)
    {
        int cooldownTicksLeft = OAInteractHandler.Instance.CooldownManager.GetCooldownTicksLeft(key);

        DiaOption diaOption = new(text);
        if (cooldownTicksLeft > 0)
        {
            diaOption.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
        }
        else
        {
            diaOption.linkLateBind = linkLateBind;
        }

        return diaOption;
    }
}
