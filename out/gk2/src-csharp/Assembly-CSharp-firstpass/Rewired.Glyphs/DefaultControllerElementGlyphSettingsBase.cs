using UnityEngine;

namespace Rewired.Glyphs;

public abstract class DefaultControllerElementGlyphSettingsBase : MonoBehaviour
{
	[Tooltip("The Controller element glyph options.")]
	[SerializeField]
	private ControllerElementGlyphSelectorOptions _options;

	[Tooltip("The prefab used for each glyph or text object.")]
	[SerializeField]
	private GameObject _glyphOrTextPrefab;

	public ControllerElementGlyphSelectorOptions options
	{
		get
		{
			return _options;
		}
		set
		{
			_options = value;
			SetDefaults();
		}
	}

	public GameObject glyphOrTextPrefab
	{
		get
		{
			return _glyphOrTextPrefab;
		}
		set
		{
			_glyphOrTextPrefab = value;
			SetDefaults();
		}
	}

	protected virtual void OnEnable()
	{
		SetDefaults();
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void SetDefaults()
	{
		SetDefaultOptions();
		SetDefaultGlyphOrTextPrefab();
	}

	protected virtual void SetDefaultOptions()
	{
		ControllerElementGlyphSelectorOptions.defaultOptions = options;
	}

	protected abstract void SetDefaultGlyphOrTextPrefab();
}
