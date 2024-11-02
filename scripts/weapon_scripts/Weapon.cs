using Godot;
using System.Threading.Tasks;

public partial class Weapon : Node3D
{
    [Export]
    protected PackedScene DecalScene;

	[Export]
	protected float Range = 75.0f;

	[Export]
	protected float DamageFalloffStart = 10;
	
	[Export]
	protected float FireRate = 0.25f; // Time between each bullet fired
	
	[Export]
	protected float Damage = 25.0f;

	[Export]
	protected bool AttachedToPlayer = false;

	protected Camera3D PlayerCameraNode = null;

    [Export]
    protected float MinimumDamage = 1.0f;

    [Export]
    protected string SoundEffectPath = "res://assets/audio/weapon/untitled.wav";

    protected bool CanShoot = true;

    protected string DecalPath;
	
    protected string PlayerCameraPath;

    protected AudioStreamPlayer3D AudioPlayer;

	protected void InitWeapon()
	{
        InitShootingHandler();
		if(AttachedToPlayer)
		{
            AudioPlayer = GetNode<AudioStreamPlayer3D>("../CameraSoundPlayer");
        }
		else
		{
			AudioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer");			
		}

        if(SoundEffectPath.Length != 0)
		{
			AudioPlayer.Stream = ResourceLoader.Load<AudioStream>(SoundEffectPath);
        }
    }

    public override void _Ready()
	{
        InitWeapon();
    }

	/// <summary>
    /// 	Apply damage falloff based on range to targert
    /// </summary>
    /// <param name="damage">The input damage to which falloff should be applied</param>
    /// <param name="distanceToTarget">The distance to the target that was hit</param>
    /// <returns>Modified damage</returns>
	protected virtual float ApplyDamageFalloff(float damage, float distanceToTarget)
	{        
		if(distanceToTarget > DamageFalloffStart)
		{
            damage -= (damage / Range) * distanceToTarget;
        }

		if (damage < MinimumDamage && damage > 0)
        {
            return MinimumDamage;
        }

        return damage;
    }

	/// <Summary>
	/// 	Apply damage to the object that was hit
	/// </Summary>
	protected virtual void ApplyDamage(Godot.Collections.Dictionary RayDict)
	{
		if(RayDict is not null)
		{
			Node HitObject = (Node)RayDict["collider"];
			if(HitObject is ICreature hurtable)
			{
				hurtable.Hurt(ApplyDamageFalloff(Damage, GlobalPosition.DistanceTo((Vector3)RayDict["position"])), (Vector3)RayDict["position"]); // Hurtable is a special method for objects which are damage-able
			}
		}
	}

	
	protected async Task SetCooldown()
	{
		CanShoot = false; // Disable shooting
		await ToSignal(GetTree().CreateTimer(FireRate), SceneTreeTimer.SignalName.Timeout); // Set timer for next shot
		CanShoot = true; // Enable shooting
	}

	public bool ReadyToShoot()
	{
        return CanShoot;
    }

	/// <Summary>
	/// 	Attempt to shoot. The weapon will only fire if its firing cooldown has expired.
	/// </Summary>
	/// <returns>
	/// <c>true</c> if weapon was successfully fired; <c>false</c> if the cooldown has not expired yet.
	/// </returns>
	public virtual bool Shoot()
	{
		if(CanShoot)
		{
			if(AttachedToPlayer && PlayerCameraNode is not null)
			{
				ShootFromCamera();
			}			

			else
			{
				ShootFromWeapon();
			}

            _ = SetCooldown();
            AudioPlayer.Play();
            return true;
		}

		return false;
	}
}
