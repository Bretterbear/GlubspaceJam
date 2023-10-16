using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class MovingPlatform : StaticBody2D, IDynamicReceiver
{
	private bool _powered;
	private Texture2D _offTexture;
	private Texture2D _onTexture;
	private bool _inverted;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_offTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Env_Placeholder-LandEnd.png");
	}

	
	public void ProvidePower()
	{
		_powered = true;
		TogglePlatform();
		
	}

	public void StopPower()
	{
		_powered = false;
		TogglePlatform();
	}

	private void TogglePlatform()
	{
		if (!_inverted && _powered || _inverted && !_powered)
		{
			SetCollisionLayerValue(9,false);
			((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
		}
		else
		{
			SetCollisionLayerValue(9,true);
			((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
		}
	}
	public bool IsOn()
	{
		return _powered;
	}

	public void Inverted()
	{
		_inverted = true;
	}
}
