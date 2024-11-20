using Godot;
using System;

public partial class TestCharacter
{
    private float ControllerSensitivity = 7.5f;

	/// <summary>
    /// 	Get the desired direction of movement relative to the global basis.
    /// </summary>
    /// <returns>The direction of movement, where X is side and Z is forward</returns>
    private Vector3 GetMoveDirection ()
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

			// if (CurrentSugar == BaseSugar)
			// {
			// }
			TargetSpeed = CurrentBaseSpeed; 
			SprintReferenceState = !SprintReferenceState;
		}
	}

	private void ApplyFallDamage(float speed)
	{
        if (speed >= FallDamageStart)
		{
            Hurt(((speed - FallDamageStart) / FatalFallSpeed) * MathF.Max(CurrentBaseSugar, CurrentSugar));
		}
    }

    private Vector3 FallHandler(Vector3 velocity, double delta)
	{
		if (IsOnGround())
		{
			if (!_RefFallState.IsGrounded)
			{
				ApplyFallDamage((_RefFallState.Vel * GetGravity().Normalized()).Y);
			}
			_RefFallState.IsGrounded = true;
		}
		else
		{
            velocity += GetGravity() * (float)delta; // Apply gravity acceleration
            _RefFallState.IsGrounded = false;
        }
		
		_RefFallState.Vel = LinearVelocity;
        return velocity;
    }

	/// <Summary>
	/// 	Handles jump input
	/// </Summary>
	private void JumpHandler()
	{
        /*Reset jumps if on floor and apply correct amount of jumps when midair*/
        if(!IsOnGround())
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
            Vector3 jumpDir = GlobalBasis.Y.Normalized();
            LinearVelocity -= LinearVelocity.Project(jumpDir);
            LinearVelocity += jumpDir * JumpVelocity;
            JumpsRemaining--;
		}
	}


	/// <Summary>
	/// 	Apply friction when on ground/air until full stop
	/// </Summary>
	private void ApplyFriction(ref Vector3 force, double delta)
	{
		float CurrentFriction;
        Vector3 simForce = force;

        if (IsOnGround())
		{
			/*
				Friction is given by F = N * μ
				N - Normal force
				μ - Friction coefficient
			*/

			CurrentFriction = Friction;
	        Vector3 normalForce = GetGravity() * Mass;
    	    simForce -= CurrentFriction * normalForce * LinearVelocity.Normalized();
		}

		else
		{
			/*
				https://www.youtube.com/watch?v=1T4hZjBrscQ
				Drag is given by F = 0.5 * ρ * C * A * v^2
				ρ - Air density
				C - Drag coeficcient
				A - Cross-sectional area of the object
				v - Object velocity
			*/
			simForce -= LinearVelocity.Normalized() * (0.5f * 1.3f * Hitbox.GetLongestAxisSize() * LinearVelocity.LengthSquared());
		}
        Vector3 simSpeed = SimulateSpeed(simForce, delta) + LinearVelocity;

        if (simSpeed.LengthSquared() <= MinVel * MinVel)
		{
            LinearVelocity = Vector3.Zero;
            return;
        }

        force = simForce;
    }



	/// <Summary>
	/// 	Applly accelleration when moving, until full speed is achieved
	/// </Summary>
	private void ApplyMovementAccel(ref Vector3 force, double delta)
	{
        Vector3 moveDirection = GetMoveDirection();

        if (moveDirection == Vector3.Zero)
		{
            return;
        }

        void Accelerate(ref Vector3 force, Vector3 baseDir, float directionScale, double delta)
        {
            Vector3 dirForce = force.Project(baseDir); // Current force in the target direction
            Vector3 targetForce = directionScale * baseDir * MovementAccel * Mass; // Force to be applied to player
            
			Vector3 dirSpeed = LinearVelocity.Project(baseDir); // Speed in the target direction

            Vector3 finalSpeed = SimulateSpeed(dirForce + targetForce, delta) + dirSpeed; // Speed after iteration

            if (finalSpeed.LengthSquared() <= TargetSpeed * TargetSpeed)
            {
                force += targetForce;
            } // Apply force if max speed won't be exceeded

            else
			{
				/*
					The target force is given by:
					
					F = m * a
					a = F / m

					v = a * t
					v = F / m * t

					F = v * m / t
				*/
                Vector3 targetDirSpeed = TargetSpeed * directionScale * baseDir; // Target speed in the target direction
                targetForce = (targetDirSpeed - dirSpeed) * Mass / (float)delta; // See derivation
                force += targetForce - dirForce; // Apply remainder to achieve target speed next iteration 
            } // If it is, calculate a smaller force that will close the gap to the target speed.
        }

        if (moveDirection.Z != 0)
		{
            Accelerate(ref force, -GlobalBasis.Z.Normalized(), moveDirection.Z, delta);
		} // Apply force on Z (forward & backward)
        
		if (moveDirection.X != 0)
        {
            Accelerate(ref force, GlobalBasis.X.Normalized(), moveDirection.X, delta);
        } // Apply force on X (left & right)
	}

	private Vector3 SimulateSpeed(Vector3 force, double delta)
	{
		/*
		Derivation (for small time steps):
		F = m * a
		a = F / m

		v = a * t
		v = F / m * t
		*/
        return force / Mass * (float)delta;
    }

	/// <Summary>
	/// 	Handles all movement-related input events
	/// </Summary>
	private void MovementHandler(double delta)
	{
        Vector3 force = Vector3.Zero;

        JumpHandler();
        FallHandler(force, delta);
        SprintHandler();
        ApplyFriction(ref force, delta);
        ApplyMovementAccel(ref force, delta);
        ApplyCentralForce(force);
    }

}
