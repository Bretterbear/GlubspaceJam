using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class DynamicsController : Node2D, IDynamicProvider, IDynamicReceiver
{
	private List<IDynamicProvider> _powerProviders;

	private List<IDynamicReceiver> _powerReceivers;

	private IDynamicReceiver _parentDynamic;

	private bool _powered;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetupCircuit();
		Debug.WriteLine(_powered);
		
	}

	private void SetupCircuit()
	{
		var childNodes = GetChildren();
		_powerProviders = new List<IDynamicProvider>();
		_powerReceivers = new List<IDynamicReceiver>();
		foreach (var child in childNodes)
		{
			if (child is IDynamicProvider)
				_powerProviders.Add((IDynamicProvider)child);
			else if (child is IDynamicReceiver)
				_powerReceivers.Add((IDynamicReceiver)child);
		}

		var parent = GetParent();
		if (parent is IDynamicReceiver)
			_parentDynamic = (IDynamicReceiver)parent;
	}
	

	public bool IsProvidingPower()
	{
		return _powered;
	}

	public void ProvidePower()
	{
		_powered = true;
		foreach (var provider in _powerProviders)
		{
			if (!provider.IsProvidingPower())
			{
				_powered = false;
				break;
			}
		}

		if (_powered)
		{
			foreach (var node in _powerReceivers)
			{
				node.ProvidePower();
			}
			if(_parentDynamic != null)
				_parentDynamic.ProvidePower();
		}
	}

	public void StopPower()
	{
		foreach (var provider in _powerProviders)
		{
			if (!provider.IsProvidingPower())
			{
				_powered = false;
				foreach (var receiver in _powerReceivers)
				{
					receiver.StopPower();
				}

				break;
			}
		}
	}

	public bool IsOn()
	{
		return _powered;
	}
}
