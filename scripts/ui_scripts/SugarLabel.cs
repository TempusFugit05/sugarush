using Godot;
using System;

public partial class SugarLabel : Label
{
	Character CharNode;
	private float CharSugar = 0.0f;
	String HealthText = "";

	public override void _Ready()
	{
		CharNode = GetNode<Character>("/root/Main/Character");
		CharSugar = CharNode.GetSugar();
		Text = Convert.ToString(CharNode.GetSugar()) + "%";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(CharNode.GetSugar() != CharSugar)
		{
			CharSugar = CharNode.GetSugar();
			Text = Convert.ToString($"{CharNode.GetSugar():0.0}") + "%";
		}
	}
}
