using Godot;
using System;
using System.Linq;

public partial class Turret : Enemy
{
	// Called when the node enters the scene tree for the first time.
	Weapon TurretWeapon;
    Node3D PlayerNode;
    Node3D WeaponHolder;

    [Export]
	private float RotationSpeed = 0.75f; // Speed of rotation in radians

	public override void _Ready()
	{
        WeaponHolder = GetNode<Node3D>("WeaponHolder");
		TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
		PlayerNode = GetParent().GetNode<Node3D>("Character");
        InitNode();
	}

    private bool IsPlayerVisible()
    {
        PhysicsRayQueryParameters3D Quary = new()
        {
            From = GlobalPosition,
            To = PlayerNode.GlobalPosition,
            Exclude = new Godot.Collections.Array<Rid> { GetRid() },
            CollideWithAreas = false,
			CollideWithBodies = true,
        };
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Quary);
        if ((Node)RayDict["collider"] is Character)
        {
            return true;
        }
        
        return false;

    }

	private bool RotateTowardsPlayerY(double delta)
	{
        if (PlayerNode is not null)
        {
            Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
            float Angle = Mathf.PosMod(Mathf.Atan2(Difference.X, Difference.Z) - GlobalRotation.Y + Mathf.Pi, Mathf.Tau);
            if (Angle >= 0.01f)
            {
                RotateY(RotationSpeed * (float)delta * ((Angle > Mathf.Tau / 2) ? -1 : 1));
                return true;
            }
        }
        return false;
    }

	private void RotateTowardsPlayerZ(double delta)
	{
        Vector3 Difference = PlayerNode.GlobalPosition - WeaponHolder.GlobalPosition;
        Basis temp = WeaponHolder.Basis;
        temp.Z = Difference;
        WeaponHolder.Basis = temp;
        // float Angle = Mathf.PosMod(Mathf.Atan2(Difference.X, Difference.Y) - GlobalRotation.X + Mathf.Pi, Mathf.Tau);

        // if (Angle >= 0.01f)
        // {
        //     Angle *= (Angle > Mathf.Tau / 2) ? -1 : 1;
        //     Quaternion rot = new(Vector3.Right, Angle);
        //     Basis = new Basis(rot.Slerp(Basis.GetRotationQuaternion(), (float)delta));
        // }

    }

	public override void _PhysicsProcess(double delta)
	{
        if (PlayerNode is not null)
        {
            if (IsPlayerVisible())
            {
                if(!RotateTowardsPlayerY(delta) && TurretWeapon.ReadyToShoot())
                {
                    TurretWeapon.Shoot();
                }
            }
        }
    }
}
