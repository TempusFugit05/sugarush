using Godot;

public partial class Enemy : CharacterBody3D, ICreature
{
	[Export]
	protected float MaxHealth = 1000;

	protected float Health = 0;

	HealthBar EnemyHealthBar;

	/// <summary>
    /// 	Init the healthbar and parameters of the node.
    /// 	This is a seperate function to allow child classes to inherit it.
    /// </summary>
	protected void InitNode()
	{
		Health = MaxHealth;
		EnemyHealthBar = (HealthBar)GetNodeOrNull("HealthBar");
        EnemyHealthBar?.SetHealthPoint(Health, MaxHealth); // Update healthbar with current healthpoints
	}

	public override void _Ready()
	{
        InitNode();
    }

	public virtual void Kill()
	{
        QueueFree();
    }

	/// <summary>
    /// 	Apply damage to enemy
    /// </summary>
    /// <param name="Damage">Amount of damage to apply to target</param>
    /// <param name="DamagePosition">The position in which the damage was applied (Can be left blank)</param>
	public void Hurt(float Damage, Vector3 DamagePosition)
	{
		DamageIndicator Indicator = new(Damage); // Create a damage indicator 
		GetTree().Root.AddChild(Indicator); // Add it to the scene
		Indicator.GlobalPosition = (DamagePosition == default) ? GlobalPosition : DamagePosition; // Set position of indicator to a specific position on body (ie bullethole) or object position for non specific damage soruce (ie fall damage)
		Health -= Damage;
		
		if(Health <= 0)
		{
			Kill();
		}

		else
		{
            EnemyHealthBar?.SetHealthPoint(Health, MaxHealth); // Update healthbar with current healthpoints
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!IsOnFloor())
		{
			Velocity += GetGravity() * (float)delta;
		}
		MoveAndSlide();
	}
}
