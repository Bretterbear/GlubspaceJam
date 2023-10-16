using Godot;
using System;
using System.ComponentModel;
using GlubspaceJam.Scripts;

public partial class LeverSynchronizer : Node2D, IDynamicReceiver, IDynamicProvider
{
	private bool _inverted;

	private bool _powered;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(!(GetParent() is IDynamicReceiver))
		{
			DynamicsSetup();
		}
	}
	
	public void ProvidePower()
	{
		_powered = true;
		ResolveSynchronizer();
	}

	public void StopPower()
	{
		_powered = false;
		ResolveSynchronizer();
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
				throw new WarningException("Inversion Behavior Redundent, place inverter on child levers");
			}
		}
		
	}

	private void ResolveSynchronizer()
	{
		var children = GetChildren();
		if (_powered)
		{
			foreach (var child in children)
			{
				if (child is Lever)
				{
					((Lever)child).Synchronize(true);
				}
			}
			if (GetParent() is IDynamicReceiver)
			{
				((IDynamicReceiver)GetParent()).ProvidePower();
			}
		}
		else
		{
			foreach (var child in children)
			{
				if (child is Lever)
				{
					((Lever)child).Synchronize(false);
				}
			}
			if (GetParent() is IDynamicReceiver)
			{
				((IDynamicReceiver)GetParent()).StopPower();
			}
		}

		
	}

	public bool IsProvidingPower()
	{
		return _powered;
	}
}
