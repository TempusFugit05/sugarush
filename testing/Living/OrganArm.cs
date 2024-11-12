using Godot;
using Helpers;

public partial class OrganArm : Organ
{
    TestWeapon AttachedWeapon;
    Godot.Collections.Array<CollisionShape3D> WeaponColliders = new();

    public void GrabWeapon(TestWeapon weapon)
	{
        WeaponColliders = HR.GetChildrenOfType<CollisionShape3D>(weapon);
        AttemptColliderReparent(true, WeaponColliders, weapon);
        AttachedWeapon = weapon;
    }

    public void DropWeapon()
	{
        AttemptColliderReparent(false, WeaponColliders, AttachedWeapon);
        AttachedWeapon.Freeze = false;
        AttachedWeapon.Reparent(GetTree().Root.GetNode("Main"));
        AttachedWeapon = null;

    }

    public override void UseOrgan()
    {
        AttachedWeapon?.Shoot();
    }

    protected override void OnKill()
    {
        DropWeapon();
    }

    public override void _Ready()
    {
        Health = 1000;
		CallDeferred("Init");
        Godot.Collections.Array<TestWeapon> childWeapons = HR.GetChildrenOfType<TestWeapon>(this);
        if (childWeapons.Count == 1)
        {
            CallDeferred("GrabWeapon", childWeapons[0]);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        DefaultPhysicsProcess();
        // foreach (CollisionShape3D collider in WeaponColliders)
        // {
        //     collider.GlobalBasis = AttachedWeapon.GlobalBasis;
        // }
    }
}
