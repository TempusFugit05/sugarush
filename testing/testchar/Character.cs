using Godot;
using Helpers;

public partial class Character : CreatureBase
{
    private Camera3D Camera;

    private MovementHandler movementHandler = new();
    private SugarHandler sugarHandler = new();
    private InteractionHandler interactionHandler = new();
    private InputHandler inputHandler = new();
    private WeaponHandler weaponHandler = new();
    private CharUi Ui;

    protected override void InitCreature()
    {
        PhysicsMaterialOverride = new(){Friction=0};
        LinearDamp = 0;
        Mass = 75.0f;

        Camera = GetNode<Camera3D>("PlayerCamera");
        HR.SetPlayerNode(this);
        Ui = GetNode<CharUi>("CharUi");

        Ui.init(this);
        sugarHandler.Init(this, State);
        movementHandler.Init(this);
        interactionHandler.init(this);
        weaponHandler.Init(this);
        inputHandler.Init(this);
        InitPickupSphere();
    }
    
	public override void Hurt(float Damage, Vector3 DamagePosition = default, ulong colliderId = default)
	{
		sugarHandler.ConsumeSugar(-Damage);
        if (State.MaxHealth <= 0)
        {
            Kill();
        }
	}

	public override void Kill()
	{
        // GD.Print("Oh no I totally died!!!!1!");
    }

    public IInteractable GetInteractingNode()
    {
        return interactionHandler.InteractingWith;
    }

	public override void _Input(InputEvent CurrentInput)
	{
        if (CurrentInput is InputEventMouseMotion MouseMotion)
		{
			inputHandler.UpdateMouse(MouseMotion.ScreenRelative);
        }
	}

    public bool GetSugarush()
    {
        return sugarHandler.Sugarush;
    }

	public override void _PhysicsProcess(double delta)
	{
		weaponHandler.Run();
        interactionHandler.Run();
        Ui.Run();

        inputHandler.Run(delta);
        sugarHandler.Run(delta);
		movementHandler.Run(delta);
    }
}
