using Godot;
using System.Threading.Tasks;

public partial class Weapon : RigidBody3D, IInteractable
{

	public enum AttachmentModeEnum
	{
		Free,
		Player,
		Creature,
		Default = Free,
	}

	public enum ProjectileTypeEnum
	{
		HitScan,
		RigidBody,
		ShapeCast,
		Default = HitScan
	}

	public enum ProjectileHitTypeEnum
	{
		Bullet,
		Explosive
	}
	
    public struct WeaponSettingsStruct
    {
		/*Decal Settings*/
        public const string DefaultDecalScenePath = "res://subscenes/ui_subscenes/BulletDecal.tscn";
        public string DecalPath = DefaultDecalScenePath;
        public PackedScene DecalScene = null;
        public bool EnableDecals = true;

		/*Sound Settings*/
        public const string DefaultSoundEffectPath = "res://assets/audio/weapon/untitled.wav";
        public string SoundEffectPath = DefaultSoundEffectPath;
        public float RandPitchScaleMin = 0.95f;
        public float RandPitchScaleMax = 1.05f;

		/*Particle Settings*/
        public const string DefaultMetallicHit = "res://assets/particles/MetallicImpact.tscn";
        public string MetallicHit = DefaultMetallicHit;
        public PackedScene Metal = null;

		/*Weapon Attributes*/
        public float Range = 75.0f;
        public float DamageFalloffStart = 10;
        public float FireRate = 8f; // Bullets per second
        public float Damage = 25.0f;
		public float MinimumDamage = 1.0f;

		/*Weapon State*/
        public Godot.Collections.Array<Rid> ExclusionList = new();
        public AttachmentModeEnum AttachmentMode = AttachmentModeEnum.Default;

	    public WeaponSettingsStruct(){}
    }

    protected WeaponSettingsStruct WeaponSettings = new();

	public Camera3D PlayerCameraNode = null;
    
	protected bool CanShoot = true;
    protected string PlayerCameraPath;

    protected AudioStreamPlayer3D AudioPlayer;

    protected Organ HostOrgan;

	protected virtual void InitWeapon(){}

    protected void Init(string customDecalPath = default, string customSoundEffectPath = default)
	{
		WeaponSettings.DecalScene = ResourceLoader.Load<PackedScene>(WeaponSettings.DecalPath);
		AudioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer");
        AudioPlayer.Stream = ResourceLoader.Load<AudioStream>(WeaponSettings.SoundEffectPath);
        WeaponSettings.Metal = ResourceLoader.Load<PackedScene>(WeaponSettings.MetallicHit);
        InitShootingHandler();
    }

	public void SetAttachmentMode(AttachmentModeEnum mode, Godot.Collections.Array<Rid> exclude = null)
	{

		if (mode == AttachmentModeEnum.Player || mode == AttachmentModeEnum.Creature)
		{
			FreezeMode = FreezeModeEnum.Static;
			Freeze = true;
			CollisionLayer = (int)GlobalEnums.CollisionLayersEnum.NoCollide;
		} // Disable physics on weapon

		if (exclude is not null)
		{
            WeaponSettings.ExclusionList = exclude;
        }
		else
		{
            WeaponSettings.ExclusionList.Clear();
        }

        switch (mode)
		{
            case AttachmentModeEnum.Free:
            {
				Freeze = false;
				CollisionLayer = (int)GlobalEnums.CollisionLayersEnum.Default;
				AudioPlayer.GlobalPosition = ProjectileSpawnPoint.GlobalPosition;
				break;
            }
            case AttachmentModeEnum.Creature:
            {
				AudioPlayer.GlobalPosition = ProjectileSpawnPoint.GlobalPosition;
				break;
            }
			case AttachmentModeEnum.Player:
			{
				PlayerCameraNode = GetNode<Camera3D>("../../PlayerCamera");
				AudioPlayer.GlobalPosition = GetNode<AudioStreamPlayer3D>("../CameraSoundPlayer").GlobalPosition;
				break;
            }
			default:
			{
				SetAttachmentMode(AttachmentModeEnum.Default);
				break;
            }
		}
		WeaponSettings.AttachmentMode = mode;
	}

    public override void _Ready()
	{
        InitWeapon();
        Init();
    }

	/// <summary>
    /// 	Apply damage falloff based on range to targert
    /// </summary>
    /// <param name="damage">The input damage to which falloff should be applied</param>
    /// <param name="distanceToTarget">The distance to the target that was hit</param>
    /// <returns>Modified damage</returns>
	protected virtual float ApplyDamageFalloff(float damage, float distanceToTarget)
	{        
		if(distanceToTarget > WeaponSettings.DamageFalloffStart)
		{
            damage -= (damage / WeaponSettings.Range) * distanceToTarget;
        }

		if (damage < WeaponSettings.MinimumDamage && damage > 0)
        {
            return WeaponSettings.MinimumDamage;
        }

        return damage;
    }
	
	protected async Task SetCooldown()
	{
		CanShoot = false; // Disable shooting
		await ToSignal(GetTree().CreateTimer(1 / WeaponSettings.FireRate), SceneTreeTimer.SignalName.Timeout); // Set timer for next shot
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
            if(WeaponSettings.AttachmentMode is AttachmentModeEnum.Player && PlayerCameraNode is not null)
			{
                ShootFromCamera();
			}			

			else
			{
                ShootFromWeapon();
			}

            _ = SetCooldown();
            AudioPlayer.PitchScale = Mathf.Clamp(GD.Randf(), WeaponSettings.RandPitchScaleMin, WeaponSettings.RandPitchScaleMax);
            AudioPlayer.Play();
            return true;
		}

		return false;
	}
}
