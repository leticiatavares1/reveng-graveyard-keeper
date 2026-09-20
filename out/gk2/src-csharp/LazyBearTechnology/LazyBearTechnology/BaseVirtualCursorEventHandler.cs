using UnityEngine;

namespace LazyBearTechnology;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseVirtualCursorEventHandler : MonoBehaviour
{
	protected Collider2D collider2D;

	private void Awake()
	{
		collider2D = GetComponent<Collider2D>();
		collider2D.isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("MapCursor"))
		{
			BaseVirtualCursor component = collision.GetComponent<BaseVirtualCursor>();
			if (component != null)
			{
				OnSelect(component);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("MapCursor"))
		{
			BaseVirtualCursor component = collision.GetComponent<BaseVirtualCursor>();
			if (component != null)
			{
				OnDeselect(component);
			}
		}
	}

	protected abstract void OnSelect(BaseVirtualCursor cursor);

	protected abstract void OnDeselect(BaseVirtualCursor cursor);

	public virtual void ForceSelect()
	{
		OnSelect(null);
	}
}
