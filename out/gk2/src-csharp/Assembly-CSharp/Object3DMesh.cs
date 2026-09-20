using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class Object3DMesh : MonoBehaviour
{
	public static readonly Color replaceBlueColor = new Color(1f, 1f, 0f);

	public static readonly Color replaceBlueTransparent = new Color(1f, 1f, 0f, 0f);

	public static readonly int matIdMainTexture = Shader.PropertyToID("_MainTex");

	public static readonly int matIdLutTexture = Shader.PropertyToID("_ReplaceLUT");

	public static readonly int matIdMainTexture2 = Shader.PropertyToID("_MainTex2");

	public static readonly int matIdNormalTexture = Shader.PropertyToID("_NormalMap");

	public static readonly int matIdUseNormalMap = Shader.PropertyToID("_UseNormalMap");

	public static readonly int matIdOpaqueShadowMesh = Shader.PropertyToID("_OpaqueShadowMesh");

	public static readonly int matIdIsTreeCrown = Shader.PropertyToID("_IsTreeCrown");

	public static readonly int matIdTreeChopAmplitude = Shader.PropertyToID("_TreeChopAmplitude");

	public static readonly int matIdUseCrownDeformOnly = Shader.PropertyToID("_UseCrownDeformOnly");

	public static readonly int matIdIsUnlit = Shader.PropertyToID("_Unlit");

	public static readonly int matIdReplaceBlue = Shader.PropertyToID("_ReplaceBlue");

	public static readonly int matIdReplaceBlueColor = Shader.PropertyToID("_ReplaceBlueColor");

	public static readonly int matIdAlphaCutoff = Shader.PropertyToID("_Cutoff");

	public static readonly int matIdTransparencyOcclusionValue = Shader.PropertyToID("_TransparencyOcclusionValue");

	public static readonly int matIdWorldWindEnabled = Shader.PropertyToID("_WorldWindEnabled");

	public static readonly int matIdSelectionTintColor = Shader.PropertyToID("_SelectionTintColor");

	public static readonly int matIdSelectionTintAmount = Shader.PropertyToID("_SelectionTintAmount");

	[FormerlySerializedAs("atoms")]
	[SerializeField]
	private List<Object3DSubMesh> subMeshes = new List<Object3DSubMesh>
	{
		new Object3DSubMesh()
	};

	[NonSerialized]
	public bool isTreeDestructing;

	[NonSerialized]
	public float treeDestructionPhase;

	[NonSerialized]
	public float treeChoppingPhase;

	[NonSerialized]
	public bool isBlueReplacingToColor;

	[NonSerialized]
	public float transparencyValue;

	[NonSerialized]
	private Color selectionTintColor = Color.white;

	[NonSerialized]
	private float selectionTintAmount;

	private Tween tweenChop;

	private Tween tweenDestruct;

	private static HashSet<Tween> tweensDestruct = new HashSet<Tween>();

	private int destructionActionTimerId = -1;

	[Range(0f, 10f)]
	public float treeHeight = 4f;

	private Renderer objRenderer;

	private bool initialized;

	public int SubMeshCount => subMeshes.Count;

	public Renderer ObjRenderer
	{
		get
		{
			if (objRenderer == null || objRenderer.gameObject == null)
			{
				objRenderer = GetComponent<Renderer>();
			}
			return objRenderer;
		}
	}

	public bool IsATree => System.Linq.Enumerable.Any(subMeshes, (Object3DSubMesh mesh) => mesh.Type == Object3DSubMesh.MeshType.TreeCrown);

	public Color SelectionTintColor => selectionTintColor;

	public float SelectionTintAmount => selectionTintAmount;

	public int GetSubMeshIndex(Object3DSubMesh subMesh)
	{
		return subMeshes.IndexOf(subMesh);
	}

	public void CustomInit()
	{
		InitSubMeshes();
		ApplyMaterials();
	}

	private void Start()
	{
		TryInit();
	}

	private void TryInit()
	{
		if (!initialized)
		{
			InitSubMeshes();
			ApplyMaterials();
			initialized = true;
		}
	}

	private void InitSubMeshes()
	{
		for (int i = 0; i < LinqTools.Enumerable.Count(subMeshes); i++)
		{
			subMeshes[i].Init(this, ObjRenderer, i);
		}
	}

	public void SetSelectionTint(Color color, float amount)
	{
		selectionTintColor = color;
		selectionTintAmount = amount;
		for (int i = 0; i < subMeshes.Count; i++)
		{
			subMeshes[i]?.SetSelectionTint(color, amount);
		}
	}

	public void ApplyMaterials()
	{
		for (int i = 0; i < subMeshes.Count; i++)
		{
			subMeshes[i].ApplyMaterial();
		}
	}

	private void Editor_PlayAnimationChop()
	{
		Object3D componentInParent = GetComponentInParent<Object3D>();
		if (componentInParent != null)
		{
			foreach (Object3DMesh object3DMesh in componentInParent.Object3DMeshes)
			{
				object3DMesh.PlayAnimationChop(null);
			}
			return;
		}
		PlayAnimationChop(null);
	}

	public void PlayAnimationChop(Action onAction)
	{
		if (tweenDestruct != null)
		{
			return;
		}
		tweenChop?.Kill();
		isTreeDestructing = true;
		treeChoppingPhase = (treeDestructionPhase = 0f);
		tweenChop = DOTween.To(() => treeChoppingPhase, delegate(float a)
		{
			treeChoppingPhase = a;
		}, 1f, LazySingletonSO<GlobalResources>.Instance.fxSettings.treeChopAnimLen).OnUpdate(ApplyMaterials).OnComplete(delegate
		{
			treeChoppingPhase = 0f;
			if ((double)treeDestructionPhase < 0.01)
			{
				isTreeDestructing = false;
			}
			tweenChop = null;
			ApplyMaterials();
		});
		onAction?.Invoke();
	}

	private void Editor_PlayAnimationDestruction()
	{
		Object3D componentInParent = GetComponentInParent<Object3D>();
		if (componentInParent != null)
		{
			foreach (Object3DMesh object3DMesh in componentInParent.Object3DMeshes)
			{
				object3DMesh.PlayAnimationDestruction(null, restoreOnComplete: true);
			}
			return;
		}
		PlayAnimationDestruction(null, restoreOnComplete: true);
	}

	public void PlayAnimationDestruction(Action onAction, bool restoreOnComplete = false)
	{
		CancelDestructionActionTimer();
		tweenChop?.Kill();
		tweenChop = null;
		if (tweenDestruct != null)
		{
			tweensDestruct.Remove(tweenDestruct);
			tweenDestruct.Kill();
			tweenDestruct = null;
		}
		isTreeDestructing = true;
		treeChoppingPhase = (treeDestructionPhase = 0f);
		tweenDestruct = DOTween.To(() => treeDestructionPhase, delegate(float a)
		{
			treeDestructionPhase = a;
		}, 1f, LazySingletonSO<GlobalResources>.Instance.fxSettings.GetTreeDestroyAnimLen(treeHeight)).OnUpdate(ApplyMaterials).OnComplete(delegate
		{
			tweensDestruct.Remove(tweenDestruct);
			tweenDestruct = null;
			if (restoreOnComplete)
			{
				treeDestructionPhase = 0f;
			}
			ApplyMaterials();
		});
		tweensDestruct.Add(tweenDestruct);
		if (onAction != null)
		{
			destructionActionTimerId = LazyTimer.AddTimer(LazySingletonSO<GlobalResources>.Instance.fxSettings.treeDestroyActionTime, onAction);
		}
	}

	private void CancelDestructionActionTimer()
	{
		if (destructionActionTimerId != -1)
		{
			LazyTimer.Stop(destructionActionTimerId);
			destructionActionTimerId = -1;
		}
	}

	public static void SetDestructionTweenPauseState(bool isPaused)
	{
		foreach (Tween item in tweensDestruct)
		{
			if (item != null)
			{
				if (isPaused)
				{
					item.Pause();
				}
				else
				{
					item.Play();
				}
			}
		}
	}

	public void ResetAnimValues()
	{
		isTreeDestructing = false;
		treeChoppingPhase = (treeDestructionPhase = 0f);
		ApplyMaterials();
	}

	public void ReplaceBlueToColor()
	{
		isBlueReplacingToColor = true;
		ApplyMaterials();
	}

	public void ReplaceBlueToTransparent()
	{
		isBlueReplacingToColor = false;
		ApplyMaterials();
	}

	public void SetLutTexture(Texture2D texture)
	{
		foreach (Object3DSubMesh subMesh in subMeshes)
		{
			subMesh.SetLutTexture(texture);
		}
	}

	public Object3DSubMesh GetSubMesh(int index)
	{
		if (index < 0 || index >= subMeshes.Count)
		{
			return null;
		}
		return subMeshes[index];
	}

	public void SetTransparency(float transparencyValue)
	{
		this.transparencyValue = transparencyValue;
		ApplyMaterials();
	}

	private void OnDestroy()
	{
		GlobalResources.TryReleaseTransparentMaterialFor(this);
		CancelDestructionActionTimer();
		tweenChop?.Kill();
		tweenChop = null;
		if (tweenDestruct != null)
		{
			tweensDestruct.Remove(tweenDestruct);
			tweenDestruct.Kill();
			tweenDestruct = null;
		}
	}
}
