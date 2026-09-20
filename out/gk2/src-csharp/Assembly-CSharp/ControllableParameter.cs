using System;

[Serializable]
public abstract class ControllableParameter
{
	public virtual void Init()
	{
	}

	public virtual void OnDisable()
	{
	}

	public abstract void UpdateParameter(float v, WeatherComponent weatherComponent);

	protected void Editor_ApplyValueChange()
	{
	}
}
