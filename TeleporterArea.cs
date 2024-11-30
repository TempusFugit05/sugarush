using Godot;
using System;

public partial class TeleporterArea : Area3D
{
	public enum ExitVelocityEnum
	{
		Override,
		Reflect,
		Preserve,
		Default = Preserve
	}

	[Export] public Node3D Endpoint;
	[Export] private Vector3 ExitVelocity = Vector3.Zero;
	[Export] private ExitVelocityEnum ExitBehavior = ExitVelocityEnum.Default;

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
			switch (ExitBehavior)
			{
				case ExitVelocityEnum.Override:
					rigidBody.LinearVelocity = ExitVelocity;
					break;

				case ExitVelocityEnum.Reflect:
					rigidBody.LinearVelocity *= -1;
					break;
			}
		}
	}
}
