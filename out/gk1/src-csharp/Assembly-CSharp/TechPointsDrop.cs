using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TechPointsDrop : MonoBehaviour
{
	private static List<TechPointsDrop> _drops = new List<TechPointsDrop>();

	private string _tech_branch_letter = "";

	private Tweener _tweener;

	private bool _flying_to_trash_can;

	public UILabel label;

	private float _delay;

	private Action _on_reached_destination;

	public static void Drop(Vector3 pos, int r, int g, int b)
	{
		TechPointsSpawner techPointsSpawner = GUIElements.me.tech_points_spawner.Copy(MainGame.me.world_root);
		techPointsSpawner.transform.position = pos;
		techPointsSpawner.Spawn(r, g, b);
	}

	public static void Drop(Vector3 pos, Item item)
	{
		switch (item.id)
		{
		case "r":
			Drop(pos, item.value, 0, 0);
			break;
		case "g":
			Drop(pos, 0, item.value, 0);
			break;
		case "b":
			Drop(pos, 0, 0, item.value);
			break;
		default:
			Debug.LogError("Wrong techpoint letter = " + item.id);
			break;
		}
	}

	public static TechPointsDrop Drop(Vector3 pos, string tech_branch_letter, Action on_reached_destinaion = null)
	{
		TechPointsDrop techPointsDrop = Prefabs.me.tech_points_drop.Copy();
		techPointsDrop._on_reached_destination = on_reached_destinaion;
		_drops.Add(techPointsDrop);
		techPointsDrop.Draw(tech_branch_letter);
		techPointsDrop.gameObject.SetActive(value: true);
		techPointsDrop._flying_to_trash_can = (float)UnityEngine.Random.Range(0, 100) < PlayerComponent.GetTechPointsLoseChance() * 100f;
		Vector2 to = GUIElements.me.hud.tech_points_bar.GetTechPointsCounterPosition(tech_branch_letter);
		if (techPointsDrop._flying_to_trash_can)
		{
			GUIElements.me.hud.tech_trash_can.Show();
			to = GUIElements.me.hud.tech_trash_can.pos.transform.position;
		}
		else
		{
			GUIElements.me.hud.tech_points_bar.Show();
		}
		techPointsDrop.transform.SetGUIPosToWorldPos(pos, MainGame.me.world_cam, MainGame.me.gui_cam);
		techPointsDrop.transform.position += new Vector3(0f, 100f);
		techPointsDrop.label.text = "(" + tech_branch_letter + ")";
		techPointsDrop._delay = UnityEngine.Random.Range(0f, 0.2f);
		techPointsDrop.StartFly(to);
		return techPointsDrop;
	}

	public static void DestroyAllTechspointsBeforeGameExit()
	{
		foreach (TechPointsDrop drop in _drops)
		{
			UnityEngine.Object.Destroy(drop.gameObject);
		}
		_drops.Clear();
	}

	private void Draw(string tech_branch_letter)
	{
		_tech_branch_letter = tech_branch_letter;
	}

	private void StartFly(Vector2 to)
	{
		_tweener = base.transform.DOMove(to, 1f + _delay).OnComplete(delegate
		{
			_tweener = null;
			OnReachedDestination();
		}).SetEase(Ease.OutCubic);
	}

	private void OnReachedDestination(bool redraw_hud = true)
	{
		if (_tweener != null)
		{
			_tweener.Kill();
		}
		UnityEngine.Object.Destroy(base.gameObject);
		if (!_flying_to_trash_can)
		{
			MainGame.me.player.AddToParams(_tech_branch_letter, 1f);
			MainGame.me.save.achievements.CheckKeyQuests("tech_collect_" + _tech_branch_letter);
		}
		if (redraw_hud && GUIElements.me.hud.tech_points_bar != null)
		{
			GUIElements.me.hud.tech_points_bar.Redraw();
		}
		_drops.Remove(this);
		_on_reached_destination.TryInvoke();
	}
}
