using System;
using UnityEngine;

namespace Rewired.Glyphs.UnityUI;

[AddComponentMenu("Rewired/Glyphs/Unity UI/Unity UI Player Controller Element Glyph")]
public class UnityUIPlayerControllerElementGlyph : UnityUIPlayerControllerElementGlyphBase
{
	[Tooltip("The Player id.")]
	[SerializeField]
	private int _playerId;

	[Tooltip("The Action name.")]
	[SerializeField]
	private string _actionName;

	[NonSerialized]
	private int _actionId = -1;

	[NonSerialized]
	private bool _actionIdCached;

	public override int playerId
	{
		get
		{
			return _playerId;
		}
		set
		{
			_playerId = value;
		}
	}

	public override int actionId
	{
		get
		{
			if (!_actionIdCached)
			{
				CacheActionId();
			}
			return _actionId;
		}
		set
		{
			if (ReInput.isReady)
			{
				InputAction action = ReInput.mapping.GetAction(value);
				if (action == null)
				{
					Debug.LogError("Invalid Action id: " + value);
					return;
				}
				_actionName = action.name;
				CacheActionId();
			}
		}
	}

	public string actionName
	{
		get
		{
			return _actionName;
		}
		set
		{
			_actionName = value;
			CacheActionId();
		}
	}

	private void CacheActionId()
	{
		if (ReInput.isReady)
		{
			_actionId = ReInput.mapping.GetAction(_actionName)?.id ?? (-1);
			_actionIdCached = true;
		}
	}
}
