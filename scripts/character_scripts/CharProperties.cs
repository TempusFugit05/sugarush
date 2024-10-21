using Godot;
using System;

public partial class Character : CharacterBody3D
{
	private const string KeyMoveLeft = "move_left";
	private const string KeyMoveRight = "move_right";
	private const string KeyMoveForward = "move_forward";
	private const string KeyMoveBackward = "move_backward";
	private const string KeySprint = "sprint";
	private const string KeyFire = "fire";
	const string KeyJump = "jump";

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

	private float CameraMaxRotation = 0.65f;
	private const float CameraSensitivity = 0.002f;

	[Export]
	private float MovementAccel = 100.0f;

	[Export]
	private float AirAccel = 50.0f;

	[Export]
	private float SprintMultiplier = 2f;

	private const float BaseSpeed = 7.0f; // Base speed of the character
	private float CurrentBaseSpeed = BaseSpeed; // Speed with modifiers applied (i.e, sprinting) 
	private float TargetSpeed = BaseSpeed; // Target velocity to reach with oversugar included

	[Export]
	private float SugarSpeedRatio = BaseSpeed/100; // How much speed oversugar grants you

	private bool IsSprinting = false;
	private bool SprintReferenceState = false; // Holds the reference state of sprinting. Only used in the SprintHandler function!

	const float JumpVelocity = 6f;


	/* Friction attributes */
	float Friction = 0.70f;
	float AirFriction = 0.25f;
	float PlayerMass = 50f;
	float MinVel = 0.3f;

	private Weapon WeaponNode;
	private Area3D PickupSphereNode;
}
