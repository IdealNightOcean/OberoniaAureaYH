using OberoniaAurea_Frame;
using RimWorld;
using UnityEngine;
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
            linkLateBind = () => Intelligence_NumberSelectNode(dialogCache, isSecret:false)
        };
        diaNode.options.Add(normalOpt);

        DiaOption secretOpt = new(OARK_ThingDefOf.OARK_IntelligenceSecret.LabelCap)
        {
            linkLateBind = () => Intelligence_NumberSelectNode(dialogCache, isSecret: true)
        };
        diaNode.options.Add(secretOpt);

        //返回按钮
        diaNode.AddDefaultGoBackOption(linkLateBind: FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator));

        return diaNode;
    }

    private static DiaNode Intelligence_NumberSelectNode(FactionDialogCache dialogCache,bool isSecret)
    {
        ThingDef intelligenceDef = isSecret ? OARK_ThingDefOf.OARK_IntelligenceSecret : OARK_ThingDefOf.OARK_IntelligenceNormal;
        DiaNode diaNode = new("OARK_Intelligence_NumberSelectNode".Translate(intelligenceDef.Named(KeyLibrary_NamedArgument.DEF)));
        float marketValue = intelligenceDef.GetStatValueAbstract(StatDefOf.MarketValue);

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
            int silverGain = Mathf.CeilToInt(marketValue * count);
            DiaOption diaOption = new($"{intelligenceDef.LabelCap} × {count}")
            {
                action = delegate
                {
                    dialogCache.Map.DestroyThingsOfDef(intelligenceDef, count);
                    OAFrame_DropPodUtility.DefaultDropThingOfDef(ThingDefOf.Silver, silverGain, dialogCache.Map);
                },
                linkLateBind = () => FactionDialogUtility.FinallyConfirmNode(
                    "OARK_Intelligence_ConfirmNode".Translate(intelligenceDef.Named(KeyLibrary_NamedArgument.DEF),
                    count.Named(KeyLibrary_NamedArgument.Count),
                    silverGain.Named(KeyLibrary_NamedArgument.Value)),
                dialogCache.Faction,
                dialogCache.Negotiator)
            };

            if (isSecret && (SecretIntelligenceCount < count))
                diaOption.Disable("OAFrame_NeedCountOfThing".Translate(intelligenceDef.LabelCap, count));

            if (!isSecret && (NormalIntelligenceCount < count))
                diaOption.Disable("OAFrame_NeedCountOfThing".Translate(intelligenceDef.LabelCap, count));

            diaNode.options.Add(diaOption);
        }
    }
}
