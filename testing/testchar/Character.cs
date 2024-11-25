using Godot;
using Helpers;

public partial class Character : CreatureBase
{

    MovementHandler movementHandler = new();
    SugarHandler sugarHandler = new();
    InteractionHandler interactionHandler = new();
    InputHandler inputHandler = new();
    WeaponHandler weaponHandler = new();
    CharUi Ui;

    protected override void InitOrgan()
    {
        PhysicsMaterialOverride = new(){Friction=0};
        LinearDamp = 0;
        Mass = 75.0f;
        Camera = GetNode<Camera3D>("PlayerCamera");
        HR.SetPlayerNode(this);
        Ui = GetNode<CharUi>("CharUi");

        Ui.init(this);
        sugarHandler.Init(this);
        movementHandler.Init(this);
        interactionHandler.init(this);
        weaponHandler.Init(this);
        inputHandler.Init(this);
        InitPickupSphere();
    }
    
	public new void Hurt(float Damage, Vector3 DamagePosition = default, ulong colliderId = default)
	{
		sugarHandler.ConsumeSugar(-Damage);
	}

	public new void Kill()
	{
        QueueFree();
    }

    public float GetHealth()
    {
        return sugarHandler.CurrentSugar;
    }

    public float GetMaxHealth()
    {
        return SugarHandler.BaseSugar;
    }

    public bool GetSugarush()
    {
        return sugarHandler.Sugarush;
    }

	public override void _Input(InputEvent CurrentInput)
	{
        if (CurrentInput is InputEventMouseMotion MouseMotion)
		{
			inputHandler.UpdateMouse(MouseMotion.ScreenRelative);
        }
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
