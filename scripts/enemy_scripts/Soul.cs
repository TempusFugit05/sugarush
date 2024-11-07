using Godot;

public partial class CreatureSoul : Node3D
{
	[Export]
	private float MaxHealth = 1000;

	protected float Health = 0;

    HealthBar CreatureHealthBar;

    Node3D Vessel;

    /// <summary>
    /// 	Init the healthbar and parameters of the node.
    /// 	This is a seperate function to allow child classes to inherit it.
    /// </summary>
    public CreatureSoul(Node3D vessel,  float maxHealth, HealthBar healthBar)
	{
		Health = maxHealth;
        MaxHealth = maxHealth;
        CreatureHealthBar =	healthBar;
        CreatureHealthBar?.SetHealthPoint(Health, MaxHealth); // Update healthbar with current healthpoints
        Vessel = vessel;
        Vessel.AddChild(this);
    }

	public virtual void Kill()
	{
		if (Vessel is ISoulful creature)
		{
        	creature.OnKill();
		}
        Vessel.QueueFree();
    }

	/// <summary>
    /// 	Apply damage to enemy
    /// </summary>
    /// <param name="damage">Amount of damage to apply to target</param>
    /// <param name="damagePosition">The position in which the damage was applied (Can be left blank)</param>
	public virtual void Hurt(float damage, Vector3 damagePosition = default)
	{
		DamageIndicator Indicator = new(damage); // Create a damage indicator 
		GetTree().Root.AddChild(Indicator); // Add it to the scene
		Indicator.GlobalPosition = (damagePosition == default) ? GlobalPosition : damagePosition; // Set position of indicator to a specific position on body (ie bullethole) or object position for non specific damage soruce (ie fall damage)
		Health -= damage;
		
		if(Health <= 0)
		{
			Kill();
		}

		else
		{
            CreatureHealthBar?.SetHealthPoint(Health, MaxHealth); // Update healthbar with current healthpoints
			if (Vessel is ISoulful creature)
			{
				creature.OnHurt(damage, damagePosition);
			}
		}
	}
}