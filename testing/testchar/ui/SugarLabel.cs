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
			Text = Convert.ToString(CharNode.GetHealth()) + "%";
            IsInitialized = true;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Run()
	{
        if (IsInitialized)
        {
            if (CharNode.GetHealth() != CharSugar)
            {
                CharSugar = CharNode.GetHealth();
                Text = Convert.ToString($"{CharNode.GetHealth():0.0}") + "%";
            }
        }
    }
}
