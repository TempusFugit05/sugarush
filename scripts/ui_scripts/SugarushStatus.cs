using Godot;
using System;

public partial class SugarushStatus : Label
{
	Character CharNode;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CharNode = GetNode<Character>("/root/Main/Character");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
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
