using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyAutoScrollLabel : MonoBehaviour
{
	private enum Direction
	{
		LeftToRight = 1,
		RightToLeft = -1,
		BottomToTop = 1,
		TopToBottom = -1
	}

	[SerializeField]
	private float baseSpeed = 25f;

	[SerializeField]
	private float additionalDistance;

	[SerializeField]
	[Space]
	private Vector2 defaultPosition;

	[Space]
	[SerializeField]
	private RectTransform.Axis axis;

	[SerializeField]
	[Space]
	private bool isPaused;

	private TextMeshProUGUI label;

	private RectTransform mask;

	private RectTransform rectTransform;

	private Direction direction = Direction.RightToLeft;

	private Vector2 speed;

	private Vector3[] selfCorners = new Vector3[4];

	private Vector3[] labelCorners = new Vector3[4];

	public bool IsPaused
	{
		get
		{
			return isPaused;
		}
		set
		{
			isPaused = value;
		}
	}

	public TextMeshProUGUI Label
	{
		get
		{
			if (label == null)
			{
				label = Mask.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			}
			return label;
		}
	}

	private RectTransform Mask
	{
		get
		{
			if (mask == null)
			{
				mask = base.transform.GetChild(0).GetComponent<RectTransform>();
			}
			return mask;
		}
	}

	public RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	public string Text
	{
		get
		{
			return Label.text;
		}
		set
		{
			Label.text = value;
		}
	}

	private float DeltaTime => LazyTime.GetUnscaledDeltaTime;

	public void Update()
	{
		SetSpeed();
		if (!isPaused)
		{
			if (IsScrollNecessary())
			{
				RectTransform.GetWorldCorners(selfCorners);
				Label.rectTransform.GetWorldCorners(labelCorners);
				if (axis == RectTransform.Axis.Horizontal)
				{
					if (direction == Direction.RightToLeft)
					{
						if (labelCorners[2].x <= selfCorners[2].x - additionalDistance * LazyUI.ScaleFactor)
						{
							direction = Direction.LeftToRight;
						}
					}
					else if (labelCorners[1].x >= selfCorners[1].x + additionalDistance * LazyUI.ScaleFactor)
					{
						direction = Direction.RightToLeft;
					}
				}
				else if (direction == Direction.RightToLeft)
				{
					if (labelCorners[1].y <= selfCorners[1].y - additionalDistance * LazyUI.ScaleFactor)
					{
						direction = Direction.LeftToRight;
					}
				}
				else if (labelCorners[0].y >= selfCorners[0].y + additionalDistance * LazyUI.ScaleFactor)
				{
					direction = Direction.RightToLeft;
				}
				Scroll();
			}
			else
			{
				SetDefaultPosition();
				SetDefaultDirection();
			}
			return;
		}
		if (axis == RectTransform.Axis.Horizontal)
		{
			if (Mathf.Abs(Label.rectTransform.anchoredPosition.x - defaultPosition.x) <= Mathf.Abs(speed.x * DeltaTime))
			{
				Label.rectTransform.anchoredPosition = defaultPosition;
			}
			else
			{
				direction = ((!(Label.rectTransform.anchoredPosition.x > defaultPosition.x)) ? Direction.LeftToRight : Direction.RightToLeft);
				Scroll();
			}
		}
		else if (Mathf.Abs(Label.rectTransform.anchoredPosition.y - defaultPosition.y) <= Mathf.Abs(speed.y * DeltaTime))
		{
			SetDefaultPosition();
		}
		else
		{
			direction = ((!(Label.rectTransform.anchoredPosition.y > defaultPosition.y)) ? Direction.LeftToRight : Direction.RightToLeft);
			Scroll();
		}
		SetDefaultDirection();
	}

	private void SetSpeed()
	{
		if (axis == RectTransform.Axis.Horizontal)
		{
			speed.x = baseSpeed * (Label.rectTransform.sizeDelta.x / RectTransform.sizeDelta.x);
		}
		else
		{
			speed.y = baseSpeed * (Label.rectTransform.sizeDelta.y / RectTransform.sizeDelta.y);
		}
	}

	private bool IsScrollNecessary()
	{
		if (axis == RectTransform.Axis.Horizontal)
		{
			return Label.rectTransform.sizeDelta.x > RectTransform.sizeDelta.x;
		}
		return Label.rectTransform.sizeDelta.y > RectTransform.sizeDelta.y;
	}

	private void Scroll()
	{
		Label.rectTransform.anchoredPosition += speed * ((float)direction * DeltaTime);
	}

	private void SetDefaultPosition()
	{
		Label.rectTransform.anchoredPosition = defaultPosition;
	}

	private void SetDefaultDirection()
	{
		direction = ((axis == RectTransform.Axis.Horizontal) ? Direction.RightToLeft : Direction.RightToLeft);
	}
}
