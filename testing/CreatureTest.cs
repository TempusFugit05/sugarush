using Godot;
using System;

public partial class CreatureTest : RigidBody3D
{
    RigidBody3D Body;

    public override void _Ready()
	{
        Body = GetNode<RigidBody3D>("Body");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        ApplyForce(Vector3.Left* 4);
        // Body.AngularVelocity = Vector3.Up;
    }
}
