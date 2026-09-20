using FlowCanvas;
using LazyBearTechnology;
using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-1)]
public class WeatherComponent : MonoBehaviour
{
	public enum AnimationType
	{
		None,
		FadeIn,
		FadeOut
	}

	[Range(0f, 1f)]
	public float intensity;

	public string sound = "";

	public bool hasOnEnableFS;

	[SerializeField]
	private FlowScript onEnableFS;

	public bool hasOnDisableFS;

	[SerializeField]
	private FlowScript onDisableFS;

	private SoundHandler soundHandler;

	[Space(10f)]
	[SerializeField]
	private ControllableParameterList parameters = new ControllableParameterList();

	private AnimationType animating;

	private float targetIntensity;

	public AnimationType Animating => animating;

	private void Awake()
	{
		parameters.Init();
		if (!WeatherSystem.Instance.components.ContainsKey(base.name))
		{
			WeatherSystem.Instance.components.Add(base.name, this);
		}
	}

	private void OnDisable()
	{
		parameters.OnDisable();
	}

	public void ClearState()
	{
		intensity = 0f;
		targetIntensity = 0f;
		animating = AnimationType.None;
		soundHandler?.Stop();
		soundHandler = null;
		OnIntensityChanged();
	}

	public void OnIntensityChanged()
	{
		parameters.UpdateParameters(intensity, this);
		if (!Application.isPlaying)
		{
			return;
		}
		if (!string.IsNullOrEmpty(sound))
		{
			if (intensity.EqualsTo(0f))
			{
				soundHandler?.Pause();
			}
			else
			{
				if (soundHandler == null)
				{
					soundHandler = LazyAudio.Play(sound);
				}
				else if (soundHandler.IsPaused)
				{
					soundHandler.UnPause();
				}
				soundHandler.SetVolume(intensity);
			}
		}
		if (hasOnDisableFS && onDisableFS != null && intensity == 0f)
		{
			GlobalScriptsManager.RunFlowScript(onDisableFS, null);
		}
	}

	public void FadeIn()
	{
		base.gameObject.SetActive(value: true);
		targetIntensity = 1f;
		animating = AnimationType.FadeIn;
		if (hasOnEnableFS && onEnableFS != null)
		{
			GlobalScriptsManager.RunFlowScript(onEnableFS, null);
		}
	}

	public void FadeOut()
	{
		base.gameObject.SetActive(value: true);
		targetIntensity = 0f;
		animating = AnimationType.FadeOut;
	}

	public void FadeOutIfActive()
	{
		if (intensity > 0f)
		{
			FadeOut();
		}
	}

	public void FadeInAndFadeOutOthers()
	{
		foreach (WeatherComponent value in WeatherSystem.Instance.components.Values)
		{
			value.FadeOutIfActive();
		}
		FadeIn();
	}

	public void CustomUpdate()
	{
		if (animating != 0)
		{
			float num = Mathf.Sign(targetIntensity - intensity);
			intensity += num * Time.deltaTime / WeatherSystem.Instance.fadeTime;
			if ((num > 0f && intensity >= targetIntensity) || (num < 0f && intensity <= targetIntensity))
			{
				intensity = targetIntensity;
				animating = AnimationType.None;
			}
			OnIntensityChanged();
		}
	}

	public T GetControllableParameterOfType<T>() where T : ControllableParameter
	{
		foreach (ControllableParameter parameter in parameters.parameters)
		{
			if (parameter is T result)
			{
				return result;
			}
		}
		return null;
	}
}
