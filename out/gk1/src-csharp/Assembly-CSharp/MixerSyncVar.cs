using System;

[AttributeUsage(AttributeTargets.Field)]
public class MixerSyncVar : Attribute
{
	public float updateInterval;

	public MixerSyncVar(double newUpdateInterval = 1.0)
	{
		updateInterval = (float)newUpdateInterval;
	}
}
