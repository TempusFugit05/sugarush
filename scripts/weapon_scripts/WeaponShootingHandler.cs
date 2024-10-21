using Godot;

public partial class Weapon : Node3D

{	private Node3D ProjectileSpawnPoint;

	private void InitShootingHandler()
	{
		ProjectileSpawnPoint = GetNode<Node3D>("ProjectileSpawnPoint");
		// DecalScene = ResourceLoader.Load<PackedScene>("res://subscenes/BulletDecal.tscn");
	}

	private void ApplyBulletHoleDecal(Godot.Collections.Dictionary RayDict)
	{
		Node3D ObjectHit = (Node3D)RayDict["collider"];
		Vector3 ObjectNormal = (Vector3)RayDict["normal"]; // Normal of the surface hit for bullet hole decal
		BulletHole ChildDecal = (BulletHole)DecalScene.Instantiate(); // Create decal on the hit node 
		ObjectHit.AddChild(ChildDecal);
		ChildDecal.InitDecal((Vector3)RayDict["normal"]);
		ChildDecal.GlobalPosition = (Vector3)RayDict["position"];		
	}
	private void ShootFromCamera()
	{
		Vector3 ProjectileStartPos = CameraNode.ProjectRayOrigin(CameraNode.GetViewport().GetMousePosition()); // Convert the middle of the screen into a point in 3d space

		Vector3 ProjectileEndPos = ProjectileStartPos + (CameraNode.ProjectRayNormal(CameraNode.GetViewport().GetMousePosition()) * Range); // Project a normal vector from the middle of the screen and scale the result by the projectile range
		
		ApplyDamage(ShootProjectile(ProjectileStartPos, ProjectileEndPos));
	}

	private void ShootFromWeapon()
	{
		Vector3 ProjectileStartPos = ProjectileSpawnPoint.GlobalPosition;

		Vector3 ProjectileEndPos = ProjectileStartPos + (GlobalTransform.Basis.X * Range);

		ApplyDamage(ShootProjectile(ProjectileStartPos, ProjectileEndPos));
	}

	private void ShootFromWeaponToCamera()
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
