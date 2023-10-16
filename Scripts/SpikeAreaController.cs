using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class SpikeAreaController : Area2D, IDynamicReceiver
{
	private bool _powered;

	private bool _inverted;
	private bool _intialized;
	// Called when the node enters the scene tree for the first time.

	public override void _PhysicsProcess(double delta)
	{
		while (!_intialized)
		{
			var collisions = GetOverlappingAreas().Count;
			if (collisions > 0)
			{
				_intialized = true;
				ToggleSpikes();
			}
		}
	}
	public void ProvidePower()
	{
		_powered = true;
		ToggleSpikes();
	}

	public void StopPower()
	{
		_powered = false;
		ToggleSpikes();
	}

	public bool IsOn()
	{
		return _powered;
	}

	public void Inverted()
	{
		_inverted = true;
		ToggleSpikes();
	}

	private void ToggleSpikes()
	{
		if (!_inverted && _powered || _inverted && !_powered)
		{
			var children = GetOverlappingAreas();
			foreach (var child in children)
			{
				if (child is Spike)
				{
					((Spike)child).TurnOffSpike();
				}
			}
		}
		else
		{
			var children = GetOverlappingAreas();
			foreach (var child in children)
			{
				if (child is Spike)
				{
					((Spike)child).TurnOnSpike();
				}
			}
		}
	}
}
