using OberoniaAurea_Frame;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

internal class CompSDAccountingRecord : CompHackable
{
    private int nextMoteTick = -1;

    private Pawn referendary;

    public void SetReferendary(Pawn pawn)
    {
        referendary = pawn;
    }

    public override void DoHack(float points, Pawn hackPawn)
    {
        base.DoHack(points, hackPawn);
        if (hackPawn == referendary && Find.TickManager.TicksGame > nextMoteTick)
        {
            nextMoteTick = Find.TickManager.TicksGame + 1200;
            ThrowText();
        }
    }

    private void ThrowText()
    {
        MoteBubble moteBubble = (MoteBubble)ThingMaker.MakeThing(OARK_ThingDefOf.OARO_Mote_Referendary);
        moteBubble.SetupMoteBubble(ContentFinder<Texture2D>.Get("UI/OARK_Bubble_Referendary"), null);
        moteBubble.Attach(referendary);
        GenSpawn.Spawn(moteBubble, referendary.Position, referendary.Map);

        GrammarRequest grammarRequest = new();
        grammarRequest.Rules.AddRange(GrammarUtility.RulesForPawn("REFERENDARY", referendary, grammarRequest.Constants));
        grammarRequest.Includes.Add(OARK_RulePackDef.OARK_RulePack_ReferendaryHackText);
        string text = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_text", grammarRequest));

        MoteAttached_Text mote = (MoteAttached_Text)ThingMaker.MakeThing(OAFrameDefOf.OAFrame_Mote_AttachedText);
        mote.text = text;
        mote.textColor = Color.white;
        mote.overrideTimeBeforeStartFadeout = mote.def.mote.Lifespan * 0.5f;
        mote.Attach(referendary);
        GenSpawn.Spawn(mote, referendary.Position, referendary.Map);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref nextMoteTick, "nextMoteTick", -1);
        Scribe_References.Look(ref referendary, "referendary");
    }
}
