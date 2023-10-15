using Godot;
using System.Diagnostics;

// Movement WASD / Arrows
// Aim Mode Toggle
// Action_Fire

public partial class Player : CharacterBody2D
{
    // ---------- Enum States Declaration ---------- //
    enum States { WALKING, AIMING, GRAPPLED, IN_TRANSIT }
    
    // ---------- Editor Variable Declarations ---------- //
    [Export] private const float _Speed = 500.0f;               // Will need to fine tune, but controls player horizontal speed
    [Export] private bool    _mouseMode = true;                 // Used to determine input type - move to options menu

    // -------- Reference Variable Declarations  -------- //
    private GlubHook  glubHook;                                 // Reference storage for our glub hook for function calling
    private TileMap   _tileMap;                                 // Reference to our level TileMap, poss unused

    // ---------- State Variable Declarations  ---------- //
    private States           _playerState;          // Maintains branch control over what input processing is done
    private Vector2   _inputDirUnitVector;          // Stores current frame movement directional input 
    private Vector2   _inputAimUnitVector;          // Modal - in WASD  mirrors dir, in mouse local vector
    private bool               _inputFire = false;  // Stores current frame "Just hit fire key" state
    private bool          _inputToggleAim = false;  // Stores current frame "Just hit aimToggle key" state

    // ------------- Constants Declarations ------------- //
    private Vector2 _offsetGrappleVis = new Vector2(32,-31);    // BAD ENGINEERING - data duplication w/ glubhook's "_offsetGrappleVis"
    private Vector2         _stepSize = new Vector2(64, 64);    // Stores our grid step size
    private float             gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    /// <summary>
    /// Initial plaer setup - sets state + grabs glubHook reference
    /// </summary>
    public override void _Ready()
    {
        // Set initial player state when game starts
        _playerState = States.WALKING;
        // Set up hook reference & make sure aim state is reset
        glubHook     = GetNode<GlubHook>("GlubHook");
       // _tileMap     = GetNode<TileMap>("../TileMap");
    }

    /// <summary>
    /// Grabs input + uses player state to carry out behaviors
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        // Gather all inputs for frame at once
        GatherInput();

        // FSM for differential handling in physics process based on player state
        switch (_playerState)
        {
            case States.WALKING:
                HandleWalking(delta);
                break;
            case States.AIMING:
                HandleAiming();
                break;
            case States.GRAPPLED:
                HandleGrappling();
                break;
            case States.IN_TRANSIT:
                // Will hold things while any animation or transition is happening
                break;
        }
    }

    /// <summary>
    /// Branch for walking and transitioning to aim mode
    /// </summary>
    private void HandleWalking(double delta)
    {
        // Grab a local handle for velocity tweakery
        Vector2 velocity = Velocity;

        // Add the gravity
        if (!IsOnFloor())
        {
            velocity.Y += gravity * (float)delta;
        }

        // Set x velocity, for now just simple binary input
        if (_inputDirUnitVector != Vector2.Zero)
        {
            velocity.X = _inputDirUnitVector.X * _Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _Speed);
        }

        Velocity = velocity;

        MoveAndSlide();

        //WILL PROBABLY GET RID OF IT
        CollisionTileHandling();

        // Check for mode swap for next frame
        // Note right now I have both aim toggle & fire being valid transition buttons
        if (_inputToggleAim || _inputFire)
        {
            ModeTransitionWalkToAim();
        }
    }

    /// <summary>
    /// Used for barrier handling (more complex solutions all sucked)
    /// </summary>
    private void CollisionTileHandling()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);

            if (collision.GetCollider().GetType() == typeof(TileMap))
            {
                TileMap maparoo = (TileMap)collision.GetCollider();
                Rid colliderRID = collision.GetColliderRid();

                Vector2I tileCoords = maparoo.GetCoordsForBodyRid(colliderRID);
                TileData rumples = maparoo.GetCellTileData(0, tileCoords);

                if ((int) rumples.GetCustomData("_keyType") == 3)
                {
                    Vector2 rumpleOrient = (Vector2) rumples.GetCustomData("_vecOrient");
                    if (collision.GetNormal() == rumpleOrient)
                    {
                        Position += -rumpleOrient * _stepSize;
                    }
                }
            }
        }
    }
    

    /// <summary>
    /// Branch for aiming + hook firing w/ state transitions to walk + to grapple
    /// </summary>
    private void HandleAiming()
    {
        glubHook.VisualizeAim(_inputAimUnitVector);

        if (_inputToggleAim)
        {
            ModeTransitionAimToWalk();
            return;
        }

        if (_inputFire)
        {
            bool successfulGrapple = glubHook.FireHook(_inputAimUnitVector);

            if (successfulGrapple)
            {
                ModeTransitionAimToGrapple();
                // INSERT SFX/VFX call for a successful glub firing
            }
            else
            {
                // INSERT SFX/VFX call for failed glub firing
            }

            return;
        }
    }

    /// <summary>
    /// Branch for being in a grapple + either warping to the hook point or disengaging
    /// </summary>
    private void HandleGrappling()
    {
        // Can jump to destination
        if (_inputFire)
        {
            // Replace this w/ a call to an ienumerator that makes a smooth motion
            // As opposed to a hop + add input disabling mid smoothmove
            JumpToGrappleDestination();
            ModeTransitionGrappleToAim();
            //GetTree().CallGroup("glubs", "_OnUpdateGlubGrappleState", false);
            return;
        }

        // Handle a request to disengage the hook so we can aim again
        if (_inputToggleAim)
        {
            ModeTransitionGrappleToAim();
            return;
        }
    }

    // Transition play mode from walk to aim
    private void ModeTransitionWalkToAim()
    {
        _playerState = States.AIMING;               // Set the state for future frames processing
        Velocity = Vector2.Zero;                    // Zero out velocity on transition
        Position = Position.Snapped(_stepSize);     // Lock us to a shooting position
        glubHook.EnableAimVisualizer();             // REPLACES ToggleAimVisualizer
    }

    // Transition play mode from aim to walk
    private void ModeTransitionAimToWalk()
    {
        _playerState = States.WALKING;
        glubHook.DisableAimVisualizer();            // REPLACES ToggleAimVisualizer disablement
    }

    // Transition play mode from aim to grappled mode
    private void ModeTransitionAimToGrapple()
    {
        _playerState = States.GRAPPLED;
        glubHook.DisableAimVisualizer();
        GetTree().CallGroup("glubs", "_OnUpdateGlubGrappleState", true);
    }

    // Transition play mode from grappling to aiming (either after a warp or a disengage)
    private void ModeTransitionGrappleToAim()
    {
        _playerState = States.AIMING;
        glubHook.DisengageHook();
        glubHook.EnableAimVisualizer();             // REPLACES ToggleAimVisualizer
    }

    /// <summary>
    /// Warp function to pull glub to destination point based on hook
    /// <para> Need to turn this into a coroutine function for movement as opposed to warp eventually</para>
    /// </summary>
    private void JumpToGrappleDestination()
    {
        // Local storage to find offset from grapple store position to correct display position
        Vector2 repOffset = Vector2.Zero;
        switch (glubHook.GetGrappleSide())
        {
            case Side.Left:
                repOffset = (Vector2.Left) * _stepSize;
                break;
            case Side.Top:
                repOffset = (Vector2.Up) * _stepSize;
                break;
            case Side.Right:
                repOffset = (Vector2.Right) * _stepSize;
                break;
            case Side.Bottom:
                repOffset = (Vector2.Down) * _stepSize;
                break;
        }

        // Set our position & snap in for further shots. Ideally snap should be a 0 distance motion
        Position = glubHook.GetHookPoint() + repOffset;
        Position = Position.Snapped(_stepSize);
    }

    /// <summary>
    /// Makes a snapping Vector 2 on 45 degree angles 
    /// </summary>
    private Vector2 GetNearest8Way(Vector2 unitVector)
    {
        // Do some quick trig to get us the nearest snappable Vector direction
        float angle           = Mathf.RadToDeg(unitVector.Angle());
        float nearestAngle    = Mathf.Round(angle / 45) * 45;
        float nearestRadAngle = Mathf.DegToRad(nearestAngle);

        return new Vector2(Mathf.Cos(nearestRadAngle), Mathf.Sin(nearestRadAngle));
    }

    /// <summary>
    /// More modular input gathering at the start of every physics frame
    /// <para> I'm not 100% on how the input 'handled' system works, so this is a compromise</para>
    /// </summary>
    private void GatherInput()
    {
        _inputDirUnitVector = Input.GetVector("move_left", "move_right", "move_up", "move_down" + "");

        if (_mouseMode)
        {
            _inputAimUnitVector = GetNearest8Way((GetLocalMousePosition() - _offsetGrappleVis).Normalized());
        }
        else
        {
            _inputAimUnitVector = _inputDirUnitVector;
        }

        _inputToggleAim = Input.IsActionJustPressed("mode_toggle_aim");
        _inputFire = Input.IsActionJustPressed("action_fire");
    }

    /// <summary>
    /// Experimenting w/ Signals, listens to the child followTimer's timeout, then should reset
    /// </summary>
    private void _BoidUpdateReceiver()
    {
        //GD.Print("We ball!");
        GetTree().CallGroup("glubs", "_OnUpdateGlubBoidTarget", GlobalPosition);
    }
}