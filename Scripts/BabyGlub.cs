using Godot;
using System;

public partial class BabyGlub : CharacterBody2D
{
    // ---------- Editor Variable Declarations ---------- //
    [Export] private const float _glubHopTime = 0.75f;
    [Export] private const float _glubHopVariability = .25f;
    [Export] private const float _glubHopVelocity = -300f;
    [Export] private const float _glubWobbleDistance = 5f;
    [Export] private const float _glubWobbleSpeed = 10f;
    [Export] private const float _glubSpeed = 300.0f;

	// -------- Non-Editor Variable Declarations -------- //
	private Vector2 _wiggleCore;								// stores self position for jiggling purposes in grapple line mode
    private Vector2 _targetPosition;							// becomes non-zero after player gets tweaked by
	private bool _inGrappleMode = false;						// Probably replace htis w/ a nice enum
	private bool _needsToJump = false;							// Stores whether this glub feels the need to jump 
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private Timer _timer;
    private float jiggleTime = 0;

    public override void _Ready()
    {
		AddToGroup("glubs");
		_timer = GetNode<Timer>("Timer_GlubHop");
		_timer.Timeout += glubbyHopper;
		_timer.Start();
    }

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

    private void JiggleCluster(double delta)
    {
        jiggleTime += (float) delta;

        // Consider adding a wobble speed to modify that triggle jiggle
        float xOffset = Mathf.Sin(jiggleTime*_glubWobbleSpeed) * _glubWobbleDistance; 
        float yOffset = Mathf.Cos(jiggleTime*_glubWobbleSpeed) * _glubWobbleDistance;

        GlobalPosition = _wiggleCore + new Vector2(xOffset, yOffset);
    }

    private void ClusterNormally(double delta)
	{
        Vector2 direction = (_targetPosition - GlobalPosition).Normalized();
        Vector2 velocity = Velocity;

        // Add the gravity.d
        if (!IsOnFloor())
        {
            velocity.Y += gravity * (float)delta;
        }
        else if (_needsToJump)
        {
            velocity.Y = _glubHopVelocity;
            _needsToJump = false;
        }

        if (direction != Vector2.Zero)
        {
            velocity.X = direction.X * _glubSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _glubSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void glubbyHopper()
    {
        _needsToJump = true;
        _timer.WaitTime = GD.RandRange(_glubHopTime * (1 - _glubHopVariability), _glubHopTime * (1 + _glubHopVariability));
        _timer.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    private void _OnUpdateGlubBoidTarget(Vector2 playerPosition)
	{
		_targetPosition = playerPosition;
	}

	private void _OnUpdateGlubGrappleState(bool playerIsGrappling)
	{
		_inGrappleMode = playerIsGrappling;

		if (_inGrappleMode)
		{
			//GD.Print("We grapple glubs meow");
            _wiggleCore = GlobalPosition;
            jiggleTime = GD.RandRange(0, 10);

			_timer.Stop();
		}
		else
		{
			//GD.Print("We ain't no grapple glubs");
			_timer.Start();
		}
	}
}
