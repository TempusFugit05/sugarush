namespace GlobalEnums
{
	// TODO
	// Add layer enums for decal culls - DONE
	// Change projectile spawner decal to apply cull - DONE
	// Add a "Weight" parameter to the damage indicator so it will feel more impactful when dealing large amounts of damage
	// Fix decals not rotating proparely with parent
	// Add Z rotation for turret
	// Fix Pickup colliders being able to collide with player
	
	enum RenderingLayersEnum : uint
	{
		Default = 1,
		UiElemetns = 2,
	}

    enum CollisionLayersEnum : uint
    {
        Default = 1,
        NoCollide = 32,
    }
}
