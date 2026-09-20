using System;

[Serializable]
public sealed class BodyZombieSkinSerializedItemProperty : SerializedItemProperty
{
	public int body;

	public int head;

	public string headLut;

	public override SerializedItemProperty Clone()
	{
		return new BodyZombieSkinSerializedItemProperty
		{
			body = body,
			head = head,
			headLut = headLut
		};
	}
}
