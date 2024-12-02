using System;
using System.Linq;
using Godot;
using Helpers;

public partial class Weapon : RigidBody3D
{	
	protected Node3D ProjectileSpawnPoint;

	protected void InitShootingHandler()
	{
		ProjectileSpawnPoint = GetNode<Node3D>("ProjectileSpawnPoint");
	}

    protected void ApplyBulletHoleDecal(Godot.Collections.Dictionary RayDict)
	{
        Node3D Collider = (Node3D)RayDict["collider"];
        if (Collider is not ICreature && Collider is not RigidBody3D && Collider is not SoftBody3D)
		{
			BulletHole ChildDecal = (BulletHole)WeaponSettings.DecalScene.Instantiate(); // Create decal on the hit node 
			Collider.AddChild(ChildDecal);
			ChildDecal.InitDecal((Vector3)RayDict["normal"]);
			ChildDecal.GlobalPosition = (Vector3)RayDict["position"];		
        }
	}

    protected void ShootFromCamera(Vector2[] angles = null)
	{
        Vector3 CameraNormal = PlayerCameraNode.ProjectRayNormal(PlayerCameraNode.GetViewport().GetVisibleRect().GetCenter());
        Vector3 ProjectileStartPos = PlayerCameraNode.ProjectRayOrigin(PlayerCameraNode.GetViewport().GetMousePosition()); // Convert the middle of the screen into a point in 3d space
				
		Vector3 cameraRight = PlayerCameraNode.GlobalBasis.X.Normalized();
		Vector3 cameraUp = PlayerCameraNode.GlobalBasis.Y.Normalized();

		if (angles is not null)
		{
            for (int i = 0; i < angles.Length; i++)
            {
				/*Apply user-defined rotations*/
				float randRotX = (GD.Randf() - 0.5f) * 2 * WeaponSettings.RandSpreadX;
				float randRotY = (GD.Randf() - 0.5f) * 2 * WeaponSettings.RandSpreadY;
				Quaternion rotLeftRight = new(cameraRight, Mathf.DegToRad(angles[i].X + randRotX));
				Quaternion rotUpDown = new(cameraUp, Mathf.DegToRad(angles[i].Y + randRotY));

				Vector3 dirVector = CameraNormal * (rotUpDown * rotLeftRight);

				Vector3 projectileEndPos = ProjectileStartPos + dirVector * WeaponSettings.Range;
				
				ShootProjectile(ProjectileStartPos, projectileEndPos);
			}
        }

		else
		{
            Vector3 ProjectileEndPos = ProjectileStartPos + (CameraNormal * WeaponSettings.Range); // Projcet a normal vector from the middle of the screen and scale the result by the projectile range
			ShootProjectile(ProjectileStartPos, ProjectileEndPos);
		}
	}

	protected void ShootFromWeapon()
	{
		Vector3 ProjectileStartPos = ProjectileSpawnPoint.GlobalPosition;
		Vector3 ProjectileEndPos = ProjectileStartPos + (GlobalTransform.Basis.X * WeaponSettings.Range);
		ShootProjectile(ProjectileStartPos, ProjectileEndPos);
	}

	protected virtual void EmitImpactParticle(Godot.Collections.Dictionary impactDict, PhysicsRayQueryParameters3D rayInfo, Node3D obj, Vector3 hitPos)
	{
		GpuParticles3D particle = (GpuParticles3D)WeaponSettings.Metal.Instantiate();
		obj.AddChild(particle);
		particle.GlobalPosition = hitPos;
		particle.Quaternion = new ((rayInfo.To - rayInfo.From).Reflect((Vector3)impactDict["normal"]).Cross(Vector3.Up).Normalized(), Mathf.DegToRad(90)); // Simulate "reflection" of partices off of the surface
		particle.Finished += particle.QueueFree; // Delete after emmision is done
		particle.Emitting = true;
	}

	/// <Summary>
	/// 	Apply damage to the object that was hit
	/// </Summary>
	protected virtual void ApplyBulletImpacts(Godot.Collections.Dictionary impactDict, PhysicsRayQueryParameters3D rayInfo)
	{
		if(impactDict is not null)
		{
			Node3D hitObject = (Node3D)impactDict["collider"];
			Vector3 damagePosition = (Vector3)impactDict["position"];
			float damageToApply = ApplyDamageFalloff(WeaponSettings.Damage, GlobalPosition.DistanceTo(damagePosition));

			if(hitObject is ICreature creature)
			{
				if (creature is RigidBody3D rigidCreature)
				{
					ulong colliderShapeId = rigidCreature.ShapeOwnerGetOwner(rigidCreature.ShapeFindOwner((int)impactDict["shape"])).GetInstanceId();
					creature.Hurt(damageToApply, damagePosition, colliderShapeId);
				}
				else
				{
					creature.Hurt(damageToApply, damagePosition);
				}
			}
			if (hitObject is RigidBody3D body)
			{
				body.ApplyImpulse((damagePosition - rayInfo.From).Normalized() * (damageToApply / WeaponSettings.Damage), damagePosition - body.GlobalPosition);
			}
			EmitImpactParticle(impactDict, rayInfo, hitObject, damagePosition);
		}
	}

	private void ShootBullet(Godot.Collections.Array<Rid> ignoreList, Vector3 start, Vector3 end)
	{
        PhysicsRayQueryParameters3D QueryParams = new()
        {
            From = start,
            To = end,
            CollideWithAreas = false,
            CollideWithBodies = true,
        };

        if (ignoreList is not null)
		{
            QueryParams.Exclude = ignoreList;

        }
        
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(QueryParams);

		if(RayDict.Count != 0)
		{ // If the dictionary is empty, nothing was hit
			if ((object)RayDict["collider"] is not null)
			{
                if (WeaponSettings.EnableDecals)
				{
					ApplyBulletHoleDecal(RayDict);	
				}
				ApplyBulletImpacts(RayDict, QueryParams); // Return ray info
			}	
		}
	}

	public void ShootProjectile(Vector3 start, Vector3 end)
	{
        // Create a projectile and apply bullet hole decal on the hit object
        Godot.Collections.Array<Rid> ExclusionList = HP.GetDefaultExclusionList();
		ExclusionList.AddRange(WeaponSettings.ExclusionList);

		switch (WeaponSettings.ProjectileType)
		{
			case ProjectileTypeEnum.HitScan:
			{
				ShootBullet(ExclusionList, start, end);
				break;
			}
		}

	}
}
