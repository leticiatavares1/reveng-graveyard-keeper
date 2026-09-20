using System;
using UnityEngine;

[Serializable]
public abstract class CPMainCameraFX<T> : ControllableParameter where T : MonoBehaviour
{
	private T fx;

	protected T FX
	{
		get
		{
			if (fx == null)
			{
				fx = CameraSystem.Instance.MainCamera.GetComponent<T>();
				if (fx == null)
				{
					Debug.LogError($"Couldn't find {typeof(T)} component on MainCamera.");
				}
			}
			return fx;
		}
	}

	protected abstract void SetIntensity(float value);

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		FX.enabled = v > 0f;
		if (v > 0f)
		{
			SetIntensity(v);
		}
	}
}
