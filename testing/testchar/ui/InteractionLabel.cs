using Godot;
using System;

public partial class InteractionLabel : Label
{
    private Character CharNode;
	public void Init(Character character)
	{
        CharNode = character;
    }
    public void Run()
    {
        if (CharNode.GetInteractingNode() is not null)
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
