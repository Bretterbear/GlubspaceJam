using Godot;
using System;
using System.Diagnostics;

public partial class GlubHook : Node2D
{
	// ---------- Editor Variable Declarations ---------- //
	[Export] private float _hookLength = 300f;  // Controls hook length, this will become a variable of glub count later
	//[Export] private

    // -------- Non-Editor Variable Declarations -------- //
    private Player	_player;					// Stores a reference to player for hook updating
	private bool	_isActiveGrapple;			// Stores hook state
	private Vector2 _grappleHookPoint;			// Stores hook location

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_player = GetParent<Player>();	// Set link to parent for communication
	}

    // Raycast event is called inside physics b/c raycasting space can be locked during the _input process
    public override void _PhysicsProcess(double delta)
    {
		if (Input.IsActionJustPressed("action_fire"))
		{
			// Brett-CONSIDER adding vfx / sfx feedback on click call, esp since FireHook might fail, or may shoot ahead of / past the click point

			// Ignore input if we've already got an active grapple going
			if (!_isActiveGrapple)
			{
				FireHook();
			}
		}
    }

    /// <summary>
    /// Tries to fire a grappling hook from player to mouse click position
    /// </summary>
    /// <returns>Whether a grapple has successfully found a target </returns>
    public bool FireHook()
	{
		DisengageHook();    // This shouldn't ever actually have to set a state, but just-in-case!

		// need to add the commented out code after testing
		Vector2 targetPosition = GetViewport().GetMousePosition();

        // Room for expansion - a vfx ping on mouse pin location, and a ghostly / desat raycast at length if miss

        // Do a raycast to see if we hit a physics object in range
        // Brett-CONSIDER maybe we don't want to be able to grapple down? Or maybe we do? For now ignore
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(this.GlobalPosition, targetPosition, _player.CollisionMask, new Godot.Collections.Array<Rid> { _player.GetRid() });
		var result = spaceState.IntersectRay(query);

		//spaceState.IntersectRay

		if (result != null)
		{
			//_isActiveGrapple = true;
			Debug.WriteLine(result.ToString());
		}

        // if raycast hit in range so, set a vector2 w/ grapple location


        Debug.WriteLine("FIRED HOOK");
        return true;
    }

	/// <summary>
	/// Resets grappling variables for player state
	/// </summary>
	public void DisengageHook()
	{
		_isActiveGrapple = false;			// Reset grapple state
		_grappleHookPoint = Vector2.Zero;	// Reset grapple hook point

	}
}
