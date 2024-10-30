using Godot;
using System;

public partial class SugarushStatus : Label
{
	private Character CharNode;
	private bool IsInitialized = false;

	public void Init(Character PlayerNode)
	{
        CharNode = PlayerNode;
		if(CharNode is not null)
		{
            IsInitialized = true;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (IsInitialized)
		{
			if(CharNode.GetSugarush())
			{
				VisibleCharacters = -1;
			}
			else
			{
				VisibleCharacters = 0;
			}
		}
	}
}
