using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

public class UIBasicBubble : MonoBehaviour
{
	public enum BubbleCornerDirection
	{
		LeftUp,
		LeftDown,
		RightUp,
		RightDown,
		BottomCenter,
		TopCenter
	}

	public enum ForceCornerPosition
	{
		Auto,
		Right,
		Left,
		BottomCenter,
		BottomRight,
		BottomLeft,
		TopCenter,
		TopRight,
		TopLeft
	}

	[SerializeField]
	protected List<UIBubbleCorner> corners;

	[SerializeField]
	private RectTransform rootTransform;

	[SerializeField]
	private bool flipBackgroundH;

	[SerializeField]
	private bool useSmoothPositioning;

	[SerializeField]
	private float instantUpdateOffset = 400f;

	[SerializeField]
	private float animationTime = 0.2f;

	protected UIBubbleCorner currentCorner;

	[NonSerialized]
	protected BubbleCornerDirection pickedCorner;

	[NonSerialized]
	protected ForceCornerPosition forcedCornerPosition;

	public RectTransform RootTransform
	{
		get
		{
			if (rootTransform == null)
			{
				rootTransform = GetComponent<RectTransform>();
			}
			return rootTransform;
		}
	}

	protected virtual void UpdatePositionAndCorner(Vector3 screenPos, ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto)
	{
		Bounds screenBounds = LazyUI.GetScreenBounds();
		Vector3 zero = Vector3.zero;
		Vector2 vector = RootTransform.rect.size * RootTransform.localScale;
		forcedCornerPosition = forceCornerPosition;
		switch (forcedCornerPosition)
		{
		case ForceCornerPosition.BottomCenter:
		{
			float num3 = screenPos.x - vector.x * 0.5f * LazyUI.ScaleFactor;
			float num4 = screenPos.x + vector.x * 0.5f * LazyUI.ScaleFactor;
			pickedCorner = BubbleCornerDirection.BottomCenter;
			if (num3 < 0f)
			{
				zero.x = 0f - num3;
			}
			else if (num4 > screenBounds.max.x)
			{
				zero.x = 0f - (num4 - screenBounds.max.x);
			}
			break;
		}
		case ForceCornerPosition.BottomLeft:
			pickedCorner = BubbleCornerDirection.LeftDown;
			break;
		case ForceCornerPosition.BottomRight:
			pickedCorner = BubbleCornerDirection.RightDown;
			break;
		case ForceCornerPosition.TopCenter:
		{
			float num = screenPos.x - vector.x * 0.5f * LazyUI.ScaleFactor;
			float num2 = screenPos.x + vector.x * 0.5f * LazyUI.ScaleFactor;
			pickedCorner = BubbleCornerDirection.TopCenter;
			if (num < 0f)
			{
				zero.x = 0f - num;
			}
			else if (num2 > screenBounds.max.x)
			{
				zero.x = 0f - (num2 - screenBounds.max.x);
			}
			break;
		}
		case ForceCornerPosition.TopRight:
			pickedCorner = BubbleCornerDirection.RightUp;
			break;
		case ForceCornerPosition.TopLeft:
			pickedCorner = BubbleCornerDirection.LeftUp;
			break;
		default:
		{
			Vector2 vector2 = vector;
			vector2.Scale(Vector2.one * LazyUI.ScaleFactor);
			vector2 += new Vector2(0f, Mathf.Abs(corners[1].rectTransform.localPosition.y)) * LazyUI.ScaleFactor;
			bool flag = screenPos.y + vector2.y < screenBounds.max.y;
			bool flag2 = screenPos.x + vector2.x < screenBounds.max.x;
			switch (forcedCornerPosition)
			{
			case ForceCornerPosition.Auto:
				pickedCorner = ((!flag) ? ((!flag2) ? BubbleCornerDirection.RightUp : BubbleCornerDirection.LeftUp) : (flag2 ? BubbleCornerDirection.LeftDown : BubbleCornerDirection.RightDown));
				break;
			case ForceCornerPosition.Right:
				pickedCorner = (flag ? BubbleCornerDirection.RightDown : BubbleCornerDirection.RightUp);
				break;
			case ForceCornerPosition.Left:
				pickedCorner = (flag ? BubbleCornerDirection.LeftDown : BubbleCornerDirection.LeftUp);
				break;
			default:
				throw new NotImplementedException();
			}
			break;
		}
		}
		ApplyCornerIndex(screenPos, zero, pickedCorner);
	}

	protected void ApplyCornerIndex(Vector3 screenPos, Vector3 offset, BubbleCornerDirection corner)
	{
		UIBubbleCorner uIBubbleCorner = currentCorner;
		Vector3 vector = screenPos - (corners[(int)corner].rectTransform.position - RootTransform.position) + offset;
		if (useSmoothPositioning)
		{
			if ((vector - base.transform.position).sqrMagnitude > instantUpdateOffset * LazyUI.ScaleFactor)
			{
				base.transform.position = vector;
			}
			else
			{
				base.transform.DOMove(vector, animationTime);
			}
		}
		else
		{
			base.transform.position = vector;
		}
		for (int i = 0; i < corners.Count; i++)
		{
			UIBubbleCorner uIBubbleCorner2 = corners[i];
			uIBubbleCorner2.gameObject.SetActive(i == (int)corner);
			if (i == (int)corner)
			{
				currentCorner = uIBubbleCorner2;
			}
		}
		if (uIBubbleCorner != currentCorner)
		{
			OnCornerChanged();
		}
	}

	protected virtual void OnCornerChanged()
	{
	}

	protected virtual void UpdatePositionAndCorner(Vector3 screenPos, Vector3 offsetForDownPosition)
	{
		UpdatePositionAndCorner(screenPos);
		if (pickedCorner == BubbleCornerDirection.LeftUp || pickedCorner == BubbleCornerDirection.RightUp)
		{
			base.transform.position += offsetForDownPosition;
		}
	}

	public bool IsBubbleOverstepsScreen(Vector2 screenPosition, ForceCornerPosition cornerPosition)
	{
		Bounds screenBounds = LazyUI.GetScreenBounds();
		int directionByCorner = (int)GetDirectionByCorner(cornerPosition);
		Rect worldRect = RootTransform.GetWorldRect();
		worldRect.position = screenPosition - (Vector2)(corners[directionByCorner].rectTransform.position - RootTransform.position) + worldRect.size * (Vector2.up - RootTransform.pivot);
		bool num = worldRect.xMax < screenBounds.max.x;
		bool flag = worldRect.xMin > screenBounds.min.x;
		bool flag2 = worldRect.yMin < screenBounds.max.y;
		bool flag3 = worldRect.yMin - worldRect.height > screenBounds.min.y;
		return !(num && flag && flag2 && flag3);
	}

	public bool IsBubbleOverstepsScreen()
	{
		Bounds screenBounds = LazyUI.GetScreenBounds();
		Rect worldRect = RootTransform.GetWorldRect();
		bool num = worldRect.xMax < screenBounds.max.x;
		bool flag = worldRect.xMin > screenBounds.min.x;
		bool flag2 = worldRect.yMin < screenBounds.max.y;
		bool flag3 = worldRect.yMin - worldRect.height > screenBounds.min.y;
		return !(num && flag && flag2 && flag3);
	}

	public float GetEffectiveSpaceOfBubbleVisibility()
	{
		Bounds screenBounds = LazyUI.GetScreenBounds();
		Rect worldRect = RootTransform.GetWorldRect();
		float num = Mathf.Abs(worldRect.xMax - worldRect.xMin) * Mathf.Abs(worldRect.yMin - worldRect.yMax);
		float num2 = ((float)Step(screenBounds.max.x, worldRect.xMax) * Mathf.Abs(screenBounds.max.x - worldRect.xMax) + (float)Step(screenBounds.min.x, worldRect.xMin) * Mathf.Abs(screenBounds.min.x - worldRect.xMin)) * Mathf.Abs(worldRect.yMin - worldRect.yMax) + ((float)Step(screenBounds.max.y, worldRect.yMax) * Mathf.Abs(screenBounds.max.y - worldRect.yMax) + (float)Step(screenBounds.min.y, worldRect.yMin) * Mathf.Abs(screenBounds.min.y - worldRect.yMin)) * Mathf.Abs(worldRect.xMin - worldRect.xMax);
		return num - num2 / num;
	}

	public bool IsBubbleOverlapsWithAnotherRects(Vector2 screenPosition, Rect[] anotherRects, ForceCornerPosition cornerPosition)
	{
		int directionByCorner = (int)GetDirectionByCorner(cornerPosition);
		Rect worldRect = RootTransform.GetWorldRect();
		worldRect.position = screenPosition - (Vector2)(corners[directionByCorner].rectTransform.position - RootTransform.position) + worldRect.size * (Vector2.up - RootTransform.pivot);
		return worldRect.LCS_IsOverlapsWithAnyOthers(anotherRects);
	}

	private BubbleCornerDirection GetDirectionByCorner(ForceCornerPosition cornerPosition)
	{
		return cornerPosition switch
		{
			ForceCornerPosition.BottomCenter => BubbleCornerDirection.BottomCenter, 
			ForceCornerPosition.BottomLeft => BubbleCornerDirection.LeftDown, 
			ForceCornerPosition.BottomRight => BubbleCornerDirection.RightDown, 
			ForceCornerPosition.TopCenter => BubbleCornerDirection.TopCenter, 
			ForceCornerPosition.TopRight => BubbleCornerDirection.RightUp, 
			ForceCornerPosition.TopLeft => BubbleCornerDirection.LeftUp, 
			_ => BubbleCornerDirection.LeftDown, 
		};
	}

	private int Step(float a, float x)
	{
		if (!(x >= a))
		{
			return 0;
		}
		return 1;
	}
}
