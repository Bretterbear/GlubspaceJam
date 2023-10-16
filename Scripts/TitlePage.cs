using Godot;
using System;

public partial class TitlePage : Control
{
	// -------- Reference Variable Declarations  -------- //
	private OptionButton sceneSelect;											// Reference to the scene select dropdown


	/// <summary>
	/// All we're doing here is filling our scenelist for menu options
	/// </summary>

	/// <summary>
	/// Load the scene specified by the dropdown menu
	/// </summary>
	public void _OnButtonStartPressed()
	{
		// pull together a full filepath to the selected scene
		string selectedScene = "res://Scenes/Levels/Level_01_MALCOLM.tscn";

		// Try initiating a scene load - catch the return message (should be 'Error.Ok')
		Error loadStatus = GetTree().ChangeSceneToFile(selectedScene);
		
		// Check our scene change return code - toss a message if we're not set up to load nicely
		switch (loadStatus)
		{
			case Error.Ok:
				break;
			case Error.CantOpen:
				GD.Print("Error - scene " + selectedScene + " can't be loaded into a packedScene");
				break;
			case Error.CantCreate:
				GD.Print("Error - scene " + selectedScene + " can't be instantiated properly");
				break;
			default:
				break;
		}
	}
}
