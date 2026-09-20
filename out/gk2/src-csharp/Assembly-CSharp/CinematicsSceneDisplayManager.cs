using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CinematicsSceneDisplayManager : LazySingleton<CinematicsSceneDisplayManager>
{
	private const string CINEMATICS_SCENE_PREFAB_PATH = "Assets/_Scenes/Cinematics";

	[SerializeField]
	private float fadeDuration = 0.5f;

	private AsyncOperationHandle<GameObject> prefabHandle;

	private Action onCompleted;

	private CinematicsScene cinematicsScene;

	private bool bloomWasActive;

	public void DisplayCinematicsScene(string id, Action onCompleted = null)
	{
		if (string.IsNullOrEmpty(id))
		{
			Debug.LogError("Failed to display cinematics scene: id is null or empty");
			return;
		}
		string text = id.Split('_')[1];
		string prefabPath = "Assets/_Scenes/Cinematics/" + text + "/" + id + ".prefab";
		this.onCompleted = onCompleted;
		UIFade uiFade = LazyUI.Get<UIFade>();
		uiFade.FadeIn(fadeDuration, delegate
		{
			prefabHandle = Addressables.InstantiateAsync(prefabPath, base.gameObject.transform, instantiateInWorldSpace: false, trackHandle: false);
			prefabHandle.Completed += delegate(AsyncOperationHandle<GameObject> handle)
			{
				if (handle.Status != AsyncOperationStatus.Succeeded)
				{
					Debug.LogError("Failed to load prefab: " + prefabPath);
					uiFade.FadeOut(fadeDuration, delegate
					{
						onCompleted?.Invoke();
						onCompleted = null;
					});
				}
				else
				{
					GameObject result = handle.Result;
					if (!result.TryGetComponent<CinematicsScene>(out cinematicsScene))
					{
						Debug.LogError("Failed to get CinematicsScene component from prefab: " + prefabPath);
						uiFade.FadeOut(fadeDuration, delegate
						{
							onCompleted?.Invoke();
							onCompleted = null;
						});
					}
					else
					{
						result.transform.localPosition = Vector3.zero;
						MainGame.PlayerController.SetControlTakenType(TakenControlType.ByCinematics, isEnabled: false);
						LazyUI.Get<HUD>().SetDisableState(HudStateType.CinematicsScene, isEnabled: false);
						CameraSystem.Instance.SetActiveCamera(CameraType.Cinematics);
						CameraSystem.Instance.ActiveCameraController.SetTarget(cinematicsScene.CameraTarget);
						WeatherSystem.Instance.AudioMixerStateController.Push(AudioMixerSnapshotLayer.Cinematics);
						WeatherSystem.Instance.SetPauseState(WeatherSystemPauseFlag.Cinematics, isPaused: true);
						SetBloomActive(active: false);
						cinematicsScene.Director.stopped += OnDirectorStopped;
						uiFade.FadeOut(fadeDuration, cinematicsScene.Play);
					}
				}
			};
		});
	}

	private void OnDirectorStopped(PlayableDirector director)
	{
		director.stopped -= OnDirectorStopped;
		LazyUI.Get<UIFade>().Fade(fadeDuration, null, delegate
		{
			MainGame.PlayerController.SetControlTakenType(TakenControlType.ByCinematics, isEnabled: true);
			LazyUI.Get<HUD>().SetDisableState(HudStateType.CinematicsScene, isEnabled: true);
			CameraSystem.Instance.ActiveCameraController.SetTarget(null);
			CameraSystem.Instance.SetActiveCamera(CameraType.Main);
			SetBloomActive(bloomWasActive);
			onCompleted?.Invoke();
			onCompleted = null;
			Addressables.Release(prefabHandle);
			WeatherSystem.Instance.AudioMixerStateController.Pop(AudioMixerSnapshotLayer.Cinematics);
			WeatherSystem.Instance.SetPauseState(WeatherSystemPauseFlag.Cinematics, isPaused: false);
		});
	}

	private void SetBloomActive(bool active)
	{
		if (CameraSystem.Instance.MainCamera.PostProcessVolume.profile.TryGetSettings<Bloom>(out var outSetting))
		{
			if (!active)
			{
				bloomWasActive = outSetting.active;
			}
			outSetting.active = active;
		}
	}
}
