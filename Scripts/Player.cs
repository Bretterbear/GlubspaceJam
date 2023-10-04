using Godot;
using System.Diagnostics;

public partial class Player : CharacterBody2D
{
    // ---------- Editor Variable Declarations ---------- //
    [Export] private const float _Speed = 500.0f;                // Will need to fine tune, but controls player horizontal speed
    [Export] private Vector2 _stepSize = new Vector2(64, 64);    // Stores our grid step size

    // -------- Non-Editor Variable Declarations -------- //
    private float      gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private bool    _inAimMode;     // BH - Likely replace this w/ a playerstate enum later
    private Vector2 _dPadInput;     // Stores current dPadInput
    private GlubHook  glubHook;     // Stores a reference to our glub hook for function calling

    /// <summary>
    /// As of now all we're doing is setting our glubHook reference here
    /// </summary>
    public override void _Ready()
    {
        glubHook = GetNode<GlubHook>("GlubHook");   // We will heavily use this glubHook reference
        _inAimMode = false;                         // We don't start out aimed
    }

    /// <summary>
    /// We're going to tweak this to basically be an enum switch func call to the various player motion states / types
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        AimModeCheckAndUpdate();
        SnagDPadInput();

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
        if (Input.IsActionJustPressed("mode_toggle_aim"))
        {
            if (_inAimMode)
            {
                _inAimMode = false;
                glubHook.ToggleAimVisualizer();
            }
            else
            {
                _inAimMode = true;
                this.Position = this.Position.Snapped(_stepSize);    // Need to hide this snap w/ a smooth motion + a little vfx dazzle puff
                Velocity = Vector2.Zero;
                glubHook.ToggleAimVisualizer();
            }
        }

        if (_inAimMode)
        {
            glubHook.VisualizeAim(GetLocalMousePosition());
        }
    }

    /// <summary>
    /// Handling for logic branch where you're in "aim/shoot" mode
    /// delta currently unused, likely will be
    /// </summary>
    private void HandleAimMode(double delta)
    {
        if (Input.IsActionJustPressed("action_fire"))
        {
            if (glubHook.FireHook(GetLocalMousePosition()))
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
            if (glubHook.IsInGrapple())
            {
                if (_dPadInput.Y < 0)
                {
                    // Replace this w/ a call to an ienumerator that makes a smooth motion as opposed to a hop + add input disabling mid smoothmove
                    this.Position = glubHook.GetHookPoint().Snapped(_stepSize);
                    glubHook.DisengageHook();
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
    /// Handling branch for any player movement not under the auspices of aim mode
    /// delta currently unused, likely will be
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
        //gives us a local handle for velocity tweakery
        Vector2 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y += gravity * (float)delta;

        if (_dPadInput != Vector2.Zero)
        {
            velocity.X = _dPadInput.X * _Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _Speed);
        }

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