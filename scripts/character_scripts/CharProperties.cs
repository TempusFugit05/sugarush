using Godot;
using System;
using System.Collections.Generic;

public partial class Character : CharacterBody3D
{
	enum InputMapEnum
	{
		KeyGoLeft,
		KeyGoRight,
		KeyGoForward,
		KeyGoBack,

		StickGoLeft,
		StickGoRight,
		StickGoForward,
		StickGoBack,

		StickLookUp,
		StickLookDown,
		StickLookLeft,
		StickLookRight,

		ActionSprint,
		ActionFire,
		ActionJump,
	}

    private readonly Dictionary<InputMapEnum, string> InputMap = new()
    {
        {InputMapEnum.KeyGoLeft, "key_go_left"},
        {InputMapEnum.KeyGoRight, "key_go_right"},
        {InputMapEnum.KeyGoForward, "key_go_forward"},
        {InputMapEnum.KeyGoBack, "key_go_back"},

        {InputMapEnum.StickGoForward, "stick_go_forward"},
        {InputMapEnum.StickGoBack, "stick_go_back"},
        {InputMapEnum.StickGoLeft, "stick_go_left"},
        {InputMapEnum.StickGoRight, "stick_go_right"},

        {InputMapEnum.StickLookUp, "stick_look_up"},
        {InputMapEnum.StickLookDown, "stick_look_down"},
        {InputMapEnum.StickLookLeft, "stick_look_left"},
        {InputMapEnum.StickLookRight, "stick_look_right"},

        {InputMapEnum.ActionSprint, "sprint"},
        {InputMapEnum.ActionFire, "fire"},
        {InputMapEnum.ActionJump, "jump"},
    };


    private const float BaseSugar = 100.0f;
	private float CurrentBaseSugar = BaseSugar; // Base amount of sugar/health
	private float CurrentSugar = BaseSugar;
	
	[Export]
	private float SugarHalfLife = 2; // Time in seconds at which oversugar amount will be cut in half
	private const float MinOverSugar = 1.0f; // Oversugar at which sugar will be set to the default value 
	[Export]
	private float SugarushThreshold = 1.5f * BaseSugar; // Threshold at which sugarush will be activated
	[Export]
	private float SugarushCalmdownThreshold = 1.25f * BaseSugar; // Threshold at which sugar will be deactivated
	[Export]
	private int SugarushNumJumps = 2; // Number of jumps allowed midair during sugarush 
	private int BaseNumJumps = 1; // Number of jumps without sugarush
	private int CurrentBaseJumps = 1; // Number of jumps currently allowed
	private int JumpsRemaining = 1;
	private bool Sugarush = false;

    private Camera3D PlayerCamera;
    private float CameraMaxRotation = 1.0f;
	private const float CameraSensitivity = 0.0025f;

	[Export]
	private float MovementAccel = 100.0f;

	[Export]
	private float AirAccel = 50.0f;

	[Export]
	private float SprintMultiplier = 2f;

	private const float BaseSpeed = 5.0f; // Base speed of the character
	private float CurrentBaseSpeed = BaseSpeed; // Speed with modifiers applied (i.e, sprinting) 
	private float TargetSpeed = BaseSpeed; // Target velocity to reach with oversugar included

	[Export]
	private float SugarSpeedRatio = BaseSpeed/100; // How much speed oversugar grants you

	private bool IsSprinting = false;
	private bool SprintReferenceState = false; // Holds the reference state of sprinting. Only used in the SprintHandler function!

	const float JumpVelocity = 8f;

    private (Vector3 Vel, bool IsGrounded) _RefFallState;
    private float FallDamageStart = 15.0f;
    private float FatalFallSpeed = 25.0f;

    /* Friction attributes */
    float Friction = 0.70f;
	float AirFriction = 0.25f;
	float PlayerMass = 50f;
	float MinVel = 0.3f;

	private TestWeapon WeaponNode;
	private Area3D PickupSphereNode;
}
