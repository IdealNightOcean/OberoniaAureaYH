using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

internal static class IntelligenceDialogMaker
{
    private static int NormalIntelligenceCount = -1;
    private static int SecretIntelligenceCount = -1;

    /// <summary>
    /// 外交交互选项
    /// </summary>
    internal static DiaOption IntelligenceOpt(FactionDialogCache dialogCache)
    {
        NormalIntelligenceCount = dialogCache.Map.GetCountOfThingDef(OARK_ThingDefOf.OARK_IntelligenceNormal);
        SecretIntelligenceCount = dialogCache.Map.GetCountOfThingDef(OARK_ThingDefOf.OARK_IntelligenceSecret);
        if (NormalIntelligenceCount <= 0 && SecretIntelligenceCount <= 0)
        {
            return null;
        }

        DiaOption diaOption = new("OARK_CommsConsloe_Intelligence".Translate())
        {
            linkLateBind = () => Intelligence_TypeNode(dialogCache)
        };
        if (!dialogCache.NegotiatorSocialEnabled)
        {
            diaOption.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
        }

        return diaOption;
    }

    private static DiaNode Intelligence_TypeNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CommsConsloe_IntelligenceNode".Translate());

        DiaOption normalOpt = new(OARK_ThingDefOf.OARK_IntelligenceNormal.LabelCap)
        {
            linkLateBind = () => NormalIntelligenceNode(dialogCache)
        };
        diaNode.options.Add(normalOpt);

        DiaOption secretOpt = new(OARK_ThingDefOf.OARK_IntelligenceSecret.LabelCap)
        {
            linkLateBind = () => SecretIntelligenceNode(dialogCache)
        };
        diaNode.options.Add(secretOpt);

        //返回按钮
        diaNode.AddDefaultGoBackOption(linkLateBind: FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator));

        return diaNode;
    }

    private static DiaNode NormalIntelligenceNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CommsConsloe_NormalIntelligenceNode".Translate());
        AddCountOption(1);
        AddCountOption(5);
        AddCountOption(10);
        AddCountOption(25);
        AddCountOption(50);
        AddCountOption(75);
        AddCountOption(100);
        AddCountOption(500);
        diaNode.AddDefaultGoBackOption(linkLateBind: () => Intelligence_TypeNode(dialogCache));
        return diaNode;

        void AddCountOption(int count)
        {
            DiaOption diaOption = new($"{OARK_ThingDefOf.OARK_IntelligenceNormal.LabelCap} × {count}")
            {

                linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("", dialogCache.Faction, dialogCache.Negotiator)
            };

            if (NormalIntelligenceCount < count)
            {
                diaOption.Disable("OAFrame_NeedCountOfThing".Translate(OARK_ThingDefOf.OARK_IntelligenceNormal.LabelCap, count));
            }

            diaNode.options.Add(diaOption);
        }
    }

    private static DiaNode SecretIntelligenceNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CommsConsloe_SecretIntelligenceNode".Translate());
        AddCountOption(1);
        AddCountOption(5);
        AddCountOption(10);
        AddCountOption(25);
        AddCountOption(50);
        AddCountOption(75);
        AddCountOption(100);
        AddCountOption(500);
        diaNode.AddDefaultGoBackOption(linkLateBind: () => Intelligence_TypeNode(dialogCache));
        return diaNode;

        void AddCountOption(int count)
        {
            DiaOption diaOption = new($"{OARK_ThingDefOf.OARK_IntelligenceNormal.LabelCap} × {count}")
            {

                linkLateBind = () => FactionDialogUtility.FinallyConfirmNode("", dialogCache.Faction, dialogCache.Negotiator)
            };
            if (SecretIntelligenceCount < count)
            {
                diaOption.Disable("OAFrame_NeedCountOfThing".Translate(OARK_ThingDefOf.OARK_IntelligenceNormal.LabelCap, count));
            }
            diaNode.options.Add(diaOption);
        }
    }
}
