using Godot;
using Helpers;

public partial class OrganArm : Organ
{
    Weapon AttachedWeapon;
    Node3D WeaponTransformReference;
    Godot.Collections.Array<CollisionShape3D> WeaponColliders;
    bool WeaponCollidersReparented = false;

    public override void OnAddedToCreature()
    {
        GrabWeapon();
    }

    protected override void OnDelete()
    {
        AttachedWeapon?.QueueFree();
    } 

    public void GrabWeapon()
	{
        Godot.Collections.Array<Weapon> childWeapons = HR.GetChildrenOfType<Weapon>(this);
        if (childWeapons.Count == 1)
        {
            AttachedWeapon = childWeapons[0];
            WeaponTransformReference = GetNodeTransformReference(AttachedWeapon);
            WeaponColliders = HR.GetChildrenOfType<CollisionShape3D>(AttachedWeapon);
            OrganBase.MergeColliders(new(HR.GetChildrenOfType<CollisionShape3D>(AttachedWeapon), AttachedWeapon));
            WeaponCollidersReparented = true;
            AttachedWeapon.Freeze = true;
        }
    }

    public void DropWeapon()
	{
        if (AttachedWeapon is not null)
        {
            WeaponCollidersReparented = false;
            AttachedWeapon.SetAttachmentMode(Weapon.AttachmentModeEnum.Free);
            OrganBase.ReturnCollider(AttachedWeapon);
            AttachedWeapon.Freeze = false;
            AttachedWeapon.Reparent(GetTree().Root.GetNode("Main"));
            AttachedWeapon = null;
        }
    }

    public override void UseOrgan()
    {
        AttachedWeapon?.Shoot();
    }

    protected override void OnDestroy()
    {
        DropWeapon();
    }

    protected override void OrganPhysicsProcess(double delta)
    {
        if (AttachedWeapon is not null)
        {
            CallDeferred("UpdateCollidersOf", WeaponTransformReference.GlobalTransform, WeaponColliders);
            // CallDeferred("UpdateCollidersOf", WeaponTransformReference.GlobalTransform, WeaponColliders);
        }
    }

    protected override void InitOrgan()
    {
        OrganSettings.Vital = true;
        OrganSettings.UpdateColliders = true;
        OrganSettings.DestructMode = DestructModeEnum.Gibs;
        OrganSettings.MaxHealth = 500;
    }
}
