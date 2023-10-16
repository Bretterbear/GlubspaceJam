using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class MovingPlatform : StaticBody2D, IDynamicReceiver
{
	private bool _powered;
	private Texture2D _offTexture;
	private Texture2D _onTexture;

	private PlatformPoweredPosition _poweredPosition;

	private PlatformUnPoweredPosition _unPoweredPosition;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlatformSetup();
		_offTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Env_Placeholder-LandEnd.png");
    
		if(!(GetParent() is IDynamicReceiver))

		{
			if (child is PlatformPoweredPosition)
				_poweredPosition = (PlatformPoweredPosition)child;
			else if (child is PlatformUnPoweredPosition)
				_unPoweredPosition = (PlatformUnPoweredPosition)child;
		}

		if (_poweredPosition == null || _unPoweredPosition == null)
		{
			throw new MissingMemberException("Missing either powered or unpowered child scene");
		}

		GlobalPosition = _unPoweredPosition.GlobalPosition;
	}
	public void ProvidePower()
	{
		_powered = true;
		GlobalPosition = _poweredPosition.GlobalPosition;
		((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
	}

	public void StopPower()
	{
		_powered = false;
		GlobalPosition = _unPoweredPosition.GlobalPosition;
		((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
	}

	public bool IsOn()
	{
		return _powered;
	}
}
