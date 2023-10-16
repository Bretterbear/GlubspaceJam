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
		throw new NotImplementedException();
	}

	private void ToggleSpikes()
	{
		
	}
}
