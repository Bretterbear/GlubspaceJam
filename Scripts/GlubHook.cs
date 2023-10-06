using Godot;
using Godot.NativeInterop;
using System;
using System.Diagnostics;
using System.Linq;

public partial class GlubHook : Node2D
{
    // ---------- Editor Variable Declarations ---------- //
    [Export] private float _hookLength = 500f;  // Controls hook length, this will become a variable of glub count later
                                                //[Export] float _senseBulbLength = 10f;	// Gives distance on a short raycast cross to grab UDLR boxed colliders if need be

    // -------- Non-Editor Variable Declarations -------- //
    //private Player	      _player;		// Stores a reference to the player for hook updating - MIGHT CUT, CURRENTLY UNNECESSARY
    private Line2D _grappleLine;    // Stores a reference to our line for the glub grappler
    private Line2D _lineAimCurrent;	// Stores a reference to our current aim line for visualizer
    private Line2D _lineAimMax;	// Stores a reference to our maximum range line for visualizer
    private RayCast2D _rayCast; // Stores a reference to our scanning raycast
    public TileMap _grappledObject;	// Stores currently hooked grapple Target
    private bool _inActiveGrapple;  // Stores hook state - hooked to a target or not
    private bool _inAimMode;    // Stores whether we are in "aim mode"
    private Vector2 _grappleHookPoint;  // Stores hook location - set to zero vector unless we're currently grappling
    private Side _grappleSide;	// Stores hook placement orientation left - top - right - bottp,

    private Vector2 _offsetGrappleVis = new Vector2(32, -31);

    /// <summary>
	/// Links Glubhook to the other nodes it needs to poke
	/// </summary>
    public override void _Ready()
    {
        //_player = GetParent<Player>();  // Set link to parent for communication - MIGHT CUT, CURRENTLY UNNECESSARY
        _grappleLine = GetNode<Line2D>("GrappleLine_Tmp");
        _lineAimCurrent = GetNode<Line2D>("Line2D_AimCurr");
        _lineAimMax = GetNode<Line2D>("Line2D_GhostAim");
        _rayCast = GetNode<RayCast2D>("Targetter_01");
    }

    /// <summary>
    /// Tries to fire a grappling hook from player to mouse click position
	/// Need to modify the IsColliding() to give a proper snap to correct positions
    /// </summary>
    /// <returns>bool Whether a grapple has successfully found a target or not</returns>
    public bool FireHook(Vector2 targetPoint)
    {
        DisengageHook();    // Here to ensure that the hook isn't engaged when we start trying to fire
        _rayCast.TargetPosition = targetPoint.Normalized() * _hookLength;
        _rayCast.ForceRaycastUpdate();

        if (_rayCast.IsColliding())
        {
            // Will likely need to change this to a Vector2 Snapped() GetCollisionNormal type of thing
            // due to the grid-snap nature of how we're gonna go in 8 directions
            // "if type = Tilemap go into sethook validation"
            _grappledObject = (TileMap)_rayCast.GetCollider();
            //GD.Print(_grappledObject);
            _grappleHookPoint = _rayCast.GetCollisionPoint();   // change this to be a validator function
            GD.Print(_grappleHookPoint);

            //SetHook(_grappledObject, _grappleHookPoint, targetPoint.Normalized());
            //TryResetHook(_grappledObject, _grappleHookPoint, targetPoint.Normalized());
            SuperSetHook(_grappledObject, _grappleHookPoint, targetPoint.Normalized());
            _grappleLine.AddPoint(_offsetGrappleVis);

            switch (_grappleSide)
            {
                case Side.Left:
                    //_grappleLine.AddPoint(ToLocal(_grappleHookPoint));
                    _grappleLine.AddPoint(ToLocal(_grappleHookPoint) - Vector2.Right*32);
                    break;
                case Side.Right:
                    //_grappleLine.AddPoint(ToLocal(_grappleHookPoint));
                    _grappleLine.AddPoint(ToLocal(_grappleHookPoint) - Vector2.Right * 32);
                    break;
                case Side.Top:
                    //_grappleLine.AddPoint(ToLocal(_grappleHookPoint));
                    _grappleLine.AddPoint(ToLocal(_grappleHookPoint) + Vector2.Up * 32);
                    break;
                case Side.Bottom:
                    //_grappleLine.AddPoint(ToLocal(_grappleHookPoint));
                    _grappleLine.AddPoint(ToLocal(_grappleHookPoint) + Vector2.Up * 32);
                    break;
            }

            //_grappleLine.AddPoint(ToLocal(_grappleHookPoint));
            _inActiveGrapple = true;
            return true;
        }
        else
        {
            // Include some sort of vfx/sfx feedback for a failed shot
            return false;
        }
    }

    private void SuperSetHook(TileMap grappledObject, Vector2 collisionPt, Vector2 targetPt)
    {
        var tilePosition = grappledObject.LocalToMap(ToLocal(collisionPt));	//stores the map local tile position
        var tileCenter = ToGlobal(grappledObject.MapToLocal(tilePosition));     //stores the "center" of the tile (weird as hell)
        var tileObj = grappledObject.GetCellTileData(0, tilePosition);          //stores the tile we're touching
        Vector2[] ptsArr = tileObj.GetCollisionPolygonPoints(0, 0);             //stores the square poly points of the tile

        bool sentX = false; //sentinel because I'm an idiot; 0 = x parity, 1 = y parity, 2 = double parity (shouldn't be poss)
        bool sentY = false;

        for (int i = 0; i < ptsArr.Length; i++)
        {
            GD.Print(i + 1 + " tilePt - " + (tileCenter - ptsArr[i]));
            GD.Print(i + 1 + " collPt - " + collisionPt);
            GD.Print("-----------------");

            if (Mathf.IsEqualApprox(((tileCenter - ptsArr[i]).X), collisionPt.X, 1))
            {
                GD.Print("x match");
                sentX = true;
            }
            if (Mathf.IsEqualApprox(((tileCenter - ptsArr[i]).Y), collisionPt.Y, 1))
            {
                GD.Print("y match");
                sentY = true;
            }
        }

        float newX = sentX ? collisionPt.X : tileCenter.X;
        float newY = sentY ? collisionPt.Y : tileCenter.Y;

        if (sentX)
        {
            if (targetPt.X > 0)
            {
                _grappleSide = Side.Left;
            }
            else
            {
                _grappleSide = Side.Right;
            }
        }
        else if (sentY)
        {
            if (targetPt.Y < 0)
            {
                _grappleSide = Side.Bottom;
            }
            else
            {
                _grappleSide = Side.Top;
            }
        }
        else
        {
            GD.Print("BROKEN ON THE GLUBHOOK SETHOOK; HIT A PERFECT CORNER");
        }

        Vector2 offsetarooski;
        switch (_grappleSide)
        {
            case Side.Left:
                // tileCenter is dead center inside of
                break;
            case Side.Right:
                // tile center is to the right of
                break;
            case Side.Top:
                // tileCenter is dead center inside of
                break;
            case Side.Bottom:
                // tileCenter is centered in the tile underneath
                break;
        }



        GD.Print(_grappleSide);
        //_grappleHookPoint = new Vector2(newX, newY);
        _grappleHookPoint = tileCenter;
        GD.Print(" tilePt - " + (tileCenter));
        GD.Print(" collPt - " + collisionPt);

    }


    private void TryResetHook(TileMap grappledObject, Vector2 collisionPt, Vector2 targetPt)
    {
        var tilePosition = grappledObject.LocalToMap(ToLocal(collisionPt));	//stores the map local tile position
        var tileCenter = ToGlobal(grappledObject.MapToLocal(tilePosition));     //stores the "center" of the tile (weird as hell)

        GD.Print(" tilePt - " + (tileCenter));
        GD.Print(" collPt - " + collisionPt);

        var tileObj = grappledObject.GetCellTileData(0, tilePosition);          //stores the tile we're touching

        tileObj.Modulate = new Color(1, 0, 0, 1);

        Vector2[] ptsArr = tileObj.GetCollisionPolygonPoints(0, 0);             //stores the square poly points of the tile

        bool sentX = false; //sentinel because I'm an idiot; 0 = x parity, 1 = y parity, 2 = double parity (shouldn't be poss)
        bool sentY = false;

        for (int i = 0; i < ptsArr.Length; i++)
        {
            GD.Print(i + 1 + " tilePt - " + (tileCenter - ptsArr[i]));
            GD.Print(i + 1 + " collPt - " + collisionPt);
            GD.Print("-----------------");

            if (Mathf.IsEqualApprox(((tileCenter - ptsArr[i]).X), collisionPt.X, 1))
            {
                GD.Print("x match");
                sentX = true;
            }
            if (Mathf.IsEqualApprox(((tileCenter - ptsArr[i]).Y), collisionPt.Y, 1))
            {
                GD.Print("y match");
                sentY = true;
            }
        }

        float newX = sentX ? collisionPt.X : tileCenter.X;
        float newY = sentY ? collisionPt.Y : tileCenter.Y;

        if (sentX)
        {
            if (targetPt.X > 0)
            {
                _grappleSide = Side.Left;
            }
            else
            {
                _grappleSide = Side.Right;   
            }
        }
        else if (sentY)
        {
            if (targetPt.Y < 0)
            {
                _grappleSide = Side.Bottom;
            }
            else
            {
                _grappleSide = Side.Top;
            }
        }
        else
        {
            GD.Print("BROKEN ON THE GLUBHOOK SETHOOK; HIT A PERFECT CORNER");
        }

        //tileObj

        switch (_grappleSide)
        {
            case Side.Left:
                break; 
            case Side.Right:
                break;
            case Side.Top:
                break;
            case Side.Bottom:
                break;
        }



        GD.Print(_grappleSide);
        _grappleHookPoint = new Vector2(newX, newY);
        //_grappleHookPoint = tileCenter;
        GD.Print(" tilePt - " + (tileCenter));
        GD.Print(" collPt - " + collisionPt);
        //_grappleHookPoint = new Vector2(tileCenter.X, tileCenter.Y);
    }

    /*
    // Iteration 1, probably need to do nearest neighbor validation
    private void SetHook(TileMap grappledObject, Vector2 collisionPoint, Vector2 pointVector)
    {
        var tilePosition = grappledObject.LocalToMap(ToLocal((collisionPoint));	//stores the map local tile position
        var tileCenter = ToGlobal(grappledObject.MapToLocal(tilePosition));		//stores the "center" of the tile (weird as hell)

        var tileObj = grappledObject.GetCellTileData(0, tilePosition);          //stores the tile we're touching
        Vector2[] ptsArr = tileObj.GetCollisionPolygonPoints(0, 0);             //stores the square poly points of the tile

        bool sentX = false; //sentinel because I'm an idiot; 0 = x parity, 1 = y parity, 2 = double parity (shouldn't be poss)
        bool sentY = false;

        for (int i = 0; i < ptsArr.Length - 1; i++)
        {
            if ((tileCenter - ptsArr[i]).X == collisionPoint.X)
            {
                sentX = true;
            }
            if ((tileCenter - ptsArr[i]).Y == collisionPoint.Y)
            {
                sentY = true;
            }
        }

        float newX = sentX ? collisionPoint.X : tileCenter.X;
        float newY = sentY ? collisionPoint.Y : tileCenter.Y;

        if (sentX)
        {
            if (pointVector.X > 0)
            {
                _grappleSide = Side.Left;
            }
            else
            {
                _grappleSide = Side.Right;
            }
        }
        else if (sentY)
        {
            if (pointVector.Y < 0)
            {
                _grappleSide = Side.Bottom;
            }
            else
            {
                _grappleSide = Side.Top;
            }
        }
        else
        {
            GD.Print("BROKEN ON THE GLUBHOOK SETHOOK; HIT A PERFECT CORNER");
        }

        GD.Print(_grappleSide);
        _grappleHookPoint = new Vector2(newX, newY);
        //_grappleHookPoint = tileCenter;
    }*/

    /// <summary>
    /// Toggles aim mode for the glub hook + aim visualizer
    /// </summary>
    public void ToggleAimVisualizer()
    {
        _inAimMode = !_inAimMode;

        if (_inAimMode)                                 // Set up line points for aim visual aid
        {
            for (int i = 0; i < 2; i++)
            {
                _lineAimCurrent.AddPoint(_offsetGrappleVis);
                _lineAimCurrent.AddPoint(_offsetGrappleVis); // Line is only duplicated b/c I'm using 4pts for the current aim line
                _lineAimMax.AddPoint(_offsetGrappleVis);
            }
        }
        else                                            // Break down aim lines & disengage hook just in case
        {
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

        // Set max aimline end point to furthest distance along vurrent point direciton
        _lineAimMax.SetPointPosition(1, (targetPoint).Normalized() * _hookLength);

        // Current aim length cannot exceed maximum reach
        if (targetPoint.Length() > _hookLength)
        {
            targetPoint = _lineAimMax.GetPointPosition(1);
        }

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
        _inActiveGrapple = false;           // Reset grapple state
        _grappleHookPoint = Vector2.Zero;   // Reset grapple hook point
        _grappleLine.ClearPoints();         // Resets the grapple line to be non-existant again
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
    /// Gives back where the grappling hook has been lodged
    /// </summary>
    public Vector2 GetHookPoint()
    {
        return _grappleHookPoint;
    }

    public Side GetGrappleSide()
    {
        return _grappleSide;
    }
}
