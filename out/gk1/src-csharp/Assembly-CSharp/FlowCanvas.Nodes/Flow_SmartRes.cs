using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("Stack", false, "")]
[Color("00ff00")]
[Category("Game Functions")]
[Name("Smart Res", 0)]
public class Flow_SmartRes : PureFunctionNode<SmartRes, SmartRes.ResType, string, float>
{
	public override SmartRes Invoke(SmartRes.ResType res_type, string id, float v)
	{
		SmartRes smartRes = new SmartRes
		{
			res_type = res_type
		};
		switch (res_type)
		{
		case SmartRes.ResType.Item:
			smartRes.item = new Item(id, Mathf.RoundToInt(v));
			if (string.IsNullOrEmpty(id))
			{
				Debug.LogError("ERROR: SmartRes - item id is empty, v = " + v);
			}
			break;
		case SmartRes.ResType.GameRes:
			smartRes.res = new GameResAtom(id, v);
			if (string.IsNullOrEmpty(id))
			{
				Debug.LogError("ERROR: SmartRes - res id is empty, v = " + v);
			}
			break;
		}
		return smartRes;
	}
}
