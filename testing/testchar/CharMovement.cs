using Godot;
using System;
using System.Collections.Generic;

public partial class Character
{
	public class MovementHandler
	{
		public void Init(Character character)
		{
			P = character;
		}

		Character P;

		private bool IsSprinting = false; // Holds the reference state of sprinting. Only used in the SprintHandler function!

		const float JumpVelocity = 8f;

		private (Vector3 Vel, bool IsGrounded) _RefFallState;
		private float FallDamageStart = 10.0f;
		private float FatalFallSpeed = 20.0f;

		/* Friction attributes */
		public float Friction = 1.0f;
		public float AirFriction = 0.25f;

		public float MinVel = 0.1f;
		private float MovementAccel = 100.0f;

		private float SprintMultiplier = 2f;
		private const float DefaultSpeed = 5.0f; // Base speed of the character
		private float BaseSpeed = DefaultSpeed; // Speed with modifiers applied (i.e, sprinting) 
		private float TargetSpeed = DefaultSpeed; // Target velocity to reach with oversugar included
		private int BaseNumJumps = 1; // Number of jumps without sugarush
		private int CurrentBaseJumps = 1; // Number of jumps currently allowed
		private int JumpsRemaining = 1;
        private int SugarushNumJumps = 2; // Number of jumps allowed midair during sugarush 

		public void StartSugarush()
		{
            CurrentBaseJumps = SugarushNumJumps;
            
            if(P.IsOnGround())
            {
                JumpsRemaining = CurrentBaseJumps;
            }
            else
            {
                JumpsRemaining = CurrentBaseJumps - 1;
            }
		}

		public void EndSugarush()
		{
			if (JumpsRemaining < CurrentBaseJumps)
			{
				JumpsRemaining = BaseNumJumps;
			}
			CurrentBaseJumps = BaseNumJumps;
		}

		public void IncreaseSpeed(float amount)
		{
			BaseSpeed = DefaultSpeed + amount;
		}

		public void ResetSpeed()
		{
			BaseSpeed = DefaultSpeed;
		}

		/// <Summary>
		/// 	Increase accelleration on sprint input
		/// </Summary>
		private void SprintHandler()
		{
			if(Input.IsActionPressed("sprint") != IsSprinting)
			{
				IsSprinting = !IsSprinting;
			}

			if(IsSprinting)
			{
				TargetSpeed = BaseSpeed * SprintMultiplier;
			}

			else
			{
				TargetSpeed = BaseSpeed;
			}
		}

		private void ApplyFallDamage(float speed)
		{
			if (speed >= FallDamageStart)
			{
				P.Hurt(((speed - FallDamageStart) / FatalFallSpeed) * Mathf.Max(P.GetHealth(), P.GetMaxHealth()));
			}
		}

		private void FallHandler()
		{
			if (P.IsOnGround())
			{
				if (!_RefFallState.IsGrounded)
				{
					ApplyFallDamage((_RefFallState.Vel * P.GetGravity().Normalized()).Length());
				}
				_RefFallState.IsGrounded = true;
			}
			else
			{
				_RefFallState.IsGrounded = false;
			}
			
			_RefFallState.Vel = P.LinearVelocity;
		}

		/// <Summary>
		/// 	Handles jump input
		/// </Summary>
		private void JumpHandler()
		{
			/*Reset jumps if on floor and apply correct amount of jumps when midair*/
			if(!P.IsOnGround())
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

			if (Input.IsActionJustPressed("jump") && JumpsRemaining > 0)
			{
				Vector3 jumpDir = P.GlobalBasis.Y.Normalized();
				Vector3 upVelocity = P.LinearVelocity.Project(jumpDir);
				Vector3 jumpVec = jumpDir * JumpVelocity;

				if (upVelocity.Dot(jumpDir) < 0)
				{
					P.ApplyCentralImpulse((jumpVec - upVelocity) * P.Mass);
				}

				else
				{
					P.ApplyCentralImpulse(jumpVec * P.Mass);
				}

				JumpsRemaining--;
			}
		}


		/// <Summary>
		/// 	Apply friction when on ground/air until full stop
		/// </Summary>
		private void ApplyFriction(ref Vector3 force, double delta)
		{
			if (P.LinearVelocity == Vector3.Zero)
			{
				return;
			}

			Vector3 simForce = force;
			
			if (P.IsOnGround())
			{
				/*
					Friction is given by F = N * μ
					N - Normal force
					μ - Friction coefficient
				*/
				Vector3 normalForce = P.GetGravity() * P.Mass;
				simForce -= normalForce.Length() * Friction * P.LinearVelocity.Normalized();
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
				simForce -= P.LinearVelocity.Normalized() * (0.5f * 0.6f * P.Hitbox.GetLongestAxisSize() * P.LinearVelocity.LengthSquared());
			}
			
			Vector3 simSpeed = SimulateSpeed(simForce, delta) + P.LinearVelocity;

			if (simSpeed.LengthSquared() <= MinVel * MinVel)
			{
				P.LinearVelocity = Vector3.Zero;
				return;
			}

			force += simForce;
		}



		/// <Summary>
		/// 	Applly accelleration when moving, until full speed is achieved
		/// </Summary>
		private void ApplyMovementAccel(ref Vector3 force, double delta)
		{
			Vector3 moveDirection = P.inputHandler.GetMoveDirection();

			if (moveDirection == Vector3.Zero)
			{
				return;
			}

			void Accelerate(ref Vector3 force, Vector3 baseDir, float directionScale, double delta)
			{
				Vector3 dirForce = force.Project(baseDir); // Current force in the target direction
				Vector3 targetForce = directionScale * baseDir * MovementAccel * P.Mass; // Force to be applied to player
				
				Vector3 dirSpeed = P.LinearVelocity.Project(baseDir); // Speed in the target direction

				Vector3 finalSpeed = SimulateSpeed(dirForce + targetForce, delta) + dirSpeed; // Speed after iteration

				if (finalSpeed.LengthSquared() <= TargetSpeed * TargetSpeed)
				{
					force += targetForce;
				} // Apply force if max speed won't be exceeded

				else
				{
					Vector3 targetDirSpeed = TargetSpeed * directionScale * baseDir; // Target speed in the target direction
					targetForce = ForceToGetSpeed(targetDirSpeed - dirSpeed, delta);
					force += targetForce - dirForce; // Apply remainder to achieve target speed next iteration 
				} // If it is, calculate a smaller force that will close the gap to the target speed.
			}

			if (moveDirection.Z != 0)
			{
				Accelerate(ref force, -P.GlobalBasis.Z.Normalized(), moveDirection.Z, delta);
			} // Apply force on Z (forward & backward)
			
			if (moveDirection.X != 0)
			{
				Accelerate(ref force, P.GlobalBasis.X.Normalized(), moveDirection.X, delta);
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
			return force / P.Mass * (float)delta;
		}

		private Vector3 ForceToGetSpeed(Vector3 targetSpeed, double delta)
		{
			/*
				The target force is given by:
				
				F = m * a
				a = F / m

				v = a * t
				v = F / m * t

				F = v * m / t
			*/
			return targetSpeed * P.Mass / (float)delta;
		}

		/// <Summary>
		/// 	Handles all movement-related input events
		/// </Summary>
		public void Run(double delta)
		{
			Vector3 force = Vector3.Zero;

			JumpHandler();
			FallHandler();
			SprintHandler();
			ApplyFriction(ref force, delta);
			ApplyMovementAccel(ref force, delta);
			P.ApplyCentralForce(force);
		}

	}
}
