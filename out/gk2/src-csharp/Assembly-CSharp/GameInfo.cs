using System;
using System.Globalization;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(fileName = "GameInfo", menuName = "GK2/Game Info")]
public class GameInfo : LazySingletonSO<GameInfo>
{
	[SerializeField]
	private string version;

	[SerializeField]
	public string gitCommitShortHash;

	[SerializeField]
	[Tooltip("If enabled, the version label stays visible during gameplay, not only in the main menu.")]
	private bool showVersionInGame;

	public string Version => version.ToString(CultureInfo.InvariantCulture);

	public bool ShowVersionInGame => showVersionInGame;

	public int GetVersion()
	{
		string text = version.Replace(".", "");
		if (int.TryParse(text, out var result))
		{
			return result;
		}
		throw new ArgumentException("Can't parse version: " + text);
	}
}
