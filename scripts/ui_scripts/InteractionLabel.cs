using Godot;
using System;

public partial class InteractionLabel : Label
{
    private Character CharNode;
	public void Init(Character character)
	{
        CharNode = character;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (CharNode.InteractingWith is not null)
		{
            Text = "WAAAAAAAAAAA!!!!";
            VisibleCharacters = -1;
        }
		else
		{
            VisibleCharacters = 0;
        }
    }
}
