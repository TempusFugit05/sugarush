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
	private float RotationSpeed = 100.0f; // Speed of rotation in radians

    [Export]
    private float DetectionRadius = 30.0f;

    [Export]
    float Angle;

    [Export]
    float GroundDetectionDistance = 50.0f;

    public override void _Ready()
	{
        Body = GetNode<Node3D>("Body");
        WeaponHolder = Body.GetNode<Node3D>("WeaponHolder");
		TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
        PlayerNode = HP.GetPlayerNode();
        InitNode();
	}

    private float GetHeightAboveGround()
    {
        PhysicsRayQueryParameters3D Query = new()
        {
            From = GlobalPosition,
            To = Vector3.Down * GroundDetectionDistance,
            Exclude = new Godot.Collections.Array<Rid> { GetRid() },
            CollideWithAreas = false,
            CollideWithBodies = true,
        };
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
        if (RayDict.Count != 0)
        {
            return ((Vector3)RayDict["position"]).DistanceTo(GlobalPosition);
        }
        return float.PositiveInfinity;
    }

    private bool IsPlayerVisible()
    {
        PhysicsRayQueryParameters3D Query = new()
        {
            From = GlobalPosition,
            To = PlayerNode.GlobalPosition,
            Exclude = new Godot.Collections.Array<Rid> { GetRid() },
            CollideWithAreas = false,
			CollideWithBodies = true,
        };
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
        if ((Node)RayDict["collider"] is Character && (((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) <= DetectionRadius))
        {
            return true;
        }
        
        return false;

    }

	private bool RotateTowardsPlayer(double delta)
	{

    static Quaternion RotateTowards(Quaternion from, Quaternion to, float speed)
    {
        float angle = from.AngleTo(to);
        if (angle > speed)
        {
            return from.Slerp(to, speed / angle);
        }

        return to;
    }

        static Quaternion ProjectQuaternion(Quaternion original, Vector3 projection)
        {
            /*Normalize inputs to produce a normalized output*/
            projection = projection.Normalized();
            original = original.Normalized();

            Vector3 Projection = original.GetAxis().Project(projection); // Project the quaternion axis onto the new axis
            Quaternion Out = new(Projection.X, Projection.Y, Projection.Z, original.W); // Construct modified quaternion
            if (Out.IsFinite())
            {
                return Out;
            }
            return original;
        }
        Quaternion targetRotation;

        targetRotation = WeaponHolder.GlobalTransform.LookingAt(PlayerNode.GlobalPosition, Vector3.Down).Basis.GetRotationQuaternion();
        targetRotation = ProjectQuaternion(targetRotation, GlobalBasis.X); // Extract rotation around x axis
        WeaponHolder.Quaternion = RotateTowards(WeaponHolder.Quaternion.Normalized(), targetRotation.Normalized(), Mathf.Pi / 8 * (float)delta);
        
        targetRotation = Body.GlobalTransform.LookingAt(PlayerNode.GlobalPosition, Vector3.Down).Basis.GetRotationQuaternion();
        targetRotation = ProjectQuaternion(targetRotation, GlobalBasis.Y); // Extract rotation around x axis
        Body.Quaternion = RotateTowards(Body.Quaternion.Normalized(), targetRotation.Normalized(), Mathf.Pi / 3 * (float)delta);

        return false;
    }

    private void ApplyFriction(double delta)
    {
        if(Velocity.Length() >= 0.01f)
        {
            Velocity -= Velocity * 0.5f * (float)delta;
        }
        else
        {
            Velocity = Vector3.Zero;
        }
    }

    private void AvoidPlayer(double delta)
    {
        Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
        float DistanceToPlayer = Difference.Length();
        if (DistanceToPlayer < 10)
        {
            Velocity += Difference.Normalized() * -2.0f * (float)delta;
        }
        else if(DistanceToPlayer < DetectionRadius)
        {
            Velocity += Difference.Normalized() * 2.0f * (float)delta;
        }
        else
        {
            ApplyFriction(delta);
        }
    }

    private void Hover(double delta)
    {
        float Distance = GetHeightAboveGround();
        if (Distance > 2.5f)
        {
            Velocity += Vector3.Up * 0.5f * (float)delta;
        }
        else
        {
            Velocity += Vector3.Down * 0.5f * (float)delta;
        }
    }

	public override void _PhysicsProcess(double delta)
	{
        if (PlayerNode is not null)
        {
            if (IsPlayerVisible())
            {
                ApplyFriction(delta);
                Hover(delta);
                AvoidPlayer(delta);
                RotateTowardsPlayer(delta);
                if(TurretWeapon.ReadyToShoot())
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
