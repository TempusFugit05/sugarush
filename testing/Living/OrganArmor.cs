using Godot;
using System;

public partial class OrganArmor : Organ
{
	protected override void InitOrgan()
	{
        OrganSettings.MaxHealth = OrganSettings.MaxDestructionHealth;
    }
}
