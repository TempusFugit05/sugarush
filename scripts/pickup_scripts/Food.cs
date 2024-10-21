using Godot;
using System;
// using PickupInterfaces;

public partial class Food : Pickup
{
	public enum FoodType
	{
		SugarCube,
		GummyBear,
		Cupcake,
		Cake
	}

	private float SugarAmount = 0;
	private bool IsPickedUp = false;
	public FoodType _FoodType = FoodType.Cake;

	public float GetSugar()
	{
		return SugarAmount;
	}

	private void InitParams()
	{
		MeshInstance3D MeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		switch (_FoodType)
		{
			case FoodType.SugarCube:
				SugarAmount = 5;
				break;
			case FoodType.GummyBear:
				SugarAmount = 15;
				MeshInstance.Mesh = ResourceLoader.Load<Mesh>("res://assets/models/gummy_bear.obj");
				break;
			case FoodType.Cupcake:
				SugarAmount = 30;
				break;
			case FoodType.Cake:
				SugarAmount = 60;
				MeshInstance.Mesh = ResourceLoader.Load<Mesh>("res://assets/models/cake.obj");
				Scale = new Vector3(5,5,5);
				break;
		}
	}

	public override void _Ready()
	{
		InitParams();
	}

	public override void _PhysicsProcess(double delta)
	{
		if(IsPickedUp)
		{
			OnPickup();
		}
	}

	public Food()
	{
		SugarAmount = 7.5f;
	}

	public override void MarkAsPickedUp()
	{
		IsPickedUp = true;
	}

	public override void OnPickup()
	{
		QueueFree();
	}

}