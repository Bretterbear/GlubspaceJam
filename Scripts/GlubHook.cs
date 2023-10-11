using Godot;
using System;
using System.Collections;

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
    private Vector2 _grappleHookPoint;          // Stores hook location - set to zero vector unless we're currently grappling
    private Side         _grappleSide;          // Stores hook placement orientation left - top - right - bottp,
    private bool     _inActiveGrapple;          // Stores hook state - hooked to a target or not
    private bool           _inAimMode;          // Stores whether we are in "aim mode"

    // ------------- Constants Declarations ------------- //
    private Vector2 _offsetGrappleVis = new(32,-31);    // Used for grapple offset position from prefab origin
    private Vector2 _stepSize         = new(64, 64);    // Denotes tilemap step-size     

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
        if (_rayCast.IsColliding() && (_rayCast.GetCollider().GetType() == typeof(TileMap)))
        {
            // Grab inputs for the SetHook function
            TileMap touchedTilemap = (TileMap)_rayCast.GetCollider();       // This refs the whole tilemap
            Vector2 rayCastCollisionPoint =  _rayCast.GetCollisionPoint();  // Exterior point of collision
            Vector2 rayCastNormalVector   = _rayCast.GetCollisionNormal();  // Collision face

            // Grab the data for this individual tile instance (yes, the process is weird)
            Vector2 tileInterior        = rayCastCollisionPoint - rayCastNormalVector * 10f;
            Vector2I tileMapCoordinates = touchedTilemap.LocalToMap(tileInterior);
            TileData tileData           = touchedTilemap.GetCellTileData(0, tileMapCoordinates);

            // Grab custom layer data for terrain handling purposes
            int tileDataTerrain     = (int)tileData.GetCustomDataByLayerId(0);
            int tileDataOrientation = (int)tileData.GetCustomDataByLayerId(1);

            // Handle the hook functionality based on terrain types
            switch (tileDataTerrain)
            {
                case -1:    // Case (-1 kill)     | Kill a Glub on the point (currently fall through to non-stick)
                case  0:    // Case (+0 nonstick) | don't stick at all, but end the push
                    return false;
                case  1:    // Case (+1 fullglub) | std stickiness  
                    SetHook(touchedTilemap, rayCastCollisionPoint, rayCastNormalVector);
                    return true;
                case  2:    // Case (+2 halfglub) | orientable stickiness
                    if (tileDataOrientation == ((int)TranslateCollisionNormal(rayCastNormalVector)) + 1)
                    {
                        SetHook(touchedTilemap, rayCastCollisionPoint, rayCastNormalVector);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case  3:    // Case (+3 barrior) | complex collision stickery
                    // do recursion here, turn off the collide & bool firehook, reset collider, return bool
                default:
                    GD.Print("Error(MapHandling) - currently unhandled terraintype: " + tileDataTerrain);
                    return false;
            }
        }
        else
        {
            // Add some sort of vfx/sfx feedback for a failed shot
            return false;
        }
    }

    public void SetHook(TileMap tileMap, Vector2 collisionPoint, Vector2 collisionNormal)
    {
        // First we get an arbitrary point partway inside our object
        // Then we convert into the map coordinate system for calls
        Vector2 tileInterior        = collisionPoint - collisionNormal * 10f;
        Vector2I tileMapCoordinates = tileMap.LocalToMap(tileInterior);

        // Use tilemap functions to get a bead on the center of the tile
        // Note this will always be in the top right corner of the visual tile
        Vector2 tileLocalCoordinates = tileMap.MapToLocal(tileMapCoordinates);

        // Set our hook orientation based on our collision normal vector
        _grappleSide = TranslateCollisionNormal(collisionNormal);

        /*
        switch (collisionNormal)
        {
            case (-1, 0):
                _grappleSide = Side.Left;
                break;
            case (0, -1):
                _grappleSide = Side.Top;
                break;
            case (1, 0):
                _grappleSide = Side.Right;
                break;
            case (0, 1):
                _grappleSide = Side.Bottom;
                break;
        }
        */

        // Set our true hook vector to the center of the tile for tracking
        _grappleHookPoint = tileLocalCoordinates + new Vector2(-32, 32);

        // Add our grapple display lines
        _grappleLine.AddPoint(Vector2.Zero);
        _grappleLine.AddPoint(ToLocal(_grappleHookPoint) + collisionNormal * (_stepSize / 2f));

        // Finally set our grapple status to active
        _inActiveGrapple = true;
    }

    private static Side TranslateCollisionNormal(Vector2 collisionNorm)
    {
        switch (collisionNorm)
        {
            case (-1, 0):
                return Side.Left;
            case (0, -1):
                return Side.Top;
            case (1, 0):
                return Side.Right;
            case (0, 1):
                return Side.Bottom;
        }

        GD.Print("Errror(GlubHook): translateCollisionNormal() - Vector " + collisionNorm + " is not cardinal");
        return Side.Left;
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
