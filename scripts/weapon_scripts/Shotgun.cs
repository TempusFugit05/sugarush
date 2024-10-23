
using Godot;

public partial class Shotgun : Weapon
{

    [Export]
    private float SpreadAngle = 5f;

    private void InitNode()
	{
        Range = 40.0f;
        FireRate = 0.5f;
        Damage = 30;
        DamageFalloffStart = 2;
        AttachedToCamera = true;
        PlayerCameraPath = "Main/Character/PlayerCamera";
        DecalPath = "res://subscenes/ui_subscenes/BulletDecal.tscn";
        
		InitShootingHandler();
	}

    public override void _Ready()
    {
        InitNode();
        DecalScene = ResourceLoader.Load<PackedScene>(DecalPath);
		CameraNode = GetParent<Camera3D>();
    }
    

	public override bool Shoot()
	{
		if(CanShoot)
		{
			if(AttachedToCamera && CameraNode is not null)
			{
                Vector2[] Angles = {new(SpreadAngle, 0), new(-SpreadAngle, 0), new(0, SpreadAngle), new(0, -SpreadAngle),
								 new(SpreadAngle, SpreadAngle), new(-SpreadAngle, -SpreadAngle), new(-SpreadAngle, SpreadAngle), new(SpreadAngle, -SpreadAngle), new(0, 0)};
                ShootFromCamera(Angles);
            }

			else
			{
				ShootFromWeapon();
			}

            _ = SetCooldown();

			return true;
		}

		return false;
	}
}