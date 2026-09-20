using System;

namespace DLCRefugees;

[Serializable]
public class RefugeeInfo
{
	public string obj_id;

	public string custom_tag;

	public RefugeeInfo()
	{
	}

	public RefugeeInfo(string obj_id, string custom_tag)
	{
		this.obj_id = obj_id;
		this.custom_tag = custom_tag;
	}
}
