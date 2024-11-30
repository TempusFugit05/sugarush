using Godot;
using System;

public partial class Character
{
    public class SugarHandler
    {
        Character P;
        CreatureState PState;

        public void Init(Character character, CreatureState state)
        {
            P = character;
            PState = state;
            SugarushThreshold = 1.5f * PState.MaxHealth;
            SugarushCalmdownThreshold = 1.25f * PState.MaxHealth;
        }

        private float SugarHalfLife = 2; // Time in seconds at which oversugar amount will be cut in half
        private const float MinOverSugar = 1.0f; // Oversugar at which sugar will be set to the default value 
        private float SugarushThreshold; // Threshold at which sugarush will be activated
        private float SugarushCalmdownThreshold; // Threshold at which sugar will be deactivated
        public bool Sugarush {get; private set;} = false;
    	private float SugarSpeedRatio = 0.025f; // How much speed oversugar grants you

        /// <<returns>>
        ///	Consume sugar in pickup
        /// </<returns>>
        public void ConsumeSugar(float Amount)
        {
            PState.Health += Amount;
        }

        public float GetSugar()
        {
            return PState.Health;
        }

        public void SetSugar(float sugar)
        {
            if (sugar >= 0)
            {
                PState.Health = sugar;
            }
        }
        public bool GetSugarush()
        {
            return Sugarush;
        }

        /// <Summary>
        ///	Increase maximum speed based on the mount of oversugar
        /// </Summary>
        void ApplySugarSpeed()
        {
            if (PState.Health > PState.MaxHealth)
            {
                P.movementHandler.IncreaseSpeed(SugarSpeedRatio * (PState.Health - PState.MaxHealth));
            }
            else
            {
                P.movementHandler.ResetSpeed();
            }
        }

        /// <Summary>
        ///	Apply half life to oversugar, after <c>SugarHalfLife</c>, oversugar will be cut in half
        /// </Summary>
        void ApplySugarHalfLife(double delta)
        {
            float SugarOverhead = PState.Health - PState.MaxHealth; // How much sugar is above baseline
            PState.Health -= SugarOverhead / (2 * SugarHalfLife / (float)delta); // Apply sugar halflife

            if (PState.Health - PState.MaxHealth <= MinOverSugar)
            {
                PState.Health = PState.MaxHealth;
            }
        }

        /// <Summary>
        /// Handles entering sugarush state, adding extra jumps
        /// </Summary>
        void EnterSugarush()
        {
            P.movementHandler.StartSugarush();
            Sugarush = true;
        }

        /// <Summary>
        /// Handles entering sugarush state
        /// </Summary>
        void ExitSugarush()
        {
            P.movementHandler.EndSugarush();
            Sugarush = false;
        }

        private void SugarushStateHandler()
        {
            if (!Sugarush && PState.Health >= SugarushThreshold)
            {
                EnterSugarush();
            }

            if (Sugarush && PState.Health <= SugarushCalmdownThreshold)
            {
                ExitSugarush();
            }
        }
        
        /// <Summary>
        /// Handles everything related to sugar, including speed, sugar halflife and sugarush states
        /// </Summary>
        public void Run(double delta)
        {
            SugarushStateHandler();

            if (PState.Health != PState.MaxHealth)
            {
                if (PState.Health > PState.MaxHealth)
                {
                    ApplySugarHalfLife(delta);
                }

                ApplySugarSpeed();
            }		
        }    
    }
}
