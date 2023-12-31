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
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Dynamics Art/PlatformOn.png");
		if(!(GetParent() is IDynamicReceiver))
		{
			DynamicsSetup();
		}
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
			((Sprite2D)GetNode("Sprite2D")).Modulate = new Color(0, 0, 0, 1f);
		}
		else
		{
			SetCollisionLayerValue(9,true);
			((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
			((Sprite2D)GetNode("Sprite2D")).Modulate = new Color(0, 0, 0, .5f);
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

	public void DynamicsSetup()
	{
		var children = GetChildren();
		foreach (var child in children)
		{
			if (child is IDynamicReceiver)
			{
				((IDynamicReceiver)child).DynamicsSetup();
			}
			
			if (child is Inverter)
			{
				_inverted = true;
			}
		}
		TogglePlatform();
	}
}
