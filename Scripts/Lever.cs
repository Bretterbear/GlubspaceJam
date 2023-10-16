using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class Lever : Area2D, IDynamicProvider, IDynamicReceiver
{
	private IDynamicReceiver _parentDynamic;

	private IDynamicProvider _childDynamic;

	private bool _isBasePower;

	private bool _isFlipped;

	private bool _powered;
	private bool _isOn;

	private Texture2D _offTexture;

	private Texture2D _onTexture;

	private bool _inverted;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetupLeverDynamics();
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Placeholder Art/SwitchOn.png");
		_offTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		if(!(GetParent() is IDynamicReceiver))
		{
			DynamicsSetup();
		}
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

	public void Inverted()
	{
		Debug.WriteLine("Inverted");
		_inverted = true;
		ResolveLever();
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
		ResolveLever();
	}

	public void Synchronize(bool power)
	{
		if(power)
			if (!_inverted)
			{
				_isFlipped = true;
				_isOn = true;
				((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
			}
			else
			{
				_isFlipped = false;
				_isOn = true;
				((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
			}
		else
		{
			if (!_inverted)
			{
				_isFlipped = false;
				_isOn = false;
				((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
			}
			else
			{
				_isFlipped = true;
				_isOn = false;
				((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
			}
		}
	}
	private void ResolveLever()
	{
		if (_isFlipped && !_inverted || !_isFlipped && _inverted)
		{
			_isOn = true;
			_parentDynamic.ProvidePower();
			((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
		}
		else
		{
			_isOn = false;
			_parentDynamic.StopPower();
			((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
		}
	}
	private void ToggleLever(Node2D body)
	{
		if (_powered)
		{
			if (_isFlipped)
			{
				_isFlipped = false;
			}
			else
			{
				_isFlipped = true;
			}
		}
		else
		{
			_isFlipped = false;
		}
		ResolveLever();
			
	}
}
