public partial class TurretWeapon : Weapon
{
	public override void _Ready()
	{
        FireRate = 1.0f;
        Damage = 5.0f;
        InitWeapon();
    }
}
