using Unity.Multiplayer.Tools.NetStats;
using Unity.Multiplayer.Tools.NetStatsMonitor;

public class CustomNetStats
{
	private RuntimeNetStatsMonitor statsMonitor;

	public CustomNetStats()
	{
	}

	public CustomNetStats(RuntimeNetStatsMonitor statsMonitor)
	{
		this.statsMonitor = statsMonitor;
	}

	public void UpdatePackageStats(int sentPackages, int receivedPackages)
	{
		if (sentPackages != 0)
		{
			statsMonitor.AddCustomValue(MetricId.Create(CustomMetric.LogicPackagesSent), sentPackages);
		}
		if (receivedPackages != 0)
		{
			statsMonitor.AddCustomValue(MetricId.Create(CustomMetric.LogicPackagesReceived), receivedPackages);
		}
		statsMonitor.AddCustomValue(MetricId.Create(CustomMetric.AveragePackagesSent), sentPackages);
		statsMonitor.AddCustomValue(MetricId.Create(CustomMetric.AveragePackagesReceived), receivedPackages);
	}
}
