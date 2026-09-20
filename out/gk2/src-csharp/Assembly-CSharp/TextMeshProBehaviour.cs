using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class TextMeshProBehaviour : PlayableBehaviour
{
	public string localeId;

	public Color color = Color.white;

	public float fadeDuration = 0.5f;

	public AnimationCurve fadeInEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve fadeOutEase = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	private bool isInitialized;

	private TextMeshPro textMeshPro;

	private CinematicsTextWidget uiWidget;

	private CinematicsTextWidgetData uiWidgetData;

	public float GetFadeMultiplier(Playable playable)
	{
		if (fadeDuration <= 0f)
		{
			return 1f;
		}
		double time = playable.GetTime();
		double duration = playable.GetDuration();
		if (time < (double)fadeDuration)
		{
			return fadeInEase.Evaluate(Mathf.Clamp01((float)(time / (double)fadeDuration)));
		}
		if (duration > (double)fadeDuration && time > duration - (double)fadeDuration)
		{
			return fadeOutEase.Evaluate(Mathf.Clamp01((float)((time - duration + (double)fadeDuration) / (double)fadeDuration)));
		}
		return 1f;
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (info.effectiveWeight <= 0f)
		{
			return;
		}
		textMeshPro = playerData as TextMeshPro;
		if (!textMeshPro)
		{
			return;
		}
		Color fadedColor = color;
		fadedColor.a = color.a * GetFadeMultiplier(playable) * info.effectiveWeight;
		if (Application.isPlaying)
		{
			CinematicsCommonObject componentInParent = textMeshPro.GetComponentInParent<CinematicsCommonObject>(includeInactive: true);
			if (componentInParent != null && !componentInParent.IsTargetLabel(textMeshPro))
			{
				CinematicsCommonObject.HideWorldLabel(textMeshPro);
			}
			else
			{
				ProcessFrameUi(fadedColor);
			}
		}
		else
		{
			ProcessFrameWorldPreview(fadedColor);
		}
	}

	public override void OnGraphStop(Playable playable)
	{
		isInitialized = false;
		base.OnGraphStop(playable);
		CleanupUiWidget();
		HideBoundWorldText();
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		if (isInitialized)
		{
			isInitialized = false;
			CleanupUiWidget();
			HideBoundWorldText();
		}
	}

	private void ProcessFrameUi(Color fadedColor)
	{
		if (EnsureUiWidget())
		{
			HideBoundWorldText();
			if (!isInitialized)
			{
				string obj = LLBase.L(localeId);
				uiWidgetData.OnTextChanged?.Invoke(obj);
				uiWidgetData.OnVisibleChanged?.Invoke(obj: true);
				isInitialized = true;
			}
			SyncWorldLayoutToData();
			uiWidgetData.Color = fadedColor;
			uiWidgetData.OnColorChanged?.Invoke(fadedColor);
		}
	}

	private void ProcessFrameWorldPreview(Color fadedColor)
	{
		if (!isInitialized)
		{
			textMeshPro.text = LLBase.L(localeId);
			textMeshPro.gameObject.SetActive(value: true);
			isInitialized = true;
		}
		textMeshPro.color = fadedColor;
	}

	private bool EnsureUiWidget()
	{
		if (uiWidget != null)
		{
			return true;
		}
		if (LazySingleton<LazyWidgetPrefabContainer>.Instance == null || GUIElements.Instance == null)
		{
			return false;
		}
		LazyWidgetBase prefabFromDataType;
		try
		{
			prefabFromDataType = LazyWidgetPrefabContainer.GetPrefabFromDataType<CinematicsTextWidgetData>();
		}
		catch (Exception ex)
		{
			Debug.LogError("[TextMeshProBehaviour] CinematicsTextWidget prefab is missing: " + ex.Message);
			return false;
		}
		RectTransform rectTransform = textMeshPro.rectTransform;
		uiWidgetData = new CinematicsTextWidgetData(GetWorldPosition(rectTransform), GetWorldSize(rectTransform))
		{
			ShowBackground = IsOverImageLabel(textMeshPro),
			Pivot = rectTransform.pivot
		};
		uiWidget = prefabFromDataType.Copy(GUIElements.Instance.Root) as CinematicsTextWidget;
		if (uiWidget == null)
		{
			Debug.LogError("[TextMeshProBehaviour] Prefab is not CinematicsTextWidget");
			return false;
		}
		if (uiWidget.TryGetComponent<CanvasGroup>(out var component))
		{
			component.ignoreParentGroups = true;
		}
		uiWidget.Init();
		uiWidget.Draw(uiWidgetData);
		return true;
	}

	private void SyncWorldLayoutToData()
	{
		RectTransform rectTransform = textMeshPro.rectTransform;
		uiWidgetData.Position = GetWorldPosition(rectTransform);
		uiWidgetData.Size = GetWorldSize(rectTransform);
		uiWidgetData.Pivot = rectTransform.pivot;
	}

	private static bool IsOverImageLabel(TextMeshPro boundText)
	{
		CinematicsCommonObject componentInParent = boundText.GetComponentInParent<CinematicsCommonObject>(includeInactive: true);
		if (componentInParent != null)
		{
			return componentInParent.labelOverImage == boundText;
		}
		return false;
	}

	private static Vector2 GetWorldPosition(RectTransform anchor)
	{
		Vector3 position = anchor.position;
		return new Vector2(position.x, position.y);
	}

	private static Vector2 GetWorldSize(RectTransform anchor)
	{
		Vector2 size = anchor.rect.size;
		Vector3 lossyScale = anchor.lossyScale;
		return new Vector2(Mathf.Abs(size.x * lossyScale.x), Mathf.Abs(size.y * lossyScale.y));
	}

	private void HideBoundWorldText()
	{
		CinematicsCommonObject.HideWorldLabel(textMeshPro);
	}

	private void CleanupUiWidget()
	{
		if (!(uiWidget == null))
		{
			uiWidgetData?.OnVisibleChanged?.Invoke(obj: false);
			uiWidget.Hide();
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(uiWidget.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(uiWidget.gameObject);
			}
			uiWidget = null;
			uiWidgetData = null;
		}
	}

	private void TryPlayVoiceOver()
	{
		if (!string.IsNullOrEmpty(localeId) && LazyAudio.IsInitialized && VoiceOverSettings.IsEnabled)
		{
			LazyAudio.VoiceOverPlayer.Play(localeId);
		}
	}

	private void TryStopVoiceOver()
	{
		if (Application.isPlaying && LazyAudio.IsInitialized)
		{
			LazyAudio.VoiceOverPlayer.Stop();
		}
	}
}
