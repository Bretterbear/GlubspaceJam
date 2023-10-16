using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class Lever : Area2D, IDynamicProvider, IDynamicReceiver
{
	private IDynamicReceiver _parentDynamic;

	private IDynamicProvider _childDynamic;

	private bool _isBasePower;

	private bool _isOn;

	private bool _powered;

	private Texture2D _offTexture;

	private Texture2D _onTexture;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetupLeverDynamics();
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Placeholder Art/SwitchOn.png");
		_offTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
	}


	private void SetupLeverDynamics()
	{
		_parentDynamic = GetParent<IDynamicReceiver>();
		var children = GetChildren();
		foreach (var child in children)
		{
			if (child is DynamicsController)
			{
				_childDynamic = (IDynamicProvider)child;
				_isBasePower = false;
				if (_childDynamic.IsProvidingPower())
				{
					ProvidePower();
					Debug.WriteLine("Provide Power");
				}
				else
				{
					StopPower();
					Debug.WriteLine("Not Providing Power");
				}
				break;
			}

			if (_childDynamic == null)
			{
				_isBasePower = true;
				ProvidePower();
			}
		}
	}
	public bool IsProvidingPower()
	{
		return _isOn;
	}

	public void ProvidePower()
	{
		_powered = true;
	}

	public void StopPower()
	{
		_powered = false;
	}

	public bool IsOn()
	{
		return _powered;
	}

	private void ToggleLever(Node2D body)
	{
		if (_powered && !_isOn)
		{
			_isOn = true;
			_parentDynamic.ProvidePower();
			((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
		}
			
	}
}
