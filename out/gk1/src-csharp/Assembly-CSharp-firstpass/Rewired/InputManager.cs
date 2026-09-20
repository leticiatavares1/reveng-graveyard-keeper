using System.ComponentModel;
using System.Text.RegularExpressions;
using Rewired.Platforms;
using Rewired.Utils;
using Rewired.Utils.Interfaces;
using UnityEngine;

namespace Rewired;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class InputManager : InputManager_Base
{
	protected override void DetectPlatform()
	{
		editorPlatform = EditorPlatform.None;
		platform = Platform.Unknown;
		webplayerPlatform = WebplayerPlatform.None;
		isEditor = false;
		if (SystemInfo.deviceName == null)
		{
			_ = string.Empty;
		}
		if (SystemInfo.deviceModel == null)
		{
			_ = string.Empty;
		}
		platform = Platform.Linux;
	}

	protected override void CheckRecompile()
	{
	}

	protected override IExternalTools GetExternalTools()
	{
		return new ExternalTools();
	}

	private bool CheckDeviceName(string searchPattern, string deviceName, string deviceModel)
	{
		if (!Regex.IsMatch(deviceName, searchPattern, RegexOptions.IgnoreCase))
		{
			return Regex.IsMatch(deviceModel, searchPattern, RegexOptions.IgnoreCase);
		}
		return true;
	}
}
