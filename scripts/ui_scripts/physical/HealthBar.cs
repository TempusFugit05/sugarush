using Godot;
public partial class HealthBar : Sprite3D
{
	Label HealthText;
    ProgressBar HealthProgressBar;
    const string UiElementsPath = "ViewPort/Control/";

    public override void _Ready()
	{
 		HealthText = GetNode<Label>(UiElementsPath + "Label");
        HealthProgressBar = GetNode<ProgressBar>(UiElementsPath + "ProgressBar");
    }

	/// <summary>
    /// 	Update the healthbar to display current hp
    /// </summary>
    /// <param name="CurrentHealth">Current health of the target</param>
    /// <param name="MaxHealth">Maximum health of target</param>
	public void SetHealthPoint(float CurrentHealth, float MaxHealth)
	{
		HealthProgressBar.Value = (CurrentHealth / MaxHealth) * 100;
		HealthText.Text = Mathf.RoundToInt(CurrentHealth).ToString();	
	}
}
