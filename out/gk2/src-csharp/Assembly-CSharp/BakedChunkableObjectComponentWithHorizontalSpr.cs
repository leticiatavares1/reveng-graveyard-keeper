public class BakedChunkableObjectComponentWithHorizontalSpr : BakedChunkableObjectComponent
{
	public BakedChunkableObjectComponentData data;

	public override BakedChunkableObjectComponentData GetData()
	{
		if (data == null)
		{
			data = new BakedChunkableObjectComponentData();
		}
		return data;
	}

	public override void SetData(BakedChunkableObjectComponentData data)
	{
		this.data = data;
	}

	public override void BakeData()
	{
		base.BakeData();
		HorizontalSprite componentInChildren = GetComponentInChildren<HorizontalSprite>(includeInactive: true);
		if (componentInChildren != null)
		{
			data.gndLocalPos = componentInChildren.transform.localPosition;
		}
	}

	public override void ApplyData()
	{
		base.ApplyData();
		HorizontalSprite componentInChildren = GetComponentInChildren<HorizontalSprite>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.transform.localPosition = data.gndLocalPos;
		}
	}
}
