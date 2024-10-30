using Godot;
using System;

public partial class SugarLabel : Label
{
	private Character CharNode;
    private bool IsInitialized = false;
    private float CharSugar = 0.0f;
	private string HealthText = "";

	public void Init(Character PlayerNode)
	{
        CharNode = PlayerNode;
		if(CharNode is not null)
		{
			CharSugar = CharNode.GetSugar();
			Text = Convert.ToString(CharNode.GetSugar()) + "%";
            IsInitialized = true;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        if (IsInitialized)
        {
            if (CharNode.GetSugar() != CharSugar)
            {
                CharSugar = CharNode.GetSugar();
                Text = Convert.ToString($"{CharNode.GetSugar():0.0}") + "%";
            }
        }
    }
}
