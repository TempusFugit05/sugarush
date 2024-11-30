using Godot;
using System;

public partial class OrganicCreature : CreatureBase
{
    protected override void InitCreature()
    {
        CreatureSettings.RecursiveHitbox = true;
        CreatureSettings.GroundDetectFromRecursive = true;
        CreatureSettings.GroundDetectFromColliders = false;
    }
}
