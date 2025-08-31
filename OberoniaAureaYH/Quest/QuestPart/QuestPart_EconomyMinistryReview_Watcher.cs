using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public class QuestPart_EconomyMinistryReview_Watcher : QuestPartActivable
{
    public string recordTag;
    public string inSignaRecordDecode;
    public string inSiganlSettle;
    private bool settled;

    public string outSignalSettled;

    public Map map;
    public Pawn referendary;

    private int decodeCount;
    private int playerDecodeCount;
    private int referendaryDecodeCount;

    private int accountInquiryCount;

    private int dropCount;
    private int lastDropTick;

    private HashSet<int> inquiriedIndex = [];

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref recordTag, "recordTag");
        Scribe_Values.Look(ref inSignaRecordDecode, "inSignaRecordDecode");
        Scribe_Values.Look(ref inSiganlSettle, "inSiganlSettle");
        Scribe_Values.Look(ref settled, "settled", defaultValue: false);

        Scribe_Values.Look(ref outSignalSettled, "outSignalSettled");

        Scribe_References.Look(ref map, "map");
        Scribe_References.Look(ref referendary, "referendary");

        Scribe_Values.Look(ref decodeCount, "decodeCount", 0);
        Scribe_Values.Look(ref playerDecodeCount, "playerDecodeCount", 0);
        Scribe_Values.Look(ref referendaryDecodeCount, "referendaryDecodeCount", 0);
        Scribe_Values.Look(ref accountInquiryCount, "accountInquiryCount", 0);
        Scribe_Values.Look(ref dropCount, "dropCount", 0);
        Scribe_Values.Look(ref lastDropTick, "lastDropTick", 0);

        Scribe_Collections.Look(ref inquiriedIndex, "inquiriedIndex", LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && quest?.State == QuestState.Ongoing)
        {
            ScienceDepartmentDialogUtility.ExtendMainNode -= MainNodeExtension;
            ScienceDepartmentDialogUtility.ExtendMainNode += MainNodeExtension;
        }
    }
    public override void PostQuestAdded()
    {
        base.PostQuestAdded();
        if (quest?.State == QuestState.Ongoing)
        {
            OAInteractHandler.Instance.CooldownManager.RegisterRecord("EconomyReviewing", cdTicks: 0, shouldRemoveWhenExpired: false);
            ScienceDepartmentDialogUtility.ExtendMainNode -= MainNodeExtension;
            ScienceDepartmentDialogUtility.ExtendMainNode += MainNodeExtension;
        }
    }

    public override void Notify_PreCleanup()
    {
        base.Notify_PreCleanup();
        if (!settled)
        {
            Settle();
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        recordTag = null;
        inSignaRecordDecode = null;
        inSiganlSettle = null;

        outSignalSettled = null;

        map = null;
        referendary = null;

        inquiriedIndex = null;

        OAInteractHandler.Instance.CooldownManager.DeregisterRecord("EconomyReviewing");
        ScienceDepartmentDialogUtility.ExtendMainNode -= MainNodeExtension;
    }

    protected override void ProcessQuestSignal(Signal signal)
    {
        base.ProcessQuestSignal(signal);
        if (signal.tag == inSignaRecordDecode)
        {
            decodeCount++;
            signal.args.TryGetArg("HACKER", out Pawn decoder);
            if (decoder is not null && decoder == referendary)
            {
                referendaryDecodeCount++;
            }
            else
            {
                playerDecodeCount++;
            }
        }
        else if (!settled && signal.tag == inSiganlSettle)
        {
            settled = true;
            Settle();
            Complete();
        }
    }

    protected override void Enable(SignalArgs receivedArgs)
    {
        base.Enable(receivedArgs);
        lastDropTick = Find.TickManager.TicksGame;
        DropAccountingRecord();
    }
    protected override void Disable()
    {
        settled = true;
        base.Disable();
    }

    public override void QuestPartTick()
    {
        base.QuestPartTick();
        if (State == QuestPartState.Enabled && dropCount < 5 && Find.TickManager.TicksGame > lastDropTick + 120000)
        {
            lastDropTick = Find.TickManager.TicksGame;
            DropAccountingRecord();
        }
    }

    private void DropAccountingRecord()
    {
        map ??= referendary?.MapHeld ?? Find.AnyPlayerHomeMap;
        Thing accountingRecord = ThingMaker.MakeThing(OARK_ThingDefOf.OARK_SDAccountingRecord);
        accountingRecord.TryGetComp<CompSDAccountingRecord>()?.SetReferendary(referendary);
        QuestUtility.AddQuestTag(accountingRecord, recordTag);
        OARK_DropPodUtility.DefaultDropSingleThing(accountingRecord, map, referendary?.Faction);
        dropCount++;
    }

    private void Settle()
    {
        if (playerDecodeCount >= 5)
        {
            TriggerOutcomeIncident(outcome: EconomyMinistryReviewOutcome.ScienceDepartment,
                                   (playerDecodeCount + accountInquiryCount) * 5);
            return;
        }

        if (accountInquiryCount >= 6)
        {
            if (playerDecodeCount == 0 && referendaryDecodeCount > 0)
            {

                TriggerOutcomeIncident(outcome: EconomyMinistryReviewOutcome.WinWin,
                                       pointsGain: (referendaryDecodeCount * 5 * 10000) + ((playerDecodeCount + accountInquiryCount) * 5));
            }
            else
            {
                TriggerOutcomeIncident(outcome: EconomyMinistryReviewOutcome.ScienceDepartment,
                                       pointsGain: (playerDecodeCount + accountInquiryCount) * 5);
            }
        }
        else
        {
            TriggerOutcomeIncident(outcome: EconomyMinistryReviewOutcome.EconomyMinistry,
                                   pointsGain: referendaryDecodeCount * 5);
        }

        Find.SignalManager.SendSignal(new Signal(outSignalSettled));
    }

    private void TriggerOutcomeIncident(EconomyMinistryReviewOutcome outcome, int pointsGain)
    {
        ScienceDepartmentInteractHandler.Instance.SetEconomyReviewOutcome(outcome);
        IncidentParms parms = new()
        {
            target = Find.World,
            faction = referendary?.Faction ?? ModUtility.OAFaction,
            points = pointsGain
        };

        OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_EconomyMinistryReview_Outcome, delayTicks: Rand.RangeInclusive(60000, 180000), parms, retryDurationTicks: 30000);
    }

    private void MainNodeExtension(DiaNode mainNode, FactionDialogCache dialogCache)
    {
        DiaOption diaOption = FactionDialogUtility.DiaOptionWithCooldown(text: "OARK_AccountInquiry".Translate(),
                                                                         key: "AccountInquiry",
                                                                         linkLateBind: () => AccountInquiryReplyNode(dialogCache));

        mainNode.options.Add(diaOption);
    }

    private DiaNode AccountInquiryReplyNode(FactionDialogCache dialogCache)
    {
        GrammarRequest grammarRequest = new();
        grammarRequest.Includes.Add(OARK_RulePackDef.OARK_RulePack_AccountInquiryText);

        TaggedString text = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_text", grammarRequest));

        DiaNode diaNode = new(text);

        DiaOption diaOption = new("GoBack".Translate())
        {
            action = delegate
            {
                OAInteractHandler.Instance.CooldownManager.RegisterRecord("AccountInquiry", cdTicks: 60000, shouldRemoveWhenExpired: true);
                accountInquiryCount++;
            },
            linkLateBind = () => ScienceDepartmentDialogUtility.AccessScienceDepartmentNode(dialogCache)
        };
        diaNode.options.Add(diaOption);
        return diaNode;
    }
}
