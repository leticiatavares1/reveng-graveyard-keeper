using UnityEngine;

public static class GpuGraphicsTierDetector
{
	private const string LOG_PREFIX = "[GpuGraphicsTierDetector]";

	public const string HIGH_REFERENCE_GPU_NAME = "GeForce RTX 3060";

	private const float MEDIUM_TIER_FRACTION = 0.5f;

	private const float LOW_TIER_FRACTION = 0.3f;

	public static GraphicsTier DetectDefaultGraphicsTier()
	{
		string graphicsDeviceName = SystemInfo.graphicsDeviceName;
		int graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID;
		int graphicsDeviceID = SystemInfo.graphicsDeviceID;
		Debug.Log("[GpuGraphicsTierDetector] SystemInfo: name='" + graphicsDeviceName + "', vendor='" + SystemInfo.graphicsDeviceVendor + "', " + $"vendorId=0x{graphicsDeviceVendorID:X4}, deviceId=0x{graphicsDeviceID:X4}, type={SystemInfo.graphicsDeviceType}, " + $"vram={SystemInfo.graphicsMemorySize}MB, version='{SystemInfo.graphicsDeviceVersion}'");
		int benchmarkPoints = GpuBenchmarkDatabase.GetBenchmarkPoints("GeForce RTX 3060", 0, 0);
		if (benchmarkPoints <= 0)
		{
			Debug.LogError(string.Format("{0} Reference GPU '{1}' not found in the benchmark database. Falling back to {2}.", "[GpuGraphicsTierDetector]", "GeForce RTX 3060", GraphicsTier.High));
			return GraphicsTier.High;
		}
		int benchmarkPoints2 = GpuBenchmarkDatabase.GetBenchmarkPoints(graphicsDeviceName, graphicsDeviceVendorID, graphicsDeviceID);
		if (benchmarkPoints2 < 0)
		{
			Debug.Log(string.Format("{0} GPU '{1}' (0x{2:X4}:0x{3:X4}) not found in the benchmark database. Falling back to {4}.", "[GpuGraphicsTierDetector]", graphicsDeviceName, graphicsDeviceVendorID, graphicsDeviceID, GraphicsTier.High));
			return GraphicsTier.High;
		}
		GraphicsTier graphicsTier = ResolveTier(benchmarkPoints2, benchmarkPoints);
		Debug.Log(string.Format("{0} GPU '{1}' found: {2} points. Reference '{3}': {4} points ", "[GpuGraphicsTierDetector]", graphicsDeviceName, benchmarkPoints2, "GeForce RTX 3060", benchmarkPoints) + $"(ratio {(float)benchmarkPoints2 / (float)benchmarkPoints:0.00}). Thresholds: High>={benchmarkPoints}, " + $"Medium>={Mathf.RoundToInt((float)benchmarkPoints * 0.5f)}, Low>={Mathf.RoundToInt((float)benchmarkPoints * 0.3f)}. " + $"Selected tier: {graphicsTier}");
		return graphicsTier;
	}

	public static GraphicsTier ResolveTier(int points, int referencePoints)
	{
		if (points >= referencePoints)
		{
			return GraphicsTier.High;
		}
		if ((float)points >= (float)referencePoints * 0.5f)
		{
			return GraphicsTier.Medium;
		}
		if ((float)points >= (float)referencePoints * 0.3f)
		{
			return GraphicsTier.Low;
		}
		return GraphicsTier.Lowest;
	}
}
