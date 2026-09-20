using UnityEngine;

public class ChurchPulpit : MonoBehaviour
{
	public SpriteRenderer buff_spr;

	public Animator animator;

	public void DoBuffSuccessAnimation()
	{
		MainGame.me.player.components.character.player.CreatePrayBuffFlyingObject(buff_spr.transform.position);
		PrayLogics.DropPrayItems();
	}

	public void OnMiddleAnimation()
	{
		GUIElements.me.pray_craft.OnMiddlePrayBuffAnimation();
	}

	public void OnPrayAnimationDone()
	{
		MainGame.me.player.components.character.SetAnimationState(CharAnimState.Idle);
		GUIElements.me.pray_craft.OnFinishedPrayBuffAnimation();
	}
}
