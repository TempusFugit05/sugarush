using Godot;

public partial class TestTurretWeapon : TestWeapon
{
	public override void _Ready()
	{
		FireRate = 1.0f;
		Damage = 5.0f;
        AttachmentMode = AttachmentModeEnum.Creature;
        InitWeapon();
	}
}
