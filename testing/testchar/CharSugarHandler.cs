using Godot;
using System;

public partial class Character
{
    public class SugarHandler
    {
        Character P;
        public void Init(Character character)
        {
            P = character;
        }

        public const float BaseSugar = 100.0f;
        public float CurrentSugar {get; private set;} = BaseSugar;        
        private float SugarHalfLife = 2; // Time in seconds at which oversugar amount will be cut in half
        private const float MinOverSugar = 1.0f; // Oversugar at which sugar will be set to the default value 
        private float SugarushThreshold = 1.5f * BaseSugar; // Threshold at which sugarush will be activated
        private float SugarushCalmdownThreshold = 1.25f * BaseSugar; // Threshold at which sugar will be deactivated
        public bool Sugarush {get; private set;} = false;
    	private float SugarSpeedRatio = 0.025f; // How much speed oversugar grants you

        /// <<returns>>
        ///	Consume sugar in pickup
        /// </<returns>>
        public void ConsumeSugar(float Amount)
        {
            CurrentSugar += Amount;
        }

        public float GetSugar()
        {
            return CurrentSugar;
        }

        public void SetSugar(float sugar)
        {
            if (sugar >= 0)
            {
                CurrentSugar = sugar;
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
            if (CurrentSugar > BaseSugar)
            {
                P.movementHandler.IncreaseSpeed(SugarSpeedRatio * (CurrentSugar - BaseSugar));
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
            float SugarOverhead = CurrentSugar - BaseSugar; // How much sugar is above baseline
            CurrentSugar -= SugarOverhead / (2 * SugarHalfLife / (float)delta); // Apply sugar halflife

            if (CurrentSugar - BaseSugar <= MinOverSugar)
            {
                CurrentSugar = BaseSugar;
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
            if (!Sugarush && CurrentSugar >= SugarushThreshold)
            {
                EnterSugarush();
            }

            if (Sugarush && CurrentSugar <= SugarushCalmdownThreshold)
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

            if (CurrentSugar != BaseSugar)
            {
                if (CurrentSugar > BaseSugar)
                {
                    ApplySugarHalfLife(delta);
                }

                ApplySugarSpeed();
            }		
        }    
    }
}
