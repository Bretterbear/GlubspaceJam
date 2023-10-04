using Godot;

/// <summary>
/// 
/// </summary>
public partial class BabyGlubSprite : Sprite2D
{
	// ---------- Editor Variable Declarations ---------- //
	[Export] private Color _overrideColor;					// Used to override & set a static color, probably toss this
	[Export] private Color[] _colorBank;					// After we settle on colors, turn this into a private static colorarray

	// -------- Non-Editor Variable Declarations -------- //
	private Color _defaultColor = new Color(0, 0, 0, 1);	// Toss this after you kil the overridability

    /// <summary>
	/// Every time a glub enters the scene, set a color out of the palette
	/// </summary>
    public override void _Ready()
	{
		Color pickedColor;
		if (_overrideColor == _defaultColor)
		{
			pickedColor = _colorBank[GD.Randi() % _colorBank.Length];
        }
        else
        {
			pickedColor = _overrideColor;
        }
		Modulate = pickedColor;
    }
}
