using Godot;
using Helpers;

public partial class PhysicalTurret : RigidBody3D, ISoulful
{
	// Called when the node enters the scene tree for the first time.
	Weapon TurretWeapon;
    Organ WeaponHolder;
    Organ Body;
    Aabb HitBox;
    Generic6DofJoint3D WeaponJoint;

    [Export]
	private float RotationSpeed = 100.0f; // Speed of rotation

    [Export]
    private float AvoidanceDistance = 10.0f;

    [Export]
    private float DetectionRadius = 75.0f;

    [Export]
    private float GroundDetectionDistance = 75.0f;

    [Export]
    private float HoverHeight = 1.5f;

    [Export]
    private float HoverForce = 10.0f;

    [Export]
    private float AttractionForce = 2.0f;

    [Export]
    private float MaintainanceForce = 1.0f;

    [Export]
    private float MaxHorizontalSpeed = 2.5f;

    [Export]
    private float MaxVerticalSpeed = 1.0f;

    [Export]
    private float ImpactVelocityDamageStart = 10.0f;

    [Export]
    private float ImpactDamagePerUnit = 5.0f;

    private Vector3 Force;

    public CreatureSoul Soul;

    private bool IsDead = false;

    private Vector3 CollisionVel;

    private Vector3 RefPos;

    private Vector3 Velocity;

    private bool WasHurt = false;

    private float Resistance = 0;
    
    public override void _Ready()
	{
        Body = GetNode<Organ>("Body");
        Body.IsVital = true;
        Body.Health = 1000;

        WeaponHolder = GetNode<Organ>("WeaponHolder");
        // WeaponHolder.Health = 1;
        TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
        
        // TurretWeapon.AddCollisionExceptionWith(WeaponHolder);
        // TurretWeapon.AddCollisionExceptionWith(Body);
        TurretWeapon.AttachmentMode = Weapon.AttachmentModeEnum.Creature;

        HitBox = Body.GetNode<MeshInstance3D>("MeshInstance3D").GetAabb();
        HoverHeight += HitBox.Size.Y / 2;

        Soul = new(this, float.PositiveInfinity, GetNode<HealthBar>("HealthBar"), new(){Body, WeaponHolder});
        
        HoverForce *= Mass;
        AttractionForce *= Mass;
        MaintainanceForce *= Mass;
	}

    public void OnHurt(float damage, Vector3 damagePosition = default)
    {
        return;
    } 
    
    public void OnKill()
    {
        IsDead = true;
        GetNode<HealthBar>("HealthBar").QueueFree();
    }

    public CreatureSoul GetSoul()
    {
        return Soul;
    }

    // protected override void OnHurt(float damage, Vector3 position = default)
    // {
    //     WasHurt = true;
    //     if (position != default)
    //     {
    //         ApplyImpulse((GlobalPosition - position).Normalized() * damage / (0.75f * Mass));
    //     }
    // }

    private float GetHeightAboveGround()
    {
        Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid> 
        { 
            GetRid(),
            Body.GetRid(),
            WeaponHolder.GetRid() 
        };

        PhysicsRayQueryParameters3D Query = new()
        {
            From = GlobalPosition,
            To = -GlobalBasis.Y * GroundDetectionDistance,
            Exclude = exclude,
            CollideWithAreas = false,
            CollideWithBodies = true,
        };
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
        float outDistance = float.NegativeInfinity;
        if (RayDict.Count != 0)
        {
            outDistance = ((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) - (HitBox.Size.Y / 2);
        }
        return outDistance;
    }

    private bool IsPlayerVisible()
    {
        Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid> 
        { GetRid(),
          Body.GetRid(),
          WeaponHolder.GetRid() };
        PhysicsRayQueryParameters3D Query = new()
        {
            From = GlobalPosition,
            To = HR.GetPlayerNode().GlobalPosition,
            Exclude = exclude,
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

	private void RotateTowardsPlayer(double delta)
	{       
        Quaternion targetRotation;
        Quaternion FinalRotation;

        Quaternion = HM.RotateTowards(Quaternion.Normalized(), new Quaternion(0, 0, 0, 1), Mathf.Pi * (float)delta);

        if (Body.IsActive)
        {
            /* Rotate body */
            targetRotation = HM.LookingAtAxis(Body, HR.GetPlayerNode(), GlobalBasis.Y);
            FinalRotation = HM.RotateTowards(Body.Quaternion, targetRotation, Mathf.Tau / 10 * (float)delta);
            Body.Quaternion = HM.ValidateQuaternion(FinalRotation);
        }

        if (WeaponHolder.IsActive)
        {
            /* Rotate weapon */
            Quaternion xComponent = HM.ProjectQuaternion(WeaponHolder.Quaternion, GlobalBasis.X);
            targetRotation = HM.LookingAtAxis(WeaponHolder, HR.GetPlayerNode(), GlobalBasis.X);
            FinalRotation = HM.RotateTowards(xComponent, targetRotation, Mathf.Tau / 10 * (float)delta);
            WeaponHolder.Quaternion = HM.ProjectQuaternion(Body.Quaternion, GlobalBasis.X).Inverse() * Body.Quaternion * FinalRotation;
        }
    }

    private bool IsLookingAtTarget(Node3D lookingNode, Node3D target)
    {
        if (lookingNode.GlobalBasis.Z.AngleTo(lookingNode.GlobalPosition - target.GlobalPosition) <= Mathf.DegToRad(5))
        {
            return true;
        }
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
        void oppose(Vector3 speed, float magnitude, float maxSpeed)
        {
            if (magnitude > maxSpeed)
            {
                speed = speed.Normalized();
                Force += -speed * Mass * MaintainanceForce;
            }
        }

        Vector3 axisSpeed = new (Velocity.X, 0, Velocity.Z);
        oppose(axisSpeed, axisSpeed.Length(), MaxHorizontalSpeed);

        // axisSpeed = new (0, Velocity.Y, 0);
        // oppose(axisSpeed, axisSpeed.Length(), MaxVerticalSpeed);

    }

    private void AvoidPlayer()
    {
        Vector3 Difference = HR.GetPlayerNode().GlobalPosition - GlobalPosition;
        Difference = new(Difference.X, 0, Difference.Z);
        float DistanceToPlayer = Difference.Length();
        Vector3 horizontalVel = new(Velocity.X, 0, Velocity.Z);
        Vector3 projectedVel = horizontalVel.Normalized().Project(Difference.Normalized()) * horizontalVel.Length();
        float velToPlayer = projectedVel.Length() * projectedVel.Sign().X;
        if (DistanceToPlayer < AvoidanceDistance && velToPlayer > -MaxHorizontalSpeed)
        {
            // float distanceFactor = Mathf.Clamp(AvoidanceDistance - DistanceToPlayer, 1.0f, 1.5f);
            Force += Difference.Normalized() * -AttractionForce;
        }

        else if(DistanceToPlayer > AvoidanceDistance && velToPlayer < MaxHorizontalSpeed)
        {
            // float distanceFactor = Mathf.Clamp(DistanceToPlayer - AvoidanceDistance, 0.75f, 1.0f);
            Force += Difference.Normalized() * AttractionForce;
        }
    }

    private void Hover()
    {
        float distance = GetHeightAboveGround();
        Vector3 hoverForce = -GetGravity().Normalized() * HoverForce;
        if (distance > 0)
        {
            if (distance < HoverHeight && Velocity.Y < MaxVerticalSpeed)
            {
                float distanceFactor = Mathf.Clamp(HoverHeight - distance, 0.0f, 0.5f);
                Force += hoverForce + (hoverForce * distanceFactor);
            }
        }
    }

    private Vector3 GetVelocity(double delta)
    {
        return (GlobalPosition - RefPos) / (float)delta;
    }

    private void ApplyForces(double delta)
    {
        Force += ApplyFriction(Velocity);
        Velocity += GetVelocity(delta);
        Force = Vector3.Zero;
    }

    public void ApplyCollisionDamage(double delta)
    {
        KinematicCollision3D Collision = MoveAndCollide(Velocity * (float)delta, true);
        if (Collision is not null)
        {
            Vector3 colliderVel = Vector3.Zero;
            if (Collision.GetColliderVelocity() != Vector3.Zero && CollisionVel != Vector3.Zero)
            {
                colliderVel = Collision.GetColliderVelocity().Project(CollisionVel);
            }

            float collisionImpact = GetVelocity(delta).Length()*Mass;
            
            if (GetVelocity(delta).Length() >= ImpactVelocityDamageStart)
            {
                Soul.Hurt(collisionImpact * ImpactDamagePerUnit);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
	{
        if (!IsDead)
        {
            Velocity = GetVelocity(delta);
            if (HR.GetPlayerNode() is not null)
            {
                WasHurt = false;
                Hover();
                MaintainSpeed();

                if (IsPlayerVisible() && WeaponHolder.IsActive)
                {
                    AvoidPlayer();
                    RotateTowardsPlayer(delta);
                    if (TurretWeapon.ReadyToShoot() && IsLookingAtTarget(WeaponHolder, HR.GetPlayerNode()))
                    {
                        TurretWeapon.Shoot();
                    }
                }

                // Velocity = GetVelocity(delta);
                // ApplyFriction(Velocity);
                ApplyCollisionDamage(delta);
                RefPos = GlobalPosition;
                ApplyCentralForce(Force);
                Force = Vector3.Zero;
            }
        }
    }
}
