// using System;
// using Godot;
// using Helpers;

// public partial class Turret : Enemy
// {
// 	// Called when the node enters the scene tree for the first time.
// 	Weapon TurretWeapon;
//     Node3D PlayerNode;
//     Node3D WeaponHolder;
//     Node3D Body;
//     Aabb HitBox;

//     [Export]
// 	private float RotationSpeed = 100.0f; // Speed of rotation

//     [Export]
//     private float AvoidanceDistance = 10.0f;

//     [Export]
//     private float DetectionRadius = 75.0f;

//     [Export]
//     private float GroundDetectionDistance = 75.0f;

//     [Export]
//     private float HoverHeight = 1.5f;

//     [Export]
//     private float HoverForce = 5.0f;

//     [Export]
//     private float AttractionForce = 10f;

//     [Export]
//     private float MaintainanceForce = 1.5f;

//     [Export]
//     private float MaxHorizontalSpeed = 4.0f;

//     [Export]
//     private float MaxVerticalSpeed = 1.0f;

//     [Export]
//     private float ImpactVelocityDamageStart = 10.0f;

//     [Export]
//     private float ImpactDamagePerUnit = 5.0f;

//     [Export]
//     private float Mass = 50.0f;

//     private Vector3 Force;

//     private Vector3 CollisionVel;

//     private Vector3 RefVel;


//     private bool WasHurt = false;

//     private float Resistance = 0;

//     public override void _Ready()
// 	{
//         var enemy = new Enemy();
//         var enemy1 = new CharacterBody3D();
//         Body = GetNode<Node3D>("Body");
//         HitBox = Body.GetNode<MeshInstance3D>("MeshInstance3D").GetAabb();
//         HoverHeight += HitBox.Size.Y / 2;
//         WeaponHolder = Body.GetNode<Node3D>("WeaponHolder");
// 		TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
//         PlayerNode = HR.GetPlayerNode();
//         InitNode();
// 	}

//     protected override void OnHurt(float damage, Vector3 position = default)
//     {
//         WasHurt = true;
//         if (position != default)
//         {
//             Velocity += (GlobalPosition - position).Normalized() * damage / (0.75f * Mass);
//         }
//     }

//     private float GetHeightAboveGround()
//     {
//         PhysicsRayQueryParameters3D Query = new()
//         {
//             From = GlobalPosition,
//             To = Vector3.Down * GroundDetectionDistance,
//             Exclude = new Godot.Collections.Array<Rid> { GetRid() },
//             CollideWithAreas = false,
//             CollideWithBodies = true,
//         };
//         Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
//         float outDistance = float.NegativeInfinity;
//         if (RayDict.Count != 0)
//         {
//             outDistance = ((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) - (HitBox.Size.Y / 2);
//         }
//         return outDistance;
//     }

//     private bool IsPlayerVisible()
//     {
//         PhysicsRayQueryParameters3D Query = new()
//         {
//             From = GlobalPosition,
//             To = PlayerNode.GlobalPosition,
//             Exclude = new Godot.Collections.Array<Rid> { GetRid() },
//             CollideWithAreas = false,
// 			CollideWithBodies = true,
//         };

//         Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
//         if (RayDict.Count != 0)
//         {
//             if ((Node)RayDict["collider"] is Character && (((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) <= DetectionRadius))
//             {
//                 return true;
//             }
//         }
        
//         return false;

//     }

// 	private void RotateTowardsPlayer(double delta)
// 	{
//         static Quaternion RotateTowards(Quaternion from, Quaternion to, float speed)
//         {
//             float angle = from.AngleTo(to);
//             if (angle > speed)
//             {
//                 return from.Slerp(to, speed / angle);
//             }

//             return to;
//         }

//         static Quaternion ProjectQuaternion(Quaternion original, Vector3 projection)
//         {
//             /*Normalize inputs to produce a normalized output*/
//             projection = projection.Normalized();
//             original = original.Normalized();

//             Vector3 Projection = original.GetAxis().Project(projection); // Project the quaternion axis onto the new axis
//             Quaternion Out = new(Projection.X, Projection.Y, Projection.Z, original.W); // Construct modified quaternion
//             if (Out.IsFinite())
//             {
//                 return Out;
//             }
//             return original;
//         }

//         static Quaternion ValidateQuaternion(Quaternion quatToCheck)
//         {
//             if (quatToCheck.IsFinite())
//             {
//                 return quatToCheck;
//             }
//             return new Quaternion(0, 0, 0, 1); // Return new normalized quaternion
//         }

//         Quaternion targetRotation;
//         Quaternion FinalRotation;

//         targetRotation = WeaponHolder.GlobalTransform.LookingAt(PlayerNode.GlobalPosition, Vector3.Down).Basis.GetRotationQuaternion();
//         targetRotation = ProjectQuaternion(targetRotation, GlobalBasis.X); // Extract rotation around x axis
//         FinalRotation = RotateTowards(WeaponHolder.Quaternion.Normalized(), targetRotation.Normalized(), Mathf.Pi / 8 * (float)delta);
//         WeaponHolder.Quaternion = ValidateQuaternion(FinalRotation);


//         targetRotation = Body.GlobalTransform.LookingAt(PlayerNode.GlobalPosition, Vector3.Down).Basis.GetRotationQuaternion();
//         targetRotation = ProjectQuaternion(targetRotation, GlobalBasis.Y); // Extract rotation around x axis
//         FinalRotation = RotateTowards(Body.Quaternion.Normalized(), targetRotation.Normalized(), Mathf.Pi / 3 * (float)delta);
//         Body.Quaternion = ValidateQuaternion(FinalRotation);
//     }

//     private bool IsLookingAtTarget(Node3D lookingNode, Node3D target)
//     {
//         if (lookingNode.GlobalBasis.Z.AngleTo(lookingNode.GlobalPosition - target.GlobalPosition) <= Mathf.DegToRad(5))
//         {
//             return true;
//         }
//         return false;
//     }

//     private Vector3 ApplyFriction(Vector3 velocity)
//     {
//         if(velocity.Length() >= 0.01f)
//         {
//             Force += -velocity * 1.5f * Mass;
//         }
//         return velocity;
//     }

//     private void MaintainSpeed()
//     {
//         void oppose(Vector3 speed, float magnitude, float maxSpeed)
//         {
//             if (magnitude > maxSpeed)
//             {
//                 speed = speed.Normalized();
//                 Force += -speed * Mass * MaintainanceForce;
//             }
//         }

//         Vector3 axisSpeed = new (Velocity.X, 0, Velocity.Z);
//         oppose(axisSpeed, axisSpeed.Length(), MaxHorizontalSpeed);

//         // axisSpeed = new (0, Velocity.Y, 0);
//         // oppose(axisSpeed, axisSpeed.Length(), MaxVerticalSpeed);

//     }

//     private void AvoidPlayer()
//     {
//         Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
//         Difference = new(Difference.X, 0, Difference.Z);
//         float DistanceToPlayer = Difference.Length();
//         Vector3 horizontalVel = new(Velocity.X, 0, Velocity.Z);
//         Vector3 projectedVel = horizontalVel.Normalized().Project(Difference.Normalized()) * horizontalVel.Length();
//         float velToPlayer = projectedVel.Length() * projectedVel.Sign().X;
//         if (DistanceToPlayer < AvoidanceDistance && velToPlayer > -MaxHorizontalSpeed)
//         {
//             // float distanceFactor = Mathf.Clamp(AvoidanceDistance - DistanceToPlayer, 1.0f, 1.5f);
//             Force += Difference.Normalized() * -(AttractionForce);
//         }

//         else if(DistanceToPlayer > AvoidanceDistance && velToPlayer < MaxHorizontalSpeed)
//         {
//             // float distanceFactor = Mathf.Clamp(DistanceToPlayer - AvoidanceDistance, 0.75f, 1.0f);
//             Force += Difference.Normalized() * (AttractionForce);
//         }
//     }

//     private void Hover()
//     {
//         float distance = GetHeightAboveGround();
//         Vector3 gravityResist = -GetGravity() * Mass;
//         Vector3 hoverForce = -GetGravity().Normalized() * HoverForce;
//         if (distance > 0)
//         {
//             if (distance < HoverHeight && Velocity.Y < MaxVerticalSpeed)
//             {
//                 float distanceFactor = Mathf.Clamp(HoverHeight - distance, 0.0f, 0.5f);
//                 Force += gravityResist + hoverForce + (hoverForce * distanceFactor);
//             }
//         }
//         else if (distance < 0)
//         {
//             Force += gravityResist;
//         }
//     }

//     private void ApplyGravity()
//     {
//         Force += GetGravity() * Mass;
//     }

//     private Vector3 GetVelocity(double delta)
//     {
//         return Force *  (float)delta / Mass;
//     }

//     private void ApplyForces(double delta)
//     {
//         Vector3 FrictionlessVel = Velocity + GetVelocity(delta);
//         Force += ApplyFriction(FrictionlessVel);
//         Velocity += GetVelocity(delta);
//         Force = Vector3.Zero;
//     }

//     public void ApplyCollisionDamage(KinematicCollision3D outsiderCollision = default)
//     {
//         KinematicCollision3D collision;
//         if (outsiderCollision == default)
//         {
//             collision = GetLastSlideCollision();
//         }
//         else
//         {
//             collision = outsiderCollision;
//         }

//         if (collision is not null)
//         {
//             Vector3 colliderVel = Vector3.Zero;
//             if (collision.GetColliderVelocity() != Vector3.Zero && CollisionVel != Vector3.Zero)
//             {
//                 colliderVel = collision.GetColliderVelocity().Project(CollisionVel);
//             }

//             float collisionImpact = (CollisionVel + colliderVel - RefVel).Length();
            
//             if (collisionImpact >= ImpactVelocityDamageStart && !WasHurt)
//             {
//                 Hurt(collisionImpact * ImpactDamagePerUnit);
//                 for (int i = 0; i < collision.GetCollisionCount(); i++)
//                 {
//                     if (collision.GetCollider(i) is Turret enemy && outsiderCollision == default)
//                     {
//                         enemy.Hurt(collisionImpact * ImpactDamagePerUnit);
//                     }
//                 }
//             }
//         }
//     }

//     public override void _PhysicsProcess(double delta)
// 	{
//         if (PlayerNode is not null)
//         {
//             ApplyCollisionDamage();
//             WasHurt = false;
//             ApplyGravity();
//             Hover();
//             MaintainSpeed();
            
//             if (IsPlayerVisible())
//             {
//                 AvoidPlayer();
//                 RotateTowardsPlayer(delta);
//                 if(TurretWeapon.ReadyToShoot() && IsLookingAtTarget(WeaponHolder, PlayerNode))
//                 {
//                     TurretWeapon.Shoot();
//                 }
//             }
//             ApplyForces(delta);
//         }
//         else
//         {
//             // GD.PushWarning("PlayerNode is not set yet, trying again...");
//             PlayerNode = HR.GetPlayerNode();
//         }
//         RefVel = Velocity; // Update reference velocity
//         if (MoveAndSlide())
//         {
//             CollisionVel = Velocity;
//             Resistance = HP.GetGroundResistance();
//         }
//         else
//         {
//             Resistance = HP.GetAirResistance();
//         }
//     }
// }
