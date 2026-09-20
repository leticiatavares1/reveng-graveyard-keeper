using System;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyPlatformDependentElement : MonoBehaviour
{
	[Header("Show when:")]
	public bool controllerGamepad = true;

	public bool controllerMouse = true;

	public bool platformPC = true;

	public bool platformAnyConsole = true;

	public bool platformXbox = true;

	public bool platformSwitch = true;

	public bool platformPS = true;

	[Space]
	[SerializeField]
	protected bool useAwakeForInit;

	public bool UseAwakeForInit => useAwakeForInit;

	public bool IsActive { get; protected set; }

	public virtual bool Init()
	{
		if (!base.enabled)
		{
			return false;
		}
		bool flag = true;
		flag = LazyAPI.Platform.GetPlatformId() switch
		{
			LazyPlatform.PC => flag & platformPC, 
			LazyPlatform.Xbox => flag & (platformXbox || platformAnyConsole), 
			LazyPlatform.PlayStation => flag & (platformPS || platformAnyConsole), 
			LazyPlatform.NintendoSwitch => flag & (platformSwitch || platformAnyConsole), 
			_ => throw new NotImplementedException(), 
		};
		flag = ((LazyAPI.Platform.GetPlatformId() != 0) ? (flag & controllerGamepad) : ((!LazyInput.IsGamepadActive) ? (flag & controllerMouse) : (flag & controllerGamepad)));
		base.gameObject.SetActive(flag);
		IsActive = flag;
		return flag;
	}

	protected virtual void Awake()
	{
		if (useAwakeForInit)
		{
			Init();
		}
	}
}
