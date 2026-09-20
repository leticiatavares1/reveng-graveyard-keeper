using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi;

[ExecuteAlways]
[HelpURL("https://documentation.therabytes.de/better-ui/BetterTextMeshPro-InputField.html")]
[AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro - Input Field", 30)]
public class BetterTextMeshProInputField : TMP_InputField, IBetterTransitionUiElement, IResolutionDependency
{
	[SerializeField]
	[DefaultTransitionStates]
	private List<Transitions> betterTransitions = new List<Transitions>();

	[SerializeField]
	private List<Graphic> additionalPlaceholders = new List<Graphic>();

	[SerializeField]
	private FloatSizeModifier pointSizeScaler = new FloatSizeModifier(36f, 10f, 500f);

	[SerializeField]
	private bool overridePointSize;

	public List<Transitions> BetterTransitions => betterTransitions;

	public List<Graphic> AdditionalPlaceholders => additionalPlaceholders;

	public FloatSizeModifier PointSizeScaler => pointSizeScaler;

	public bool OverridePointSizeSettings
	{
		get
		{
			return overridePointSize;
		}
		set
		{
			overridePointSize = value;
		}
	}

	public new float pointSize
	{
		get
		{
			return base.pointSize;
		}
		set
		{
			Config.Set(value, delegate(float o)
			{
				base.pointSize = o;
			}, delegate(float o)
			{
				PointSizeScaler.SetSize(this, o);
			});
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (Transitions betterTransition in betterTransitions)
		{
			betterTransition.SetState(state.ToString(), instant);
		}
	}

	public override void OnUpdateSelected(BaseEventData eventData)
	{
		base.OnUpdateSelected(eventData);
		DisplayPlaceholders(base.text);
	}

	private void DisplayPlaceholders(string input)
	{
		bool flag = string.IsNullOrEmpty(input);
		if (!Application.isPlaying)
		{
			return;
		}
		foreach (Graphic additionalPlaceholder in additionalPlaceholders)
		{
			additionalPlaceholder.enabled = flag;
		}
	}

	protected override void OnEnable()
	{
		CalculateSize();
		base.OnEnable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		CalculateSize();
	}

	public void OnResolutionChanged()
	{
		CalculateSize();
	}

	public void CalculateSize()
	{
		if (overridePointSize)
		{
			base.pointSize = pointSizeScaler.CalculateSize(this, "pointSizeScaler");
		}
		OverrideBetterTextMeshSize(m_Placeholder as BetterTextMeshProUGUI, pointSize);
		OverrideBetterTextMeshSize(m_TextComponent as BetterTextMeshProUGUI, pointSize);
		foreach (Graphic additionalPlaceholder in additionalPlaceholders)
		{
			OverrideBetterTextMeshSize(additionalPlaceholder as BetterTextMeshProUGUI, pointSize);
		}
	}

	private void OverrideBetterTextMeshSize(BetterTextMeshProUGUI better, float size)
	{
		if (!(better == null))
		{
			better.IgnoreFontSizerOptions = overridePointSize;
			if (overridePointSize)
			{
				better.FontSizer.OverrideLastCalculatedSize(size);
				better.fontSize = size;
			}
			else
			{
				better.FontSizer.CalculateSize(this, "FontSizer");
			}
		}
	}
}
