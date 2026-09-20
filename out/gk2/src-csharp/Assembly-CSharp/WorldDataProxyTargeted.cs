public class WorldDataProxyTargeted : WorldData, IProxyClassTargeted<WorldData>
{
	public WorldData Target { get; set; }
}
