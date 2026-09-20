using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Playables;

public class CinematicsScene : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private PlayableDirector director;

	[SerializeField]
	private Transform cameraTarget;

	[SerializeField]
	private Vector3 cinematicViewScaleX1 = Vector3.one;

	[SerializeField]
	private Vector3 cinematicViewScaleX2 = Vector3.one * 2f;

	private CinematicsCommonObject commonObject;

	public Transform CameraTarget => cameraTarget;

	public Animator Animator => animator;

	public PlayableDirector Director => director;

	private void Awake()
	{
		animator.enabled = false;
		director.playOnAwake = false;
		director.initialTime = 0.0;
		commonObject = GetComponentInChildren<CinematicsCommonObject>(includeInactive: true);
		ApplyCinematicViewScaleByCurrentResolution();
	}

	public void Play()
	{
		animator.runtimeAnimatorController = null;
		animator.enabled = true;
		TextStyleComponent componentInChildren = GetComponentInChildren<TextStyleComponent>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			componentInChildren.ApplyStyle();
		}
		animator.Rebind();
		animator.Update(0f);
		director.Stop();
		director.time = 0.0;
		director.RebuildGraph();
		director.Play();
	}

	private void ApplyCinematicViewScaleByCurrentResolution()
	{
		bool flag = PlatformFeatureConfig.Get(GamePlatformResolver.Current).platform == GamePlatform.Switch;
		bool flag2 = (flag && ResolutionConfig.currentResolution.AppliedHeight <= 720) || (ResolutionConfig.currentResolution != null && ResolutionConfig.currentResolution.UseMainMenuScaleX2);
		base.transform.localScale = (flag2 ? cinematicViewScaleX2 : cinematicViewScaleX1);
		if (!(commonObject == null))
		{
			commonObject.SetSwitchVariant(flag);
			Transform center = commonObject.GetCenter(flag);
			if (center != null)
			{
				cameraTarget = center;
			}
		}
	}
}
