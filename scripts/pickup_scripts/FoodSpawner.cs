using Godot;
using System;

[Tool]
public partial class FoodSpawner : Node3D
{
    // private Food.FoodType SpawnType = Food.FoodType.GummyBear;

    private string EditorModel = "res://assets/models/Factory.obj";

    [Export] private float RespawnTime = 5.0f;

    [Export] private bool DisablePhysics = true;

    private string DefaultFoodScenePath = "res://subscenes/pickup_subscenes/BasePickup.tscn";
	[Export] private PackedScene FoodScene;

    Food SpawnedPickup;

	private void InitPickup()
	{
        SpawnedPickup = FoodScene.Instantiate<Food>();
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
            if (FoodScene is null)
            {
                FoodScene = ResourceLoader.Load<PackedScene>(DefaultFoodScenePath);
            }
            CallDeferred("InitPickup");
        }
    }
}
