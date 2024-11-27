using Godot;
using System;

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

	public override void _Ready()
	{
		BodyEntered += OnAreaEntered;
	}

	private void OnAreaEntered(Node3D obj)
	{
		switch (DamageMode)
		{
			case LiquidDamageMode.InstaKill:
			{
				if (obj is ICreature creature)
				{
					creature.Kill();
				}
				break;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (DamageMode == LiquidDamageMode.Damage)
		{
			foreach (var obj in GetOverlappingBodies())
			{
				if (obj is Character creature)
				{
					creature.Hurt(DamagePerSecond * (float)delta);
				}
			}
		}
	}
}
