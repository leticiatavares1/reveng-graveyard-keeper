using System.Collections.Generic;
using LinqTools;
using UnityEngine;

namespace LazyBearTechnology;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class BaseVirtualCursor : MonoBehaviour
{
	public const string TAG = "MapCursor";

	[SerializeField]
	protected float currentSpeed;

	[SerializeField]
	protected float minSpeed = 3f;

	[SerializeField]
	protected float maxSpeed = 6f;

	[SerializeField]
	protected float accelerateSpeed = 6f;

	[SerializeField]
	protected float zPosition = -500f;

	[SerializeField]
	[Space]
	protected List<VirtualCursorVisualPair> cursorVisualPairs;

	protected Collider2D collider2D;

	protected Rigidbody2D rigidbody2D;

	protected VirtualCursorVisualPair activeVisualPair;

	protected Vector2 minCoords;

	protected Vector2 maxCoords;

	protected virtual void Awake()
	{
		Init();
	}

	private VirtualCursorVisualPair GetPairByState(VirtualCursorState cursorState)
	{
		return cursorVisualPairs.FirstOrDefault((VirtualCursorVisualPair c) => c.cursorState == cursorState);
	}

	protected void AccelerateSpeed(bool isCursorActive)
	{
		if (isCursorActive)
		{
			currentSpeed += accelerateSpeed * Time.deltaTime;
		}
		else
		{
			currentSpeed -= accelerateSpeed * Time.deltaTime;
		}
		currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
	}

	protected virtual void Init()
	{
		base.gameObject.SetActive(value: false);
		collider2D = GetComponent<Collider2D>();
		rigidbody2D = GetComponent<Rigidbody2D>();
		collider2D.tag = "MapCursor";
		collider2D.isTrigger = false;
		rigidbody2D.isKinematic = true;
		currentSpeed = minSpeed;
		if (cursorVisualPairs.Count == 0)
		{
			Debug.LogError("Cursors not found, please setup pairs in inspector");
			return;
		}
		foreach (VirtualCursorVisualPair cursorVisualPair in cursorVisualPairs)
		{
			cursorVisualPair.gameObject.SetActive(value: false);
		}
		activeVisualPair = GetPairByState(VirtualCursorState.Default);
		activeVisualPair.gameObject.SetActive(value: true);
	}

	protected virtual void OnEnable()
	{
		CalculateBounds();
		OverlapColliders(null);
		currentSpeed = minSpeed;
	}

	protected virtual void Update()
	{
		Vector2 direction = LazyInput.GetDirection();
		AccelerateSpeed(direction != Vector2.zero);
		Vector2 value = (Vector2)base.transform.position + GetSpeed() * direction;
		value = value.Clamp(minCoords, maxCoords);
		base.transform.position = new Vector3(value.x, value.y, zPosition);
	}

	protected virtual float GetSpeed()
	{
		return currentSpeed * Time.deltaTime;
	}

	public abstract void CalculateBounds();

	public virtual void SetState(VirtualCursorState cursorState)
	{
		VirtualCursorVisualPair pairByState = GetPairByState(cursorState);
		if (pairByState == null)
		{
			Debug.LogError($"Invalid cursor state:[{cursorState}]");
			return;
		}
		activeVisualPair.gameObject.SetActive(value: false);
		pairByState.gameObject.SetActive(value: true);
		activeVisualPair = pairByState;
	}

	public virtual void OverlapColliders(List<BaseVirtualCursorEventHandler> exclude)
	{
		List<Collider2D> list = new List<Collider2D>();
		ContactFilter2D contactFilter2D = default(ContactFilter2D);
		contactFilter2D.useTriggers = true;
		ContactFilter2D contactFilter = contactFilter2D;
		collider2D.Overlap(contactFilter, list);
		foreach (Collider2D item in list)
		{
			BaseVirtualCursorEventHandler component = item.GetComponent<BaseVirtualCursorEventHandler>();
			if (component != null && (exclude == null || !exclude.Contains(component)))
			{
				component.ForceSelect();
			}
		}
	}
}
