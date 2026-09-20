using System;
using System.Collections.Generic;
using UnityEngine;

public class SmartAnimationController2 : MonoBehaviour
{
	[Serializable]
	public class SmartAnimationActivator
	{
		public string boolean = "";

		public CheckPeriod period = CheckPeriod.EveryFrame;

		public SmartConditionsList conditions = new SmartConditionsList();
	}

	[Serializable]
	public enum CheckPeriod
	{
		EveryFrame = 1,
		Every10Frames = 2,
		EverySec = 3,
		Every10Sec = 4,
		EveryMin = 5,
		Manual = 200
	}

	public List<SmartAnimationActivator> conditions = new List<SmartAnimationActivator>();

	private float _time_counter;

	private int _frame_counter;

	private Animator _anim;

	public void Start()
	{
		_anim = GetComponent<Animator>();
		if (_anim == null)
		{
			Debug.LogError("Animator not found on " + GetType().Name + ", obj = " + base.name, this);
			base.enabled = false;
		}
	}

	public void Update()
	{
		_frame_counter++;
		_time_counter += Time.deltaTime;
		for (int i = 0; i < conditions.Count; i++)
		{
			bool flag = false;
			SmartAnimationActivator smartAnimationActivator = conditions[i];
			switch (smartAnimationActivator.period)
			{
			case CheckPeriod.EveryFrame:
				flag = true;
				break;
			case CheckPeriod.Every10Frames:
				if (_frame_counter >= 10)
				{
					flag = true;
					_frame_counter = 0;
				}
				break;
			case CheckPeriod.EverySec:
				if (_time_counter >= 1f)
				{
					_time_counter = 0f;
					flag = true;
				}
				break;
			case CheckPeriod.Every10Sec:
				if (_time_counter >= 10f)
				{
					_time_counter = 0f;
					flag = true;
				}
				break;
			case CheckPeriod.EveryMin:
				if (_time_counter >= 60f)
				{
					_time_counter = 0f;
					flag = true;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case CheckPeriod.Manual:
				break;
			}
			if (flag && _anim != null)
			{
				_anim.SetBool(smartAnimationActivator.boolean, smartAnimationActivator.conditions.CheckCondition());
			}
		}
	}
}
