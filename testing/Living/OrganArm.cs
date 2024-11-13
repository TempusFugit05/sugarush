using Godot;
using Helpers;

public partial class OrganArm : Organ
{
    TestWeapon AttachedWeapon;
    Godot.Collections.Array<CollisionShape3D> WeaponColliders;
    bool WeaponCollidersReparented = false;

    // protected override void OnParentOrganAdded()
    // {
    //     DefaultOnParentOrganAdded();
    //     if (!WeaponCollidersReparented && AttachedWeapon is not null)
    //     {
    //         GrabWeapon(AttachedWeapon);
    //     }
    // }

    public override void OnAddedToCreature()
    {
        GrabWeapon();
    }

    public void GrabWeapon()
	{
        Godot.Collections.Array<TestWeapon> childWeapons = HR.GetChildrenOfType<TestWeapon>(this);
        if (childWeapons.Count == 1)
        {
            AttachedWeapon = childWeapons[0];
            ColliderBank.CombineColliders(new(HR.GetChildrenOfType<CollisionShape3D>(AttachedWeapon), AttachedWeapon));
            WeaponCollidersReparented = true;
        }
    }

    public void DropWeapon()
	{
        if (AttachedWeapon is not null)
        {
            WeaponCollidersReparented = false;
            ColliderBank.ReturnCollider(AttachedWeapon);
            AttachedWeapon.Freeze = false;
            AttachedWeapon.Reparent(GetTree().Root.GetNode("Main"));
            AttachedWeapon = null;

        }
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
        Init();
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
