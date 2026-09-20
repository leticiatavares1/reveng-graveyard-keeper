using System;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	[Serializable]
	public class LightFlickerStep
	{
		public int len;

		public int len2;

		public Color c;
	}

	public int fps = 30;

	public List<LightFlickerStep> steps = new List<LightFlickerStep>();

	private float lastTime;

	private float dt;

	private int curFrameCounter;

	private int curStep;

	private int curStepLen;

	private Light lightSource;

	private LightFaker lightFaker;

	public float transTime;

	private bool isFirst = true;

	private bool isInTrans;

	private float transT0;

	private Color c0;

	private Color c1;

	public void Start()
	{
		lastTime = Time.realtimeSinceStartup;
		dt = 0f;
		curFrameCounter = 0;
		curStep = 0;
		lightSource = GetComponent<Light>();
		lightFaker = GetComponent<LightFaker>();
		isFirst = true;
		GenerateStep();
	}

	private void GenerateStep()
	{
		int count = steps.Count;
		if (count != 0)
		{
			if (curStep >= count)
			{
				curStep = 0;
			}
			LightFlickerStep lightFlickerStep = steps[curStep];
			if (lightFlickerStep.len2 == 0)
			{
				curStepLen = lightFlickerStep.len;
			}
			else
			{
				curStepLen = UnityEngine.Random.Range(lightFlickerStep.len, lightFlickerStep.len2);
			}
			curStepLen += Mathf.RoundToInt(transTime * (float)fps);
			transT0 = 0f;
			if (isFirst)
			{
				ApplyColor(lightFlickerStep.c);
				isFirst = false;
			}
			else
			{
				isInTrans = true;
				c1 = lightFlickerStep.c;
				c0 = ((lightSource != null) ? lightSource.color : c1);
			}
		}
	}

	private void ApplyColor(Color color)
	{
		if (lightSource != null && SwitchLightPolicy.AllowRealPointLights)
		{
			lightSource.color = color;
		}
		if ((bool)lightFaker)
		{
			float intensity = ((lightSource != null) ? lightSource.intensity : 1f);
			float range = ((lightSource != null) ? lightSource.range : 8f);
			lightFaker.ApplyExternalState(color, intensity, range);
		}
	}

	public void Update()
	{
		if (steps.Count == 0)
		{
			return;
		}
		float num = Time.realtimeSinceStartup - lastTime;
		transT0 += Time.deltaTime;
		if (isInTrans)
		{
			float t = transT0 / transTime;
			Color color = new Color(Mathf.Lerp(c0.r, c1.r, t), Mathf.Lerp(c0.g, c1.g, t), Mathf.Lerp(c0.b, c1.b, t));
			ApplyColor(color);
			if (transT0 > transTime)
			{
				isInTrans = false;
				isFirst = false;
			}
		}
		lastTime = Time.realtimeSinceStartup;
		dt += num;
		float num2 = 1f / (float)fps;
		if (dt > num2)
		{
			dt -= num2;
			curFrameCounter++;
			if (curStep >= steps.Count)
			{
				curStep = 0;
			}
			else if (curFrameCounter >= curStepLen)
			{
				curStep++;
				curFrameCounter = 0;
				GenerateStep();
			}
		}
	}
}
