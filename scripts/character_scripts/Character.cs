using Godot;
using System;

public partial class Character : CharacterBody3D, IHurtable
{
	public override void _Ready()
	{
		PickupSphereInit();
		InitWeaponHandler();
	}
	public void Hurt(float Damage, Vector3 DamagePosition)
	{
		CurrentSugar -= Damage;
	}

	public override void _PhysicsProcess(double delta)
	{
		SugarHandler(delta);
		WeaponHandler();
		MovementHandler(delta);
	}
}
