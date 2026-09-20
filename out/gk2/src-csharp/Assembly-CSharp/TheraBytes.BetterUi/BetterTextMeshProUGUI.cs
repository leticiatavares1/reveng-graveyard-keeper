using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheraBytes.BetterUi;

[ExecuteAlways]
[HelpURL("https://documentation.therabytes.de/better-ui/BetterTextMeshProUGUI.html")]
[AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro Text", 30)]
public class BetterTextMeshProUGUI : TextMeshProUGUI, IResolutionDependency
{
	[SerializeField]
	private BetterText.FittingMode fitting;

	[FormerlySerializedAs("marginSizer")]
	[SerializeField]
	private MarginSizeModifier marginSizerFallback = new MarginSizeModifier(new Margin(), new Margin(), new Margin(1000, 1000, 1000, 1000));

	[SerializeField]
	private MarginSizeConfigCollection customMarginSizers = new MarginSizeConfigCollection();

	[FormerlySerializedAs("fontSizer")]
	[SerializeField]
	private FloatSizeModifier fontSizerFallback = new FloatSizeModifier(36f, 10f, 500f);

	[SerializeField]
	private FloatSizeConfigCollection customFontSizers = new FloatSizeConfigCollection();

	[FormerlySerializedAs("minFontSizer")]
	[SerializeField]
	private FloatSizeModifier minFontSizerFallback = new FloatSizeModifier(10f, 10f, 500f);

	[SerializeField]
	private FloatSizeConfigCollection customMinFontSizers = new FloatSizeConfigCollection();

	[FormerlySerializedAs("maxFontSizer")]
	[SerializeField]
	private FloatSizeModifier maxFontSizerFallback = new FloatSizeModifier(500f, 500f, 500f);

	[SerializeField]
	private FloatSizeConfigCollection customMaxFontSizers = new FloatSizeConfigCollection();

	public BetterText.FittingMode Fitting
	{
		get
		{
			return fitting;
		}
		set
		{
			if (fitting != value)
			{
				fitting = value;
				CalculateSize();
			}
		}
	}

	public MarginSizeModifier MarginSizer => customMarginSizers.GetCurrentItem(marginSizerFallback);

	public FloatSizeModifier FontSizer => customFontSizers.GetCurrentItem(fontSizerFallback);

	public FloatSizeModifier MinFontSizer => customMinFontSizers.GetCurrentItem(minFontSizerFallback);

	public FloatSizeModifier MaxFontSizer => customMaxFontSizers.GetCurrentItem(maxFontSizerFallback);

	public bool IgnoreFontSizerOptions { get; set; }

	public new float fontSize
	{
		get
		{
			return base.fontSize;
		}
		set
		{
			Config.Set(value, delegate(float o)
			{
				base.fontSize = o;
			}, delegate(float o)
			{
				FontSizer.SetSize(this, o);
			});
		}
	}

	public new float fontSizeMin
	{
		get
		{
			return base.fontSizeMin;
		}
		set
		{
			Config.Set(value, delegate(float o)
			{
				base.fontSizeMin = o;
			}, delegate(float o)
			{
				MinFontSizer.SetSize(this, o);
			});
		}
	}

	public new float fontSizeMax
	{
		get
		{
			return base.fontSizeMax;
		}
		set
		{
			Config.Set(value, delegate(float o)
			{
				base.fontSizeMax = o;
			}, delegate(float o)
			{
				MaxFontSizer.SetSize(this, o);
			});
		}
	}

	public new Vector4 margin
	{
		get
		{
			return base.margin;
		}
		set
		{
			Config.Set(value, delegate(Vector4 o)
			{
				base.margin = o;
			}, delegate(Vector4 o)
			{
				MarginSizer.SetSize(this, new Margin(o));
			});
		}
	}

	protected override void OnEnable()
	{
		CalculateSize();
		base.OnEnable();
	}

	public void OnResolutionChanged()
	{
		CalculateSize();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		CalculateSize();
	}

	public void CalculateSize()
	{
		if (IgnoreFontSizerOptions)
		{
			base.enableAutoSizing = false;
		}
		else
		{
			switch (fitting)
			{
			case BetterText.FittingMode.SizerOnly:
				base.enableAutoSizing = false;
				base.fontSize = FontSizer.CalculateSize(this, "FontSizer");
				break;
			case BetterText.FittingMode.StayInBounds:
				base.enableAutoSizing = true;
				base.fontSizeMin = MinFontSizer.CalculateSize(this, "MinFontSizer");
				base.fontSizeMax = FontSizer.CalculateSize(this, "FontSizer");
				break;
			case BetterText.FittingMode.BestFit:
				base.enableAutoSizing = true;
				base.fontSizeMin = MinFontSizer.CalculateSize(this, "MinFontSizer");
				base.fontSizeMax = MaxFontSizer.CalculateSize(this, "MaxFontSizer");
				break;
			}
		}
		base.margin = MarginSizer.CalculateSize(this, "MarginSizer").ToVector4();
	}

	public void RegisterMaterials(Material[] materials)
	{
		GetMaterials(materials);
	}
}
