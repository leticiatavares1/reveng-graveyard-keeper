using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;

public class MapVirtualCursor : BaseVirtualCursor
{
	private static readonly Vector3[] cornersBuffer = new Vector3[4];

	[SerializeField]
	private RectTransform cursorRectTransform;

	private Vector2 initialCursorSizeDelta;

	private Tweener moveTween;

	private Tweener sizeTween;

	private float animationDuration = 0.3f;

	private Ease animationEase = Ease.OutQuart;

	private RectTransform previousRectTransform;

	private RectTransform boundsRectTransform;

	private Vector3 currentPos;

	private Vector2 desiredMovementDelta;

	private bool isMoveTweenActive;

	private bool isSizeTweenActive;

	public Vector2 DesiredMovementDelta => desiredMovementDelta;

	public float CurrentFrameSpeed => GetSpeed();

	public override void CalculateBounds()
	{
	}

	public void SetBoundsRectTransform(RectTransform boundsRectTransform)
	{
		this.boundsRectTransform = boundsRectTransform;
	}

	public void SetPosition(Vector3 position)
	{
		ResetMoveAnimationState();
		currentPos = position;
		base.transform.position = currentPos;
		if (cursorRectTransform != null)
		{
			cursorRectTransform.position = currentPos;
		}
	}

	public void DoAnimationTo(RectTransform rectTransform)
	{
		if (cursorRectTransform == null)
		{
			ResetAnimationState();
			return;
		}
		previousRectTransform = rectTransform;
		if (isMoveTweenActive)
		{
			moveTween?.Kill();
		}
		if (isSizeTweenActive)
		{
			sizeTween?.Kill();
		}
		Vector3 endValue = ((rectTransform != null) ? rectTransform.position : currentPos);
		Vector2 endValue2 = ((rectTransform != null) ? rectTransform.sizeDelta : initialCursorSizeDelta);
		moveTween = cursorRectTransform.DOMove(endValue, animationDuration).SetEase(animationEase).OnComplete(delegate
		{
			isMoveTweenActive = false;
			if (rectTransform == null)
			{
				previousRectTransform = null;
			}
		});
		sizeTween = cursorRectTransform.DOSizeDelta(endValue2, animationDuration).SetEase(animationEase).OnComplete(delegate
		{
			isSizeTweenActive = false;
		});
		isMoveTweenActive = true;
		isSizeTweenActive = true;
	}

	private void ResetAnimationState()
	{
		moveTween?.Kill();
		sizeTween?.Kill();
		moveTween = null;
		sizeTween = null;
		previousRectTransform = null;
		isMoveTweenActive = false;
		isSizeTweenActive = false;
	}

	private void ResetMoveAnimationState()
	{
		moveTween?.Kill();
		moveTween = null;
		previousRectTransform = null;
		isMoveTweenActive = false;
	}

	protected override float GetSpeed()
	{
		return currentSpeed * LazyUI.ScaleFactor * Time.deltaTime;
	}

	protected override void Update()
	{
		Vector2 direction = LazyInput.GetDirection();
		AccelerateSpeed(direction != Vector2.zero);
		desiredMovementDelta = GetSpeed() * direction;
		if (previousRectTransform != null && isMoveTweenActive && (previousRectTransform.position - base.transform.position).sqrMagnitude > 0.001f)
		{
			moveTween.ChangeEndValue(previousRectTransform.position, snapStartValue: true);
		}
		Vector2 targetPosition = (Vector2)base.transform.position + GetSpeed() * direction;
		Vector2 cursorHalfSizeWorld = GetCursorHalfSizeWorld();
		targetPosition = ClampWithCursorSize(targetPosition, cursorHalfSizeWorld);
		currentPos = new Vector3(targetPosition.x, targetPosition.y, zPosition);
		base.transform.position = currentPos;
		if (!(cursorRectTransform == null) && (!IsTweening() || !(previousRectTransform != null)))
		{
			cursorRectTransform.position = ((previousRectTransform != null) ? previousRectTransform.position : currentPos);
		}
	}

	private bool IsTweening()
	{
		if (!isMoveTweenActive)
		{
			return isSizeTweenActive;
		}
		return true;
	}

	private Vector2 ClampWithCursorSize(Vector2 targetPosition, Vector2 halfCursorSizeWorld)
	{
		if (boundsRectTransform == null)
		{
			return targetPosition;
		}
		(Vector2 bottomLeft, Vector2 topUp) sides = GetSides();
		Vector2 item = sides.bottomLeft;
		Vector2 item2 = sides.topUp;
		float num = item.x + halfCursorSizeWorld.x;
		float num2 = item2.x - halfCursorSizeWorld.x;
		float num3 = item.y + halfCursorSizeWorld.y;
		float num4 = item2.y - halfCursorSizeWorld.y;
		float x = ((num <= num2) ? Mathf.Clamp(targetPosition.x, num, num2) : ((item.x + item2.x) * 0.5f));
		float y = ((num3 <= num4) ? Mathf.Clamp(targetPosition.y, num3, num4) : ((item.y + item2.y) * 0.5f));
		return new Vector2(x, y);
	}

	private Vector2 GetCursorHalfSizeWorld()
	{
		if (cursorRectTransform == null)
		{
			return Vector2.zero;
		}
		cursorRectTransform.GetWorldCorners(cornersBuffer);
		float num = Mathf.Abs(cornersBuffer[3].x - cornersBuffer[0].x);
		float num2 = Mathf.Abs(cornersBuffer[1].y - cornersBuffer[0].y);
		return new Vector2(num * 0.5f, num2 * 0.5f);
	}

	protected override void Awake()
	{
		base.Awake();
		if (cursorRectTransform != null)
		{
			initialCursorSizeDelta = cursorRectTransform.sizeDelta;
		}
		currentPos = base.transform.position;
	}

	private void OnDrawGizmosSelected()
	{
		if (!(boundsRectTransform == null))
		{
			Gizmos.color = Color.yellow;
			(Vector2 bottomLeft, Vector2 topUp) sides = GetSides();
			Vector2 item = sides.bottomLeft;
			Vector2 item2 = sides.topUp;
			Vector3 center = new Vector3((item.x + item2.x) * 0.5f, (item.y + item2.y) * 0.5f, base.transform.position.z);
			Vector3 size = new Vector3(item2.x - item.x, item2.y - item.y, 0.01f);
			Gizmos.DrawWireCube(center, size);
		}
	}

	private (Vector2 bottomLeft, Vector2 topUp) GetSides()
	{
		if (boundsRectTransform == null)
		{
			return (bottomLeft: Vector2.zero, topUp: Vector2.zero);
		}
		Vector3[] array = new Vector3[4];
		boundsRectTransform.GetWorldCorners(array);
		Vector2 item = array[0];
		Vector2 item2 = array[2];
		return (bottomLeft: item, topUp: item2);
	}
}
