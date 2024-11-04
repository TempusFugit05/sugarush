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
    Aabb HitBox;

    [Export]
	private float RotationSpeed = 100.0f; // Speed of rotation in radians

    [Export]
    private float AvoidanceDistance = 10.0f;

    [Export]
    private float DetectionRadius = 30.0f;

    [Export]
    private float Angle;

    [Export]
    private float GroundDetectionDistance = 50.0f;

    [Export]
    private float HoverHeight = 2.0f;

    [Export]
    private float HoverForce = 3.0f;

    [Export]
    private float AttractionForce = 1.5f;

    [Export]
    private float MaxHorizontalSpeed = 2.0f;

    private Vector3 Force;

    private float Mass = 75.0f;

    public override void _Ready()
	{
        Body = GetNode<Node3D>("Body");
        HitBox = Body.GetNode<MeshInstance3D>("MeshInstance3D").GetAabb();
        HoverHeight += HitBox.Size.Y / 2;
        WeaponHolder = Body.GetNode<Node3D>("WeaponHolder");
		TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
        PlayerNode = HP.GetPlayerNode();
        InitNode();
	}

    protected override void OnHurt(float damage, Vector3 position)
    {
        Velocity += (GlobalPosition - position).Normalized() * damage / (0.75f * Mass);
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
            return ((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) - (HitBox.Size.Y / 2);
        }
        return -1.0f;
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
        if (RayDict.Count != 0)
        {
            if ((Node)RayDict["collider"] is Character && (((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) <= DetectionRadius))
            {
                return true;
            }
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

        static Quaternion ValidateQuaternion(Quaternion quatToCheck)
        {
            if (quatToCheck.IsFinite())
            {
                return quatToCheck;
            }
            return new Quaternion();
        }

        Quaternion targetRotation;
        Quaternion FinalRotation;

        targetRotation = WeaponHolder.GlobalTransform.LookingAt(PlayerNode.GlobalPosition, Vector3.Down).Basis.GetRotationQuaternion();
        targetRotation = ProjectQuaternion(targetRotation, GlobalBasis.X); // Extract rotation around x axis
        FinalRotation = RotateTowards(WeaponHolder.Quaternion.Normalized(), targetRotation.Normalized(), Mathf.Pi / 8 * (float)delta);
        WeaponHolder.Quaternion = ValidateQuaternion(FinalRotation);


        targetRotation = Body.GlobalTransform.LookingAt(PlayerNode.GlobalPosition, Vector3.Down).Basis.GetRotationQuaternion();
        targetRotation = ProjectQuaternion(targetRotation, GlobalBasis.Y); // Extract rotation around x axis
        FinalRotation = RotateTowards(Body.Quaternion.Normalized(), targetRotation.Normalized(), Mathf.Pi / 3 * (float)delta);
        Body.Quaternion = ValidateQuaternion(FinalRotation);

        return false;
    }

    private Vector3 ApplyFriction(Vector3 velocity)
    {
        if(velocity.Length() >= 0.01f)
        {
            Force += -velocity * 1.5f * Mass;
        }
        return velocity;
    }

    private void MaintainSpeed()
    {
        Vector3 speedXY = new (Velocity.X, 0, Velocity.Z);
        float speedMagnitude = speedXY.Length();

        if (speedMagnitude > MaxHorizontalSpeed)
        {
            speedXY = speedXY.Normalized();
            Force += -speedXY * Mass * AttractionForce;
        }
    }

    private void AvoidPlayer()
    {
        Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
        float DistanceToPlayer = Difference.Length();

        if (DistanceToPlayer < AvoidanceDistance)
        {
            Force += Mass * Difference.Normalized() * (-AttractionForce / Mathf.Max(AvoidanceDistance / DistanceToPlayer, 0.01f));
        }

        else if(DistanceToPlayer < DetectionRadius)
        {
            Force += Mass * Difference.Normalized() * (AttractionForce / Mathf.Max(AvoidanceDistance / DistanceToPlayer, 0.01f));
        }
    }

    private void Hover()
    {
        float distance = GetHeightAboveGround();
        Vector3 gravityResist = -GetGravity() * Mass;
        if (distance < HoverHeight && distance > 0)
        {
            Vector3 hoverForce = -GetGravity().Normalized() * Mass * HoverForce;
            Force += gravityResist + hoverForce;
        }
        else if (distance < 0)
        {
            Force += gravityResist;
        }
        else
        {
            float distanceFactor =  1 - Mathf.Clamp(distance - HoverHeight, 0, 1);
            Force += gravityResist * distanceFactor;
        }
    }

    private void ApplyGravity()
    {
        Force += GetGravity() * Mass;
    }

    private Vector3 GetVelocity(double delta)
    {
        return (Force / Mass) * (float)delta;
    }

    private void ApplyForces(double delta)
    {
        Vector3 FrictionlessVel = Velocity + GetVelocity(delta);
        Force += ApplyFriction(FrictionlessVel);
        Velocity += GetVelocity(delta);
        Force = Vector3.Zero;
    }

    public override void _PhysicsProcess(double delta)
	{
        if (PlayerNode is not null)
        {
            if (IsPlayerVisible())
            {
                ApplyGravity();
                Hover();
                MaintainSpeed();
                AvoidPlayer();
                ApplyForces(delta);
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
