using LazyBearTechnology;
using UnityEngine;

public class UIMouseTooltipCollider : MonoBehaviour
{
	[SerializeField]
	private string lngId;

	[SerializeField]
	private Collider2D collisionCollider;

	[SerializeField]
	private RectTransform overrideTarget;

	private bool entered;

	private RectTransform Target
	{
		get
		{
			if (!(overrideTarget != null))
			{
				return base.transform as RectTransform;
			}
			return overrideTarget;
		}
	}

	public static UIMouseTooltipCollider Attach(Collider2D collider, string lngId, RectTransform overrideTarget = null)
	{
		if (collider == null)
		{
			return null;
		}
		GameObject gameObject = collider.gameObject;
		UIMouseTooltipCollider uIMouseTooltipCollider = gameObject.GetComponent<UIMouseTooltipCollider>();
		if (uIMouseTooltipCollider == null)
		{
			uIMouseTooltipCollider = gameObject.AddComponent<UIMouseTooltipCollider>();
		}
		uIMouseTooltipCollider.collisionCollider = collider;
		uIMouseTooltipCollider.lngId = lngId;
		uIMouseTooltipCollider.overrideTarget = overrideTarget;
		return uIMouseTooltipCollider;
	}

	public void SetLocalizationId(string lngId)
	{
		if (!(this.lngId == lngId))
		{
			HideTooltip(immediately: true);
			this.lngId = lngId;
		}
	}

	private void Awake()
	{
		if (collisionCollider == null)
		{
			collisionCollider = GetComponent<Collider2D>();
		}
	}

	private void OnEnable()
	{
		LazyInput.OnInputChanged += OnInputChanged;
		GameSettings.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDisable()
	{
		LazyInput.OnInputChanged -= OnInputChanged;
		GameSettings.OnLanguageChanged -= OnLanguageChanged;
		HideTooltip(immediately: true);
		entered = false;
	}

	private void Update()
	{
		if (collisionCollider == null || !UIMouseTooltip.IsAvailable)
		{
			if (entered)
			{
				HideTooltip(immediately: true);
				entered = false;
			}
			return;
		}
		bool flag = IsMouseOvered();
		if (!entered)
		{
			if (flag)
			{
				entered = true;
				UIMouseTooltip.TryShow(Target, lngId);
			}
		}
		else if (!flag)
		{
			entered = false;
			HideTooltip(immediately: false);
		}
	}

	private void OnInputChanged()
	{
		if (!UIMouseTooltip.IsAvailable)
		{
			HideTooltip(immediately: true);
			entered = false;
		}
	}

	private void OnLanguageChanged()
	{
		HideTooltip(immediately: true);
		entered = false;
	}

	private void HideTooltip(bool immediately)
	{
		UIMouseTooltip.HideIfShowingAt(Target, immediately);
	}

	private bool IsMouseOvered()
	{
		Vector2 vector = Input.mousePosition;
		if (collisionCollider.OverlapPoint(vector))
		{
			return true;
		}
		Camera main = Camera.main;
		if (main == null)
		{
			return false;
		}
		Vector3 position = vector;
		position.z = collisionCollider.transform.position.z - main.transform.position.z;
		return collisionCollider.OverlapPoint(main.ScreenToWorldPoint(position));
	}
}
