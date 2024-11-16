
using Godot;

public partial class Shotgun : Weapon
{

    [Export]
    private float SpreadAngle = 2.5f;

    protected override void InitWeapon()
    {
        WeaponSettings.FireRate = 2f;
        WeaponSettings.Damage = 50f;
        WeaponSettings.SoundEffectPath = "res://assets/audio/weapon/ShotgunSoundPlaceholder.wav";
    }

	public override bool Shoot()
	{
		if(CanShoot)
		{
			if(WeaponSettings.AttachmentMode is AttachmentModeEnum.Player && PlayerCameraNode is not null)
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