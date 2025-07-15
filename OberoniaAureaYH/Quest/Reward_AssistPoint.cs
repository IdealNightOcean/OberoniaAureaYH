using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public class Reward_AssistPoint : Reward
{
    public int amount;

    public override IEnumerable<GenUI.AnonymousStackElement> StackElements
    {
        get
        {
            yield return QuestPartUtility.GetStandardRewardStackElement(label: "OARK_AssistPoint".Translate() + " " + amount.ToStringWithSign(),
                                                                        iconDrawer: delegate (Rect r)
                                                                        {
                                                                            GUI.DrawTexture(r, ModUtility.OAFaction.def.RoyalFavorIcon);
                                                                            GUI.color = Color.white;
                                                                        },
                                                                        tipGetter: () => "OARK_AssistPointTip".Translate(ModUtility.OAFaction).Resolve(), delegate
                                                                        {
                                                                            Find.WindowStack.Add(new Dialog_InfoCard(ModUtility.OAFaction));
                                                                        });
        }
    }

    public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
    {
        amount = Mathf.Max(amount, 0);
        valueActuallyUsed = amount;
    }

    public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
    {
        yield return new QuestPart_OaAssistPointsChange(inSignalTrigger: QuestGen.slate.Get<string>("inSignal"),
                                                        change: amount);
    }

    public override string GetDescription(RewardsGeneratorParams parms)
    {
        return "OARK_Reward_AssistPointTip".Translate(amount).Resolve();
    }

    public override string ToString()
    {
        return GetType().Name + " (faction=" + ModUtility.OAFaction.ToStringSafe() + ", amount=" + amount + ")";
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref amount, "amount", 0);
    }
}