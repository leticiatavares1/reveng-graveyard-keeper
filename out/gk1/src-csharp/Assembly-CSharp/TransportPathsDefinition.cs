using System;
using System.Collections.Generic;

[Serializable]
public class TransportPathsDefinition : BalanceBaseObject
{
	public const string DEFAULT_DESTINATION_ZONE_ID = "mf_wood";

	public string source_zone_id;

	public string station_wgo_id;

	public string destination_zone_id;

	public List<string> transport_items = new List<string>();
}
