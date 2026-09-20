using System;
using UnityEngine;

namespace Rewired.Glyphs;

public abstract class GlyphOrTextBase : MonoBehaviour
{
	[Flags]
	protected enum TypeFlags
	{
		None = 0,
		Glyph = 1,
		Text = 2,
		All = -1
	}

	protected abstract string textString { get; set; }

	public abstract void ShowText(string text);

	public abstract void ShowGlyph(object glyph);

	public virtual void Hide()
	{
		Hide(TypeFlags.All);
	}

	protected abstract void Hide(TypeFlags flags);
}
public abstract class GlyphOrTextBase<TGlyphComponent, TGlyphGraphic, TTextComponent> : GlyphOrTextBase where TGlyphComponent : Behaviour where TGlyphGraphic : class where TTextComponent : Behaviour
{
	[SerializeField]
	private TTextComponent _textComponent;

	[SerializeField]
	private TGlyphComponent _glyphComponent;

	public TTextComponent textComponent
	{
		get
		{
			return _textComponent;
		}
		set
		{
			_textComponent = value;
		}
	}

	public TGlyphComponent glyphComponent
	{
		get
		{
			return _glyphComponent;
		}
		set
		{
			_glyphComponent = value;
		}
	}

	protected abstract TGlyphGraphic glyphGraphic { get; set; }

	public override void ShowText(string text)
	{
		if (_textComponent == null)
		{
			return;
		}
		if (!string.Equals(textString, text, StringComparison.Ordinal))
		{
			textString = text;
		}
		if (!_textComponent.gameObject.activeSelf)
		{
			_textComponent.gameObject.SetActive(value: true);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		Hide(TypeFlags.Glyph);
	}

	public override void ShowGlyph(object glyph)
	{
		if (glyph != null && !(glyph is TGlyphGraphic))
		{
			Debug.LogError("Rewired: Glyph does not implement " + typeof(TGlyphGraphic).Name + ".");
		}
		else
		{
			ShowGlyph((TGlyphGraphic)glyph);
		}
	}

	public virtual void ShowGlyph(TGlyphGraphic glyph)
	{
		if (_glyphComponent == null)
		{
			return;
		}
		if (glyphGraphic != glyph)
		{
			glyphGraphic = glyph;
		}
		if (!_glyphComponent.gameObject.activeSelf)
		{
			_glyphComponent.gameObject.SetActive(value: true);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		Hide(TypeFlags.Text);
	}

	protected override void Hide(TypeFlags flags)
	{
		if (_textComponent != null && (flags & TypeFlags.Text) != 0 && _textComponent.gameObject.activeSelf)
		{
			_textComponent.gameObject.SetActive(value: false);
		}
		if (_glyphComponent != null && (flags & TypeFlags.Glyph) != 0 && _glyphComponent.gameObject.activeSelf)
		{
			_glyphComponent.gameObject.SetActive(value: false);
		}
		if ((_glyphComponent == null || !_glyphComponent.gameObject.activeSelf) && (_textComponent == null || !_textComponent.gameObject.activeSelf))
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
