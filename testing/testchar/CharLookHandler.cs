using Godot;
using System;

public partial class TestCharacter
{
    Vector2 MouseMovement;

    private void LookHandler(double delta)
	{
        Vector2 lookDirection = GetLookDirection();
        float toRotate;
        Vector3 finalRotation;

		if (lookDirection.Y != 0)
		{
            finalRotation = PlayerCamera.RotationDegrees;
            toRotate = -lookDirection.Y * MouseSensitivity * (float)delta;
            finalRotation.X += toRotate;
            finalRotation.X = Mathf.Clamp(finalRotation.X, -CameraRotationDeg, CameraRotationDeg); // Limit camera rotation
            PlayerCamera.RotationDegrees = finalRotation;
        } // Rotate camera up and down

		if (lookDirection.X != 0)
		{
            finalRotation = RotationDegrees;
            toRotate = -lookDirection.X * MouseSensitivity * (float)delta;
            finalRotation.Y += toRotate;
	        RotationDegrees = finalRotation;
        } // Rotate body left and right
    }

	private Vector2 GetLookDirection()
	{
        Vector2 Direction;
        if (MouseMovement == Vector2.Zero)
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

	// /// <Summary>
	// /// 	Rotate Player body on Y axis based on mouse movement
	// /// </Summary>
	// private void RotateBody(Vector2 MouseMovement)
	// {
	// 	if (MouseMovement.X != 0)
	// 	{
	// 		RotateY(-MouseMovement.X * CameraSensitivity);
	// 	}
	// }

	// /// <Summary>
	// /// 	Rotate the camera on Z axis based on mouse movement, capped to <c>+-CameraMaxRotation</c>
	// /// </Summary>
	// private void RotateHead(Vector2 MouseMovement)
	// {
	// 	if ((MouseMovement.Y > 0 && PlayerCamera.Transform.Basis.GetEuler().X > -CameraMaxRotation) ||
	// 	(MouseMovement.Y < 0 && PlayerCamera.Transform.Basis.GetEuler().X < CameraMaxRotation))
	// 	{
	// 		PlayerCamera.RotateX(-MouseMovement.Y * CameraSensitivity);
	// 	}
	// }
}