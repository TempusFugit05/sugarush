using Godot;
using System;

public partial class Character : CharacterBody3D, ICreature
{
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
}