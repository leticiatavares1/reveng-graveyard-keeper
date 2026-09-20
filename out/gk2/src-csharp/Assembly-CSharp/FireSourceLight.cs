using UnityEngine;

[ExecuteInEditMode]
public class FireSourceLight : MonoBehaviour
{
	[Header("Position change")]
	public float posChangePeriod = 0.1f;

	public Vector3 posRandomDelta = Vector3.zero;

	private float curPosPeriod;

	private Vector3 curPosDirection = Vector3.zero;

	[Space]
	[Header("Color change")]
	public float lightChangePeriod = 0.03f;

	public Color[] colors;

	private float curLightPeriod;

	private int prevLightN = -1;

	private Light pointLight;

	private LightFaker lightFaker;

	private Light PointLight => pointLight ?? (pointLight = GetComponent<Light>());

	private LightFaker LightFaker
	{
		get
		{
			LightFaker obj = this.lightFaker;
			if ((object)obj == null)
			{
				LightFaker obj2 = GetComponent<LightFaker>() ?? GetComponentInParent<LightFaker>();
				LightFaker lightFaker = obj2;
				this.lightFaker = obj2;
				obj = lightFaker;
			}
			return obj;
		}
	}

	private void Update()
	{
		if (!PointLight)
		{
			return;
		}
		curPosPeriod -= Time.deltaTime;
		if (curPosPeriod < 0f)
		{
			Vector3 vector = new Vector3(Random.Range(0f - posRandomDelta.x, posRandomDelta.x), Random.Range(0f - posRandomDelta.y, posRandomDelta.y), Random.Range(0f - posRandomDelta.z, posRandomDelta.z));
			curPosPeriod = posChangePeriod;
			curPosDirection = (vector - base.transform.localPosition) / posChangePeriod;
		}
		else
		{
			base.transform.localPosition += curPosDirection * Time.deltaTime;
		}
		SyncLightFakerLocalOffset();
		curLightPeriod -= Time.deltaTime;
		if (!(curLightPeriod < 0f))
		{
			return;
		}
		if (colors != null && colors.Length > 1)
		{
			int num;
			do
			{
				num = Random.Range(0, colors.Length);
			}
			while (num == prevLightN);
			if (PointLight != null && SwitchLightPolicy.AllowRealPointLights)
			{
				PointLight.color = colors[num];
			}
			if (SwitchLightPolicy.UseLightRT && (bool)LightFaker)
			{
				float intensity = ((PointLight != null) ? PointLight.intensity : 1f);
				float range = ((PointLight != null) ? PointLight.range : 8f);
				LightFaker.ApplyExternalState(colors[num], intensity, range);
			}
			prevLightN = num;
		}
		curLightPeriod = lightChangePeriod;
	}

	private void SyncLightFakerLocalOffset()
	{
		if (SwitchLightPolicy.UseLightRT && (bool)LightFaker)
		{
			LightFaker.SetLocalVisualOffset(base.transform.position - LightFaker.transform.position);
		}
	}
}
