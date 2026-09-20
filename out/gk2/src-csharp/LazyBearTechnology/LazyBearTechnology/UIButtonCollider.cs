using UnityEngine;
using UnityEngine.Events;

namespace LazyBearTechnology;

[RequireComponent(typeof(RectTransform), typeof(Collider2D))]
[ExecuteInEditMode]
public class UIButtonCollider : MonoBehaviour
{
	[SerializeField]
	private bool interactable;

	[SerializeField]
	private UnityEvent OnEnter;

	[SerializeField]
	private UnityEvent OnClick;

	[SerializeField]
	private UnityEvent OnExit;

	[SerializeField]
	private Collider2D collisionCollider2D;

	[SerializeField]
	private BoxCollider2D boxCollider2D;

	[SerializeField]
	private RectTransform rectTransform;

	private bool entered;

	public bool syncSizeWithUIRect;

	private void Awake()
	{
		if (collisionCollider2D == null)
		{
			collisionCollider2D = GetComponent<Collider2D>();
		}
		if (boxCollider2D == null)
		{
			boxCollider2D = collisionCollider2D as BoxCollider2D;
		}
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
	}

	private void Update()
	{
		if (!interactable)
		{
			return;
		}
		if (!entered)
		{
			if (IsMouseOvered())
			{
				entered = true;
				OnEnter.Invoke();
			}
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			OnClick.Invoke();
		}
		if (!IsMouseOvered())
		{
			entered = false;
			OnExit.Invoke();
		}
	}

	public void SetInteractable(bool state)
	{
		interactable = state;
	}

	private bool IsMouseOvered()
	{
		return collisionCollider2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
	}

	private void OnRectTransformDimensionsChange()
	{
		if (syncSizeWithUIRect && !(boxCollider2D == null))
		{
			boxCollider2D.size = rectTransform.rect.size;
		}
	}
}
