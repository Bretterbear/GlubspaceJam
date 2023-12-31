using Godot;
using Godot.Collections;
using System.Diagnostics;

public partial class GlubHook : Node2D
{
    // ---------- Editor Variable Declarations ---------- //
    [Export] private float _hookLength = 900f;                  // Controls hook length, this will become a variable of glub count later
    // The below will be replaced by multiple raycast points. distributed on character
    // Keeping the below here to remind me of the thought process
    //[Export] float _senseBulbLength = 10f;	// Gives distance on a short raycast cross to grab UDLR boxed colliders if need be

    // -------- Reference Variable Declarations  -------- //
    private Line2D       _grappleLine;          // Reference storage for our line for the glub grappler
    private Line2D    _lineAimDextrus;          // Reference storage for our right side hook range visualizer
    private Line2D   _lineAimSinister;          // Reference storage for our right side hook range visualizer

    // ---------- State Variable Declarations  ---------- //
    private Vector2 _grappleHookPoint;          // Stores hook location - set to zero vector unless we're currently grappling
    private Side         _grappleSide;          // Stores hook placement orientation left - top - right - bottom,
    private TileMap          _tileMap;          // Stores a ref to currently active tilemap

    // ------------- Constants Declarations ------------- //
    private Vector2 _offsetGrappleVis = new(32,-31);    // Used for grapple offset position from prefab origin
    private Vector2         _stepSize = new(64, 64);    // Denotes tilemap step-size     
    private float      _coaxialSpread = 16f;

    // -------------- Label Declarations --------------- //
    private string     _labelTileType = "TileTypes";    // Label for customdata layer 0 (tiletypes) -- corresponds to TileDataTypes.TileTypes enum
    private string   _labelTileOrient = "TileOrients";  // Label for costomdata layer 1 (Vector2) -- corresponds to normal vector comparator for orientable tiles
    private string     _labelTileData = "tileData";     // Label for collision added data for access ease
    private string    _labelMapCoords = "tileMapCoords";// Label for collision added data for access ease
    private string     _labelInterior = "tileInterior"; // Label for collision added data for access ease

    /// <summary>
    /// Links Glubhook to the other nodes it needs to poke
    /// </summary>
    public override void _Ready()
    {
        // Grabbing node references we'll need inside object
        _grappleLine     = GetNode<Line2D>("GrappleLine_Tmp");
        _lineAimDextrus  = GetNode<Line2D>("Line2D_AimDextrus");
        _lineAimSinister = GetNode<Line2D>("Line2D_AimSinister");
    }

    /// <summary>
    /// Initiator function to start the firing + hooksetting process
    /// </summary>
    /// <param name="localFireVector">local mouse position relative to glub centerpoint</param>
    public bool FireHook(Vector2 localFireVector)
    {
        // Ensure we aren't currently hooked into a target
        DisengageHook();

        // Set our coaxial spread for our double raycast system
        Vector2 targetAxialOffset = localFireVector.Orthogonal().Normalized() * _coaxialSpread;

        // Set our centerpoint starts & finishes
        Vector2 startPoint  = GlobalPosition + _offsetGrappleVis;
        Vector2 targetPoint = ToGlobal(localFireVector.Normalized() * _hookLength + _offsetGrappleVis);

        // Send two raycasts at a slight offset to one another & compare to get good behavior
        Dictionary dextrusCollision  = TileSeeker(startPoint - targetAxialOffset, targetPoint - targetAxialOffset); 
        Dictionary sinisterCollision = TileSeeker(startPoint + targetAxialOffset, targetPoint + targetAxialOffset);

        // An if structure for case handling
        if (dextrusCollision == null && sinisterCollision == null)  // Case 1 - no good collisions
        {
            return false;
        }
        else if (sinisterCollision == null)                         // Case 2 - solid collision on dextrus side
        {
            return EvaluateCollision(dextrusCollision);
        }
        else if (dextrusCollision == null)                          // Case 3 - solid collisions on sinister side
        {
            return EvaluateCollision(sinisterCollision);
        }
        else                                                        // Case 4 - solid collisions on both sides
        {
            return DoubleCollisionEvaluation(sinisterCollision, dextrusCollision);
        }
    }

    private bool EvaluateCollision(Dictionary collision)
    {
        if (collision["collider"].AsGodotObject().GetType() == typeof(Lever))
        {
            Lever leverGrab = (Lever)collision["collider"];
            leverGrab.ToggleLever();
            return false;
        }


        TileMap  collisionTileMap = (TileMap)collision["collider"];
        Vector2    collisionPoint = (Vector2)collision["position"];
        Vector2   collisionNormal = (Vector2)collision["normal"];

        TileTypes tileDataTerrain = (TileTypes)(int)collision[_labelTileType];
        Vector2    tileDataOrient =      (Vector2)collision[_labelTileOrient];

        // Handle the hook functionality based on terrain types
        switch (tileDataTerrain)
        {
            case TileTypes.hazard:    // Case (-1 kill)     | Kill a Glub on the point (currently fall through to non-stick)
                GD.Print("GH Status - Case -1, hit a kill object, no handling yet");
                return false;

            case TileTypes.nonstick:    // Case (+3 nonstick) | don't stick at all, but end the push
                GD.Print("GH Status - Case 3, hit a non-stick block");
                return false;

            case TileTypes.fullstick:    // Case (+1 fullglub) | std stickiness
                GD.Print("GH Status - Case 1, seting hook to full block");
                SetHook(collisionTileMap, collisionPoint, collisionNormal);
                return true;

            case TileTypes.halfstick:    // Case (+2 halfglub) | orientable stickiness
                if (tileDataOrient == collisionNormal)
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

            case TileTypes.barrier:    // Case (+3 barrior) | recursive barrier handling
                // If the collided face is orientated proximally to us, we can hook to it
                if (tileDataOrient == collisionNormal)
                {
                    //GD.Print("Correct orient branch of barrier case");
                    SetHook(collisionTileMap, collisionPoint, collisionNormal);
                    return true;
                }
                else
                {
                    return false;
                }

            default:
                GD.Print("Error(MapHandling) - currently unhandled terraintype: " + tileDataTerrain);
                return false;
        }
    }

    private bool DoubleCollisionEvaluation(Dictionary collisionOne, Dictionary collisionTwo)
    {
        // same object?
        // normals match? Cardinal = success
        // different normals? diagonal = corner, no match

        // branch for the same object
        if (collisionOne["collider"].AsGodotObject().GetType() == typeof(Lever))
        {
            return EvaluateCollision(collisionOne);
        }
        else if (collisionTwo["collider"].AsGodotObject().GetType() == typeof(Lever))
        {
            return EvaluateCollision(collisionTwo);
        }

        if ((Vector2I) collisionOne[_labelMapCoords] == (Vector2I)collisionTwo[_labelMapCoords])
            {
            if ((Vector2)collisionOne["normal"] == (Vector2)collisionTwo["normal"])
            {
                return EvaluateCollision(collisionOne);
            }
            else
            {
                // Corner, a failure case
                return false;
            }
        }

        // different objects? shorter x distance first
        float oneX = Mathf.Abs(((Vector2)collisionOne["position"]).X - GlobalPosition.X);
        float twoX = Mathf.Abs(((Vector2)collisionTwo["position"]).X - GlobalPosition.X);

        if (oneX < twoX)
        {
            return EvaluateCollision(collisionOne);
        }
        else
        {
            return EvaluateCollision(collisionTwo);
        }
    }


    /// <summary>
    /// Tests for collidable tiles, finds furthest along one - then returns it in a dict
    /// </summary>
    private Dictionary TileSeeker(Vector2 startPoint, Vector2 targetPoint, Rid prevCollisionRid = new Rid())
    {
        // Make our raycast in space
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(startPoint, targetPoint);

        // Add specificity for collision mask & the RID of currentl collider if called recursively for barriers
        query.CollisionMask = 512;
        query.Exclude = new Array<Rid> { prevCollisionRid };
        query.CollideWithAreas = true;

        // Result is a dictionary denoting the qualities of the raycast collision
        var result = spaceState.IntersectRay(query);

        // If no raycast collision is in range, return a failure
        if (result.Count == 0)
        {
            //GD.Print("GH Status - no hookable object in reach");
            return null;
        }

        if (result["collider"].AsGodotObject().GetType() == typeof(Lever))
        {
            return result;
        }
        else if (result["collider"].AsGodotObject().GetType() == typeof(TileMap))
        {
            // Grab key info to translate collision dict into tile information
            //Debug.WriteLine("Collision Coordinates: " + ((Vector2)result["position"]).X + " " + ((Vector2)result["position"]).Y);
            Vector2 tileInterior = ((Vector2)result["position"]) - ((Vector2)result["normal"]) * 10f;
            //Debug.WriteLine("tileInterior Coordinates: " + tileInterior.X + " " + tileInterior.Y);
            Vector2I tileMapCoords = ((TileMap)result["collider"]).LocalToMap(tileInterior);
            TileData tileData = ((TileMap)result["collider"]).GetCellTileData(0, tileMapCoords);

            // Hotfix for dealing w/ bad tilemaps that don't have data layers
            if (((TileMap)result["collider"]).TileSet.GetCustomDataLayersCount() < 2)
            {
                GD.Print("TileSeeker Error - We're bypassing terain type handling in GlubHook.TileSeeker()");
                return result;
            }

            // Grab custom layer data for terrain handling purposes
            int tileDataTerrain         =     (int)tileData.GetCustomDataByLayerId(0);
            Vector2 tileDataOrientation = (Vector2)tileData.GetCustomDataByLayerId(1);

            // Grab one last bit of data required for possible recursive passthrough
            //int collisionOrientation = ((int)TranslateCollisionNormal((Vector2)result["normal"])) + 1;

            // Adding pertinent data for the passback
            result.Add(_labelInterior  , tileInterior);
            result.Add(_labelMapCoords , tileMapCoords);
            result.Add(_labelTileData  , tileData);
            result.Add(_labelTileType  , tileDataTerrain);
            result.Add(_labelTileOrient, tileDataOrientation);

            // Special recursive case if we're hitting the back of a barrier
            if ((TileTypes)tileDataTerrain == TileTypes.barrier && tileDataOrientation == -(Vector2)(result["normal"]))
            {
                return TileSeeker(((Vector2)result["position"]), targetPoint, (Rid)result["rid"]);
            }
            else
            {
                // return terminal collision
                return result;
            }
        }
        else
        {
            GD.Print("TileSeeker Error - collided w/ non-tilemap object somehow - may have misconfigured physics layer in scene");
            return null;
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

    public void EnableAimVisualizer()
    {
        // Set up line points for guides
        for (int i = 0; i < 2; i++)
        {
            _lineAimDextrus.AddPoint(Vector2.Zero);
            _lineAimSinister.AddPoint(Vector2.Zero);
        }
    }

    public void DisableAimVisualizer()
    {
        // Break down line points & disengage hook for safety
        _lineAimDextrus.ClearPoints();
        _lineAimSinister.ClearPoints();
    }

    /// <summary>
    /// Creates a visual display of reachable tiles by grappling hook
    /// </summary>
    public void VisualizeAim(Vector2 targetVector)
    {

        // Find our coaxial offsets for our dueling rays (important for correct resolution of diagonals)
        Vector2 targetAxialOffset = targetVector.Orthogonal().Normalized() * _coaxialSpread;

        // Sets our zero points (off axis to one another)
        _lineAimDextrus.SetPointPosition(0, Vector2.Zero - targetAxialOffset);
        _lineAimSinister.SetPointPosition(0, Vector2.Zero + targetAxialOffset);

        // Do some center setting
        Vector2 startCenter = GlobalPosition + _offsetGrappleVis;
        Vector2 endCenter   = ToGlobal((targetVector.Normalized() * _hookLength) + _offsetGrappleVis);

        // Call our handy dandy endpoint checker
        Godot.Collections.Dictionary dextrusCollision  = TileSeeker(startCenter - targetAxialOffset, endCenter - targetAxialOffset);
        Godot.Collections.Dictionary sinisterCollision = TileSeeker(startCenter + targetAxialOffset, endCenter + targetAxialOffset);

        // Get our end position vectors
        Vector2 dextrusEnd  = dextrusCollision  != null ? (Vector2)dextrusCollision["position"] - _offsetGrappleVis : endCenter - targetAxialOffset - _offsetGrappleVis;
        Vector2 sinisterEnd = sinisterCollision != null ? (Vector2)sinisterCollision["position"] - _offsetGrappleVis : endCenter + targetAxialOffset - _offsetGrappleVis;

        _lineAimDextrus.SetPointPosition( 1, ToLocal(dextrusEnd));
        _lineAimSinister.SetPointPosition(1, ToLocal(sinisterEnd));
    }

    /// <summary>
    /// Resets grappling variables for player state
    /// </summary>
    public void DisengageHook()
    {
        // Reset grappling hook
        _grappleHookPoint = Vector2.Zero;   // Reset grapple hook point
        _grappleLine.ClearPoints();         // Resets the grapple line to be non-existant again

        // Tell the glubs to go back to non-grapple behavior
        // NOTE - this will likely change w/ Tom's more complex glub handling system
        GetTree().CallGroup("glubs", "_OnUpdateGlubGrappleState", false);
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
