using Godot;
using System;
// using PickupInterfaces;

public partial class Food : RigidBody3D, IPickable
{

	// public enum FoodType
	// {
	// 	SugarCube,
	// 	GummyBear,
	// 	Cupcake,
	// 	Cake
	// }
	// public FoodType PickupFoodType = FoodType.Cake;
	
	private Vector3 BobOffset = new (0, 0.1f, 0);
	private float BobAnimationCycleTime = 1f;
	private float SpinAnimationCycleTime = 2f;
	private float SpawnFadeTime = 0.75f;
   
    [Export] public bool DisablePhysics = true;
    [Export] private float SugarAmount = 0;

	public Vector3 InitialPosition;

	private FoodSpawner Spawner;

	MeshInstance3D MeshInstance;

	public void SetSpawner(FoodSpawner spawner) 
	{
		Spawner = spawner;
	}

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
		if (DisablePhysics)
		{
			Tween BobTween = GetTree().CreateTween().BindNode(this).SetLoops();
			BobTween.SetTrans(Tween.TransitionType.Sine);
			BobTween.SetEase(Tween.EaseType.InOut);
			BobTween.TweenProperty(this, "global_position", InitialPosition - BobOffset, BobAnimationCycleTime / 2);
			BobTween.TweenProperty(this, "global_position", InitialPosition + BobOffset, BobAnimationCycleTime / 2);

			Tween SpinTween = GetTree().CreateTween().BindNode(this).SetLoops();
			SpinTween.TweenProperty(this, "global_rotation", new Vector3(0, Mathf.DegToRad(360), 0), SpinAnimationCycleTime);
			SpinTween.SetParallel();
		}
	}

	private void InitNode()
	{
		MeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

		if (Spawner is not null)
		{
			GlobalPosition = InitialPosition + BobOffset;
		}

		if (DisablePhysics)
		{
            AddToGroup("GIgnoreWeapons");
        }
        CollisionLayer = 2;


		BoxShape3D pickupShape = new();
		Aabb boundingBox = GetNode<MeshInstance3D>("MeshInstance3D").GetAabb();
		pickupShape.Size = boundingBox.Size;

		CollisionShape3D pickupRange = new();
		pickupRange.Shape = pickupShape;
		pickupRange.Position = (boundingBox.End + boundingBox.Position)/2;

		Area3D pickupArea = new();
		AddChild(pickupArea);
		pickupArea.AddChild(pickupRange);

		InitTweens();
	}
	

	public override void _Ready()
	{
		CallDeferred("InitNode");
	}

    public override void _PhysicsProcess(double delta)
    {
		if (DisablePhysics)
		{
            ConstantForce = Vector3.Zero;
        }
    }
    

	/// <summary>
	/// Notify pickup that it has been picked up
	/// </summary>
	public void Pickup()
	{
		Spawner?.NotifyPickedUp();
		QueueFree();
	}

}
