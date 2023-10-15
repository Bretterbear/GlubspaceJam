using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class SpikeAreaController : Area2D, IDynamicReceiver
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

	private void ToggleSpikes()
	{
		Debug.WriteLine("ToggleSpikes");
		var children = GetOverlappingAreas();
		Debug.WriteLine(children.Count);
		foreach (var child in children)
		{
			if (child is Spike)
			{
				if (_powered)
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
