using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class Inverter : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetParent<IDynamicReceiver>().Inverted();
	}
}
