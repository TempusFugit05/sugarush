using Godot;
using System;

public partial class Character : CharacterBody3D, IHurtable
{
	public override void _Ready()
	{
        PlayerCamera = GetNode<Camera3D>("PlayerCamera");
        InitPickupSphere();
		InitWeaponHandler();
	}
	public void Hurt(float Damage, Vector3 DamagePosition = default)
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
