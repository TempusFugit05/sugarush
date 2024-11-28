using Godot;
using Helpers;
using System;
using System.Drawing;

public partial class Liquid : Area3D
{
	public enum LiquidDamageMode
	{
		InstaKill,
		Damage,
		None,
		Default = None
	};

	[Export] public bool Swimmable = true;
	[Export] public float DamagePerSecond = 10.0f;
	[Export] public LiquidDamageMode DamageMode = LiquidDamageMode.Damage;
	[Export] public bool EnableBouyancy = true;
	[Export] public float Density = 1000.0f;

	private Aabb HitBox;

	void foo()
	{
		HitBox = GetNode<MeshInstance3D>("MeshInstance3D").GetAabb();
		GD.Print(HitBox.Size);
		GD.Print(HitBox.Position);
		GD.Print(GlobalPosition);
		HitBox.Position = GlobalPosition + HitBox.Position;
		GD.Print(HitBox.Position);

		// GD.Print((HitBox*GlobalTransform).Size);
		// GD.Print((HitBox*GlobalTransform).Position);

		HitBox = new Aabb(GlobalPosition, HitBox.Size * GlobalBasis.Scale);

		MeshInstance3D debug = (MeshInstance3D)ResourceLoader.Load<PackedScene>("res://testing/AabbDebugger.tscn").Instantiate();
		GetTree().Root.AddChild(debug);
		debug.GlobalPosition = GlobalPosition;
		((BoxMesh)debug.Mesh).Size = HitBox.Size;
	}

	public override void _Ready()
	{
		BodyEntered += OnAreaEntered;
		CallDeferred("foo");
	}

	private void OnAreaEntered(Node3D obj)
	{
		if(DamageMode == LiquidDamageMode.InstaKill)
		{
			if (obj is ICreature creature)
			{
				creature.Kill();
			}
		}
	}

	private void ApplyInteraction(Node3D obj)
	{
		if (EnableBouyancy)
		{
			if (obj is RigidBody3D body)
			{
				MeshInstance3D bodyMesh = body.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
				if (bodyMesh is not null)
				{
					Aabb bodyBounds = bodyMesh.GetAabb();
					bodyBounds = new(body.GlobalPosition, bodyBounds.Size);
					GD.Print(HitBox.Intersection(bodyBounds).Volume);
					body.ApplyCentralForce(-body.GetGravity() * HitBox.Intersection(bodyBounds).Volume * Density);
				}
			}
		}
	}

	private void HurtCreature(double delta, Node3D obj)
	{
		if (obj is Character creature)
		{
			creature.Hurt(DamagePerSecond * (float)delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		foreach (var obj in GetOverlappingBodies())
		{
			if (EnableBouyancy)
			{
				ApplyInteraction(obj);
			}
			if (DamageMode == LiquidDamageMode.Damage)
			{
				HurtCreature(delta, obj);
			}
		}
	}
}
