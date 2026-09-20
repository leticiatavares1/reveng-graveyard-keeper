using UnityEngine;

public class NPCItemGUI : MonoBehaviour
{
	public NPCListQuestText quest_prefab;

	public UI2DSprite char_spr;

	public UIProgressBar relation_pb;

	public UILabel relation_txt;

	public UILabel npc_name;

	public UILabel npc_descr;

	public UILabel top_icon_txt;

	public GameObject go_relation;

	public SimpleUITable table;

	private KnownNPC _linked_npc;

	public void Draw(KnownNPC npc)
	{
		_linked_npc = npc;
		quest_prefab.gameObject.SetActive(value: false);
		if (npc == null)
		{
			Debug.LogError("Trying to draw a null npc");
			return;
		}
		ObjectDefinition dataOrNull = GameBalance.me.GetDataOrNull<ObjectDefinition>(npc.npc_id);
		NPCListQuestText[] componentsInChildren = GetComponentsInChildren<NPCListQuestText>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			NGUITools.Destroy(componentsInChildren[i]);
		}
		foreach (KnownNPC.TaskState task in _linked_npc.tasks)
		{
			if (task.state != KnownNPC.TaskState.State.Complete)
			{
				quest_prefab.Copy().Draw(task);
			}
		}
		npc_name.text = GJL.L(npc.npc_id);
		npc_descr.text = GJL.L("desc_" + npc.npc_id);
		top_icon_txt.text = ((dataOrNull == null) ? "" : dataOrNull.day_icon);
		int relation = WorldGameObject.GetRelation(npc.npc_id);
		relation_txt.text = relation.ToString();
		relation_pb.value = (float)relation / 100f;
		npc_descr.ProcessText();
		npc_name.ProcessText();
		string sprite_name = "char_" + npc.npc_id;
		if (dataOrNull != null && !string.IsNullOrEmpty(dataOrNull.npc_alias))
		{
			sprite_name = "char_" + dataOrNull.npc_alias;
		}
		char_spr.sprite2D = EasySpritesCollection.GetSprite(sprite_name);
		go_relation.SetActive(npc.npc_id != "player");
		table.Reposition();
	}
}
