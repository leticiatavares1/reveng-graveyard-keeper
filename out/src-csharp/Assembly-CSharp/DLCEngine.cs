using System.IO;
using UnityEngine;

public static class DLCEngine
{
	public enum DLCVersion
	{
		None,
		BreakingDead,
		Stories,
		Refugees,
		Souls
	}

	public const DLCVersion LAST_DLC = DLCVersion.Souls;

	private const string _DLC_STORIES_CHECK_FILE_REL_PATH = "/gamedata_2.dat";

	private const string _DLC_REFUGEES_CHECK_FILE_REL_PATH = "/gamedata_3.dat";

	private const string _DLC_SOULS_CHECK_FILE_REL_PATH = "/gamedata_4.dat";

	public static bool IsDLCAvailable(DLCVersion dlc_version)
	{
		return dlc_version switch
		{
			DLCVersion.None => true, 
			DLCVersion.Stories => IsDLCStoriesAvailable(), 
			DLCVersion.Refugees => IsDLCRefugeesAvailable(), 
			DLCVersion.Souls => IsDLCSoulsAvailable(), 
			_ => true, 
		};
	}

	public static int DLCAvailableCount()
	{
		int num = 0;
		for (int i = 1; i <= 4; i++)
		{
			if (IsDLCAvailable((DLCVersion)i))
			{
				num++;
			}
		}
		return num;
	}

	private static bool IsDLCStoriesAvailable()
	{
		return File.Exists(Application.dataPath + "/gamedata_2.dat");
	}

	private static bool IsDLCRefugeesAvailable()
	{
		string path = Application.dataPath + "/gamedata_3.dat";
		Debug.Log(Application.dataPath);
		return File.Exists(path);
	}

	private static bool IsDLCSoulsAvailable()
	{
		string path = Application.dataPath + "/gamedata_4.dat";
		Debug.Log(Application.dataPath);
		return File.Exists(path);
	}
}
