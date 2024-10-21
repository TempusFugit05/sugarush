using Godot;
using System;

public partial class Turret : Enemy
{
	// Called when the node enters the scene tree for the first time.
	Weapon TurretWeapon;

	[Export]
	private float RotationSpeed = 0.5f; // Speed of rotation in radians

	public override void _Ready()
	{
		TurretWeapon = GetNode<Weapon>("TurretWeapon");
		InitNode();
	}

	public override void _PhysicsProcess(double delta)
	{
		Node3D PlayerNode = GetTree().Root.GetNode<Node3D>("Main/Character");
		Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
		float Angle = Mathf.PosMod(Mathf.Atan2(Difference.X, Difference.Z) - GlobalRotation.Y + Mathf.Pi, Mathf.Tau);
		Vector3 temp = GlobalRotation;
		temp.Y += RotationSpeed * (float)delta * ((Angle > Mathf.Tau/2) ? -1 : 1);
		GlobalRotation = temp;
	}
}
