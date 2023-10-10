using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Enumeration;
using System.Runtime.CompilerServices;

/// <summary>
/// Runs basic main menu w/ a dropdown menu to select scene \n
/// Please note currently allows reloading of main menu scene \n
/// Please note we might move our scene loading to a packedScene system for preloading?
/// </summary>
public partial class MainMenu : Control
{
    // -------- Reference Variable Declarations  -------- //
    private OptionButton sceneSelect;						// Reference to the scene select dropdown

    // ------------- Constants Declarations ------------- //
    private string levelFolderPath = "res://Scenes/Levels";	// Path to the folder containing all levels

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		//Call our method to fill out scene paths from the scenes folder
		PopulateSceneList(levelFolderPath);
	}

	/// <summary>
	/// Load the scene specified by the dropdown menu
	/// </summary>
	private void _OnButtonStartPressed()
	{
		// pull together a full filepath to the selected scene
		string selectedScene = levelFolderPath + "/" + sceneSelect.GetItemText(sceneSelect.GetSelectedId());

		// Try initiating a scene load - catch the return message (should be 'Error.Ok')
		Error loadStatus = GetTree().ChangeSceneToFile(selectedScene);
		
		// Toss a message if we're not set up to load nicely
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

	/// <summary>
	/// A quit function to exit game
	/// </summary>
	private void _OnButtonQuitPressed()
	{
		// May want to expand this w/ a short delay & an audio cue if one pops up?
		GetTree().Quit();
	}

    /// <summary>
    /// Fills the scenelist dropdown with all scenes in the specified folder
    /// </summary>
    /// <param name="absFolderPath">needs to be a project path to the levels folder</param>
    private void PopulateSceneList(string absFolderPath)
	{
		// Try to grab a reference to the scene select dropdown and error check that path
        sceneSelect = GetNode<OptionButton>("VBox_Menu/HBox_Start/Button_Options");
        if (sceneSelect == null)
		{
            GD.Print("Error(MainMenu) | invalid dropdown menu filepath");
            return;
		}

		//Theoretically shouldn't ever do anything - but never hurts to check
		sceneSelect.Clear();

        // Try to open the folder specified in Ready to make a usable directory ref
        DirAccess dir = DirAccess.Open(absFolderPath);

		// DirAccess defaults to null if the file path fails, so there's our error handling
		if (dir != null)
		{
			// Start up a stream for the directory folder
			dir.ListDirBegin();

			// Initialize filename w/ the first file in the folder
			string fileName = dir.GetNext();

			// Move through the files in the absFolderPath Directory, add as appropriate
			while (fileName != "")
			{
				// Only add the file to the dropdown if it's a tscn file
				if (fileName.GetExtension() == "tscn")
				{
                    sceneSelect.AddItem(fileName);
                }
				fileName = dir.GetNext();
            }

			// Technically this might be unnecessary - I'm uncertain if the DirAccess stream autocloses on empty
			dir.ListDirEnd();
		}
		else
		{
			// Print an error if we can't access the directory at absFolderPath
			GD.Print("Error (MainMenu) | Path - ", absFolderPath, " | Error - ", DirAccess.GetOpenError());
		}
	}
}
