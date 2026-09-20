using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class LightFlicker : MonoBehaviour
{
	public int fps = 30;

	public List<LightFlickerStep> steps = new List<LightFlickerStep>();

	private float lastTime;

	private float dt;

	private int curFrameCounter;

	private int curStep;

	private int curStepLen;

	private Light curLight;

	public float transTime;

	private bool isFirst = true;

	private bool isTransing;

	private float transT0;

	private Color c0;

	private Color c1;

	public void Start()
	{
		lastTime = Time.realtimeSinceStartup;
		dt = 0f;
		curFrameCounter = 0;
		curStep = 0;
		curLight = GetComponent<Light>();
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
				curStepLen = Random.Range(lightFlickerStep.len, lightFlickerStep.len2);
			}
			curStepLen += Mathf.RoundToInt(transTime * (float)fps);
			transT0 = 0f;
			if (isFirst)
			{
				curLight.color = lightFlickerStep.c;
				isFirst = false;
			}
			else
			{
				isTransing = true;
				c1 = lightFlickerStep.c;
				c0 = curLight.color;
			}
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
		if (isTransing)
		{
			float t = transT0 / transTime;
			curLight.color = new Color(Mathf.Lerp(c0.r, c1.r, t), Mathf.Lerp(c0.g, c1.g, t), Mathf.Lerp(c0.b, c1.b, t));
			if (transT0 > transTime)
			{
				isTransing = false;
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
