using Unity.Multiplayer.Tools.NetStats;

[MetricTypeEnum(DisplayName = "CustomMetric")]
internal enum CustomMetric
{
	[MetricMetadata(Units = Units.None, MetricKind = MetricKind.Counter)]
	LogicPackagesSent,
	[MetricMetadata(Units = Units.None, MetricKind = MetricKind.Counter)]
	LogicPackagesReceived,
	[MetricMetadata(Units = Units.None, MetricKind = MetricKind.Counter)]
	AveragePackagesSent,
	[MetricMetadata(Units = Units.None, MetricKind = MetricKind.Counter)]
	AveragePackagesReceived
}
