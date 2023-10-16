using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class InvertedSpikeAreaController : Area2D, IDynamicReceiver
{
	private bool _powered;

	private bool _intialized;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		
	}

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
					((Spike)child).TurnOnSpike();
				}
				else
				{
					((Spike)child).TurnOffSpike();
				}
			}
		}
	}
}
