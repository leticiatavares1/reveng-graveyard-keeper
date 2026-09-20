using UnityEngine;

namespace LazyBearTechnology.CloudSync;

public class LazyCloudSyncSaveData
{
	private const char HEADER_BODY_SEPARATOR = '|';

	public string header;

	public string data;

	public string CombinedBody => header + "|" + data;

	public LazyCloudSyncSaveData()
	{
	}

	public LazyCloudSyncSaveData(string combinedData)
	{
		int num = combinedData.IndexOf('|');
		if (num == -1)
		{
			Debug.LogError("LBCloudSyncSaveData constructor error -- separator character not found!");
			return;
		}
		header = combinedData.Substring(0, num);
		data = combinedData.Substring(num + 1);
	}

	public LazyCloudSyncSaveData(string header, string data)
	{
		this.header = header;
		this.data = data;
	}
}
