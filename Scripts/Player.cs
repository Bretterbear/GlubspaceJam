using Godot;
using System.Diagnostics;

public partial class Player : CharacterBody2D
{
    // ---------- Editor Variable Declarations ---------- //
    [Export] private const float _Speed = 500.0f;               // Will need to fine tune, but controls player horizontal speed

    // -------- Reference Variable Declarations  -------- //
    private GlubHook  glubHook;                                 // Reference storage for our glub hook for function calling

    // ---------- State Variable Declarations  ---------- //
    private Vector2 _dPadInput;                                 // Stores current dPadInput
    private bool    _inAimMode;                                 // BH - Likely replace this w/ a playerstate enum later

    // ------------- Constants Declarations ------------- //
    private Vector2 _offsetGrappleVis = new Vector2(32,-31);    // BAD ENGINEERING - data duplication w/ glubhook's "_offsetGrappleVis"
    private Vector2         _stepSize = new Vector2(64, 64);    // Stores our grid step size
    private float             gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    /// <summary>
    /// As of now all we're doing is setting our glubHook reference here
    /// </summary>
    public override void _Ready()
    {
        // Set up hook reference & make sure aim state is reset
        glubHook   = GetNode<GlubHook>("GlubHook");
        _inAimMode = false;
    }

    /// <summary>
    /// We're going to tweak this to basically be an enum switch func call to the various player motion states / types
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        // Run a check to see if we are / should be in aim mode or not
        AimModeCheckAndUpdate();

        // Grab d-pad input - for now it's just for movement, but will likely be for aim purposes
        SnagDPadInput();

        // Call free move / aim mode update
        if (_inAimMode)
        {
            HandleAimMode(delta);
        }
        else
        {
            HandleFreeMode(delta);
        }
    }

    /// <summary>
    /// Checks for mode_toggle_aim input and sets the mode accordingly in Player + GlubHook
    /// </summary>
    private void AimModeCheckAndUpdate()
    {
        // Handle aim mode toggling
        if (Input.IsActionJustPressed("mode_toggle_aim"))
        {
            // Logic branch for toggling
            if (_inAimMode)
            {
                //Toggle off aim mode in both player & glubHook
                _inAimMode = false;
                glubHook.ToggleAimVisualizer();
            }
            else
            {
                // Toggle off aim mode in player & glubhook
                _inAimMode    = true;
                this.Position = this.Position.Snapped(_stepSize);    // Need to hide this snap w/ a smooth motion + a little vfx dazzle puff
                Velocity      = Vector2.Zero;
                glubHook.ToggleAimVisualizer();
            }
        }

        // Aim mode update in glubhook
        if (_inAimMode)
        {
            // We're adjusting for the offset between origin (low-left corner) and visual object center
            glubHook.VisualizeAim(GetLocalMousePosition() - _offsetGrappleVis);
        }

        // I think this is useless, but I'm afraid to pull it out b/c the hook system is so funky
        MoveAndSlide();
    }

    /// <summary>
    /// Handling for logic branch where you're in "aim/shoot" mode
    /// delta currently unused, likely will be pulled later
    /// </summary>
    private void HandleAimMode(double delta)
    {
        // Handling for glub firing call
        if (Input.IsActionJustPressed("action_fire"))
        {
            // Fires hook & uses return to see if we need to update glubs
            if (glubHook.FireHook(GetLocalMousePosition() - _offsetGrappleVis))
            {
                // Replace this w/ success SFX/VFX caLL
                GetTree().CallGroup("glubs", "_OnUpdateGlubGrappleState",true);
            }
            else
            {
                // Replace this w/ fail SFX/VFX call
            }
        }
        else
        {
            // Grapple handling branch
            if (glubHook.IsInGrapple())
            {
                // Hard coded input to allow dpad input to trigger either a grapple cancel or a grapple confirm
                if (_dPadInput.Y < 0)
                {
                    // Replace this w/ a call to an ienumerator that makes a smooth motion as opposed to a hop + add input disabling mid smoothmove
                    JumpToGrappleDestation();
                    //GetTree().CallGroup("glubs", "_OnUpdateGlubGrappleState", false);
                    // Need to add a ground check for autodisengage if you're on the ground, made somewhat stickier by "snapped" grid positioning
                }
                else if (_dPadInput.Y > 0)
                {
                    glubHook.DisengageHook();
                    //GetTree().CallGroup("glubs", "_OnUpdateGlubGrappleState", false);
                }
            }
        }
    }

    /// <summary>
    /// Warp function to pull glub to destination point based on hook
    /// <para> Need to turn this into a coroutine function for movement as opposed to warp eventually</para>
    /// </summary>
    private void JumpToGrappleDestation() 
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
        this.Position = glubHook.GetHookPoint() + repOffset;
        this.Position =    this.Position.Snapped(_stepSize);

        // Finish by disengaging our hook
        glubHook.DisengageHook();
    }

    /// <summary>
    /// Handling branch for any player movement not under the auspices of aim mode
    /// </summary>
    private void HandleFreeMode(double delta)
    {
        Velocity = SetVelocityRegularly(delta);
        MoveAndSlide();
    }

    /// <summary>
    /// Some fairly basic movement code, to be made nicer later
    /// </summary>
    private Vector2 SetVelocityRegularly(double delta)
    {
        // Grab a local handle for velocity tweakery
        Vector2 velocity = Velocity;

        // Add the gravity
        if (!IsOnFloor())
            velocity.Y += gravity * (float)delta;

        // Set x velocity, for now just simple binary input
        if (_dPadInput != Vector2.Zero)
        {
            velocity.X = _dPadInput.X * _Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _Speed);
        }

        // Not sure why I broke this function out, but here we are!
        return velocity;
    }

    /// <summary>
    /// Currently unused, will be used to handle smooth position changing for a grapple transition
    /// </summary>
    private Vector2 SetVelocityGrapple(double delta)
    {
        return Vector2.Zero;
    }

    /// <summary>
    /// One line function to set _dPadInput - might grow more complex if we start adding movement magnitude, etc
    /// </summary>
    private void SnagDPadInput()
    {
        _dPadInput = Input.GetVector("move_left", "move_right", "move_up", "move_down" + "");
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