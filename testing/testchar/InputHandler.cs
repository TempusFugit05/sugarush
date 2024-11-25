using System.Collections.Generic;
using Godot;

public partial class Character
{
    public class InputHandler
    {
        Vector2 MouseMovement = Vector2.Zero;
        private const float MouseSensitivity = 10f;
        private float ControllerSensitivity = 7.5f;
        private float CameraRotationDeg = 80.0f;
        private Character P;

        public void Init(Character character)
        {
            P = character;
        }

		public enum InputMapEnum
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

        public void UpdateMouse(Vector2 movement)
        {
            MouseMovement += movement;
        }

        public string GetInput(InputMapEnum inputName)
        {
            return InputMap[inputName];
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

		/// <summary>
		/// 	Get the desired direction of movement relative to the global basis.
		/// </summary>
		/// <returns>The direction of movement, where X is side and Z is forward</returns>
		public Vector3 GetMoveDirection ()
		{
			Vector2 input2D;

			// Get the desired direction of movement input as a vector
			if (Input.GetJoyAxis(0, JoyAxis.RightX) == 0 && Input.GetJoyAxis(0, JoyAxis.RightY) == 0)
			{
				input2D = Input.GetVector(InputMap[InputMapEnum.KeyGoLeft],
												InputMap[InputMapEnum.KeyGoRight],
												InputMap[InputMapEnum.KeyGoBack],
												InputMap[InputMapEnum.KeyGoForward]);
			}

			else
			{
				input2D.X = Input.GetActionStrength(InputMap[InputMapEnum.StickGoRight]) - Input.GetActionStrength(InputMap[InputMapEnum.StickGoLeft]);
				input2D.Y = Input.GetActionStrength(InputMap[InputMapEnum.StickGoBack]) - Input.GetActionStrength(InputMap[InputMapEnum.StickGoForward]);
			}

			Vector3 input3D = Vector3.Zero;
			input3D.X = input2D.X;
			input3D.Z = input2D.Y;

			return input3D.Normalized();
		}

        public void Run(double delta)
        {
            Vector2 lookDirection = GetLookDirection();
            float toRotate;
            Vector3 finalRotation;

            if (lookDirection.Y != 0)
            {
                finalRotation = P.Camera.RotationDegrees;
                toRotate = -lookDirection.Y * MouseSensitivity * (float)delta;
                finalRotation.X += toRotate;
                finalRotation.X = Mathf.Clamp(finalRotation.X, -CameraRotationDeg, CameraRotationDeg); // Limit camera rotation
                P.Camera.RotationDegrees = finalRotation;
            } // Rotate camera up and down

            if (lookDirection.X != 0)
            {
                finalRotation = P.RotationDegrees;
                toRotate = -lookDirection.X * MouseSensitivity * (float)delta;
                finalRotation.Y += toRotate;
                P.RotationDegrees = finalRotation;
            } // Rotate body left and right
        }
    }
}