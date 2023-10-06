using Godot;
using System;

public partial class BabyGlub : CharacterBody2D
{
    // ---------- Editor Variable Declarations ---------- //
    [Export] private const float        _glubHopTime =   0.75f;     // Center of glub hop time distribution
    [Export] private const float _glubHopVariability =   0.25f;     // Percentage variation in glub hop time
    [Export] private const float    _glubHopVelocity = -300.0f;     // How powerfully the glub jumps
    [Export] private const float _glubWobbleDistance =    5.0f;     // How far to jiggle in grapple mode
    [Export] private const float    _glubWobbleSpeed =   10.0f;     // Speed of jiggling in grapple mode
    [Export] private const float          _glubSpeed =  300.0f;     // Glub movement speed

    // -------- Reference Variable Declarations  -------- //
    private Timer _refGlubHopTimer;                                 // Reference storage for our pseudorandom hop trigger timer

    // ---------- State Variable Declarations  ---------- //
    private Vector2 _targetPosition;	                            // Points towards player / controller glub (when glub is in flock
    private Vector2     _wiggleCore;                                // Stores self position for jiggling purposes in grapple line mode
    private float        jiggleTime;                                // Stores time in jiggle mode for random jiggling behavior in grapple line
    private bool       _needsToJump = false;                        // Stores whether this glub feels the need to jump 
    private bool     _inGrappleMode = false;                        // Probably replace htis w/ a nice enum


    // ------------- Constants Declarations ------------- //
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    /// <summary>
    /// Baby glub setup
    /// </summary>
    public override void _Ready()
    {
        // Add this glub into the signals group for broadcast purposes
		AddToGroup("glubs");

        // Set up glub hop timer inside prefab
		_refGlubHopTimer = GetNode<Timer>("Timer_GlubHop");
		_refGlubHopTimer.Timeout += glubHopReset;
		_refGlubHopTimer.Start();
    }

    /// <summary>
    /// Core call loop
    /// </summary>
    public override void _PhysicsProcess(double delta)
	{
		if (_inGrappleMode)
		{
            JiggleCluster(delta);
		}
		else
		{
			ClusterNormally(delta);
		}
    }

    /// <summary>
    /// This will be a call to join the glub line to a target, but for now they just freeze and jiggle
    /// </summary>
    private void JiggleCluster(double delta)
    {
        jiggleTime += (float) delta;

        // Consider adding a wobble speed to modify that triggle jiggle
        float xOffset = Mathf.Sin(jiggleTime*_glubWobbleSpeed) * _glubWobbleDistance; 
        float yOffset = Mathf.Cos(jiggleTime*_glubWobbleSpeed) * _glubWobbleDistance;

        GlobalPosition = _wiggleCore + new Vector2(xOffset, yOffset);
    }

    /// <summary>
    /// Normal glub motion mode, will need to add some boid logic here to make it functional and fun looking
    /// </summary>
    private void ClusterNormally(double delta)
	{
        // Set our direction & grab a velocity variable to manipulate
        Vector2 direction = (_targetPosition - GlobalPosition).Normalized();
        Vector2 velocity = Velocity;

        // Add gravity for non-grounded glubs & jumps if need be
        if (!IsOnFloor())
        {
            velocity.Y += gravity * (float)delta;
        }
        else if (_needsToJump)
        {
            velocity.Y = _glubHopVelocity;
            _needsToJump = false;
        }

        // Set directionality of the glub
        if (direction != Vector2.Zero)
        {
            velocity.X = direction.X * _glubSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _glubSpeed);
        }

        // Toss those changes into the CharacterBody2D velocity and call the move function
        Velocity = velocity;
        MoveAndSlide();
    }

    /// <summary>
    /// Signal linked function to reset glub hop timer + have this glub jump in the next physics frame
    /// </summary>
    private void glubHopReset()
    {
        // Set the bool to trigger a jump in the next physics frame
        _needsToJump = true;
        
        // Randomize and restart the hop timer
        _refGlubHopTimer.WaitTime = GD.RandRange(_glubHopTime * (1 - _glubHopVariability), _glubHopTime * (1 + _glubHopVariability));
        _refGlubHopTimer.Start();
    }

    /// <summary>
    /// Signal linked function to reset player position reference for the glub (for homing purposes)
    /// </summary>
    private void _OnUpdateGlubBoidTarget(Vector2 playerPosition)
	{
		_targetPosition = playerPosition;
	}

    /// <summary>
    /// Signal linked function to toggle glub grapple state
    /// </summary>
	private void _OnUpdateGlubGrappleState(bool playerIsGrappling)
	{
		_inGrappleMode = playerIsGrappling;

		if (_inGrappleMode)
		{
            // Set state variables for non-coordinated jiggling behavior
            _wiggleCore = GlobalPosition;
            jiggleTime = GD.RandRange(0, 10);

            // Stop the hop timer (since they can't hop during a jiggle
			_refGlubHopTimer.Stop();
		}
		else
		{
            // Restart the hop timer now that they're done jiggling
			_refGlubHopTimer.Start();
		}
	}
}
