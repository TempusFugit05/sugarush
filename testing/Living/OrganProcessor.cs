using Godot;
using System;

public partial class OrganProcessor : Organ
{
    protected override void InitOrgan()
    {
        OrganSettings.Vital = true;
    }
}
