public class WgoPartStateData
{
	public string id;

	public int rotationIndex;

	public bool mirror;

	public WgoPartStateData(string id, int rotationIndex, bool mirror)
	{
		this.id = id;
		this.rotationIndex = rotationIndex;
		this.mirror = mirror;
	}
}
