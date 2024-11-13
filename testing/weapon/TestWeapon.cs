using Godot;
using System.Threading.Tasks;

public partial class TestWeapon : RigidBody3D
{

	public enum AttachmentModeEnum
	{
		Free,
		Player,
		Creature,
		Default = Free,
	}

    [Export]
    protected PackedScene DecalScene;
    protected string DefaultDecalScenePath = "res://subscenes/ui_subscenes/BulletDecal.tscn";

    [Export]
	protected float Range = 75.0f;

	[Export]
	protected float DamageFalloffStart = 10;
	
	[Export]
	protected float FireRate = 0.05f; // Time between each bullet fired
	
	[Export]
	protected float Damage = 25.0f;

	[Export]
	public AttachmentModeEnum AttachmentMode = AttachmentModeEnum.Default;

    [Export]
    protected float MinimumDamage = 1.0f;

    [Export]
    protected string SoundEffectPath = "res://assets/audio/weapon/untitled.wav";

	protected Camera3D PlayerCameraNode = null;
    
	protected bool CanShoot = true;

    protected string DecalPath;
	
    protected string PlayerCameraPath;

    protected AudioStreamPlayer3D AudioPlayer;

    protected Organ HostOrgan;

    protected void InitWeapon()
	{
		if (DecalScene == null)
		{
            DecalScene = ResourceLoader.Load<PackedScene>(DefaultDecalScenePath);
        }
        InitShootingHandler();
        SetAttachmentMode(AttachmentMode); // Set default attachment mode
    }

	public void SetAttachmentMode(AttachmentModeEnum mode)
	{
		switch (mode)
		{
            case AttachmentModeEnum.Free:
            {
				Freeze = false;
				CollisionLayer = (int)GlobalEnums.CollisionLayersEnum.Default;
				AudioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer"); // Play from own sound player
				break;
            }
            case AttachmentModeEnum.Creature:
            {
				FreezeMode = FreezeModeEnum.Static;
				Freeze = true;
				CollisionLayer = (int)GlobalEnums.CollisionLayersEnum.NoCollide;
				AudioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer"); // Play from own sound player
				break;
            }
			case AttachmentModeEnum.Player:
			{
				FreezeMode = FreezeModeEnum.Static;
				Freeze = true;
				CollisionLayer = (int)GlobalEnums.CollisionLayersEnum.NoCollide;
				AudioPlayer = GetNode<AudioStreamPlayer3D>("../CameraSoundPlayer"); // Play from player sound player
				PlayerCameraNode = GetNode<Camera3D>("../../PlayerCamera");
				break;
            }
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
			if(AttachmentMode is AttachmentModeEnum.Player && PlayerCameraNode is not null)
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
