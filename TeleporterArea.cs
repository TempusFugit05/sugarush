using Godot;
using System;

public partial class TeleporterArea : Area3D
{
	[Export] Node3D Endpoint;
	[Export] Vector3 ExitVelocity = Vector3.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnAreaEntered;
	}

	private void OnAreaEntered(Node3D body)
	{
		if (Endpoint is not null && body is RigidBody3D rigidBody)
		{
			rigidBody.GlobalPosition = Endpoint.GlobalPosition;
			rigidBody.LinearVelocity = ExitVelocity;
		}
	}
}
