using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFightingTimelineRendererWidget : LazyWidget<UIFightingTimelineRendererData>
{
	public static readonly float TIME_TO_SIZE = 10f;

	[Header("Viewport Setup")]
	[SerializeField]
	private RectTransform linesContainer;

	[SerializeField]
	private RectTransform rightGroup;

	[SerializeField]
	private float rightGroupOffsetFromIcons;

	[SerializeField]
	private UIFightingLine fightingLinePrefab;

	[SerializeField]
	private Sprite[] directionIcons;

	public TextMeshProUGUI timeLabel;

	public GameObject spaceBetweenTimeAndShields;

	public TextMeshProUGUI shieldsLabel;

	public TextStyle shieldsTextStyleRed;

	public TextStyle shieldsTextStyleGreen;

	private List<UIFightingLine> drawnFightingLines = new List<UIFightingLine>();

	private List<FightingCapturePoint> subscribedCapturePoints = new List<FightingCapturePoint>();

	public override void Init()
	{
		base.Init();
		if (fightingLinePrefab != null)
		{
			fightingLinePrefab.gameObject.SetActive(value: false);
		}
	}

	public override void Hide()
	{
		UnsubscribeFromCapturePoints();
		if (data != null)
		{
			data.UnsubscribeFromProcessor();
			data.OnProgressChanged -= HandleProgressNormalized;
		}
		foreach (UIFightingLine drawnFightingLine in drawnFightingLines)
		{
			if (!(drawnFightingLine == null))
			{
				drawnFightingLine.Clear();
				UnityEngine.Object.Destroy(drawnFightingLine.gameObject);
			}
		}
		drawnFightingLines.Clear();
		base.Hide();
	}

	protected override void SetData(UIFightingTimelineRendererData data)
	{
		base.SetData(data);
		if (data?.Preset == null || data.CurrentLevel == null || fightingLinePrefab == null || linesContainer == null)
		{
			return;
		}
		for (int i = 0; i < data.Preset.lines.Count; i++)
		{
			if (i < data.CurrentLevel.FightingLines.Count)
			{
				FightingLevelPreset.FightingLineData fightingLineData = data.Preset.lines[i];
				UIFightingLine uIFightingLine = fightingLinePrefab.Copy();
				uIFightingLine.Draw(fightingLineData, data.TotalTime, i, data.CurrentLevel.FightingLines[i]);
				drawnFightingLines.Add(uIFightingLine);
				uIFightingLine.transform.SetParent(linesContainer);
				uIFightingLine.transform.SetAsLastSibling();
				uIFightingLine.gameObject.SetActive(value: true);
			}
		}
		linesContainer.anchoredPosition = new Vector2(0f, -8f);
		data.OnProgressChanged += HandleProgressNormalized;
		foreach (UIFightingLine drawnFightingLine in drawnFightingLines)
		{
			drawnFightingLine.slider.anchoredPosition = Vector2.zero;
		}
		UpdateDirectionIcons();
		SubscribeToCapturePoints();
	}

	public override void Redraw()
	{
		base.Redraw();
		RefreshLinesContainer();
		UpdateRightGroup();
	}

	private void UpdateRightGroup()
	{
		UpdateTimeLabel();
		UpdateShieldsLabel();
		RefreshLinesContainer();
		UpdateRightGroupPosition();
	}

	private void HandleProgressNormalized(float curProgress)
	{
		UpdateTimeLabel();
		foreach (UIFightingLine drawnFightingLine in drawnFightingLines)
		{
			float x = drawnFightingLine.viewportRect.sizeDelta.x;
			bool num = drawnFightingLine.fightingLine != null && drawnFightingLine.fightingLine.AreAllSpawnZonesDisabled;
			float num2 = curProgress * data.TotalTime;
			bool flag = drawnFightingLine.secondsAtEndOfLastSpawnPhase >= 0f && num2 >= drawnFightingLine.secondsAtEndOfLastSpawnPhase;
			drawnFightingLine.slider.anchoredPosition = new Vector2(Mathf.Lerp(0f, x, curProgress), 0f);
			if (num || flag)
			{
				drawnFightingLine.overlayProgressObj.SetActive(value: false);
				drawnFightingLine.overlayFullObj.SetActive(value: true);
			}
			else
			{
				drawnFightingLine.overlayProgressObj.SetActive(value: true);
				drawnFightingLine.overlayFullObj.SetActive(value: false);
			}
		}
	}

	private Sprite GetDirectionIcon(int lineIdx)
	{
		if (directionIcons == null || lineIdx < 0 || lineIdx >= directionIcons.Length)
		{
			return null;
		}
		return directionIcons[lineIdx];
	}

	private void UpdateDirectionIcons()
	{
		for (int i = 0; i < drawnFightingLines.Count; i++)
		{
			if (data?.CurrentLevel == null || i >= data.CurrentLevel.FightingLines.Count)
			{
				drawnFightingLines[i].SetDirectionIcon(null);
			}
			else
			{
				drawnFightingLines[i].SetDirectionIcon(GetDirectionIcon(i));
			}
		}
	}

	private void UpdateTimeLabel()
	{
		if (!(timeLabel == null) && !(data?.Processor == null))
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, data.TotalTime - data.Processor.CurrentProgress));
			timeLabel.text = string.Format("{0}{1:00}:{2:00}", "icon_time".FontIcon(), (int)timeSpan.TotalMinutes, timeSpan.Seconds);
		}
	}

	private void UpdateShieldsLabel()
	{
		if (shieldsLabel == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (UIFightingLine drawnFightingLine in drawnFightingLines)
		{
			foreach (UIFightingCaptureIcon captureIcon in drawnFightingLine.CaptureIcons)
			{
				if (!(captureIcon.CapturePoint == null))
				{
					num++;
					if (captureIcon.CapturePoint.OwnedByTeam == LazyConsts.Fighting.TeamType.Player)
					{
						num2++;
					}
				}
			}
		}
		if (num <= 0)
		{
			shieldsLabel.gameObject.SetActive(value: false);
			spaceBetweenTimeAndShields.SetActive(value: false);
			return;
		}
		shieldsLabel.gameObject.SetActive(value: true);
		spaceBetweenTimeAndShields.SetActive(value: true);
		TextStyle textStyle = ((num2 < num) ? shieldsTextStyleRed : shieldsTextStyleGreen);
		string arg = ((textStyle != null) ? textStyle.ApplyStyleToString(num2.ToString()) : num2.ToString());
		shieldsLabel.text = string.Format("{0}{1}/{2}", "icon-flag_secondary-blue-timer".FontIcon(), arg, num);
	}

	private void SubscribeToCapturePoints()
	{
		UnsubscribeFromCapturePoints();
		foreach (UIFightingLine drawnFightingLine in drawnFightingLines)
		{
			foreach (UIFightingCaptureIcon captureIcon in drawnFightingLine.CaptureIcons)
			{
				if (!(captureIcon.CapturePoint == null) && !subscribedCapturePoints.Contains(captureIcon.CapturePoint))
				{
					captureIcon.CapturePoint.OnCapturedByTeam += HandleCapturePointCaptured;
					subscribedCapturePoints.Add(captureIcon.CapturePoint);
				}
			}
		}
	}

	private void UnsubscribeFromCapturePoints()
	{
		foreach (FightingCapturePoint subscribedCapturePoint in subscribedCapturePoints)
		{
			if (subscribedCapturePoint != null)
			{
				subscribedCapturePoint.OnCapturedByTeam -= HandleCapturePointCaptured;
			}
		}
		subscribedCapturePoints.Clear();
	}

	private void HandleCapturePointCaptured(FightingCapturePoint capturePoint)
	{
		UpdateShieldsLabel();
	}

	private void UpdateRightGroupPosition()
	{
		if (rightGroup == null || drawnFightingLines.Count == 0)
		{
			return;
		}
		float num = 0f;
		foreach (UIFightingLine drawnFightingLine in drawnFightingLines)
		{
			if (!(drawnFightingLine.CapturePointsParent == null))
			{
				num = Mathf.Max(num, drawnFightingLine.CapturePointsWidth);
			}
		}
		rightGroup.anchoredPosition = new Vector2(num + rightGroupOffsetFromIcons, 0f);
	}

	private void RefreshLinesContainer()
	{
		if (linesContainer != null)
		{
			linesContainer.RefreshContentFitter();
		}
	}

	protected override void TestDraw()
	{
	}
}
