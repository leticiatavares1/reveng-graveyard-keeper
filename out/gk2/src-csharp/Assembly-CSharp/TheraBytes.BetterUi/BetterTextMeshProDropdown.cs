using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi;

[HelpURL("https://documentation.therabytes.de/better-ui/BetterTextMeshPro-Dropdown.html")]
[AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro - Dropdown", 30)]
public class BetterTextMeshProDropdown : TMP_Dropdown, IBetterTransitionUiElement
{
	[SerializeField]
	[DefaultTransitionStates]
	private List<Transitions> betterTransitions = new List<Transitions>();

	[SerializeField]
	[TransitionStates(new string[] { "Show", "Hide" })]
	private List<Transitions> showHideTransitions = new List<Transitions>();

	public List<Transitions> BetterTransitions => betterTransitions;

	public List<Transitions> ShowHideTransitions => showHideTransitions;

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (Transitions betterTransition in betterTransitions)
		{
			betterTransition.SetState(state.ToString(), instant);
		}
	}

	protected override GameObject CreateDropdownList(GameObject template)
	{
		foreach (Transitions showHideTransition in showHideTransitions)
		{
			showHideTransition.SetState("Show", instant: false);
		}
		return base.CreateDropdownList(template);
	}

	protected override void DestroyDropdownList(GameObject dropdownList)
	{
		foreach (Transitions showHideTransition in showHideTransitions)
		{
			showHideTransition.SetState("Hide", instant: false);
		}
		base.DestroyDropdownList(dropdownList);
	}
}
