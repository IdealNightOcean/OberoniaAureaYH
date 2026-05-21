using Verse;

namespace OberoniaAurea;

public class CompIntelligenceDraft : CompHackable
{
    public override void FinishHack(Pawn hackPawn)
    {
        hackPoint = 0f;
        if (parent.stackCount > 1)
        {
            parent.stackCount--;
        }

        float rand = Rand.Value;
        if (rand < 0.028f)
        {

        }
        else if (rand <= 0.3f)
        {

        }

        if (parent.stackCount <= 1)
        {
            base.FinishHack(hackPawn);
        }
    }


}
