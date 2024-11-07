using Godot;
using System;

[Tool]
public partial class FoodSpawner : Node3D
{
    [Export]
    private Food.FoodType SpawnType = Food.FoodType.GummyBear;

    [Export]
    private float RespawnTime = 5.0f;

    [Export]
    private string EditorModel = "res://assets/models/Factory.obj";

    [Export]
    private bool DisablePhysics = true;

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
        SpawnedPickup.DisablePhysics = DisablePhysics;
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
            EditorMesh.Mesh = (Mesh)ResourceLoader.Load(EditorModel);
        }

        else
        {
            FoodScene = ResourceLoader.Load<PackedScene>(FoodScenePath);
            CallDeferred("InitPickup");
        }
    }
}
