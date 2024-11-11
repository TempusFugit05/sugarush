using Godot;

public partial class Organ : RigidBody3D, ICreature
{
    private float Shilding = 50.0f;
    public float Health = 500.0f;
    public bool IsActive = true;
	public bool IsVital = false;
    CreatureSoul HostSoul;

    [Signal]
    public delegate void OrganDestroyedEventHandler(ulong organId);

    public void Init(CreatureSoul host)
	{
        HostSoul = host;
    }

    public override void _Ready()
    {
        // FreezeMode = FreezeModeEnum.Static;
    }

	public virtual void Kill()
	{
        EmitSignal(SignalName.OrganDestroyed, GetInstanceId());
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
            // EnemyHealthBar?.SetHealthPoint(Health, MaxHealth); // Update healthbar with current healthpoints
			// OnHurt(damage, damagePosition);
		}
	}


}