using Godot;

/// <summary>
/// Runs basic main menu w/ a dropdown menu to select scene \n
/// Please note we might move our scene loading to a packedScene system for preloading?
/// </summary>
public partial class MainMenu : Control
{
    // -------- Reference Variable Declarations  -------- //
    private OptionButton sceneSelect;											// Reference to the scene select dropdown

    // ------------- Constants Declarations ------------- //
    private string	 pathLevelFolder = "res://Scenes/Levels";					// Path to the folder containing all levels
	private string pathOptionsButton = "VBox_Menu/HBox_Start/Button_Options";	// Path to button for level select dropdown

    /// <summary>
	/// All we're doing here is filling our scenelist for menu options
	/// </summary>
    public override void _Ready()
	{
		// Calls our method to fill out scene paths from the scenes folder
		PopulateSceneList(pathLevelFolder);
	}

	/// <summary>
	/// Load the scene specified by the dropdown menu
	/// </summary>
	private void _OnButtonStartPressed()
	{
		// pull together a full filepath to the selected scene
		string selectedScene = pathLevelFolder + "/" + sceneSelect.GetItemText(sceneSelect.GetSelectedId());

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
        sceneSelect = GetNode<OptionButton>(pathOptionsButton);
        if (sceneSelect == null)
		{
            GD.Print("Error(MainMenu) | invalid dropdown menu filepath");
            return;
		}

		sceneSelect.Clear();					// Shouldn't do anything - but can't hurt to check

        // Try to open the folder specified in Ready to make a usable directory ref
        DirAccess dir = DirAccess.Open(absFolderPath);

		// DirAccess defaults to null if the file path fails, so there's our error handling
		if (dir != null)
		{
			dir.ListDirBegin();					// Start up a stream for the directory folder
			string fileName = dir.GetNext();	// Initialize filename w/ the first file in the folder

            // Move through the files in the absFolderPath Directory, add as appropriate
            while (fileName != "")
			{
				// Only add the file to the dropdown if it's a tscn file
				if (fileName.GetExtension() == "tscn")
				{
                    sceneSelect.AddItem(fileName);
                }

				fileName = dir.GetNext();		// func will iterate & end on empty string
            }

			dir.ListDirEnd();					// Might be unnecessary - unknown if DirAccess stream autocloses on end
        }
		else
		{
			// Print an error if we can't access the directory at absFolderPath
			GD.Print("Error (MainMenu) | Path - ", absFolderPath, " | Error - ", DirAccess.GetOpenError());
		}
	}
}