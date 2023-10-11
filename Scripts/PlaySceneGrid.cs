using Godot;

/// <summary>
/// Quick tool to give us a visual grid in level for layout / testing purposes
/// </summary>
public partial class PlaySceneGrid : Control
{
	// ---------- Editor Variable Declarations ---------- //
    [Export] private Color   _lineColor = new(0.40f, 0.40f, 0.45f, 0.40f); 
    [Export] private int   _cellSize    =    64;        // Grid snap size
    [Export] private int _xDrawDistance =  1216;        // x spread of lines
    [Export] private int _yDrawDistance =   704;        // y spread of lines

    /// <summary>
    /// Draw gridlines according to above noted settings
    /// </summary>
    public override void _Draw()
    {
        // Draw your vertical lines
        for (int x = 0; x < _xDrawDistance; x += _cellSize)
        {
            Vector2 yMin = new Vector2(x, -_yDrawDistance);
            Vector2 yMax = new Vector2(x, _yDrawDistance);

            DrawLine(yMin, yMax, _lineColor);
            DrawLine(-yMin, -yMax, _lineColor);
        }

        // Draw your horizontal lines
        for (int y = 0; y < _yDrawDistance; y += _cellSize)
        {
            Vector2 xMin = new Vector2(-_xDrawDistance, y);
            Vector2 xMax = new Vector2(_xDrawDistance, y);

            DrawLine(xMin, xMax, _lineColor);
            DrawLine(-xMin, -xMax, _lineColor);
        }
    }
}
