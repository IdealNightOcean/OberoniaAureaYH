using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class OARatkinGroup
{
    public PawnKindDef pawnKindDef;
    public int pawnCount;
}

public class OARatkinGroupExtension : DefModExtension
{
    public List<OARatkinGroup> extraGroups;
}
