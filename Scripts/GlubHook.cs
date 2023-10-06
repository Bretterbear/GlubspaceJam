using Godot;
using System;

public partial class GlubHook : Node2D
{
    // ---------- Editor Variable Declarations ---------- //
    [Export] private float _hookLength = 500f;                  // Controls hook length, this will become a variable of glub count later
    // The below will be replaced by multiple raycast points. distributed on character
    // Keeping the below here to remind me of the thought process
    //[Export] float _senseBulbLength = 10f;	// Gives distance on a short raycast cross to grab UDLR boxed colliders if need be

    // -------- Reference Variable Declarations  -------- //
    private Line2D       _grappleLine;          // Reference storage for our line for the glub grappler
    private Line2D    _lineAimCurrent;	        // Reference storage for our current aim line for visualizer
    private Line2D        _lineAimMax;	        // Reference storage for our maximum range line for visualizer
    private RayCast2D        _rayCast;          // Reference storage for our scanning raycast

    // ---------- State Variable Declarations  ---------- //
    public TileMap    _grappledObject;	        // Stores currently hooked grapple Target
    private Vector2 _grappleHookPoint;          // Stores hook location - set to zero vector unless we're currently grappling
    private Side         _grappleSide;          // Stores hook placement orientation left - top - right - bottp,
    private bool     _inActiveGrapple;          // Stores hook state - hooked to a target or not
    private bool           _inAimMode;          // Stores whether we are in "aim mode"

    // ------------- Constants Declarations ------------- //
    private Vector2 _offsetGrappleVis = new Vector2(32, -31);   // Used for grapple offset position from prefab origin

    /// <summary>
	/// Links Glubhook to the other nodes it needs to poke
	/// </summary>
    public override void _Ready()
    {
        // Grabbing node references we'll need inside object
        _grappleLine    = GetNode<Line2D>("GrappleLine_Tmp");
        _lineAimCurrent =  GetNode<Line2D>("Line2D_AimCurr");
        _lineAimMax     = GetNode<Line2D>("Line2D_GhostAim");
        _rayCast        = GetNode<RayCast2D>("Targetter_01");
    }

    /// <summary>
    /// Tries to fire a grappling hook from player to mouse click position
	/// <para> NEED CHANGE - modify the IsColliding() to give a proper snap to correct positions </para>
    /// </summary>
    /// <param name="targetPoint"> local mouse position -> player base and glub base MUST have same position</param>
    /// <returns> bool - whether a grapple has successfully found a target or not</returns>
    public bool FireHook(Vector2 targetPoint)
    {
        // Extra call to make sure the hook is neutral as we start this process
        DisengageHook();
        
        // Make our initial raycast, forcing an update after target setting. Might change this for efficiency
        _rayCast.TargetPosition = targetPoint.Normalized() * _hookLength;
        _rayCast.ForceRaycastUpdate();

        // On collision, run checks & set the hook on validity
        if (_rayCast.IsColliding()) //change this to a validator function for behavior
        {
            // Grab inputs for the SetHook function
            _grappledObject   = (TileMap)_rayCast.GetCollider();    // This grabs a reference to the entire TileMap
            _grappleHookPoint = _rayCast.GetCollisionPoint();       // Use hookpoint as tmp storage for our collision point

            // Sets hook & WILL validate the process was successful.
            return SetHook(_grappledObject, _grappleHookPoint, targetPoint.Normalized());
        }
        else
        {
            // Add some sort of vfx/sfx feedback for a failed shot
            return false;
        }
    }

    /// <summary>
    /// Sets the glub hook position to the center of hook adjacent tile
    /// Also sets the grapplSide variable, giving the hook information on orientation of attachment
    /// This logic will need to change when we move to double raycasts at offset mode
    /// </summary>
    private bool SetHook(TileMap grappledObject, Vector2 collisionPt, Vector2 targetPt)
    {
        // Block to manipulate and work with the TileMap system - it's really odd
        Vector2I tilePosition    = grappledObject.LocalToMap(ToLocal(collisionPt));	    // Stores the map local tile position
        Vector2 tileCenter       = ToGlobal(grappledObject.MapToLocal(tilePosition));   // Stores the "center" of the tile (weird as hell)
        TileData tileObj         = grappledObject.GetCellTileData(0, tilePosition);     // Stores the tile we're touching
        Vector2[] tilePolyPoints = tileObj.GetCollisionPolygonPoints(0, 0);             // Stores the square poly points of the tile

        // Actually set our grapple point
        _grappleHookPoint = tileCenter;     // Set to the middle of oriented adjacent tile
        _inActiveGrapple  = true;           // Set early, may need to move this past validity checks

        // We're using sentinel values because i'm an idiot & don't have the brainspaceto make this clever right now
        // Key: X,Y = 0,0 -> no parity (BAD, code bug), X,Y = 0,1||1,0 single parity (GOOD), X,Y = 1,1 (BAD, perfect corner)
        bool sentX = false; 
        bool sentY = false;

        // Run along the adjacent tile edge points to find our mating surface
        for (int i = 0; i < tilePolyPoints.Length; i++)
        {
            if (Mathf.IsEqualApprox(((tileCenter - tilePolyPoints[i]).X), collisionPt.X, 1))
            {
                GD.Print("Dimension match - X match");
                sentX = true;
            }
            if (Mathf.IsEqualApprox(((tileCenter - tilePolyPoints[i]).Y), collisionPt.Y, 1))
            {
                GD.Print("Dimension match - Y match");
                sentY = true;
            }

            // Print block is for diagnostic purposes & can be removed or disabled
            GD.Print(i + 1 + " tilePt - " + (tileCenter - tilePolyPoints[i]));
            GD.Print(i + 1 + " collPt - " + collisionPt);
            GD.Print("-----------------");
        }

        // Now we set our visual grapple-line points (will be replaced w/ some glub calls likely
        // But we'll still likely use the end points of the zero vector & hook point for display purposes

        // Set our grapple line start point
        _grappleLine.AddPoint(Vector2.Zero);

        // Set our grapple line end point & set our "_grappleSide" at the same time
        // Please note that these weird seemingly arbitrary offsets are a function of the tile alignment system
        if (sentX)
        {
            if (targetPt.X > 0)
            {
                _grappleSide = Side.Left;
                _grappleLine.AddPoint(ToLocal(_grappleHookPoint) - Vector2.Right * 32 - _offsetGrappleVis);
            }
            else
            {
                _grappleSide = Side.Right;
                _grappleLine.AddPoint(ToLocal(_grappleHookPoint) - Vector2.Right * 32 - _offsetGrappleVis);
            }
        }
        else if (sentY)
        {
            if (targetPt.Y < 0)
            {
                _grappleSide = Side.Bottom;
                _grappleLine.AddPoint(ToLocal(_grappleHookPoint) + Vector2.Up * 32 - _offsetGrappleVis);
            }
            else
            {
                _grappleSide = Side.Top;
                _grappleLine.AddPoint(ToLocal(_grappleHookPoint) + Vector2.Up * 32 - _offsetGrappleVis);
            }
        }
        else
        {
            GD.Print("GlubHook SetHook broken - sentinel vals suggests hit perfect corner");
        }

        // Below print functions are for diagnostic purposes if the tilemap alignment starts screwing up
        GD.Print(_grappleSide);
        GD.Print(" tilePt - " + (tileCenter));
        GD.Print(" collPt - " + collisionPt);

        // As of now just always returns true, will likely add some validation logic in here
        // And have some false return paths
        return true;

    }

    /// <summary>
    /// Toggles aim mode for the glub hook + aim visualizer
    /// </summary>
    public void ToggleAimVisualizer()
    {
        // We flop into / out of mode immediately, then do the setup process
        _inAimMode = !_inAimMode;

        // Logic branch to either set up or teardown AimMode
        if (_inAimMode)
        {
            // Set up line points for guides
            for (int i = 0; i < 2; i++)
            {
                // Note I'm doubling the current aim points because of the visual "cross-bar" effect
                _lineAimCurrent.AddPoint(Vector2.Zero);
                _lineAimCurrent.AddPoint(Vector2.Zero);
                _lineAimMax.AddPoint(Vector2.Zero);
            }
        }
        else
        {
            // Break down line points & disengage hook for safety
            _lineAimCurrent.ClearPoints();
            _lineAimMax.ClearPoints();
            GetNode<Label>("../Label").Text = "";       // Delete this line if you get rid of the debugVisualize display below (and "Label" node in prefab)
            DisengageHook();
        }
    }

    /// <summary>
    /// Creates a visual display of reachable tiles by grappling hook
    /// </summary>
    public void VisualizeAim(Vector2 targetPoint)
    {
        // Local scope variables for behavior manipulation
        float aimTBarSize = 10f;    // Sets the size of the aiming "crosshair" for aim path
        bool debugDisplay = true;   // TMP; whether to display aim info on screen, if you get rid of this, kill the label on the player prefab

        // Set max aimline end point to furthest distance along current point direction
        _lineAimMax.SetPointPosition(1, (targetPoint).Normalized() * _hookLength);

        // Current aim length cannot exceed maximum reach
        if (targetPoint.Length() > _hookLength)
        {
            targetPoint = _lineAimMax.GetPointPosition(1);
        }

        // Set our current aim visual point
        _lineAimCurrent.SetPointPosition(1, targetPoint);

        // Forms a little "t-bar" at the current aim point (just for ease of sight)
        Vector2 headOffsetVector = targetPoint.Orthogonal().Normalized();
        _lineAimCurrent.SetPointPosition(2, targetPoint + headOffsetVector * aimTBarSize);
        _lineAimCurrent.SetPointPosition(3, targetPoint + headOffsetVector * -aimTBarSize);

        // Delete the below code after you've gotten the hook functioning how you want!
        if (debugDisplay)
        {
            Label Label = GetNode<Label>("../Label");
            Label.Text = "target pos - " + targetPoint.ToString() + "\nlength - " + (Math.Round(targetPoint.Length(), 2).ToString());
        }
    }

    /// <summary>
    /// Resets grappling variables for player state
    /// </summary>
    public void DisengageHook()
    {
        // Reset grappling hook
        _inActiveGrapple = false;           // Reset grapple state
        _grappleHookPoint = Vector2.Zero;   // Reset grapple hook point
        _grappleLine.ClearPoints();         // Resets the grapple line to be non-existant again

        // Tell the glubs to go back to non-grapple behavior
        // NOTE - this will likely change w/ Tom's more complex glub handling system
        GetTree().CallGroup("glubs", "_OnUpdateGlubGrappleState", false);
    }

    /// <summary>
    /// Getter for glubHook's understanding of whether we're in a grapple right now
    /// </summary>
    public bool IsInGrapple()
    {
        return _inActiveGrapple;
    }

    /// <summary>
    /// Getter for where the grappling hook has been lodged
    /// </summary>
    public Vector2 GetHookPoint()
    {
        return _grappleHookPoint;
    }

    /// <summary>
    /// Getter for current grapple orientation - used by warper in Player currently
    /// </summary>
    public Side GetGrappleSide()
    {
        return _grappleSide;
    }
}
