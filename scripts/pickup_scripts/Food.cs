using Godot;
using System;
// using PickupInterfaces;

public partial class Food : RigidBody3D, IPickable
{

	public enum FoodType
	{
		SugarCube,
		GummyBear,
		Cupcake,
		Cake
	}

	[Export]
	public FoodType PickupFoodType = FoodType.Cake;

	[Export]
	private Vector3 BobOffset = new (0, 0.1f, 0);

	[Export]
	private float BobAnimationCycleTime = 1f;

	[Export]
	private float SpinAnimationCycleTime = 2f;

	[Export]
	private float SpawnFadeTime = 0.75f;

    [Export]
    public bool DisablePhysics = false;

    private float SugarAmount = 0;

	public Vector3 InitialPosition;

	private FoodSpawner Spawner;

	MeshInstance3D MeshInstance;

	public void SetSpawner(FoodSpawner spawner) { Spawner = spawner; }

	/// <summary>
	/// Returns the amount of sugar this pickup holds
	/// </summary>
	/// <returns>The amount of sugar</returns>
	public float GetSugar()
	{
		return SugarAmount;
	}

	private void InitTweens()
	{
		if (Spawner is not null)
		{
			Tween FadeTween = GetTree().CreateTween().BindNode(this).SetLoops();
			MeshInstance.Transparency = 1;
			FadeTween.TweenProperty(MeshInstance, "transparency", 0, SpawnFadeTime);
		}

		Tween BobTween = GetTree().CreateTween().BindNode(this).SetLoops();
		BobTween.SetTrans(Tween.TransitionType.Sine);
		BobTween.SetEase(Tween.EaseType.InOut);
		BobTween.TweenProperty(this, "global_position", InitialPosition - BobOffset, BobAnimationCycleTime / 2);
		BobTween.TweenProperty(this, "global_position", InitialPosition + BobOffset, BobAnimationCycleTime / 2);

		Tween SpinTween = GetTree().CreateTween().BindNode(this).SetLoops();
		SpinTween.TweenProperty(this, "global_rotation", new Vector3(0, Mathf.DegToRad(360), 0), SpinAnimationCycleTime);
		SpinTween.SetParallel();
	}


    /// <summary>
    ///     Create the pickup and collision boxes of the node.
    /// </summary>
    private void CreateCollisionBox()
    {
        void CreateBoxShape(CollisionShape3D Node)
        {
            BoxShape3D Box = (BoxShape3D)Node.GetShape();
            Box.Size = (MeshInstance.GetAabb()*MeshInstance.GlobalTransform).Size;
        }
        CreateBoxShape(GetNode<CollisionShape3D>("FoodPickupRange/PickupBox")); // Create box shape for player pickup
        CreateBoxShape(GetNode<CollisionShape3D>("CollisionShape3D")); // Create box shape for ccollisions
        // GetNode<CollisionShape3D>("CollisionShape3D").;
    }

	private void InitNode()
	{
		MeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

		if (Spawner is not null)
		{
			GlobalPosition = InitialPosition + BobOffset;
		}
		
		switch (PickupFoodType)
		{
			case FoodType.SugarCube:
				SugarAmount = 5;
				break;
			case FoodType.GummyBear:
				SugarAmount = 15;
				MeshInstance.Mesh = ResourceLoader.Load<Mesh>("res://assets/models/gummy_bear.obj");
				break;
			case FoodType.Cupcake:
				SugarAmount = 50;
				break;
			case FoodType.Cake:
				SugarAmount = 100;
				MeshInstance.Mesh = ResourceLoader.Load<Mesh>("res://assets/models/cake.obj");
				MeshInstance.Scale = new Vector3(0.75f, 0.75f, 0.75f);
				break;
		}

        CollisionLayer = 2;
        CreateCollisionBox();
        InitTweens();
	}
	

	public override void _Ready()
	{
		CallDeferred("InitNode");
	}

    public override void _PhysicsProcess(double delta)
    {
        
        // MoveAndCollide();
    }

    /// <summary>
    /// Notify pickup that it has been picked up
    /// </summary>
    

	/// <summary>
	/// Notify pickup that it has been picked up
	/// </summary>
	public void Pickup()
	{
		Spawner?.NotifyPickedUp();
		QueueFree();
	}

}
