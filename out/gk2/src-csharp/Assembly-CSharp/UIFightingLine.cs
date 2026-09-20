using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFightingLine : MonoBehaviour
{
	public LayoutElement spawnPhase;

	public LayoutElement pausePhase;

	public RectTransform viewportRect;

	public RectTransform capturePointsParent;

	public GameObject overlayProgressObj;

	public GameObject overlayFullObj;

	public RectTransform slider;

	public GameObject slider1;

	public GameObject slider2;

	public Image directionIcon;

	[NonSerialized]
	[HideInInspector]
	public FightingLine fightingLine;

	[NonSerialized]
	[HideInInspector]
	public int lineIndex;

	[NonSerialized]
	[HideInInspector]
	public float secondsAtEndOfLastSpawnPhase = -1f;

	private List<LayoutElement> createdPhases = new List<LayoutElement>();

	private List<UIFightingCaptureIcon> captureIcons = new List<UIFightingCaptureIcon>();

	public IReadOnlyList<UIFightingCaptureIcon> CaptureIcons => captureIcons;

	public RectTransform CapturePointsParent => capturePointsParent;

	public float CapturePointsWidth
	{
		get
		{
			if (!(capturePointsParent != null))
			{
				return 0f;
			}
			return capturePointsParent.sizeDelta.x;
		}
	}

	public void SetDirectionIcon(Sprite sprite)
	{
		if (!(directionIcon == null))
		{
			directionIcon.sprite = sprite;
			directionIcon.gameObject.SetActive(sprite != null);
			directionIcon.SetNativeSize();
		}
	}

	public void Draw(FightingLevelPreset.FightingLineData data, float totalTime, int lineIndex, FightingLine line)
	{
		fightingLine = line;
		this.lineIndex = lineIndex;
		secondsAtEndOfLastSpawnPhase = data.SecondsAtEndOfLastSpawnEnemyPhase();
		int mergedSpawnDuration = 0;
		foreach (FightingPhaseData phase in data.phases)
		{
			if (phase is FightingPhaseSpawnEnemiesData)
			{
				mergedSpawnDuration += phase.duration;
				continue;
			}
			FlushMergedSpawnSegment();
			LayoutElement layoutElement = null;
			if (phase is FightingPhasePauseData)
			{
				layoutElement = pausePhase.Copy(pausePhase.transform.parent, activate: true, "Pause Phase");
			}
			if (layoutElement != null)
			{
				layoutElement.preferredWidth = (float)phase.duration * UIFightingTimelineRendererWidget.TIME_TO_SIZE;
				layoutElement.transform.SetParent(base.transform);
				layoutElement.transform.SetAsLastSibling();
				createdPhases.Add(layoutElement);
			}
		}
		FlushMergedSpawnSegment();
		float totalTime2 = data.TotalTime;
		if ((totalTime - totalTime2).EqualsOrMore(0.001f))
		{
			LayoutElement layoutElement2 = pausePhase.Copy(pausePhase.transform.parent, activate: true, "Pause Phase");
			layoutElement2.preferredWidth = (totalTime - totalTime2) * UIFightingTimelineRendererWidget.TIME_TO_SIZE;
			layoutElement2.transform.SetParent(base.transform);
			layoutElement2.transform.SetAsLastSibling();
		}
		if (lineIndex == 0)
		{
			slider1.gameObject.SetActive(value: true);
			slider2.gameObject.SetActive(value: false);
		}
		else
		{
			slider1.gameObject.SetActive(value: false);
			slider2.gameObject.SetActive(value: true);
		}
		foreach (FightingSector sector in line.sectors)
		{
			UIFightingCaptureIcon elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIFightingCaptureIcon>(capturePointsParent);
			captureIcons.Add(elementFromPool);
			elementFromPool.Draw(sector.point);
		}
		viewportRect.SetAsLastSibling();
		void FlushMergedSpawnSegment()
		{
			if (mergedSpawnDuration > 0)
			{
				LayoutElement layoutElement3 = spawnPhase.Copy(spawnPhase.transform.parent, activate: true, "Spawn Phase");
				layoutElement3.preferredWidth = (float)mergedSpawnDuration * UIFightingTimelineRendererWidget.TIME_TO_SIZE;
				layoutElement3.transform.SetParent(base.transform);
				layoutElement3.transform.SetAsLastSibling();
				createdPhases.Add(layoutElement3);
				mergedSpawnDuration = 0;
			}
		}
	}

	public void Clear()
	{
		foreach (LayoutElement createdPhase in createdPhases)
		{
			UnityEngine.Object.Destroy(createdPhase.gameObject);
		}
		foreach (UIFightingCaptureIcon captureIcon in captureIcons)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(captureIcon);
		}
		captureIcons.Clear();
		createdPhases.Clear();
	}

	private void Awake()
	{
		spawnPhase.gameObject.SetActive(value: false);
		pausePhase.gameObject.SetActive(value: false);
		ResetOverlayFullRectStretch();
		if (overlayFullObj != null)
		{
			overlayFullObj.SetActive(value: false);
		}
	}

	private void OnTransformParentChanged()
	{
		ResetOverlayFullRectStretch();
	}

	private void ResetOverlayFullRectStretch()
	{
		if (!(overlayFullObj == null))
		{
			RectTransform rectTransform = overlayFullObj.transform as RectTransform;
			if (!(rectTransform == null))
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.offsetMin = Vector2.zero;
				rectTransform.offsetMax = Vector2.zero;
			}
		}
	}
}
