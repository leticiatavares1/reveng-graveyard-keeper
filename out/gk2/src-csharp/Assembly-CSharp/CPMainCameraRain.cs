using System;

[Serializable]
public class CPMainCameraRain : CPMainCameraFX<CameraFilterPack_Atmosphere_Rain>
{
	protected override void SetIntensity(float value)
	{
		base.FX.Fade = value;
	}
}
