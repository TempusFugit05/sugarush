using Godot;
using System;

public partial class Character : CharacterBody3D
{
    private Vector2 MouseMovement;
    private float ControllerSensitivity = 7.5f;

    /// <Summary>
    /// 	Get the desired direction of movement based on user input
    /// </Summary>
    private Vector3 GetMoveDirection ()
	{
        Vector2 InputDirection;

        // Get the desired direction of movement input as a vector
        if (Input.GetJoyAxis(0, JoyAxis.RightX) == 0 && Input.GetJoyAxis(0, JoyAxis.RightY) == 0)
		{
			InputDirection = Input.GetVector(InputMap[InputMapEnum.KeyGoLeft],
											 InputMap[InputMapEnum.KeyGoRight],
											 InputMap[InputMapEnum.KeyGoForward],
											 InputMap[InputMapEnum.KeyGoBack]);
		}

		else
		{
        	InputDirection = new(Input.GetActionStrength(InputMap[InputMapEnum.StickGoRight])
								 - Input.GetActionStrength(InputMap[InputMapEnum.StickGoLeft]),
								 Input.GetActionStrength(InputMap[InputMapEnum.StickGoBack])
								 - Input.GetActionStrength(InputMap[InputMapEnum.StickGoForward]));
		}

		return (Transform.Basis * new Vector3(InputDirection.X, 0, InputDirection.Y)).Normalized();
	}

	private Vector2 GetLookDirection()
	{
        Vector2 Direction;
        if (MouseMovement.Equals(Vector2.Zero))
		{
        	Direction = new(Input.GetActionStrength(InputMap[InputMapEnum.StickLookRight])
													- Input.GetActionStrength(InputMap[InputMapEnum.StickLookLeft]),
													Input.GetActionStrength(InputMap[InputMapEnum.StickLookDown])
													- Input.GetActionStrength(InputMap[InputMapEnum.StickLookUp]));
            Direction *= ControllerSensitivity;
        }
		
		else
		{
            Direction = MouseMovement;
            MouseMovement = Vector2.Zero;
        }

        return Direction;
    }

	private void LookHandler()
	{
        Vector2 LookDirection = GetLookDirection();
        if (LookDirection.Y != 0)
		{
			RotateHead(LookDirection);
		}
		if (LookDirection.X != 0)
		{
			RotateBody(LookDirection);
		}
	}

	/// <Summary>
	/// 	Increase accelleration on sprint input
	/// </Summary>
	private void SprintHandler()
	{
		if(Input.IsActionPressed(InputMap[InputMapEnum.ActionSprint]) != SprintReferenceState)
		{
			if(!SprintReferenceState)
			{
				CurrentBaseSpeed = BaseSpeed * SprintMultiplier;
			}

			else
			{
				CurrentBaseSpeed = BaseSpeed;
			}

			if (CurrentSugar == BaseSugar)
			{
				TargetSpeed = CurrentBaseSpeed; 
			}

			SprintReferenceState = !SprintReferenceState;
		}
	}

	/// <Summary>
	/// 	Handles jump input
	/// </Summary>
	private float JumpHandler(float VelY)
	{
		/*Reset jumps if on floor and apply correct amount of jumps when midair*/
		if(!IsOnFloor())
		{
			if(JumpsRemaining == CurrentBaseJumps)
			{
				JumpsRemaining--;
			}
		}
		else
		{
			JumpsRemaining = CurrentBaseJumps;
		}

		if (Input.IsActionJustPressed(InputMap[InputMapEnum.ActionJump]) && JumpsRemaining > 0)
		{
			VelY = JumpVelocity;
			JumpsRemaining--;
		}

		return VelY;
	}


	/// <Summary>
	/// 	Apply friction when on ground/air until full stop
	/// </Summary>
	private float ApplyFriction(float CurrentVel, float VelDirection, double delta)
	{
		float CurrentFriction;
		if (IsOnFloor())
		{
			CurrentFriction = Friction;
		}
		else
		{
			CurrentFriction = AirFriction;
		}

		if (Mathf.Abs(CurrentVel) >= MinVel)
		{
			float Vel = VelDirection * CurrentFriction * PlayerMass * (float)delta; // Speed to detract from current speed
			if ((CurrentVel - Vel < 0 && CurrentVel - Vel <= -MinVel)  || (CurrentVel - Vel > 0 && CurrentVel - Vel >= MinVel))
			{

				CurrentVel -= Vel;
			}
			else
			{
				CurrentVel = 0;
			}
		}
		else
		{
			CurrentVel = 0;
		}

		return CurrentVel;
	}


	/// <Summary>
	/// 	Applly accelleration when moving, until full speed is achieved
	/// </Summary>
	private float ApplyMovementAccel (float CurrentVel, float Direction, double delta)
	{
		float Vel = Direction * MovementAccel * (float)delta; // Speed to add
		float MaxSpeed = Direction * TargetSpeed;
		if (Mathf.Abs(Vel + CurrentVel) <= Mathf.Abs(MaxSpeed))
		{
			return CurrentVel + Vel;
		}
		else
		{
			return MaxSpeed;
		}

	}

	/// <Summary>
	/// 	Rotate the camera on Z axis based on mouse movement, capped to <c>+-CameraMaxRotation</c>
	/// </Summary>
	private void RotateHead(Vector2 MouseMovement)
	{
		if ((MouseMovement.Y > 0 && PlayerCamera.Transform.Basis.GetEuler().X > -CameraMaxRotation) ||
		(MouseMovement.Y < 0 && PlayerCamera.Transform.Basis.GetEuler().X < CameraMaxRotation))
		{
			PlayerCamera.RotateX(-MouseMovement.Y * CameraSensitivity);
		}
	}

	/// <Summary>
	/// 	Rotate Player body on Y axis based on mouse movement
	/// </Summary>
	private void RotateBody(Vector2 MouseMovement)
	{
		if (MouseMovement.X != 0)
		{
			RotateY(-MouseMovement.X * CameraSensitivity);
		}
	}

	public override void _Input(InputEvent CurrentInput)
	{
        if (CurrentInput is InputEventMouseMotion MouseMotion)
		{
			MouseMovement = MouseMotion.Relative;
		}
	}
	/// <Summary>
	/// 	Handles all movement-related input events
	/// </Summary>
	private void MovementHandler(double delta)
	{
		Vector3 CurrentVel = Velocity;

		if (!IsOnFloor())
		{
			CurrentVel += GetGravity() * (float)delta; // Apply gravity acceleration
		}
		
		CurrentVel.Y = JumpHandler(CurrentVel.Y);

		SprintHandler();
        LookHandler();

        Vector3 MoveDirection = GetMoveDirection();


		if (MoveDirection.X != 0)
		{
			CurrentVel.X = ApplyMovementAccel(CurrentVel.X, MoveDirection.X, delta);		
		}
		else
		{
			if (CurrentVel.X != 0)
			{
				CurrentVel.X = ApplyFriction(CurrentVel.X, new Vector3(CurrentVel.X, 0, CurrentVel.Z).Normalized().X, delta);
			}
		}
		
		if (MoveDirection.Z != 0)
		{
			CurrentVel.Z = ApplyMovementAccel(CurrentVel.Z, MoveDirection.Z, delta);		
		}
		else
		{
			if (CurrentVel.Z != 0)
			{
				CurrentVel.Z = ApplyFriction(CurrentVel.Z, new Vector3(CurrentVel.X, 0, CurrentVel.Z).Normalized().Z, delta);
			}
		}

		Velocity = CurrentVel;
		MoveAndSlide();
	}

}
