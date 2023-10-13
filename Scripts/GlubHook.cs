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
    }

    /// <summary>
    /// Initiator function to start the firing + hooksetting process
    /// </summary>
    /// <param name="localClickPoint">local mouse position relative to glub centerpoint</param>
    public bool FireHook(Vector2 localClickPoint)
    {
        // Ensure we aren't currently hooked into a target
        DisengageHook();

        // Get our initial start point (glub center) and endpoint (grapple max) vectors
        Vector2 startPoint  = GlobalPosition + _offsetGrappleVis;
        Vector2 targetPoint = ToGlobal(localClickPoint.Normalized() * _hookLength + _offsetGrappleVis);

        // Call our raycast function which will return a bool denoting success/failure of attachment
        return MakeGrappleRaycast(startPoint, targetPoint);
    }

    /// <summary>
    /// Recursively callable function to raycast out to objects + hook them where appropriate
    /// </summary>
    private bool MakeGrappleRaycast(Vector2 startPoint, Vector2 targetPoint, Rid prevCollisionRid = new Rid())
    {
        // Make our raycast in space
        var spaceState = GetWorld2D().DirectSpaceState;
        var query      = PhysicsRayQueryParameters2D.Create(startPoint, targetPoint);

        // Add specificity for collision mask & the RID of currentl collider if called recursively for barriers
        query.CollisionMask = 512;
        query.Exclude       = new Godot.Collections.Array<Rid> { prevCollisionRid };

        // Result is a dictionary denoting the qualities of the raycast collision
        var result = spaceState.IntersectRay(query);

        // If no raycast collision is in range, return a failure
        if (result.Count == 0)
        {
            //GD.Print("GH Status - no hookable object in reach");
            return false;
        }

        if (result["collider"].AsGodotObject().GetType() == typeof(TileMap))
        {
            // Grab basic collision data
            Vector2 collisionPoint   = (Vector2)result["position"];
            Vector2 collisionNormal  = (Vector2)result["normal"];
            TileMap collisionTileMap = (TileMap)result["collider"];

            // Use basic collision data to translate into tile information
            Vector2 tileInterior   = collisionPoint - collisionNormal * 10f;
            Vector2I tileMapCoords = collisionTileMap.LocalToMap(tileInterior);
            TileData tileData      = collisionTileMap.GetCellTileData(0, tileMapCoords);

            // Hotfix for dealing w/ bad tilemaps that don't have data layers
            if (collisionTileMap.TileSet.GetCustomDataLayersCount() < 2)
            {
                GD.Print("We're bypassing terain type handling in GlubHook.FireHook()");
                SetHook(collisionTileMap, collisionPoint, collisionNormal);
                return true;
            }

            // Grab custom layer data for terrain handling purposes
            int tileDataTerrain     = (int)tileData.GetCustomDataByLayerId(0);
            int tileDataOrientation = (int)tileData.GetCustomDataByLayerId(1);

            // Handle the hook functionality based on terrain types
            switch (tileDataTerrain)
            {
                case -1:    // Case (-1 kill)     | Kill a Glub on the point (currently fall through to non-stick)
                    GD.Print("GH Status - Case -1, hit a kill object, no handling yet");
                    return false;

                case 0:    // Case (+0 nonstick) | don't stick at all, but end the push
                    GD.Print("GH Status - Case 0, hit a non-stick block");
                    return false;

                case 1:    // Case (+1 fullglub) | std stickiness
                    GD.Print("GH Status - Case 1, seting hook to full block");
                    SetHook(collisionTileMap, collisionPoint, collisionNormal);
                    return true;

                case 2:    // Case (+2 halfglub) | orientable stickiness
                    if (tileDataOrientation == ((int)TranslateCollisionNormal(collisionNormal)) + 1)
                    {
                        //GD.Print("GH Status - Case 2 - Successfully stuck to an orientable halfblock");
                        SetHook(collisionTileMap, collisionPoint, collisionNormal);
                        return true;
                    }
                    else
                    {
                        //GD.Print("GH Status - Case 2 - failure, hit wrong face of half block");
                        return false;
                    }

                case 3:    // Case (+3 barrior) | recursive barrier handling
                    // First get our collision orientation
                    int collisionOrientation = ((int)TranslateCollisionNormal(collisionNormal)) + 1;

                    // If the collided face is orientated proximally to us, we can hook to it
                    if (tileDataOrientation == collisionOrientation)
                    {
                        //GD.Print("Correct orient branch of barrier case");
                        SetHook(collisionTileMap, collisionPoint, collisionNormal);
                        return true;
                    }
                    // If the collided face is oriented distally to us we can pass through it
                    else if (tileDataOrientation == (collisionOrientation + 2) % 4)
                    {
                        //GD.Print("Inverse orient branch of barrier case");
                        // A recursive call to restart a raycast at our collision point ignoring the current barrier block
                        return MakeGrappleRaycast(collisionPoint, targetPoint, (Rid) result["rid"]);
                    }
                    // Otherwise we're hitting it edge on, which is a non-grappleable orientation per the rules
                    else
                    {
                       //GD.Print("Fail branch of barrier case");
                        return false;
                    }

                default:
                    GD.Print("Error(MapHandling) - currently unhandled terraintype: " + tileDataTerrain);
                    return false;
            }
        }
        else
        {
            GD.Print("GH Status - intersected w/ invalid object");
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
