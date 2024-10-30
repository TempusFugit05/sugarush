using Godot;
using System;

public partial class Character : CharacterBody3D
{
	/// <<returns>>
	///	Consume sugar in pickup
	/// </<returns>>
	void ConsumeSugar(float Amount)
	{
		CurrentSugar += Amount;
	}

	/// <<returns>>
	///	Current amount of sugar
	/// </<returns>>
	public float GetSugar()
	{
		return CurrentSugar;
	}

	/// <<returns>>
	///	Sugarush state
	/// </<returns>>
	public bool GetSugarush()
	{
		return Sugarush;
	}

	/// <Summary>
	///	Increase maximum speed based on the mount of oversugar
	/// </Summary>
	void ApplySugarSpeed()
	{
		TargetSpeed = CurrentBaseSpeed + (SugarSpeedRatio * (CurrentSugar - BaseSugar));
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
		CurrentBaseJumps = SugarushNumJumps;
		
		if(IsOnFloor())
		{
			JumpsRemaining = CurrentBaseJumps;
		}
		else
		{
			JumpsRemaining = CurrentBaseJumps - 1;
		}

		Sugarush = true;
	}

	/// <Summary>
	/// Handles entering sugarush state
	/// </Summary>
	void ExitSugarush()
	{
		CurrentBaseJumps = BaseNumJumps;
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
    void SugarHandler(double delta)
	{
        SugarushStateHandler();

		if (CurrentSugar > BaseSugar)
		{
			ApplySugarHalfLife(delta);
			ApplySugarSpeed();
		}		
    }
}
