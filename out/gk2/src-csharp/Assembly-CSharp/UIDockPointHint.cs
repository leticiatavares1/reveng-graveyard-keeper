using Cinemachine;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIDockPointHint : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private Image icon;

	private Wgo targetWgo;

	private DockPoint targetDockPoint;

	public void Display(DockPoint dockPoint)
	{
		CinemachineCore.CameraUpdatedEvent.AddListener(UpdatePos);
		targetWgo = dockPoint.Owner;
		targetDockPoint = dockPoint;
		string text = targetDockPoint.Direction.ToString()[0].ToString();
		icon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("dockpoint_marker_" + text);
		base.gameObject.SetActive(value: true);
	}

	public void Remove()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePos);
		Object.Destroy(base.gameObject);
	}

	private void UpdatePos(CinemachineBrain brain)
	{
		base.transform.position = CameraSystem.WorldToScreenPoint(targetDockPoint.transform.position);
	}
}
