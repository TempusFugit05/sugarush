using Godot;
using Helpers;

public partial class Weapon : Node3D

{	
	protected Node3D ProjectileSpawnPoint;

	protected void InitShootingHandler()
	{
		ProjectileSpawnPoint = GetNode<Node3D>("ProjectileSpawnPoint");
	}

    protected void ApplyBulletHoleDecal(Godot.Collections.Dictionary RayDict)
	{
        Node3D Collider = (Node3D)RayDict["collider"];
        if (Collider is Isoulful || Collider is RigidBody3D)
		{
            return;
        }
        Node3D ObjectHit = Collider;
		BulletHole ChildDecal = (BulletHole)DecalScene.Instantiate(); // Create decal on the hit node 
		ObjectHit.AddChild(ChildDecal);
        ChildDecal.InitDecal((Vector3)RayDict["normal"]);
		ChildDecal.GlobalPosition = (Vector3)RayDict["position"];		
	}

    protected Vector3 GetCameraNormal()
	{
		return PlayerCameraNode.ProjectRayNormal(PlayerCameraNode.GetViewport().GetVisibleRect().GetCenter());
    }

    protected void ShootFromCamera(Vector2[] angles = null)
	{
        Vector3 CameraNormal = GetCameraNormal();
        Vector3 ProjectileStartPos = PlayerCameraNode.ProjectRayOrigin(PlayerCameraNode.GetViewport().GetMousePosition()); // Convert the middle of the screen into a point in 3d space
		
		// Create a basis for rotation
		Basis rotationBasis;
		
		// Get the camera's right vector (X axis)
		Vector3 cameraRight = CameraNormal.Cross(Vector3.Up).Normalized();
		
		// Get the camera's up vector (Y axis)
		Vector3 cameraUp = cameraRight.Cross(CameraNormal).Normalized();
		

		if (angles is not null)
		{
            for (int i = 0; i < angles.Length; i++)
            {

			// Create rotation around local right axis (pitch)
			Basis pitchRotation = new(cameraRight, Mathf.DegToRad(angles[i].Y));
			// Create rotation around local up axis (yaw)
			Basis yawRotation = new(cameraUp, Mathf.DegToRad(angles[i].X));
			
			// Combine rotations
			rotationBasis = yawRotation * pitchRotation;
			
			// Apply rotation to the camera normal
			Vector3 dirVector = rotationBasis * CameraNormal;

            Vector3 projectileEndPos = ProjectileStartPos + dirVector * Range;
            
            ShootProjectile(ProjectileStartPos, projectileEndPos);
			}
        }

		else
		{
			Vector3 ProjectileEndPos = ProjectileStartPos + (CameraNormal * Range); // Projcet a normal vector from the middle of the screen and scale the result by the projectile range
			ShootProjectile(ProjectileStartPos, ProjectileEndPos);
		}
	}

	protected void ShootFromWeapon()
	{
		Vector3 ProjectileStartPos = ProjectileSpawnPoint.GlobalPosition;
		Vector3 ProjectileEndPos = ProjectileStartPos + (GlobalTransform.Basis.X * Range);
		ShootProjectile(ProjectileStartPos, ProjectileEndPos);
	}

	/// <Summary>
	/// 	Apply damage to the object that was hit
	/// </Summary>
	protected virtual void ApplyBulletImpacts(Godot.Collections.Dictionary ImpactDict, PhysicsRayQueryParameters3D RayInfo)
	{
		if(ImpactDict is not null)
		{
			Node HitObject = (Node)ImpactDict["collider"];
            Vector3 DamagePosition = (Vector3)ImpactDict["position"];
            float DamageToApply = ApplyDamageFalloff(Damage, GlobalPosition.DistanceTo(DamagePosition));
            if(HitObject is Isoulful creature)
			{
				creature.Hurt(DamageToApply, DamagePosition); // Hurtable is a special method for objects which are damage-able
			}
			if (HitObject is RigidBody3D body)
			{
                body.ApplyImpulse((DamagePosition - RayInfo.From).Normalized() * (DamageToApply / Damage), DamagePosition - body.GlobalPosition);
            }
			if (HitObject is ISoulful soul)
			{
                soul.GetSoul().Hurt(DamageToApply, DamagePosition);
            }
		}
	}

	public void ShootProjectile(Vector3 RayStart, Vector3 RayEnd)
	{
        // Create a projectile and apply bullet hole decal on the hit object
        Godot.Collections.Array<Rid> ExclusionList = HP.GetDefaultExclusionList();
        PhysicsRayQueryParameters3D QueryParams = new()
        {
            From = RayStart,
            To = RayEnd,
            CollideWithAreas = false,
            CollideWithBodies = true,
        };
		if (ExclusionList is not null)
		{
            QueryParams.Exclude = ExclusionList;
        }
		
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(QueryParams);

		if(RayDict.Count != 0)
		{ // If the dictionary is empty, nothing was hit
			ApplyBulletHoleDecal(RayDict);
			ApplyBulletImpacts(RayDict, QueryParams); // Return ray info
		}
	}
}
