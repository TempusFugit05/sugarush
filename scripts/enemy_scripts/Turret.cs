using System;
using Godot;
using Helpers;

public partial class Turret : Enemy
{
	// Called when the node enters the scene tree for the first time.
	Weapon TurretWeapon;
    Node3D PlayerNode;
    Node3D WeaponHolder;
    Node3D Body;

    [Export]
	private float RotationSpeed = 0.5f; // Speed of rotation in radians

    [Export]
    private float DetectionRadius = 30.0f;

    public override void _Ready()
	{
        Body = GetNode<Node3D>("Body");
        WeaponHolder = Body.GetNode<Node3D>("WeaponHolder");
		TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
        PlayerNode = HP.GetPlayerNode();
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
        if ((Node)RayDict["collider"] is Character && (((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) <= DetectionRadius))
        {
            return true;
        }
        
        return false;

    }

	private bool RotateTowardsPlayerY(double delta)
	{
        Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
        float Angle = Mathf.PosMod(Mathf.Atan2(Difference.X, Difference.Z) - WeaponHolder.GlobalRotation.Y + Mathf.Pi, Mathf.Tau);
        if (Angle >= 0.01f)
        {
            Body.RotateY(RotationSpeed * (float)delta * ((Angle > Mathf.Tau / 2) ? -1 : 1));
            return true;
        }
        return false;
    }

	private bool RotateTowardsPlayerZ(double delta)
	{
        Vector3 Reference = Body.GlobalRotation;
        Body.LookAt(PlayerNode.GlobalPosition);
        float Angle = Body.Basis.Z.AngleTo(PlayerNode.GlobalPosition);
        Vector3 Difference = Body.GlobalRotation - Reference;
        Vector3 AmountToRotate = Difference * RotationSpeed * (float)delta;
        Reference += AmountToRotate;//( < Difference) ? AmountToRotate : Difference;
        Body.GlobalRotation = Reference;
        // if (Mathf.PosMod(Angle, MathF.Tau/2) < Mathf.DegToRad(45))
        // {
            // return true;
        // }
        // return false;
        return true;

    }

    private void AvoidPlayer(double delta)
    {
        Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
        float DistanceToPlayer = Difference.Length();
        if (DistanceToPlayer < 10)
        {
            Velocity += Difference.Normalized() * -2.0f * (float)delta;
        }
        else if(DistanceToPlayer < 40)
        {
            Velocity += Difference.Normalized() * 2.0f * (float)delta;
        }
        else
        {
            Velocity = Vector3.Zero;
        }
    }

	public override void _PhysicsProcess(double delta)
	{
        if (PlayerNode is not null)
        {
            if (IsPlayerVisible())
            {
                AvoidPlayer(delta);
                RotateTowardsPlayerY(delta);
                
                if(TurretWeapon.ReadyToShoot() && RotateTowardsPlayerZ(delta))
                {
                    TurretWeapon.Shoot();
                }
            }
        }
        else
        {
            // GD.PushWarning("PlayerNode is not set yet, trying again...");
            PlayerNode = HP.GetPlayerNode();
        }

        MoveAndSlide();
    }
}
