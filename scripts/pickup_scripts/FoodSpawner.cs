using Godot;
using System;

[Tool]
public partial class FoodSpawner : Node3D
{
    [Export]
    private Food.FoodType SpawnType = Food.FoodType.GummyBear;

    [Export]
    private float RespawnTime = 5.0f;

    private string FoodScenePath = "res://subscenes/pickup_subscenes/Food.tscn";

	private PackedScene FoodScene;

    Food SpawnedPickup;

	private void InitPickup()
	{
        SpawnedPickup = FoodScene.Instantiate<Food>();
        SpawnedPickup.PickupFoodType = SpawnType;
        GetTree().Root.AddChild(SpawnedPickup);
        SpawnedPickup.InitialPosition = GlobalPosition;
        SpawnedPickup.SetSpawner(this);
    }

    public void NotifyPickedUp()
    {
        GetTree().CreateTimer(RespawnTime).Timeout += InitPickup;
    }

    public override void _Ready()
	{
        if (Engine.IsEditorHint())
        {
            MeshInstance3D EditorMesh = new ();
            AddChild(EditorMesh);
            EditorMesh.Mesh = new SphereMesh()
            {
                Radius = 0.5f,
                Height = 1.0f,
            };
        }

        else
        {
            FoodScene = ResourceLoader.Load<PackedScene>(FoodScenePath);
            CallDeferred("InitPickup");
        }
    }
}
