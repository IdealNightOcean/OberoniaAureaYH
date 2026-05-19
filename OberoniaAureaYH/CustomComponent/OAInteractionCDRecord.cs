using Verse;

namespace OberoniaAurea;

public struct OAInteractionCDRecord : IExposable
{
    public int lastActiveTick;
    public int nextAvailableTick;

    public OAInteractionCDRecord()
    {
        lastActiveTick = -1;
        nextAvailableTick = -1;
    }

    public OAInteractionCDRecord(int cooldownTicks)
    {
        lastActiveTick = Find.TickManager.TicksGame;
        nextAvailableTick = lastActiveTick + cooldownTicks;
    }

    public readonly int CooldownTicksLeft => lastActiveTick < 0 ? -1 : nextAvailableTick - Find.TickManager.TicksGame;

    public readonly int TicksSinceLastActive => lastActiveTick < 0 ? -1 : Find.TickManager.TicksGame - lastActiveTick;

    public readonly bool IsInCooldown => nextAvailableTick > 0 && nextAvailableTick > Find.TickManager.TicksGame;

    public override readonly string ToString() => $"LastActiveTick: {lastActiveTick}, NextAvailableTick: {nextAvailableTick}";

    public void ExposeData()
    {
        Scribe_Values.Look(ref lastActiveTick, "lastActiveTick", -1);
        Scribe_Values.Look(ref nextAvailableTick, "nextAvailableTick", -1);
    }
}