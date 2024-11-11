
using Godot;

public partial class Shotgun : Weapon
{

    [Export]
    private float SpreadAngle = 2.5f;

    private void InitNode()
	{
        Range = 40.0f;
        FireRate = 0.5f;
        Damage = 30;
        DamageFalloffStart = 2;
        PlayerCameraPath = "Main/Character/PlayerCamera";
        DecalPath = "res://subscenes/ui_subscenes/BulletDecal.tscn";
        SoundEffectPath = "res://assets/audio/weapon/ShotgunSoundPlaceholder.wav";
    }

    public override void _Ready()
    {
        InitNode();
        DecalScene = ResourceLoader.Load<PackedScene>(DecalPath);
		PlayerCameraNode = GetParent<Camera3D>();
        InitWeapon();
    }
    

	public override bool Shoot()
	{
		if(CanShoot)
		{
			if(AttachmentMode is AttachmentModeEnum.Player && PlayerCameraNode is not null)
			{
                Vector2[] Angles = {new(SpreadAngle, 0), new(-SpreadAngle, 0), new(0, SpreadAngle), new(0, -SpreadAngle),
								 new(SpreadAngle, SpreadAngle), new(-SpreadAngle, -SpreadAngle), new(-SpreadAngle, SpreadAngle), new(SpreadAngle, -SpreadAngle), new(0, 0)};
                ShootFromCamera(Angles);
            }

			else
			{
				ShootFromWeapon();
			}
            AudioPlayer.Play();
            _ = SetCooldown();

			return true;
		}

		return false;
	}
}