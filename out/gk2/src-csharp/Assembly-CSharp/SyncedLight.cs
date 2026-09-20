using UnityEngine;

[ExecuteInEditMode]
public class SyncedLight : MonoBehaviour
{
	public Light light;

	public bool syncRange = true;

	private Light thisLight;

	private Light ThisLight
	{
		get
		{
			if (!thisLight || thisLight == null)
			{
				thisLight = GetComponent<Light>();
			}
			return thisLight;
		}
	}

	private void Update()
	{
		if ((bool)light)
		{
			ThisLight.color = light.color;
			ThisLight.intensity = light.intensity;
			if (ThisLight.type == LightType.Directional)
			{
				light.transform.rotation = ThisLight.transform.rotation;
			}
			else if (syncRange)
			{
				ThisLight.range = light.range;
			}
			ThisLight.bounceIntensity = light.bounceIntensity;
		}
	}
}
