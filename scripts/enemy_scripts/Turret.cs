using Godot;
using System;

public partial class Turret : Enemy
{
	// Called when the node enters the scene tree for the first time.
	Weapon TurretWeapon;
    Node3D PlayerNode;
    Node3D WeaponHolder;

    [Export]
	private float RotationSpeed = 0.5f; // Speed of rotation in radians

	public override void _Ready()
	{
        WeaponHolder = GetNode<Node3D>("WeaponHolder");
		TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
		PlayerNode = GetTree().Root.GetNode<Node3D>("Main/Character");
        InitNode();
	}

	private void RotateTowardsPlayerY(double delta)
	{
		Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
		float Angle = Mathf.PosMod(Mathf.Atan2(Difference.X, Difference.Z) - GlobalRotation.Y + Mathf.Pi, Mathf.Tau);
        RotateY(RotationSpeed * (float)delta * ((Angle > Mathf.Tau/2) ? -1 : 1));
	}

	private void RotateTowardsPlayerZ(double delta)
	{
		//TODO!!!
    }

	public override void _PhysicsProcess(double delta)
	{
        RotateTowardsPlayerZ(delta);
        RotateTowardsPlayerY(delta);
    }
}
