using Godot;

public partial class TestTurretWeapon : Weapon
{
	public override void _Ready()
	{
		WeaponSettings.FireRate = 1.0f;
		WeaponSettings.Damage = 5.0f;
        WeaponSettings.AttachmentMode = AttachmentModeEnum.Creature;
        Init();
	}
}
