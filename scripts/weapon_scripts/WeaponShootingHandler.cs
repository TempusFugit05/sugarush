using Godot;

public partial class Weapon : Node3D

{	
	protected Node3D ProjectileSpawnPoint;

	protected void InitShootingHandler()
	{
		ProjectileSpawnPoint = GetNode<Node3D>("ProjectileSpawnPoint");
		// DecalScene = ResourceLoader.Load<PackedScene>("res://subscenes/BulletDecal.tscn");
	}

    protected void ApplyBulletHoleDecal(Godot.Collections.Dictionary RayDict)
	{
		Node3D ObjectHit = (Node3D)RayDict["collider"];
		BulletHole ChildDecal = (BulletHole)DecalScene.Instantiate(); // Create decal on the hit node 
		ObjectHit.AddChild(ChildDecal);
		ChildDecal.InitDecal((Vector3)RayDict["normal"]);
		ChildDecal.GlobalPosition = (Vector3)RayDict["position"];		
	}

    protected Vector3 GetCameraNormal()
	{
		return CameraNode.ProjectRayNormal(CameraNode.GetViewport().GetVisibleRect().GetCenter());
    }

    protected void ShootFromCamera(Vector2[] angles = null)
	{
        Vector3 CameraNormal = GetCameraNormal();
        Vector3 ProjectileStartPos = CameraNode.ProjectRayOrigin(CameraNode.GetViewport().GetMousePosition()); // Convert the middle of the screen into a point in 3d space
		if (angles is not null)
		{
            for (int i = 0; i < angles.Length; i++)
            {
            // Create a basis for rotation
            Basis rotationBasis = new Basis();
            
            // Get the camera's right vector (X axis)
            Vector3 cameraRight = CameraNormal.Cross(Vector3.Up).Normalized();
            // Get the camera's up vector (Y axis)
            Vector3 cameraUp = cameraRight.Cross(CameraNormal).Normalized();
            
            // Create rotation around local right axis (pitch)
            Basis pitchRotation = new Basis(cameraRight, Mathf.DegToRad(angles[i].Y));
            // Create rotation around local up axis (yaw)
            Basis yawRotation = new Basis(cameraUp, Mathf.DegToRad(angles[i].X));
            
            // Combine rotations
            rotationBasis = yawRotation * pitchRotation;
            
            // Apply rotation to the camera normal
            Vector3 dirVector = rotationBasis * CameraNormal;
            Vector3 projectileEndPos = ProjectileStartPos + dirVector * Range;
            
            ApplyDamage(ShootProjectile(ProjectileStartPos, projectileEndPos));
			}
        }

		else
		{
			Vector3 ProjectileEndPos = ProjectileStartPos + (CameraNormal * Range); // Projcet a normal vector from the middle of the screen and scale the result by the projectile range
			ApplyDamage(ShootProjectile(ProjectileStartPos, ProjectileEndPos));
		}
	}

	protected void ShootFromWeapon()
	{
		Vector3 ProjectileStartPos = ProjectileSpawnPoint.GlobalPosition;
		Vector3 ProjectileEndPos = ProjectileStartPos + (GlobalTransform.Basis.X * Range);
		ApplyDamage(ShootProjectile(ProjectileStartPos, ProjectileEndPos));
	}

	protected void ShootFromWeaponToCamera()
	{
		Vector3 ProjectileStartPos = ProjectileSpawnPoint.GlobalPosition;

		PhysicsRayQueryParameters3D QueryParams = new ()
		{
			From = CameraNode.ProjectRayOrigin(CameraNode.GetViewport().GetMousePosition()),
			To = ProjectileStartPos + CameraNode.ProjectRayNormal(CameraNode.GetViewport().GetMousePosition()) * 100000,
			CollideWithAreas = false,
			CollideWithBodies = true,
		}; // Query parameters for the ray query
		
		var RayDict = GetWorld3D().DirectSpaceState.IntersectRay(QueryParams);

		if(RayDict.Count != 0)
		{
			if(ProjectileStartPos.DistanceTo((Vector3)RayDict["position"]) <= Range)
			{
				Vector3 ProjectileEndPos = ProjectileStartPos + ((Vector3)RayDict["position"] * 10); // Project a normal vector from the middle of the screen and scale the result by the projectile range
				ApplyDamage(ShootProjectile(ProjectileStartPos, ProjectileEndPos));
			}
		}
	}

	public Godot.Collections.Dictionary ShootProjectile(Vector3 RayStart, Vector3 RayEnd)
	{
		// Create a projectile and apply bullet hole decal on the hit object

		PhysicsRayQueryParameters3D QueryParams = new ()
		{
			From = RayStart,
			To = RayEnd,
			CollideWithAreas = false,
			CollideWithBodies = true,
		};
		Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(QueryParams);

		if(RayDict.Count != 0)
		{ // If the dictionary is empty, nothing was hit
			ApplyBulletHoleDecal(RayDict);
			return RayDict; // Return ray info
		}


		return null; // Return nothing if no object was hit
	}
}
