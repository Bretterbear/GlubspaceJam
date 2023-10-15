using Godot;
using System;

public partial class InvertedSpikeAreaController : Area2D
{
	private bool _powered;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		ToggleSpikes();
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

	private void ToggleSpikes()
	{
		var children = GetOverlappingBodies();
		foreach (var child in children)
		{
			if (child is Spike)
			{
				if (!_powered)
				{
					((Spike)child).TurnOffSpike();
				}
				else
				{
					((Spike)child).TurnOnSpike();
				}
			}
		}
	}
}
