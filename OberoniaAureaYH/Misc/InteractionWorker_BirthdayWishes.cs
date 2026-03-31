using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class InteractionWorker_BirthdayWishes : InteractionWorker
{
    private static readonly SimpleCurve CompatibilityFactorCurve =
    [
        new CurvePoint(-1.5f, 0f),
        new CurvePoint(-1f, 0.1f),
        new CurvePoint(-0.5f, 0.5f),
        new CurvePoint(0f, 1f),
        new CurvePoint(1f, 2f),
        new CurvePoint(2f, 3f)
    ];

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (initiator.Inhumanized() || !recipient.IsOnBirthday())
        {
            return 0f;
        }
        float weight = GetPreceptFactor_Birthday(initiator);
        if (weight <= 0f)
        {
            return 0f;
        }

        weight *= CompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
        weight *= 0.125f;
        return weight;
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef, out lookTargets);

        if (recipient.needs?.mood is null)
        {
            return;
        }
        int stage = GetThoughtStage(recipient);
        if (stage < 0)
        {
            return;
        }

        Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(OARK_ThoughtDefOf.OARK_BirthdayWishes);
        thought.SetForcedStage(stage);

        recipient.needs.mood.thoughts.memories.TryGainMemory(thought, initiator);
    }

    private static float GetPreceptFactor_Birthday(Pawn p)
    {
        if (!ModsConfig.IdeologyActive)
        {
            return 1f;
        }

        Ideo ideo = p.Ideo;
        if (ideo is null)
        {
            return 1f;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Fear))
        {
            return -1f;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Ordinary))
        {
            return 2f;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Appreciate))
        {
            return 4f;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Solemn))
        {
            return 8f;
        }
        return 1f;
    }

    private static int GetThoughtStage(Pawn p)
    {
        if (!ModsConfig.IdeologyActive)
        {
            return 1;
        }

        Ideo ideo = p.Ideo;
        if (ideo is null)
        {
            return 1;
        }

        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Fear))
        {
            return 0;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Ignore))
        {
            return -1;
        }

        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Appreciate))
        {
            return 2;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Solemn))
        {
            return 3;
        }

        return 1;
    }
}