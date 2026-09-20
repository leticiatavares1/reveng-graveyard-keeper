using FlowCanvas;
using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using NodeCanvas.Tasks.Actions;
using NodeCanvas.Tasks.Conditions;
using UnityEngine;

namespace ParadoxNotion.Internal;

internal class AOTDummy
{
	private class FlowCanvas_BinderConnection_System_Boolean : BinderConnection<bool>
	{
	}

	private class FlowCanvas_BinderConnection_System_Single : BinderConnection<float>
	{
	}

	private class FlowCanvas_BinderConnection_System_Int32 : BinderConnection<int>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Vector2 : BinderConnection<Vector2>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Vector3 : BinderConnection<Vector3>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Vector4 : BinderConnection<Vector4>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Quaternion : BinderConnection<Quaternion>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Keyframe : BinderConnection<Keyframe>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Bounds : BinderConnection<Bounds>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Color : BinderConnection<Color>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Rect : BinderConnection<Rect>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_ContactPoint : BinderConnection<ContactPoint>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_ContactPoint2D : BinderConnection<ContactPoint2D>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Collision : BinderConnection<Collision>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Collision2D : BinderConnection<Collision2D>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_RaycastHit : BinderConnection<RaycastHit>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_RaycastHit2D : BinderConnection<RaycastHit2D>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Ray : BinderConnection<Ray>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_Space : BinderConnection<Space>
	{
	}

	private class FlowCanvas_BinderConnection_Direction : BinderConnection<Direction>
	{
	}

	private class FlowCanvas_BinderConnection_ItemDefinition_EquipmentType : BinderConnection<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_BinderConnection_MovementComponent_GoToMethod : BinderConnection<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_BinderConnection_GDPoint_IdlePointPrefix : BinderConnection<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_BinderConnection_UnityEngine_LayerMask : BinderConnection<LayerMask>
	{
	}

	private class FlowCanvas_ValueInput_System_Boolean : ValueInput<bool>
	{
	}

	private class FlowCanvas_ValueInput_System_Single : ValueInput<float>
	{
	}

	private class FlowCanvas_ValueInput_System_Int32 : ValueInput<int>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Vector2 : ValueInput<Vector2>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Vector3 : ValueInput<Vector3>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Vector4 : ValueInput<Vector4>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Quaternion : ValueInput<Quaternion>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Keyframe : ValueInput<Keyframe>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Bounds : ValueInput<Bounds>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Color : ValueInput<Color>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Rect : ValueInput<Rect>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_ContactPoint : ValueInput<ContactPoint>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_ContactPoint2D : ValueInput<ContactPoint2D>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Collision : ValueInput<Collision>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Collision2D : ValueInput<Collision2D>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_RaycastHit : ValueInput<RaycastHit>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_RaycastHit2D : ValueInput<RaycastHit2D>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Ray : ValueInput<Ray>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_Space : ValueInput<Space>
	{
	}

	private class FlowCanvas_ValueInput_Direction : ValueInput<Direction>
	{
	}

	private class FlowCanvas_ValueInput_ItemDefinition_EquipmentType : ValueInput<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_ValueInput_MovementComponent_GoToMethod : ValueInput<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_ValueInput_GDPoint_IdlePointPrefix : ValueInput<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_ValueInput_UnityEngine_LayerMask : ValueInput<LayerMask>
	{
	}

	private class FlowCanvas_ValueOutput_System_Boolean : ValueOutput<bool>
	{
	}

	private class FlowCanvas_ValueOutput_System_Single : ValueOutput<float>
	{
	}

	private class FlowCanvas_ValueOutput_System_Int32 : ValueOutput<int>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Vector2 : ValueOutput<Vector2>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Vector3 : ValueOutput<Vector3>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Vector4 : ValueOutput<Vector4>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Quaternion : ValueOutput<Quaternion>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Keyframe : ValueOutput<Keyframe>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Bounds : ValueOutput<Bounds>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Color : ValueOutput<Color>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Rect : ValueOutput<Rect>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_ContactPoint : ValueOutput<ContactPoint>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_ContactPoint2D : ValueOutput<ContactPoint2D>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Collision : ValueOutput<Collision>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Collision2D : ValueOutput<Collision2D>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_RaycastHit : ValueOutput<RaycastHit>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_RaycastHit2D : ValueOutput<RaycastHit2D>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Ray : ValueOutput<Ray>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_Space : ValueOutput<Space>
	{
	}

	private class FlowCanvas_ValueOutput_Direction : ValueOutput<Direction>
	{
	}

	private class FlowCanvas_ValueOutput_ItemDefinition_EquipmentType : ValueOutput<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_ValueOutput_MovementComponent_GoToMethod : ValueOutput<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_ValueOutput_GDPoint_IdlePointPrefix : ValueOutput<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_ValueOutput_UnityEngine_LayerMask : ValueOutput<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_System_Boolean : AddDictionaryItem<bool>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_System_Single : AddDictionaryItem<float>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_System_Int32 : AddDictionaryItem<int>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Vector2 : AddDictionaryItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Vector3 : AddDictionaryItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Vector4 : AddDictionaryItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Quaternion : AddDictionaryItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Keyframe : AddDictionaryItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Bounds : AddDictionaryItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Color : AddDictionaryItem<Color>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Rect : AddDictionaryItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_ContactPoint : AddDictionaryItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_ContactPoint2D : AddDictionaryItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Collision : AddDictionaryItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Collision2D : AddDictionaryItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_RaycastHit : AddDictionaryItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_RaycastHit2D : AddDictionaryItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Ray : AddDictionaryItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_Space : AddDictionaryItem<Space>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_Direction : AddDictionaryItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_ItemDefinition_EquipmentType : AddDictionaryItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_MovementComponent_GoToMethod : AddDictionaryItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_GDPoint_IdlePointPrefix : AddDictionaryItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_AddDictionaryItem_UnityEngine_LayerMask : AddDictionaryItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_System_Boolean : AddListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_System_Single : AddListItem<float>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_System_Int32 : AddListItem<int>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Vector2 : AddListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Vector3 : AddListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Vector4 : AddListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Quaternion : AddListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Keyframe : AddListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Bounds : AddListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Color : AddListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Rect : AddListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_ContactPoint : AddListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_ContactPoint2D : AddListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Collision : AddListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Collision2D : AddListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_RaycastHit : AddListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_RaycastHit2D : AddListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Ray : AddListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_Space : AddListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_Direction : AddListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_ItemDefinition_EquipmentType : AddListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_MovementComponent_GoToMethod : AddListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_GDPoint_IdlePointPrefix : AddListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_AddListItem_UnityEngine_LayerMask : AddListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_Cache_System_Boolean : Cache<bool>
	{
	}

	private class FlowCanvas_Nodes_Cache_System_Single : Cache<float>
	{
	}

	private class FlowCanvas_Nodes_Cache_System_Int32 : Cache<int>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Vector2 : Cache<Vector2>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Vector3 : Cache<Vector3>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Vector4 : Cache<Vector4>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Quaternion : Cache<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Keyframe : Cache<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Bounds : Cache<Bounds>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Color : Cache<Color>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Rect : Cache<Rect>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_ContactPoint : Cache<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_ContactPoint2D : Cache<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Collision : Cache<Collision>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Collision2D : Cache<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_RaycastHit : Cache<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_RaycastHit2D : Cache<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Ray : Cache<Ray>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_Space : Cache<Space>
	{
	}

	private class FlowCanvas_Nodes_Cache_Direction : Cache<Direction>
	{
	}

	private class FlowCanvas_Nodes_Cache_ItemDefinition_EquipmentType : Cache<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_Cache_MovementComponent_GoToMethod : Cache<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_Cache_GDPoint_IdlePointPrefix : Cache<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_Cache_UnityEngine_LayerMask : Cache<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_System_Boolean : CreateCollection<bool>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_System_Single : CreateCollection<float>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_System_Int32 : CreateCollection<int>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Vector2 : CreateCollection<Vector2>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Vector3 : CreateCollection<Vector3>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Vector4 : CreateCollection<Vector4>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Quaternion : CreateCollection<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Keyframe : CreateCollection<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Bounds : CreateCollection<Bounds>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Color : CreateCollection<Color>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Rect : CreateCollection<Rect>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_ContactPoint : CreateCollection<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_ContactPoint2D : CreateCollection<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Collision : CreateCollection<Collision>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Collision2D : CreateCollection<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_RaycastHit : CreateCollection<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_RaycastHit2D : CreateCollection<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Ray : CreateCollection<Ray>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_Space : CreateCollection<Space>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_Direction : CreateCollection<Direction>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_ItemDefinition_EquipmentType : CreateCollection<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_MovementComponent_GoToMethod : CreateCollection<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_GDPoint_IdlePointPrefix : CreateCollection<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_CreateCollection_UnityEngine_LayerMask : CreateCollection<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_System_Boolean : CreateDictionary<bool>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_System_Single : CreateDictionary<float>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_System_Int32 : CreateDictionary<int>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Vector2 : CreateDictionary<Vector2>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Vector3 : CreateDictionary<Vector3>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Vector4 : CreateDictionary<Vector4>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Quaternion : CreateDictionary<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Keyframe : CreateDictionary<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Bounds : CreateDictionary<Bounds>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Color : CreateDictionary<Color>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Rect : CreateDictionary<Rect>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_ContactPoint : CreateDictionary<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_ContactPoint2D : CreateDictionary<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Collision : CreateDictionary<Collision>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Collision2D : CreateDictionary<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_RaycastHit : CreateDictionary<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_RaycastHit2D : CreateDictionary<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Ray : CreateDictionary<Ray>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_Space : CreateDictionary<Space>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_Direction : CreateDictionary<Direction>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_ItemDefinition_EquipmentType : CreateDictionary<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_MovementComponent_GoToMethod : CreateDictionary<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_GDPoint_IdlePointPrefix : CreateDictionary<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_CreateDictionary_UnityEngine_LayerMask : CreateDictionary<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_System_Boolean : CustomEvent<bool>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_System_Single : CustomEvent<float>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_System_Int32 : CustomEvent<int>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Vector2 : CustomEvent<Vector2>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Vector3 : CustomEvent<Vector3>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Vector4 : CustomEvent<Vector4>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Quaternion : CustomEvent<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Keyframe : CustomEvent<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Bounds : CustomEvent<Bounds>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Color : CustomEvent<Color>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Rect : CustomEvent<Rect>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_ContactPoint : CustomEvent<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_ContactPoint2D : CustomEvent<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Collision : CustomEvent<Collision>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Collision2D : CustomEvent<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_RaycastHit : CustomEvent<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_RaycastHit2D : CustomEvent<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Ray : CustomEvent<Ray>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_Space : CustomEvent<Space>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_Direction : CustomEvent<Direction>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_ItemDefinition_EquipmentType : CustomEvent<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_MovementComponent_GoToMethod : CustomEvent<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_GDPoint_IdlePointPrefix : CustomEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_CustomEvent_UnityEngine_LayerMask : CustomEvent<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_System_Boolean : DictionaryContainsKey<bool>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_System_Single : DictionaryContainsKey<float>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_System_Int32 : DictionaryContainsKey<int>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Vector2 : DictionaryContainsKey<Vector2>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Vector3 : DictionaryContainsKey<Vector3>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Vector4 : DictionaryContainsKey<Vector4>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Quaternion : DictionaryContainsKey<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Keyframe : DictionaryContainsKey<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Bounds : DictionaryContainsKey<Bounds>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Color : DictionaryContainsKey<Color>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Rect : DictionaryContainsKey<Rect>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_ContactPoint : DictionaryContainsKey<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_ContactPoint2D : DictionaryContainsKey<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Collision : DictionaryContainsKey<Collision>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Collision2D : DictionaryContainsKey<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_RaycastHit : DictionaryContainsKey<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_RaycastHit2D : DictionaryContainsKey<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Ray : DictionaryContainsKey<Ray>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_Space : DictionaryContainsKey<Space>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_Direction : DictionaryContainsKey<Direction>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_ItemDefinition_EquipmentType : DictionaryContainsKey<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_MovementComponent_GoToMethod : DictionaryContainsKey<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_GDPoint_IdlePointPrefix : DictionaryContainsKey<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_DictionaryContainsKey_UnityEngine_LayerMask : DictionaryContainsKey<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_System_Boolean : Flow_WaitObj<bool>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_System_Single : Flow_WaitObj<float>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_System_Int32 : Flow_WaitObj<int>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Vector2 : Flow_WaitObj<Vector2>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Vector3 : Flow_WaitObj<Vector3>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Vector4 : Flow_WaitObj<Vector4>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Quaternion : Flow_WaitObj<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Keyframe : Flow_WaitObj<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Bounds : Flow_WaitObj<Bounds>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Color : Flow_WaitObj<Color>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Rect : Flow_WaitObj<Rect>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_ContactPoint : Flow_WaitObj<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_ContactPoint2D : Flow_WaitObj<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Collision : Flow_WaitObj<Collision>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Collision2D : Flow_WaitObj<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_RaycastHit : Flow_WaitObj<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_RaycastHit2D : Flow_WaitObj<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Ray : Flow_WaitObj<Ray>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_Space : Flow_WaitObj<Space>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_Direction : Flow_WaitObj<Direction>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_ItemDefinition_EquipmentType : Flow_WaitObj<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_MovementComponent_GoToMethod : Flow_WaitObj<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_GDPoint_IdlePointPrefix : Flow_WaitObj<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_Flow_WaitObj_UnityEngine_LayerMask : Flow_WaitObj<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_ForEach_System_Boolean : ForEach<bool>
	{
	}

	private class FlowCanvas_Nodes_ForEach_System_Single : ForEach<float>
	{
	}

	private class FlowCanvas_Nodes_ForEach_System_Int32 : ForEach<int>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Vector2 : ForEach<Vector2>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Vector3 : ForEach<Vector3>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Vector4 : ForEach<Vector4>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Quaternion : ForEach<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Keyframe : ForEach<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Bounds : ForEach<Bounds>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Color : ForEach<Color>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Rect : ForEach<Rect>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_ContactPoint : ForEach<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_ContactPoint2D : ForEach<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Collision : ForEach<Collision>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Collision2D : ForEach<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_RaycastHit : ForEach<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_RaycastHit2D : ForEach<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Ray : ForEach<Ray>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_Space : ForEach<Space>
	{
	}

	private class FlowCanvas_Nodes_ForEach_Direction : ForEach<Direction>
	{
	}

	private class FlowCanvas_Nodes_ForEach_ItemDefinition_EquipmentType : ForEach<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_ForEach_MovementComponent_GoToMethod : ForEach<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_ForEach_GDPoint_IdlePointPrefix : ForEach<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_ForEach_UnityEngine_LayerMask : ForEach<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_System_Boolean : GetDictionaryItem<bool>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_System_Single : GetDictionaryItem<float>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_System_Int32 : GetDictionaryItem<int>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Vector2 : GetDictionaryItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Vector3 : GetDictionaryItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Vector4 : GetDictionaryItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Quaternion : GetDictionaryItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Keyframe : GetDictionaryItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Bounds : GetDictionaryItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Color : GetDictionaryItem<Color>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Rect : GetDictionaryItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_ContactPoint : GetDictionaryItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_ContactPoint2D : GetDictionaryItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Collision : GetDictionaryItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Collision2D : GetDictionaryItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_RaycastHit : GetDictionaryItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_RaycastHit2D : GetDictionaryItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Ray : GetDictionaryItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_Space : GetDictionaryItem<Space>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_Direction : GetDictionaryItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_ItemDefinition_EquipmentType : GetDictionaryItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_MovementComponent_GoToMethod : GetDictionaryItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_GDPoint_IdlePointPrefix : GetDictionaryItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_GetDictionaryItem_UnityEngine_LayerMask : GetDictionaryItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_System_Boolean : GetFirstListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_System_Single : GetFirstListItem<float>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_System_Int32 : GetFirstListItem<int>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Vector2 : GetFirstListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Vector3 : GetFirstListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Vector4 : GetFirstListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Quaternion : GetFirstListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Keyframe : GetFirstListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Bounds : GetFirstListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Color : GetFirstListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Rect : GetFirstListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_ContactPoint : GetFirstListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_ContactPoint2D : GetFirstListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Collision : GetFirstListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Collision2D : GetFirstListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_RaycastHit : GetFirstListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_RaycastHit2D : GetFirstListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Ray : GetFirstListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_Space : GetFirstListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_Direction : GetFirstListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_ItemDefinition_EquipmentType : GetFirstListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_MovementComponent_GoToMethod : GetFirstListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_GDPoint_IdlePointPrefix : GetFirstListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_GetFirstListItem_UnityEngine_LayerMask : GetFirstListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_System_Boolean : GetLastListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_System_Single : GetLastListItem<float>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_System_Int32 : GetLastListItem<int>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Vector2 : GetLastListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Vector3 : GetLastListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Vector4 : GetLastListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Quaternion : GetLastListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Keyframe : GetLastListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Bounds : GetLastListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Color : GetLastListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Rect : GetLastListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_ContactPoint : GetLastListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_ContactPoint2D : GetLastListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Collision : GetLastListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Collision2D : GetLastListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_RaycastHit : GetLastListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_RaycastHit2D : GetLastListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Ray : GetLastListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_Space : GetLastListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_Direction : GetLastListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_ItemDefinition_EquipmentType : GetLastListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_MovementComponent_GoToMethod : GetLastListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_GDPoint_IdlePointPrefix : GetLastListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_GetLastListItem_UnityEngine_LayerMask : GetLastListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_System_Boolean : GetListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_System_Single : GetListItem<float>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_System_Int32 : GetListItem<int>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Vector2 : GetListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Vector3 : GetListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Vector4 : GetListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Quaternion : GetListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Keyframe : GetListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Bounds : GetListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Color : GetListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Rect : GetListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_ContactPoint : GetListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_ContactPoint2D : GetListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Collision : GetListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Collision2D : GetListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_RaycastHit : GetListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_RaycastHit2D : GetListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Ray : GetListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_Space : GetListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_Direction : GetListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_ItemDefinition_EquipmentType : GetListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_MovementComponent_GoToMethod : GetListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_GDPoint_IdlePointPrefix : GetListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_GetListItem_UnityEngine_LayerMask : GetListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_System_Boolean : GetOtherVariable<bool>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_System_Single : GetOtherVariable<float>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_System_Int32 : GetOtherVariable<int>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Vector2 : GetOtherVariable<Vector2>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Vector3 : GetOtherVariable<Vector3>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Vector4 : GetOtherVariable<Vector4>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Quaternion : GetOtherVariable<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Keyframe : GetOtherVariable<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Bounds : GetOtherVariable<Bounds>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Color : GetOtherVariable<Color>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Rect : GetOtherVariable<Rect>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_ContactPoint : GetOtherVariable<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_ContactPoint2D : GetOtherVariable<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Collision : GetOtherVariable<Collision>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Collision2D : GetOtherVariable<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_RaycastHit : GetOtherVariable<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_RaycastHit2D : GetOtherVariable<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Ray : GetOtherVariable<Ray>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_Space : GetOtherVariable<Space>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_Direction : GetOtherVariable<Direction>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_ItemDefinition_EquipmentType : GetOtherVariable<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_MovementComponent_GoToMethod : GetOtherVariable<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_GDPoint_IdlePointPrefix : GetOtherVariable<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_GetOtherVariable_UnityEngine_LayerMask : GetOtherVariable<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_System_Boolean : GetRandomListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_System_Single : GetRandomListItem<float>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_System_Int32 : GetRandomListItem<int>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Vector2 : GetRandomListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Vector3 : GetRandomListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Vector4 : GetRandomListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Quaternion : GetRandomListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Keyframe : GetRandomListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Bounds : GetRandomListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Color : GetRandomListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Rect : GetRandomListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_ContactPoint : GetRandomListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_ContactPoint2D : GetRandomListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Collision : GetRandomListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Collision2D : GetRandomListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_RaycastHit : GetRandomListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_RaycastHit2D : GetRandomListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Ray : GetRandomListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_Space : GetRandomListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_Direction : GetRandomListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_ItemDefinition_EquipmentType : GetRandomListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_MovementComponent_GoToMethod : GetRandomListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_GDPoint_IdlePointPrefix : GetRandomListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_GetRandomListItem_UnityEngine_LayerMask : GetRandomListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_System_Boolean : GetVariable<bool>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_System_Single : GetVariable<float>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_System_Int32 : GetVariable<int>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Vector2 : GetVariable<Vector2>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Vector3 : GetVariable<Vector3>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Vector4 : GetVariable<Vector4>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Quaternion : GetVariable<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Keyframe : GetVariable<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Bounds : GetVariable<Bounds>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Color : GetVariable<Color>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Rect : GetVariable<Rect>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_ContactPoint : GetVariable<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_ContactPoint2D : GetVariable<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Collision : GetVariable<Collision>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Collision2D : GetVariable<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_RaycastHit : GetVariable<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_RaycastHit2D : GetVariable<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Ray : GetVariable<Ray>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_Space : GetVariable<Space>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_Direction : GetVariable<Direction>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_ItemDefinition_EquipmentType : GetVariable<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_MovementComponent_GoToMethod : GetVariable<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_GDPoint_IdlePointPrefix : GetVariable<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_GetVariable_UnityEngine_LayerMask : GetVariable<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_Identity_System_Boolean : Identity<bool>
	{
	}

	private class FlowCanvas_Nodes_Identity_System_Single : Identity<float>
	{
	}

	private class FlowCanvas_Nodes_Identity_System_Int32 : Identity<int>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Vector2 : Identity<Vector2>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Vector3 : Identity<Vector3>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Vector4 : Identity<Vector4>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Quaternion : Identity<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Keyframe : Identity<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Bounds : Identity<Bounds>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Color : Identity<Color>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Rect : Identity<Rect>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_ContactPoint : Identity<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_ContactPoint2D : Identity<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Collision : Identity<Collision>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Collision2D : Identity<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_RaycastHit : Identity<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_RaycastHit2D : Identity<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Ray : Identity<Ray>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_Space : Identity<Space>
	{
	}

	private class FlowCanvas_Nodes_Identity_Direction : Identity<Direction>
	{
	}

	private class FlowCanvas_Nodes_Identity_ItemDefinition_EquipmentType : Identity<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_Identity_MovementComponent_GoToMethod : Identity<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_Identity_GDPoint_IdlePointPrefix : Identity<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_Identity_UnityEngine_LayerMask : Identity<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_System_Boolean : InsertListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_System_Single : InsertListItem<float>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_System_Int32 : InsertListItem<int>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Vector2 : InsertListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Vector3 : InsertListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Vector4 : InsertListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Quaternion : InsertListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Keyframe : InsertListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Bounds : InsertListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Color : InsertListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Rect : InsertListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_ContactPoint : InsertListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_ContactPoint2D : InsertListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Collision : InsertListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Collision2D : InsertListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_RaycastHit : InsertListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_RaycastHit2D : InsertListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Ray : InsertListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_Space : InsertListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_Direction : InsertListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_ItemDefinition_EquipmentType : InsertListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_MovementComponent_GoToMethod : InsertListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_GDPoint_IdlePointPrefix : InsertListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_InsertListItem_UnityEngine_LayerMask : InsertListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_System_Boolean : ReadFlowParameter<bool>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_System_Single : ReadFlowParameter<float>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_System_Int32 : ReadFlowParameter<int>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Vector2 : ReadFlowParameter<Vector2>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Vector3 : ReadFlowParameter<Vector3>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Vector4 : ReadFlowParameter<Vector4>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Quaternion : ReadFlowParameter<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Keyframe : ReadFlowParameter<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Bounds : ReadFlowParameter<Bounds>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Color : ReadFlowParameter<Color>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Rect : ReadFlowParameter<Rect>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_ContactPoint : ReadFlowParameter<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_ContactPoint2D : ReadFlowParameter<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Collision : ReadFlowParameter<Collision>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Collision2D : ReadFlowParameter<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_RaycastHit : ReadFlowParameter<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_RaycastHit2D : ReadFlowParameter<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Ray : ReadFlowParameter<Ray>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_Space : ReadFlowParameter<Space>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_Direction : ReadFlowParameter<Direction>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_ItemDefinition_EquipmentType : ReadFlowParameter<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_MovementComponent_GoToMethod : ReadFlowParameter<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_GDPoint_IdlePointPrefix : ReadFlowParameter<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_ReadFlowParameter_UnityEngine_LayerMask : ReadFlowParameter<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_System_Boolean : ReflectedExtractorNodeWrapper<bool>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_System_Single : ReflectedExtractorNodeWrapper<float>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_System_Int32 : ReflectedExtractorNodeWrapper<int>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Vector2 : ReflectedExtractorNodeWrapper<Vector2>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Vector3 : ReflectedExtractorNodeWrapper<Vector3>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Vector4 : ReflectedExtractorNodeWrapper<Vector4>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Quaternion : ReflectedExtractorNodeWrapper<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Keyframe : ReflectedExtractorNodeWrapper<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Bounds : ReflectedExtractorNodeWrapper<Bounds>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Color : ReflectedExtractorNodeWrapper<Color>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Rect : ReflectedExtractorNodeWrapper<Rect>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_ContactPoint : ReflectedExtractorNodeWrapper<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_ContactPoint2D : ReflectedExtractorNodeWrapper<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Collision : ReflectedExtractorNodeWrapper<Collision>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Collision2D : ReflectedExtractorNodeWrapper<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_RaycastHit : ReflectedExtractorNodeWrapper<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_RaycastHit2D : ReflectedExtractorNodeWrapper<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Ray : ReflectedExtractorNodeWrapper<Ray>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_Space : ReflectedExtractorNodeWrapper<Space>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_Direction : ReflectedExtractorNodeWrapper<Direction>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_ItemDefinition_EquipmentType : ReflectedExtractorNodeWrapper<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_MovementComponent_GoToMethod : ReflectedExtractorNodeWrapper<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_GDPoint_IdlePointPrefix : ReflectedExtractorNodeWrapper<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_ReflectedExtractorNodeWrapper_UnityEngine_LayerMask : ReflectedExtractorNodeWrapper<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_System_Boolean : RelayValueInput<bool>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_System_Single : RelayValueInput<float>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_System_Int32 : RelayValueInput<int>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Vector2 : RelayValueInput<Vector2>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Vector3 : RelayValueInput<Vector3>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Vector4 : RelayValueInput<Vector4>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Quaternion : RelayValueInput<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Keyframe : RelayValueInput<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Bounds : RelayValueInput<Bounds>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Color : RelayValueInput<Color>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Rect : RelayValueInput<Rect>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_ContactPoint : RelayValueInput<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_ContactPoint2D : RelayValueInput<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Collision : RelayValueInput<Collision>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Collision2D : RelayValueInput<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_RaycastHit : RelayValueInput<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_RaycastHit2D : RelayValueInput<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Ray : RelayValueInput<Ray>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_Space : RelayValueInput<Space>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_Direction : RelayValueInput<Direction>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_ItemDefinition_EquipmentType : RelayValueInput<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_MovementComponent_GoToMethod : RelayValueInput<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_GDPoint_IdlePointPrefix : RelayValueInput<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_RelayValueInput_UnityEngine_LayerMask : RelayValueInput<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_System_Boolean : RelayValueOutput<bool>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_System_Single : RelayValueOutput<float>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_System_Int32 : RelayValueOutput<int>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Vector2 : RelayValueOutput<Vector2>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Vector3 : RelayValueOutput<Vector3>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Vector4 : RelayValueOutput<Vector4>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Quaternion : RelayValueOutput<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Keyframe : RelayValueOutput<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Bounds : RelayValueOutput<Bounds>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Color : RelayValueOutput<Color>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Rect : RelayValueOutput<Rect>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_ContactPoint : RelayValueOutput<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_ContactPoint2D : RelayValueOutput<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Collision : RelayValueOutput<Collision>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Collision2D : RelayValueOutput<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_RaycastHit : RelayValueOutput<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_RaycastHit2D : RelayValueOutput<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Ray : RelayValueOutput<Ray>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_Space : RelayValueOutput<Space>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_Direction : RelayValueOutput<Direction>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_ItemDefinition_EquipmentType : RelayValueOutput<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_MovementComponent_GoToMethod : RelayValueOutput<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_GDPoint_IdlePointPrefix : RelayValueOutput<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_RelayValueOutput_UnityEngine_LayerMask : RelayValueOutput<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_System_Boolean : RemoveDictionaryKey<bool>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_System_Single : RemoveDictionaryKey<float>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_System_Int32 : RemoveDictionaryKey<int>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Vector2 : RemoveDictionaryKey<Vector2>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Vector3 : RemoveDictionaryKey<Vector3>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Vector4 : RemoveDictionaryKey<Vector4>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Quaternion : RemoveDictionaryKey<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Keyframe : RemoveDictionaryKey<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Bounds : RemoveDictionaryKey<Bounds>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Color : RemoveDictionaryKey<Color>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Rect : RemoveDictionaryKey<Rect>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_ContactPoint : RemoveDictionaryKey<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_ContactPoint2D : RemoveDictionaryKey<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Collision : RemoveDictionaryKey<Collision>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Collision2D : RemoveDictionaryKey<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_RaycastHit : RemoveDictionaryKey<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_RaycastHit2D : RemoveDictionaryKey<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Ray : RemoveDictionaryKey<Ray>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_Space : RemoveDictionaryKey<Space>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_Direction : RemoveDictionaryKey<Direction>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_ItemDefinition_EquipmentType : RemoveDictionaryKey<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_MovementComponent_GoToMethod : RemoveDictionaryKey<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_GDPoint_IdlePointPrefix : RemoveDictionaryKey<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_RemoveDictionaryKey_UnityEngine_LayerMask : RemoveDictionaryKey<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_System_Boolean : RemoveListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_System_Single : RemoveListItem<float>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_System_Int32 : RemoveListItem<int>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Vector2 : RemoveListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Vector3 : RemoveListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Vector4 : RemoveListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Quaternion : RemoveListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Keyframe : RemoveListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Bounds : RemoveListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Color : RemoveListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Rect : RemoveListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_ContactPoint : RemoveListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_ContactPoint2D : RemoveListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Collision : RemoveListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Collision2D : RemoveListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_RaycastHit : RemoveListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_RaycastHit2D : RemoveListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Ray : RemoveListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_Space : RemoveListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_Direction : RemoveListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_ItemDefinition_EquipmentType : RemoveListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_MovementComponent_GoToMethod : RemoveListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_GDPoint_IdlePointPrefix : RemoveListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItem_UnityEngine_LayerMask : RemoveListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_System_Boolean : RemoveListItemAt<bool>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_System_Single : RemoveListItemAt<float>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_System_Int32 : RemoveListItemAt<int>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Vector2 : RemoveListItemAt<Vector2>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Vector3 : RemoveListItemAt<Vector3>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Vector4 : RemoveListItemAt<Vector4>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Quaternion : RemoveListItemAt<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Keyframe : RemoveListItemAt<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Bounds : RemoveListItemAt<Bounds>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Color : RemoveListItemAt<Color>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Rect : RemoveListItemAt<Rect>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_ContactPoint : RemoveListItemAt<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_ContactPoint2D : RemoveListItemAt<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Collision : RemoveListItemAt<Collision>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Collision2D : RemoveListItemAt<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_RaycastHit : RemoveListItemAt<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_RaycastHit2D : RemoveListItemAt<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Ray : RemoveListItemAt<Ray>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_Space : RemoveListItemAt<Space>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_Direction : RemoveListItemAt<Direction>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_ItemDefinition_EquipmentType : RemoveListItemAt<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_MovementComponent_GoToMethod : RemoveListItemAt<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_GDPoint_IdlePointPrefix : RemoveListItemAt<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_RemoveListItemAt_UnityEngine_LayerMask : RemoveListItemAt<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_System_Boolean : FlowCanvas.Nodes.SendEvent<bool>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_System_Single : FlowCanvas.Nodes.SendEvent<float>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_System_Int32 : FlowCanvas.Nodes.SendEvent<int>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Vector2 : FlowCanvas.Nodes.SendEvent<Vector2>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Vector3 : FlowCanvas.Nodes.SendEvent<Vector3>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Vector4 : FlowCanvas.Nodes.SendEvent<Vector4>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Quaternion : FlowCanvas.Nodes.SendEvent<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Keyframe : FlowCanvas.Nodes.SendEvent<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Bounds : FlowCanvas.Nodes.SendEvent<Bounds>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Color : FlowCanvas.Nodes.SendEvent<Color>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Rect : FlowCanvas.Nodes.SendEvent<Rect>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_ContactPoint : FlowCanvas.Nodes.SendEvent<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_ContactPoint2D : FlowCanvas.Nodes.SendEvent<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Collision : FlowCanvas.Nodes.SendEvent<Collision>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Collision2D : FlowCanvas.Nodes.SendEvent<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_RaycastHit : FlowCanvas.Nodes.SendEvent<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_RaycastHit2D : FlowCanvas.Nodes.SendEvent<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Ray : FlowCanvas.Nodes.SendEvent<Ray>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_Space : FlowCanvas.Nodes.SendEvent<Space>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_Direction : FlowCanvas.Nodes.SendEvent<Direction>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_ItemDefinition_EquipmentType : FlowCanvas.Nodes.SendEvent<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_MovementComponent_GoToMethod : FlowCanvas.Nodes.SendEvent<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_GDPoint_IdlePointPrefix : FlowCanvas.Nodes.SendEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_SendEvent_UnityEngine_LayerMask : FlowCanvas.Nodes.SendEvent<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_System_Boolean : SendGlobalEvent<bool>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_System_Single : SendGlobalEvent<float>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_System_Int32 : SendGlobalEvent<int>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Vector2 : SendGlobalEvent<Vector2>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Vector3 : SendGlobalEvent<Vector3>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Vector4 : SendGlobalEvent<Vector4>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Quaternion : SendGlobalEvent<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Keyframe : SendGlobalEvent<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Bounds : SendGlobalEvent<Bounds>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Color : SendGlobalEvent<Color>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Rect : SendGlobalEvent<Rect>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_ContactPoint : SendGlobalEvent<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_ContactPoint2D : SendGlobalEvent<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Collision : SendGlobalEvent<Collision>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Collision2D : SendGlobalEvent<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_RaycastHit : SendGlobalEvent<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_RaycastHit2D : SendGlobalEvent<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Ray : SendGlobalEvent<Ray>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_Space : SendGlobalEvent<Space>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_Direction : SendGlobalEvent<Direction>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_ItemDefinition_EquipmentType : SendGlobalEvent<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_MovementComponent_GoToMethod : SendGlobalEvent<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_GDPoint_IdlePointPrefix : SendGlobalEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_SendGlobalEvent_UnityEngine_LayerMask : SendGlobalEvent<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_System_Boolean : SetListItem<bool>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_System_Single : SetListItem<float>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_System_Int32 : SetListItem<int>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Vector2 : SetListItem<Vector2>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Vector3 : SetListItem<Vector3>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Vector4 : SetListItem<Vector4>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Quaternion : SetListItem<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Keyframe : SetListItem<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Bounds : SetListItem<Bounds>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Color : SetListItem<Color>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Rect : SetListItem<Rect>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_ContactPoint : SetListItem<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_ContactPoint2D : SetListItem<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Collision : SetListItem<Collision>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Collision2D : SetListItem<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_RaycastHit : SetListItem<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_RaycastHit2D : SetListItem<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Ray : SetListItem<Ray>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_Space : SetListItem<Space>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_Direction : SetListItem<Direction>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_ItemDefinition_EquipmentType : SetListItem<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_MovementComponent_GoToMethod : SetListItem<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_GDPoint_IdlePointPrefix : SetListItem<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_SetListItem_UnityEngine_LayerMask : SetListItem<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_System_Boolean : SetOtherVariable<bool>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_System_Single : SetOtherVariable<float>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_System_Int32 : SetOtherVariable<int>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Vector2 : SetOtherVariable<Vector2>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Vector3 : SetOtherVariable<Vector3>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Vector4 : SetOtherVariable<Vector4>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Quaternion : SetOtherVariable<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Keyframe : SetOtherVariable<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Bounds : SetOtherVariable<Bounds>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Color : SetOtherVariable<Color>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Rect : SetOtherVariable<Rect>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_ContactPoint : SetOtherVariable<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_ContactPoint2D : SetOtherVariable<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Collision : SetOtherVariable<Collision>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Collision2D : SetOtherVariable<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_RaycastHit : SetOtherVariable<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_RaycastHit2D : SetOtherVariable<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Ray : SetOtherVariable<Ray>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_Space : SetOtherVariable<Space>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_Direction : SetOtherVariable<Direction>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_ItemDefinition_EquipmentType : SetOtherVariable<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_MovementComponent_GoToMethod : SetOtherVariable<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_GDPoint_IdlePointPrefix : SetOtherVariable<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_SetOtherVariable_UnityEngine_LayerMask : SetOtherVariable<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_System_Boolean : FlowCanvas.Nodes.SetVariable<bool>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_System_Single : FlowCanvas.Nodes.SetVariable<float>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_System_Int32 : FlowCanvas.Nodes.SetVariable<int>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Vector2 : FlowCanvas.Nodes.SetVariable<Vector2>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Vector3 : FlowCanvas.Nodes.SetVariable<Vector3>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Vector4 : FlowCanvas.Nodes.SetVariable<Vector4>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Quaternion : FlowCanvas.Nodes.SetVariable<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Keyframe : FlowCanvas.Nodes.SetVariable<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Bounds : FlowCanvas.Nodes.SetVariable<Bounds>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Color : FlowCanvas.Nodes.SetVariable<Color>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Rect : FlowCanvas.Nodes.SetVariable<Rect>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_ContactPoint : FlowCanvas.Nodes.SetVariable<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_ContactPoint2D : FlowCanvas.Nodes.SetVariable<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Collision : FlowCanvas.Nodes.SetVariable<Collision>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Collision2D : FlowCanvas.Nodes.SetVariable<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_RaycastHit : FlowCanvas.Nodes.SetVariable<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_RaycastHit2D : FlowCanvas.Nodes.SetVariable<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Ray : FlowCanvas.Nodes.SetVariable<Ray>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_Space : FlowCanvas.Nodes.SetVariable<Space>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_Direction : FlowCanvas.Nodes.SetVariable<Direction>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_ItemDefinition_EquipmentType : FlowCanvas.Nodes.SetVariable<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_MovementComponent_GoToMethod : FlowCanvas.Nodes.SetVariable<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_GDPoint_IdlePointPrefix : FlowCanvas.Nodes.SetVariable<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_SetVariable_UnityEngine_LayerMask : FlowCanvas.Nodes.SetVariable<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_System_Boolean : ShuffleList<bool>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_System_Single : ShuffleList<float>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_System_Int32 : ShuffleList<int>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Vector2 : ShuffleList<Vector2>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Vector3 : ShuffleList<Vector3>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Vector4 : ShuffleList<Vector4>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Quaternion : ShuffleList<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Keyframe : ShuffleList<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Bounds : ShuffleList<Bounds>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Color : ShuffleList<Color>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Rect : ShuffleList<Rect>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_ContactPoint : ShuffleList<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_ContactPoint2D : ShuffleList<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Collision : ShuffleList<Collision>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Collision2D : ShuffleList<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_RaycastHit : ShuffleList<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_RaycastHit2D : ShuffleList<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Ray : ShuffleList<Ray>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_Space : ShuffleList<Space>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_Direction : ShuffleList<Direction>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_ItemDefinition_EquipmentType : ShuffleList<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_MovementComponent_GoToMethod : ShuffleList<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_GDPoint_IdlePointPrefix : ShuffleList<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_ShuffleList_UnityEngine_LayerMask : ShuffleList<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_System_Boolean : SwitchValue<bool>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_System_Single : SwitchValue<float>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_System_Int32 : SwitchValue<int>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Vector2 : SwitchValue<Vector2>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Vector3 : SwitchValue<Vector3>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Vector4 : SwitchValue<Vector4>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Quaternion : SwitchValue<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Keyframe : SwitchValue<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Bounds : SwitchValue<Bounds>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Color : SwitchValue<Color>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Rect : SwitchValue<Rect>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_ContactPoint : SwitchValue<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_ContactPoint2D : SwitchValue<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Collision : SwitchValue<Collision>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Collision2D : SwitchValue<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_RaycastHit : SwitchValue<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_RaycastHit2D : SwitchValue<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Ray : SwitchValue<Ray>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_Space : SwitchValue<Space>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_Direction : SwitchValue<Direction>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_ItemDefinition_EquipmentType : SwitchValue<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_MovementComponent_GoToMethod : SwitchValue<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_GDPoint_IdlePointPrefix : SwitchValue<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_SwitchValue_UnityEngine_LayerMask : SwitchValue<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_System_Boolean : FlowCanvas.Nodes.TryGetValue<bool>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_System_Single : FlowCanvas.Nodes.TryGetValue<float>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_System_Int32 : FlowCanvas.Nodes.TryGetValue<int>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Vector2 : FlowCanvas.Nodes.TryGetValue<Vector2>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Vector3 : FlowCanvas.Nodes.TryGetValue<Vector3>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Vector4 : FlowCanvas.Nodes.TryGetValue<Vector4>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Quaternion : FlowCanvas.Nodes.TryGetValue<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Keyframe : FlowCanvas.Nodes.TryGetValue<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Bounds : FlowCanvas.Nodes.TryGetValue<Bounds>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Color : FlowCanvas.Nodes.TryGetValue<Color>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Rect : FlowCanvas.Nodes.TryGetValue<Rect>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_ContactPoint : FlowCanvas.Nodes.TryGetValue<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_ContactPoint2D : FlowCanvas.Nodes.TryGetValue<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Collision : FlowCanvas.Nodes.TryGetValue<Collision>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Collision2D : FlowCanvas.Nodes.TryGetValue<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_RaycastHit : FlowCanvas.Nodes.TryGetValue<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_RaycastHit2D : FlowCanvas.Nodes.TryGetValue<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Ray : FlowCanvas.Nodes.TryGetValue<Ray>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_Space : FlowCanvas.Nodes.TryGetValue<Space>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_Direction : FlowCanvas.Nodes.TryGetValue<Direction>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_ItemDefinition_EquipmentType : FlowCanvas.Nodes.TryGetValue<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_MovementComponent_GoToMethod : FlowCanvas.Nodes.TryGetValue<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_GDPoint_IdlePointPrefix : FlowCanvas.Nodes.TryGetValue<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_TryGetValue_UnityEngine_LayerMask : FlowCanvas.Nodes.TryGetValue<LayerMask>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_System_Boolean : WriteFlowParameter<bool>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_System_Single : WriteFlowParameter<float>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_System_Int32 : WriteFlowParameter<int>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Vector2 : WriteFlowParameter<Vector2>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Vector3 : WriteFlowParameter<Vector3>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Vector4 : WriteFlowParameter<Vector4>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Quaternion : WriteFlowParameter<Quaternion>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Keyframe : WriteFlowParameter<Keyframe>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Bounds : WriteFlowParameter<Bounds>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Color : WriteFlowParameter<Color>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Rect : WriteFlowParameter<Rect>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_ContactPoint : WriteFlowParameter<ContactPoint>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_ContactPoint2D : WriteFlowParameter<ContactPoint2D>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Collision : WriteFlowParameter<Collision>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Collision2D : WriteFlowParameter<Collision2D>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_RaycastHit : WriteFlowParameter<RaycastHit>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_RaycastHit2D : WriteFlowParameter<RaycastHit2D>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Ray : WriteFlowParameter<Ray>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_Space : WriteFlowParameter<Space>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_Direction : WriteFlowParameter<Direction>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_ItemDefinition_EquipmentType : WriteFlowParameter<ItemDefinition.EquipmentType>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_MovementComponent_GoToMethod : WriteFlowParameter<MovementComponent.GoToMethod>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_GDPoint_IdlePointPrefix : WriteFlowParameter<GDPoint.IdlePointPrefix>
	{
	}

	private class FlowCanvas_Nodes_WriteFlowParameter_UnityEngine_LayerMask : WriteFlowParameter<LayerMask>
	{
	}

	private class NodeCanvas_Framework_BBParameter_System_Boolean : BBParameter<bool>
	{
	}

	private class NodeCanvas_Framework_BBParameter_System_Single : BBParameter<float>
	{
	}

	private class NodeCanvas_Framework_BBParameter_System_Int32 : BBParameter<int>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Vector2 : BBParameter<Vector2>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Vector3 : BBParameter<Vector3>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Vector4 : BBParameter<Vector4>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Quaternion : BBParameter<Quaternion>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Keyframe : BBParameter<Keyframe>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Bounds : BBParameter<Bounds>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Color : BBParameter<Color>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Rect : BBParameter<Rect>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_ContactPoint : BBParameter<ContactPoint>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_ContactPoint2D : BBParameter<ContactPoint2D>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Collision : BBParameter<Collision>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Collision2D : BBParameter<Collision2D>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_RaycastHit : BBParameter<RaycastHit>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_RaycastHit2D : BBParameter<RaycastHit2D>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Ray : BBParameter<Ray>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_Space : BBParameter<Space>
	{
	}

	private class NodeCanvas_Framework_BBParameter_Direction : BBParameter<Direction>
	{
	}

	private class NodeCanvas_Framework_BBParameter_ItemDefinition_EquipmentType : BBParameter<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Framework_BBParameter_MovementComponent_GoToMethod : BBParameter<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Framework_BBParameter_GDPoint_IdlePointPrefix : BBParameter<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Framework_BBParameter_UnityEngine_LayerMask : BBParameter<LayerMask>
	{
	}

	private class NodeCanvas_Framework_Variable_System_Boolean : Variable<bool>
	{
	}

	private class NodeCanvas_Framework_Variable_System_Single : Variable<float>
	{
	}

	private class NodeCanvas_Framework_Variable_System_Int32 : Variable<int>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Vector2 : Variable<Vector2>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Vector3 : Variable<Vector3>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Vector4 : Variable<Vector4>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Quaternion : Variable<Quaternion>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Keyframe : Variable<Keyframe>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Bounds : Variable<Bounds>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Color : Variable<Color>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Rect : Variable<Rect>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_ContactPoint : Variable<ContactPoint>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_ContactPoint2D : Variable<ContactPoint2D>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Collision : Variable<Collision>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Collision2D : Variable<Collision2D>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_RaycastHit : Variable<RaycastHit>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_RaycastHit2D : Variable<RaycastHit2D>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Ray : Variable<Ray>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_Space : Variable<Space>
	{
	}

	private class NodeCanvas_Framework_Variable_Direction : Variable<Direction>
	{
	}

	private class NodeCanvas_Framework_Variable_ItemDefinition_EquipmentType : Variable<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Framework_Variable_MovementComponent_GoToMethod : Variable<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Framework_Variable_GDPoint_IdlePointPrefix : Variable<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Framework_Variable_UnityEngine_LayerMask : Variable<LayerMask>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_System_Boolean : ReflectedAction<bool>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_System_Single : ReflectedAction<float>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_System_Int32 : ReflectedAction<int>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Vector2 : ReflectedAction<Vector2>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Vector3 : ReflectedAction<Vector3>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Vector4 : ReflectedAction<Vector4>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Quaternion : ReflectedAction<Quaternion>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Keyframe : ReflectedAction<Keyframe>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Bounds : ReflectedAction<Bounds>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Color : ReflectedAction<Color>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Rect : ReflectedAction<Rect>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_ContactPoint : ReflectedAction<ContactPoint>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_ContactPoint2D : ReflectedAction<ContactPoint2D>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Collision : ReflectedAction<Collision>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Collision2D : ReflectedAction<Collision2D>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_RaycastHit : ReflectedAction<RaycastHit>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_RaycastHit2D : ReflectedAction<RaycastHit2D>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Ray : ReflectedAction<Ray>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_Space : ReflectedAction<Space>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_Direction : ReflectedAction<Direction>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_ItemDefinition_EquipmentType : ReflectedAction<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_MovementComponent_GoToMethod : ReflectedAction<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_GDPoint_IdlePointPrefix : ReflectedAction<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedAction_UnityEngine_LayerMask : ReflectedAction<LayerMask>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_System_Boolean : ReflectedFunction<bool>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_System_Single : ReflectedFunction<float>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_System_Int32 : ReflectedFunction<int>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Vector2 : ReflectedFunction<Vector2>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Vector3 : ReflectedFunction<Vector3>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Vector4 : ReflectedFunction<Vector4>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Quaternion : ReflectedFunction<Quaternion>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Keyframe : ReflectedFunction<Keyframe>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Bounds : ReflectedFunction<Bounds>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Color : ReflectedFunction<Color>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Rect : ReflectedFunction<Rect>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_ContactPoint : ReflectedFunction<ContactPoint>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_ContactPoint2D : ReflectedFunction<ContactPoint2D>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Collision : ReflectedFunction<Collision>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Collision2D : ReflectedFunction<Collision2D>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_RaycastHit : ReflectedFunction<RaycastHit>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_RaycastHit2D : ReflectedFunction<RaycastHit2D>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Ray : ReflectedFunction<Ray>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_Space : ReflectedFunction<Space>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_Direction : ReflectedFunction<Direction>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_ItemDefinition_EquipmentType : ReflectedFunction<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_MovementComponent_GoToMethod : ReflectedFunction<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_GDPoint_IdlePointPrefix : ReflectedFunction<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Framework_Internal_ReflectedFunction_UnityEngine_LayerMask : ReflectedFunction<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_System_Boolean : AddElementToDictionary<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_System_Single : AddElementToDictionary<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_System_Int32 : AddElementToDictionary<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Vector2 : AddElementToDictionary<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Vector3 : AddElementToDictionary<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Vector4 : AddElementToDictionary<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Quaternion : AddElementToDictionary<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Keyframe : AddElementToDictionary<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Bounds : AddElementToDictionary<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Color : AddElementToDictionary<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Rect : AddElementToDictionary<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_ContactPoint : AddElementToDictionary<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_ContactPoint2D : AddElementToDictionary<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Collision : AddElementToDictionary<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Collision2D : AddElementToDictionary<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_RaycastHit : AddElementToDictionary<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_RaycastHit2D : AddElementToDictionary<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Ray : AddElementToDictionary<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_Space : AddElementToDictionary<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_Direction : AddElementToDictionary<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_ItemDefinition_EquipmentType : AddElementToDictionary<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_MovementComponent_GoToMethod : AddElementToDictionary<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_GDPoint_IdlePointPrefix : AddElementToDictionary<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToDictionary_UnityEngine_LayerMask : AddElementToDictionary<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_System_Boolean : AddElementToList<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_System_Single : AddElementToList<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_System_Int32 : AddElementToList<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Vector2 : AddElementToList<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Vector3 : AddElementToList<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Vector4 : AddElementToList<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Quaternion : AddElementToList<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Keyframe : AddElementToList<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Bounds : AddElementToList<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Color : AddElementToList<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Rect : AddElementToList<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_ContactPoint : AddElementToList<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_ContactPoint2D : AddElementToList<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Collision : AddElementToList<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Collision2D : AddElementToList<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_RaycastHit : AddElementToList<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_RaycastHit2D : AddElementToList<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Ray : AddElementToList<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_Space : AddElementToList<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_Direction : AddElementToList<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_ItemDefinition_EquipmentType : AddElementToList<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_MovementComponent_GoToMethod : AddElementToList<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_GDPoint_IdlePointPrefix : AddElementToList<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_AddElementToList_UnityEngine_LayerMask : AddElementToList<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_System_Boolean : GetDictionaryElement<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_System_Single : GetDictionaryElement<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_System_Int32 : GetDictionaryElement<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Vector2 : GetDictionaryElement<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Vector3 : GetDictionaryElement<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Vector4 : GetDictionaryElement<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Quaternion : GetDictionaryElement<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Keyframe : GetDictionaryElement<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Bounds : GetDictionaryElement<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Color : GetDictionaryElement<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Rect : GetDictionaryElement<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_ContactPoint : GetDictionaryElement<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_ContactPoint2D : GetDictionaryElement<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Collision : GetDictionaryElement<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Collision2D : GetDictionaryElement<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_RaycastHit : GetDictionaryElement<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_RaycastHit2D : GetDictionaryElement<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Ray : GetDictionaryElement<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_Space : GetDictionaryElement<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_Direction : GetDictionaryElement<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_ItemDefinition_EquipmentType : GetDictionaryElement<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_MovementComponent_GoToMethod : GetDictionaryElement<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_GDPoint_IdlePointPrefix : GetDictionaryElement<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetDictionaryElement_UnityEngine_LayerMask : GetDictionaryElement<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_System_Boolean : GetIndexOfElement<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_System_Single : GetIndexOfElement<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_System_Int32 : GetIndexOfElement<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Vector2 : GetIndexOfElement<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Vector3 : GetIndexOfElement<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Vector4 : GetIndexOfElement<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Quaternion : GetIndexOfElement<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Keyframe : GetIndexOfElement<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Bounds : GetIndexOfElement<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Color : GetIndexOfElement<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Rect : GetIndexOfElement<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_ContactPoint : GetIndexOfElement<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_ContactPoint2D : GetIndexOfElement<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Collision : GetIndexOfElement<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Collision2D : GetIndexOfElement<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_RaycastHit : GetIndexOfElement<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_RaycastHit2D : GetIndexOfElement<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Ray : GetIndexOfElement<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_Space : GetIndexOfElement<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_Direction : GetIndexOfElement<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_ItemDefinition_EquipmentType : GetIndexOfElement<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_MovementComponent_GoToMethod : GetIndexOfElement<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_GDPoint_IdlePointPrefix : GetIndexOfElement<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_GetIndexOfElement_UnityEngine_LayerMask : GetIndexOfElement<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_System_Boolean : InsertElementToList<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_System_Single : InsertElementToList<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_System_Int32 : InsertElementToList<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Vector2 : InsertElementToList<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Vector3 : InsertElementToList<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Vector4 : InsertElementToList<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Quaternion : InsertElementToList<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Keyframe : InsertElementToList<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Bounds : InsertElementToList<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Color : InsertElementToList<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Rect : InsertElementToList<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_ContactPoint : InsertElementToList<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_ContactPoint2D : InsertElementToList<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Collision : InsertElementToList<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Collision2D : InsertElementToList<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_RaycastHit : InsertElementToList<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_RaycastHit2D : InsertElementToList<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Ray : InsertElementToList<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_Space : InsertElementToList<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_Direction : InsertElementToList<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_ItemDefinition_EquipmentType : InsertElementToList<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_MovementComponent_GoToMethod : InsertElementToList<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_GDPoint_IdlePointPrefix : InsertElementToList<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_InsertElementToList_UnityEngine_LayerMask : InsertElementToList<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_System_Boolean : PickListElement<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_System_Single : PickListElement<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_System_Int32 : PickListElement<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Vector2 : PickListElement<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Vector3 : PickListElement<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Vector4 : PickListElement<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Quaternion : PickListElement<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Keyframe : PickListElement<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Bounds : PickListElement<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Color : PickListElement<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Rect : PickListElement<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_ContactPoint : PickListElement<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_ContactPoint2D : PickListElement<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Collision : PickListElement<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Collision2D : PickListElement<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_RaycastHit : PickListElement<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_RaycastHit2D : PickListElement<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Ray : PickListElement<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_Space : PickListElement<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_Direction : PickListElement<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_ItemDefinition_EquipmentType : PickListElement<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_MovementComponent_GoToMethod : PickListElement<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_GDPoint_IdlePointPrefix : PickListElement<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickListElement_UnityEngine_LayerMask : PickListElement<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_System_Boolean : PickRandomListElement<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_System_Single : PickRandomListElement<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_System_Int32 : PickRandomListElement<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Vector2 : PickRandomListElement<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Vector3 : PickRandomListElement<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Vector4 : PickRandomListElement<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Quaternion : PickRandomListElement<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Keyframe : PickRandomListElement<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Bounds : PickRandomListElement<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Color : PickRandomListElement<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Rect : PickRandomListElement<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_ContactPoint : PickRandomListElement<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_ContactPoint2D : PickRandomListElement<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Collision : PickRandomListElement<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Collision2D : PickRandomListElement<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_RaycastHit : PickRandomListElement<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_RaycastHit2D : PickRandomListElement<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Ray : PickRandomListElement<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_Space : PickRandomListElement<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_Direction : PickRandomListElement<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_ItemDefinition_EquipmentType : PickRandomListElement<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_MovementComponent_GoToMethod : PickRandomListElement<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_GDPoint_IdlePointPrefix : PickRandomListElement<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_PickRandomListElement_UnityEngine_LayerMask : PickRandomListElement<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_System_Boolean : RemoveElementFromList<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_System_Single : RemoveElementFromList<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_System_Int32 : RemoveElementFromList<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Vector2 : RemoveElementFromList<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Vector3 : RemoveElementFromList<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Vector4 : RemoveElementFromList<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Quaternion : RemoveElementFromList<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Keyframe : RemoveElementFromList<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Bounds : RemoveElementFromList<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Color : RemoveElementFromList<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Rect : RemoveElementFromList<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_ContactPoint : RemoveElementFromList<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_ContactPoint2D : RemoveElementFromList<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Collision : RemoveElementFromList<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Collision2D : RemoveElementFromList<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_RaycastHit : RemoveElementFromList<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_RaycastHit2D : RemoveElementFromList<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Ray : RemoveElementFromList<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_Space : RemoveElementFromList<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_Direction : RemoveElementFromList<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_ItemDefinition_EquipmentType : RemoveElementFromList<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_MovementComponent_GoToMethod : RemoveElementFromList<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_GDPoint_IdlePointPrefix : RemoveElementFromList<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_RemoveElementFromList_UnityEngine_LayerMask : RemoveElementFromList<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_System_Boolean : NodeCanvas.Tasks.Actions.SendEvent<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_System_Single : NodeCanvas.Tasks.Actions.SendEvent<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_System_Int32 : NodeCanvas.Tasks.Actions.SendEvent<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Vector2 : NodeCanvas.Tasks.Actions.SendEvent<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Vector3 : NodeCanvas.Tasks.Actions.SendEvent<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Vector4 : NodeCanvas.Tasks.Actions.SendEvent<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Quaternion : NodeCanvas.Tasks.Actions.SendEvent<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Keyframe : NodeCanvas.Tasks.Actions.SendEvent<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Bounds : NodeCanvas.Tasks.Actions.SendEvent<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Color : NodeCanvas.Tasks.Actions.SendEvent<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Rect : NodeCanvas.Tasks.Actions.SendEvent<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_ContactPoint : NodeCanvas.Tasks.Actions.SendEvent<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_ContactPoint2D : NodeCanvas.Tasks.Actions.SendEvent<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Collision : NodeCanvas.Tasks.Actions.SendEvent<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Collision2D : NodeCanvas.Tasks.Actions.SendEvent<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_RaycastHit : NodeCanvas.Tasks.Actions.SendEvent<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_RaycastHit2D : NodeCanvas.Tasks.Actions.SendEvent<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Ray : NodeCanvas.Tasks.Actions.SendEvent<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_Space : NodeCanvas.Tasks.Actions.SendEvent<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_Direction : NodeCanvas.Tasks.Actions.SendEvent<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_ItemDefinition_EquipmentType : NodeCanvas.Tasks.Actions.SendEvent<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_MovementComponent_GoToMethod : NodeCanvas.Tasks.Actions.SendEvent<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_GDPoint_IdlePointPrefix : NodeCanvas.Tasks.Actions.SendEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEvent_UnityEngine_LayerMask : NodeCanvas.Tasks.Actions.SendEvent<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_System_Boolean : SendEventToObjects<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_System_Single : SendEventToObjects<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_System_Int32 : SendEventToObjects<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Vector2 : SendEventToObjects<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Vector3 : SendEventToObjects<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Vector4 : SendEventToObjects<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Quaternion : SendEventToObjects<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Keyframe : SendEventToObjects<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Bounds : SendEventToObjects<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Color : SendEventToObjects<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Rect : SendEventToObjects<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_ContactPoint : SendEventToObjects<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_ContactPoint2D : SendEventToObjects<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Collision : SendEventToObjects<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Collision2D : SendEventToObjects<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_RaycastHit : SendEventToObjects<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_RaycastHit2D : SendEventToObjects<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Ray : SendEventToObjects<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_Space : SendEventToObjects<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_Direction : SendEventToObjects<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_ItemDefinition_EquipmentType : SendEventToObjects<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_MovementComponent_GoToMethod : SendEventToObjects<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_GDPoint_IdlePointPrefix : SendEventToObjects<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendEventToObjects_UnityEngine_LayerMask : SendEventToObjects<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_System_Boolean : SendMessage<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_System_Single : SendMessage<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_System_Int32 : SendMessage<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Vector2 : SendMessage<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Vector3 : SendMessage<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Vector4 : SendMessage<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Quaternion : SendMessage<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Keyframe : SendMessage<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Bounds : SendMessage<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Color : SendMessage<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Rect : SendMessage<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_ContactPoint : SendMessage<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_ContactPoint2D : SendMessage<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Collision : SendMessage<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Collision2D : SendMessage<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_RaycastHit : SendMessage<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_RaycastHit2D : SendMessage<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Ray : SendMessage<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_Space : SendMessage<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_Direction : SendMessage<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_ItemDefinition_EquipmentType : SendMessage<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_MovementComponent_GoToMethod : SendMessage<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_GDPoint_IdlePointPrefix : SendMessage<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_SendMessage_UnityEngine_LayerMask : SendMessage<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_System_Boolean : SetListElement<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_System_Single : SetListElement<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_System_Int32 : SetListElement<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Vector2 : SetListElement<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Vector3 : SetListElement<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Vector4 : SetListElement<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Quaternion : SetListElement<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Keyframe : SetListElement<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Bounds : SetListElement<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Color : SetListElement<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Rect : SetListElement<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_ContactPoint : SetListElement<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_ContactPoint2D : SetListElement<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Collision : SetListElement<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Collision2D : SetListElement<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_RaycastHit : SetListElement<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_RaycastHit2D : SetListElement<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Ray : SetListElement<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_Space : SetListElement<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_Direction : SetListElement<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_ItemDefinition_EquipmentType : SetListElement<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_MovementComponent_GoToMethod : SetListElement<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_GDPoint_IdlePointPrefix : SetListElement<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetListElement_UnityEngine_LayerMask : SetListElement<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_System_Boolean : NodeCanvas.Tasks.Actions.SetVariable<bool>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_System_Single : NodeCanvas.Tasks.Actions.SetVariable<float>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_System_Int32 : NodeCanvas.Tasks.Actions.SetVariable<int>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Vector2 : NodeCanvas.Tasks.Actions.SetVariable<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Vector3 : NodeCanvas.Tasks.Actions.SetVariable<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Vector4 : NodeCanvas.Tasks.Actions.SetVariable<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Quaternion : NodeCanvas.Tasks.Actions.SetVariable<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Keyframe : NodeCanvas.Tasks.Actions.SetVariable<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Bounds : NodeCanvas.Tasks.Actions.SetVariable<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Color : NodeCanvas.Tasks.Actions.SetVariable<Color>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Rect : NodeCanvas.Tasks.Actions.SetVariable<Rect>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_ContactPoint : NodeCanvas.Tasks.Actions.SetVariable<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_ContactPoint2D : NodeCanvas.Tasks.Actions.SetVariable<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Collision : NodeCanvas.Tasks.Actions.SetVariable<Collision>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Collision2D : NodeCanvas.Tasks.Actions.SetVariable<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_RaycastHit : NodeCanvas.Tasks.Actions.SetVariable<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_RaycastHit2D : NodeCanvas.Tasks.Actions.SetVariable<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Ray : NodeCanvas.Tasks.Actions.SetVariable<Ray>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_Space : NodeCanvas.Tasks.Actions.SetVariable<Space>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_Direction : NodeCanvas.Tasks.Actions.SetVariable<Direction>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_ItemDefinition_EquipmentType : NodeCanvas.Tasks.Actions.SetVariable<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_MovementComponent_GoToMethod : NodeCanvas.Tasks.Actions.SetVariable<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_GDPoint_IdlePointPrefix : NodeCanvas.Tasks.Actions.SetVariable<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Actions_SetVariable_UnityEngine_LayerMask : NodeCanvas.Tasks.Actions.SetVariable<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_System_Boolean : CheckCSharpEvent<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_System_Single : CheckCSharpEvent<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_System_Int32 : CheckCSharpEvent<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Vector2 : CheckCSharpEvent<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Vector3 : CheckCSharpEvent<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Vector4 : CheckCSharpEvent<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Quaternion : CheckCSharpEvent<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Keyframe : CheckCSharpEvent<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Bounds : CheckCSharpEvent<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Color : CheckCSharpEvent<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Rect : CheckCSharpEvent<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_ContactPoint : CheckCSharpEvent<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_ContactPoint2D : CheckCSharpEvent<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Collision : CheckCSharpEvent<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Collision2D : CheckCSharpEvent<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_RaycastHit : CheckCSharpEvent<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_RaycastHit2D : CheckCSharpEvent<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Ray : CheckCSharpEvent<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_Space : CheckCSharpEvent<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_Direction : CheckCSharpEvent<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_ItemDefinition_EquipmentType : CheckCSharpEvent<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_MovementComponent_GoToMethod : CheckCSharpEvent<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_GDPoint_IdlePointPrefix : CheckCSharpEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEvent_UnityEngine_LayerMask : CheckCSharpEvent<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_System_Boolean : CheckCSharpEventValue<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_System_Single : CheckCSharpEventValue<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_System_Int32 : CheckCSharpEventValue<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Vector2 : CheckCSharpEventValue<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Vector3 : CheckCSharpEventValue<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Vector4 : CheckCSharpEventValue<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Quaternion : CheckCSharpEventValue<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Keyframe : CheckCSharpEventValue<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Bounds : CheckCSharpEventValue<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Color : CheckCSharpEventValue<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Rect : CheckCSharpEventValue<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_ContactPoint : CheckCSharpEventValue<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_ContactPoint2D : CheckCSharpEventValue<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Collision : CheckCSharpEventValue<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Collision2D : CheckCSharpEventValue<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_RaycastHit : CheckCSharpEventValue<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_RaycastHit2D : CheckCSharpEventValue<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Ray : CheckCSharpEventValue<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_Space : CheckCSharpEventValue<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_Direction : CheckCSharpEventValue<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_ItemDefinition_EquipmentType : CheckCSharpEventValue<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_MovementComponent_GoToMethod : CheckCSharpEventValue<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_GDPoint_IdlePointPrefix : CheckCSharpEventValue<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckCSharpEventValue_UnityEngine_LayerMask : CheckCSharpEventValue<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_System_Boolean : CheckEvent<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_System_Single : CheckEvent<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_System_Int32 : CheckEvent<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Vector2 : CheckEvent<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Vector3 : CheckEvent<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Vector4 : CheckEvent<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Quaternion : CheckEvent<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Keyframe : CheckEvent<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Bounds : CheckEvent<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Color : CheckEvent<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Rect : CheckEvent<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_ContactPoint : CheckEvent<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_ContactPoint2D : CheckEvent<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Collision : CheckEvent<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Collision2D : CheckEvent<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_RaycastHit : CheckEvent<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_RaycastHit2D : CheckEvent<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Ray : CheckEvent<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_Space : CheckEvent<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_Direction : CheckEvent<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_ItemDefinition_EquipmentType : CheckEvent<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_MovementComponent_GoToMethod : CheckEvent<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_GDPoint_IdlePointPrefix : CheckEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEvent_UnityEngine_LayerMask : CheckEvent<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_System_Boolean : CheckEventValue<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_System_Single : CheckEventValue<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_System_Int32 : CheckEventValue<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Vector2 : CheckEventValue<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Vector3 : CheckEventValue<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Vector4 : CheckEventValue<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Quaternion : CheckEventValue<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Keyframe : CheckEventValue<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Bounds : CheckEventValue<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Color : CheckEventValue<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Rect : CheckEventValue<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_ContactPoint : CheckEventValue<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_ContactPoint2D : CheckEventValue<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Collision : CheckEventValue<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Collision2D : CheckEventValue<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_RaycastHit : CheckEventValue<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_RaycastHit2D : CheckEventValue<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Ray : CheckEventValue<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_Space : CheckEventValue<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_Direction : CheckEventValue<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_ItemDefinition_EquipmentType : CheckEventValue<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_MovementComponent_GoToMethod : CheckEventValue<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_GDPoint_IdlePointPrefix : CheckEventValue<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckEventValue_UnityEngine_LayerMask : CheckEventValue<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_System_Boolean : CheckStaticCSharpEvent<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_System_Single : CheckStaticCSharpEvent<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_System_Int32 : CheckStaticCSharpEvent<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Vector2 : CheckStaticCSharpEvent<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Vector3 : CheckStaticCSharpEvent<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Vector4 : CheckStaticCSharpEvent<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Quaternion : CheckStaticCSharpEvent<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Keyframe : CheckStaticCSharpEvent<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Bounds : CheckStaticCSharpEvent<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Color : CheckStaticCSharpEvent<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Rect : CheckStaticCSharpEvent<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_ContactPoint : CheckStaticCSharpEvent<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_ContactPoint2D : CheckStaticCSharpEvent<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Collision : CheckStaticCSharpEvent<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Collision2D : CheckStaticCSharpEvent<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_RaycastHit : CheckStaticCSharpEvent<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_RaycastHit2D : CheckStaticCSharpEvent<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Ray : CheckStaticCSharpEvent<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_Space : CheckStaticCSharpEvent<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_Direction : CheckStaticCSharpEvent<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_ItemDefinition_EquipmentType : CheckStaticCSharpEvent<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_MovementComponent_GoToMethod : CheckStaticCSharpEvent<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_GDPoint_IdlePointPrefix : CheckStaticCSharpEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckStaticCSharpEvent_UnityEngine_LayerMask : CheckStaticCSharpEvent<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_System_Boolean : CheckUnityEvent<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_System_Single : CheckUnityEvent<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_System_Int32 : CheckUnityEvent<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Vector2 : CheckUnityEvent<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Vector3 : CheckUnityEvent<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Vector4 : CheckUnityEvent<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Quaternion : CheckUnityEvent<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Keyframe : CheckUnityEvent<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Bounds : CheckUnityEvent<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Color : CheckUnityEvent<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Rect : CheckUnityEvent<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_ContactPoint : CheckUnityEvent<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_ContactPoint2D : CheckUnityEvent<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Collision : CheckUnityEvent<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Collision2D : CheckUnityEvent<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_RaycastHit : CheckUnityEvent<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_RaycastHit2D : CheckUnityEvent<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Ray : CheckUnityEvent<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_Space : CheckUnityEvent<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_Direction : CheckUnityEvent<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_ItemDefinition_EquipmentType : CheckUnityEvent<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_MovementComponent_GoToMethod : CheckUnityEvent<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_GDPoint_IdlePointPrefix : CheckUnityEvent<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEvent_UnityEngine_LayerMask : CheckUnityEvent<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_System_Boolean : CheckUnityEventValue<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_System_Single : CheckUnityEventValue<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_System_Int32 : CheckUnityEventValue<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Vector2 : CheckUnityEventValue<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Vector3 : CheckUnityEventValue<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Vector4 : CheckUnityEventValue<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Quaternion : CheckUnityEventValue<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Keyframe : CheckUnityEventValue<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Bounds : CheckUnityEventValue<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Color : CheckUnityEventValue<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Rect : CheckUnityEventValue<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_ContactPoint : CheckUnityEventValue<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_ContactPoint2D : CheckUnityEventValue<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Collision : CheckUnityEventValue<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Collision2D : CheckUnityEventValue<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_RaycastHit : CheckUnityEventValue<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_RaycastHit2D : CheckUnityEventValue<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Ray : CheckUnityEventValue<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_Space : CheckUnityEventValue<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_Direction : CheckUnityEventValue<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_ItemDefinition_EquipmentType : CheckUnityEventValue<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_MovementComponent_GoToMethod : CheckUnityEventValue<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_GDPoint_IdlePointPrefix : CheckUnityEventValue<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckUnityEventValue_UnityEngine_LayerMask : CheckUnityEventValue<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_System_Boolean : CheckVariable<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_System_Single : CheckVariable<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_System_Int32 : CheckVariable<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Vector2 : CheckVariable<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Vector3 : CheckVariable<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Vector4 : CheckVariable<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Quaternion : CheckVariable<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Keyframe : CheckVariable<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Bounds : CheckVariable<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Color : CheckVariable<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Rect : CheckVariable<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_ContactPoint : CheckVariable<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_ContactPoint2D : CheckVariable<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Collision : CheckVariable<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Collision2D : CheckVariable<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_RaycastHit : CheckVariable<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_RaycastHit2D : CheckVariable<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Ray : CheckVariable<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_Space : CheckVariable<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_Direction : CheckVariable<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_ItemDefinition_EquipmentType : CheckVariable<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_MovementComponent_GoToMethod : CheckVariable<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_GDPoint_IdlePointPrefix : CheckVariable<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_CheckVariable_UnityEngine_LayerMask : CheckVariable<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_System_Boolean : ListContainsElement<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_System_Single : ListContainsElement<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_System_Int32 : ListContainsElement<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Vector2 : ListContainsElement<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Vector3 : ListContainsElement<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Vector4 : ListContainsElement<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Quaternion : ListContainsElement<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Keyframe : ListContainsElement<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Bounds : ListContainsElement<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Color : ListContainsElement<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Rect : ListContainsElement<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_ContactPoint : ListContainsElement<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_ContactPoint2D : ListContainsElement<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Collision : ListContainsElement<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Collision2D : ListContainsElement<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_RaycastHit : ListContainsElement<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_RaycastHit2D : ListContainsElement<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Ray : ListContainsElement<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_Space : ListContainsElement<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_Direction : ListContainsElement<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_ItemDefinition_EquipmentType : ListContainsElement<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_MovementComponent_GoToMethod : ListContainsElement<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_GDPoint_IdlePointPrefix : ListContainsElement<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_ListContainsElement_UnityEngine_LayerMask : ListContainsElement<LayerMask>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_System_Boolean : NodeCanvas.Tasks.Conditions.TryGetValue<bool>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_System_Single : NodeCanvas.Tasks.Conditions.TryGetValue<float>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_System_Int32 : NodeCanvas.Tasks.Conditions.TryGetValue<int>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Vector2 : NodeCanvas.Tasks.Conditions.TryGetValue<Vector2>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Vector3 : NodeCanvas.Tasks.Conditions.TryGetValue<Vector3>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Vector4 : NodeCanvas.Tasks.Conditions.TryGetValue<Vector4>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Quaternion : NodeCanvas.Tasks.Conditions.TryGetValue<Quaternion>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Keyframe : NodeCanvas.Tasks.Conditions.TryGetValue<Keyframe>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Bounds : NodeCanvas.Tasks.Conditions.TryGetValue<Bounds>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Color : NodeCanvas.Tasks.Conditions.TryGetValue<Color>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Rect : NodeCanvas.Tasks.Conditions.TryGetValue<Rect>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_ContactPoint : NodeCanvas.Tasks.Conditions.TryGetValue<ContactPoint>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_ContactPoint2D : NodeCanvas.Tasks.Conditions.TryGetValue<ContactPoint2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Collision : NodeCanvas.Tasks.Conditions.TryGetValue<Collision>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Collision2D : NodeCanvas.Tasks.Conditions.TryGetValue<Collision2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_RaycastHit : NodeCanvas.Tasks.Conditions.TryGetValue<RaycastHit>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_RaycastHit2D : NodeCanvas.Tasks.Conditions.TryGetValue<RaycastHit2D>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Ray : NodeCanvas.Tasks.Conditions.TryGetValue<Ray>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_Space : NodeCanvas.Tasks.Conditions.TryGetValue<Space>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_Direction : NodeCanvas.Tasks.Conditions.TryGetValue<Direction>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_ItemDefinition_EquipmentType : NodeCanvas.Tasks.Conditions.TryGetValue<ItemDefinition.EquipmentType>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_MovementComponent_GoToMethod : NodeCanvas.Tasks.Conditions.TryGetValue<MovementComponent.GoToMethod>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_GDPoint_IdlePointPrefix : NodeCanvas.Tasks.Conditions.TryGetValue<GDPoint.IdlePointPrefix>
	{
	}

	private class NodeCanvas_Tasks_Conditions_TryGetValue_UnityEngine_LayerMask : NodeCanvas.Tasks.Conditions.TryGetValue<LayerMask>
	{
	}

	private object o;

	private void FlowCanvas_ValueHandler_Delegate()
	{
	}

	private void FlowCanvas_Flow_ReadParameter_1()
	{
		Flow flow = default(Flow);
		flow.ReadParameter<bool>((string)o);
		flow.ReadParameter<float>((string)o);
		flow.ReadParameter<int>((string)o);
		flow.ReadParameter<Vector2>((string)o);
		flow.ReadParameter<Vector3>((string)o);
		flow.ReadParameter<Vector4>((string)o);
		flow.ReadParameter<Quaternion>((string)o);
		flow.ReadParameter<Keyframe>((string)o);
		flow.ReadParameter<Bounds>((string)o);
		flow.ReadParameter<Color>((string)o);
		flow.ReadParameter<Rect>((string)o);
		flow.ReadParameter<ContactPoint>((string)o);
		flow.ReadParameter<ContactPoint2D>((string)o);
		flow.ReadParameter<Collision>((string)o);
		flow.ReadParameter<Collision2D>((string)o);
		flow.ReadParameter<RaycastHit>((string)o);
		flow.ReadParameter<RaycastHit2D>((string)o);
		flow.ReadParameter<Ray>((string)o);
		flow.ReadParameter<Space>((string)o);
		flow.ReadParameter<Direction>((string)o);
		flow.ReadParameter<ItemDefinition.EquipmentType>((string)o);
		flow.ReadParameter<MovementComponent.GoToMethod>((string)o);
		flow.ReadParameter<GDPoint.IdlePointPrefix>((string)o);
		flow.ReadParameter<LayerMask>((string)o);
	}

	private void FlowCanvas_Flow_WriteParameter_2()
	{
		Flow flow = default(Flow);
		flow.WriteParameter((string)o, (bool)o);
		flow.WriteParameter((string)o, (float)o);
		flow.WriteParameter((string)o, (int)o);
		flow.WriteParameter((string)o, (Vector2)o);
		flow.WriteParameter((string)o, (Vector3)o);
		flow.WriteParameter((string)o, (Vector4)o);
		flow.WriteParameter((string)o, (Quaternion)o);
		flow.WriteParameter((string)o, (Keyframe)o);
		flow.WriteParameter((string)o, (Bounds)o);
		flow.WriteParameter((string)o, (Color)o);
		flow.WriteParameter((string)o, (Rect)o);
		flow.WriteParameter((string)o, (ContactPoint)o);
		flow.WriteParameter((string)o, (ContactPoint2D)o);
		flow.WriteParameter((string)o, (Collision)o);
		flow.WriteParameter((string)o, (Collision2D)o);
		flow.WriteParameter((string)o, (RaycastHit)o);
		flow.WriteParameter((string)o, (RaycastHit2D)o);
		flow.WriteParameter((string)o, (Ray)o);
		flow.WriteParameter((string)o, (Space)o);
		flow.WriteParameter((string)o, (Direction)o);
		flow.WriteParameter((string)o, (ItemDefinition.EquipmentType)o);
		flow.WriteParameter((string)o, (MovementComponent.GoToMethod)o);
		flow.WriteParameter((string)o, (GDPoint.IdlePointPrefix)o);
		flow.WriteParameter((string)o, (LayerMask)o);
	}

	private void FlowCanvas_FlowNode_AddValueInput_1()
	{
		((FlowNode)null).AddValueInput<bool>((string)o, (string)o);
		((FlowNode)null).AddValueInput<float>((string)o, (string)o);
		((FlowNode)null).AddValueInput<int>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Vector2>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Vector3>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Vector4>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Quaternion>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Keyframe>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Bounds>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Color>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Rect>((string)o, (string)o);
		((FlowNode)null).AddValueInput<ContactPoint>((string)o, (string)o);
		((FlowNode)null).AddValueInput<ContactPoint2D>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Collision>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Collision2D>((string)o, (string)o);
		((FlowNode)null).AddValueInput<RaycastHit>((string)o, (string)o);
		((FlowNode)null).AddValueInput<RaycastHit2D>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Ray>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Space>((string)o, (string)o);
		((FlowNode)null).AddValueInput<Direction>((string)o, (string)o);
		((FlowNode)null).AddValueInput<ItemDefinition.EquipmentType>((string)o, (string)o);
		((FlowNode)null).AddValueInput<MovementComponent.GoToMethod>((string)o, (string)o);
		((FlowNode)null).AddValueInput<GDPoint.IdlePointPrefix>((string)o, (string)o);
		((FlowNode)null).AddValueInput<LayerMask>((string)o, (string)o);
	}

	private void FlowCanvas_FlowNode_AddVerticalValueInput_2()
	{
		((FlowNode)null).AddVerticalValueInput<bool>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<float>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<int>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Vector2>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Vector3>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Vector4>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Quaternion>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Keyframe>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Bounds>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Color>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Rect>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<ContactPoint>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<ContactPoint2D>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Collision>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Collision2D>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<RaycastHit>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<RaycastHit2D>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Ray>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Space>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<Direction>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<ItemDefinition.EquipmentType>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<MovementComponent.GoToMethod>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<GDPoint.IdlePointPrefix>((string)o, (string)o);
		((FlowNode)null).AddVerticalValueInput<LayerMask>((string)o, (string)o);
	}

	private void FlowCanvas_FlowNode_AddValueOutput_3()
	{
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<bool>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<float>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<int>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Vector2>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Vector3>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Vector4>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Quaternion>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Keyframe>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Bounds>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Color>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Rect>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<ContactPoint>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<ContactPoint2D>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Collision>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Collision2D>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<RaycastHit>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<RaycastHit2D>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Ray>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Space>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<Direction>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<ItemDefinition.EquipmentType>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<MovementComponent.GoToMethod>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<GDPoint.IdlePointPrefix>)o);
		((FlowNode)null).AddValueOutput((string)o, (string)o, (ValueHandler<LayerMask>)o);
	}

	private void FlowCanvas_FlowNode_AddValueOutput_4()
	{
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<bool>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<float>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<int>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Vector2>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Vector3>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Vector4>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Quaternion>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Keyframe>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Bounds>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Color>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Rect>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<ContactPoint>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<ContactPoint2D>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Collision>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Collision2D>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<RaycastHit>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<RaycastHit2D>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Ray>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Space>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<Direction>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<ItemDefinition.EquipmentType>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<MovementComponent.GoToMethod>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<GDPoint.IdlePointPrefix>)o, (string)o);
		((FlowNode)null).AddValueOutput((string)o, (ValueHandler<LayerMask>)o, (string)o);
	}

	private void FlowCanvas_ValueInput_CreateInstance_1()
	{
		ValueInput.CreateInstance<bool>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<float>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<int>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Vector2>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Vector3>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Vector4>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Quaternion>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Keyframe>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Bounds>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Color>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Rect>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<ContactPoint>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<ContactPoint2D>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Collision>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Collision2D>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<RaycastHit>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<RaycastHit2D>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Ray>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Space>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<Direction>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<ItemDefinition.EquipmentType>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<MovementComponent.GoToMethod>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<GDPoint.IdlePointPrefix>((FlowNode)o, (string)o, (string)o);
		ValueInput.CreateInstance<LayerMask>((FlowNode)o, (string)o, (string)o);
	}

	private void FlowCanvas_ValueOutput_CreateInstance_1()
	{
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<bool>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<float>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<int>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Vector2>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Vector3>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Vector4>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Quaternion>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Keyframe>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Bounds>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Color>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Rect>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<ContactPoint>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<ContactPoint2D>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Collision>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Collision2D>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<RaycastHit>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<RaycastHit2D>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Ray>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Space>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<Direction>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<ItemDefinition.EquipmentType>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<MovementComponent.GoToMethod>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<GDPoint.IdlePointPrefix>)o);
		ValueOutput.CreateInstance((FlowNode)o, (string)o, (string)o, (ValueHandler<LayerMask>)o);
	}

	private void FlowCanvas_Nodes_ReflectedDelegateEvent_Callback1_1()
	{
		((ReflectedDelegateEvent)null).Callback1((bool)o);
		((ReflectedDelegateEvent)null).Callback1((float)o);
		((ReflectedDelegateEvent)null).Callback1((int)o);
		((ReflectedDelegateEvent)null).Callback1((Vector2)o);
		((ReflectedDelegateEvent)null).Callback1((Vector3)o);
		((ReflectedDelegateEvent)null).Callback1((Vector4)o);
		((ReflectedDelegateEvent)null).Callback1((Quaternion)o);
		((ReflectedDelegateEvent)null).Callback1((Keyframe)o);
		((ReflectedDelegateEvent)null).Callback1((Bounds)o);
		((ReflectedDelegateEvent)null).Callback1((Color)o);
		((ReflectedDelegateEvent)null).Callback1((Rect)o);
		((ReflectedDelegateEvent)null).Callback1((ContactPoint)o);
		((ReflectedDelegateEvent)null).Callback1((ContactPoint2D)o);
		((ReflectedDelegateEvent)null).Callback1((Collision)o);
		((ReflectedDelegateEvent)null).Callback1((Collision2D)o);
		((ReflectedDelegateEvent)null).Callback1((RaycastHit)o);
		((ReflectedDelegateEvent)null).Callback1((RaycastHit2D)o);
		((ReflectedDelegateEvent)null).Callback1((Ray)o);
		((ReflectedDelegateEvent)null).Callback1((Space)o);
		((ReflectedDelegateEvent)null).Callback1((Direction)o);
		((ReflectedDelegateEvent)null).Callback1((ItemDefinition.EquipmentType)o);
		((ReflectedDelegateEvent)null).Callback1((MovementComponent.GoToMethod)o);
		((ReflectedDelegateEvent)null).Callback1((GDPoint.IdlePointPrefix)o);
		((ReflectedDelegateEvent)null).Callback1((LayerMask)o);
	}

	private void FlowCanvas_Nodes_ReflectedUnityEvent_CallbackMethod1_1()
	{
		((ReflectedUnityEvent)null).CallbackMethod1((bool)o);
		((ReflectedUnityEvent)null).CallbackMethod1((float)o);
		((ReflectedUnityEvent)null).CallbackMethod1((int)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Vector2)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Vector3)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Vector4)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Quaternion)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Keyframe)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Bounds)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Color)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Rect)o);
		((ReflectedUnityEvent)null).CallbackMethod1((ContactPoint)o);
		((ReflectedUnityEvent)null).CallbackMethod1((ContactPoint2D)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Collision)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Collision2D)o);
		((ReflectedUnityEvent)null).CallbackMethod1((RaycastHit)o);
		((ReflectedUnityEvent)null).CallbackMethod1((RaycastHit2D)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Ray)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Space)o);
		((ReflectedUnityEvent)null).CallbackMethod1((Direction)o);
		((ReflectedUnityEvent)null).CallbackMethod1((ItemDefinition.EquipmentType)o);
		((ReflectedUnityEvent)null).CallbackMethod1((MovementComponent.GoToMethod)o);
		((ReflectedUnityEvent)null).CallbackMethod1((GDPoint.IdlePointPrefix)o);
		((ReflectedUnityEvent)null).CallbackMethod1((LayerMask)o);
	}

	private void NodeCanvas_Framework_Blackboard_GetVariable_1()
	{
		((Blackboard)null).GetVariable<bool>((string)o);
		((Blackboard)null).GetVariable<float>((string)o);
		((Blackboard)null).GetVariable<int>((string)o);
		((Blackboard)null).GetVariable<Vector2>((string)o);
		((Blackboard)null).GetVariable<Vector3>((string)o);
		((Blackboard)null).GetVariable<Vector4>((string)o);
		((Blackboard)null).GetVariable<Quaternion>((string)o);
		((Blackboard)null).GetVariable<Keyframe>((string)o);
		((Blackboard)null).GetVariable<Bounds>((string)o);
		((Blackboard)null).GetVariable<Color>((string)o);
		((Blackboard)null).GetVariable<Rect>((string)o);
		((Blackboard)null).GetVariable<ContactPoint>((string)o);
		((Blackboard)null).GetVariable<ContactPoint2D>((string)o);
		((Blackboard)null).GetVariable<Collision>((string)o);
		((Blackboard)null).GetVariable<Collision2D>((string)o);
		((Blackboard)null).GetVariable<RaycastHit>((string)o);
		((Blackboard)null).GetVariable<RaycastHit2D>((string)o);
		((Blackboard)null).GetVariable<Ray>((string)o);
		((Blackboard)null).GetVariable<Space>((string)o);
		((Blackboard)null).GetVariable<Direction>((string)o);
		((Blackboard)null).GetVariable<ItemDefinition.EquipmentType>((string)o);
		((Blackboard)null).GetVariable<MovementComponent.GoToMethod>((string)o);
		((Blackboard)null).GetVariable<GDPoint.IdlePointPrefix>((string)o);
		((Blackboard)null).GetVariable<LayerMask>((string)o);
	}

	private void NodeCanvas_Framework_Blackboard_GetValue_2()
	{
		((Blackboard)null).GetValue<bool>((string)o);
		((Blackboard)null).GetValue<float>((string)o);
		((Blackboard)null).GetValue<int>((string)o);
		((Blackboard)null).GetValue<Vector2>((string)o);
		((Blackboard)null).GetValue<Vector3>((string)o);
		((Blackboard)null).GetValue<Vector4>((string)o);
		((Blackboard)null).GetValue<Quaternion>((string)o);
		((Blackboard)null).GetValue<Keyframe>((string)o);
		((Blackboard)null).GetValue<Bounds>((string)o);
		((Blackboard)null).GetValue<Color>((string)o);
		((Blackboard)null).GetValue<Rect>((string)o);
		((Blackboard)null).GetValue<ContactPoint>((string)o);
		((Blackboard)null).GetValue<ContactPoint2D>((string)o);
		((Blackboard)null).GetValue<Collision>((string)o);
		((Blackboard)null).GetValue<Collision2D>((string)o);
		((Blackboard)null).GetValue<RaycastHit>((string)o);
		((Blackboard)null).GetValue<RaycastHit2D>((string)o);
		((Blackboard)null).GetValue<Ray>((string)o);
		((Blackboard)null).GetValue<Space>((string)o);
		((Blackboard)null).GetValue<Direction>((string)o);
		((Blackboard)null).GetValue<ItemDefinition.EquipmentType>((string)o);
		((Blackboard)null).GetValue<MovementComponent.GoToMethod>((string)o);
		((Blackboard)null).GetValue<GDPoint.IdlePointPrefix>((string)o);
		((Blackboard)null).GetValue<LayerMask>((string)o);
	}

	private void NodeCanvas_Framework_IBlackboard_GetVariable_1()
	{
		((IBlackboard)null).GetVariable<bool>((string)o);
		((IBlackboard)null).GetVariable<float>((string)o);
		((IBlackboard)null).GetVariable<int>((string)o);
		((IBlackboard)null).GetVariable<Vector2>((string)o);
		((IBlackboard)null).GetVariable<Vector3>((string)o);
		((IBlackboard)null).GetVariable<Vector4>((string)o);
		((IBlackboard)null).GetVariable<Quaternion>((string)o);
		((IBlackboard)null).GetVariable<Keyframe>((string)o);
		((IBlackboard)null).GetVariable<Bounds>((string)o);
		((IBlackboard)null).GetVariable<Color>((string)o);
		((IBlackboard)null).GetVariable<Rect>((string)o);
		((IBlackboard)null).GetVariable<ContactPoint>((string)o);
		((IBlackboard)null).GetVariable<ContactPoint2D>((string)o);
		((IBlackboard)null).GetVariable<Collision>((string)o);
		((IBlackboard)null).GetVariable<Collision2D>((string)o);
		((IBlackboard)null).GetVariable<RaycastHit>((string)o);
		((IBlackboard)null).GetVariable<RaycastHit2D>((string)o);
		((IBlackboard)null).GetVariable<Ray>((string)o);
		((IBlackboard)null).GetVariable<Space>((string)o);
		((IBlackboard)null).GetVariable<Direction>((string)o);
		((IBlackboard)null).GetVariable<ItemDefinition.EquipmentType>((string)o);
		((IBlackboard)null).GetVariable<MovementComponent.GoToMethod>((string)o);
		((IBlackboard)null).GetVariable<GDPoint.IdlePointPrefix>((string)o);
		((IBlackboard)null).GetVariable<LayerMask>((string)o);
	}

	private void NodeCanvas_Framework_IBlackboard_GetValue_2()
	{
		((IBlackboard)null).GetValue<bool>((string)o);
		((IBlackboard)null).GetValue<float>((string)o);
		((IBlackboard)null).GetValue<int>((string)o);
		((IBlackboard)null).GetValue<Vector2>((string)o);
		((IBlackboard)null).GetValue<Vector3>((string)o);
		((IBlackboard)null).GetValue<Vector4>((string)o);
		((IBlackboard)null).GetValue<Quaternion>((string)o);
		((IBlackboard)null).GetValue<Keyframe>((string)o);
		((IBlackboard)null).GetValue<Bounds>((string)o);
		((IBlackboard)null).GetValue<Color>((string)o);
		((IBlackboard)null).GetValue<Rect>((string)o);
		((IBlackboard)null).GetValue<ContactPoint>((string)o);
		((IBlackboard)null).GetValue<ContactPoint2D>((string)o);
		((IBlackboard)null).GetValue<Collision>((string)o);
		((IBlackboard)null).GetValue<Collision2D>((string)o);
		((IBlackboard)null).GetValue<RaycastHit>((string)o);
		((IBlackboard)null).GetValue<RaycastHit2D>((string)o);
		((IBlackboard)null).GetValue<Ray>((string)o);
		((IBlackboard)null).GetValue<Space>((string)o);
		((IBlackboard)null).GetValue<Direction>((string)o);
		((IBlackboard)null).GetValue<ItemDefinition.EquipmentType>((string)o);
		((IBlackboard)null).GetValue<MovementComponent.GoToMethod>((string)o);
		((IBlackboard)null).GetValue<GDPoint.IdlePointPrefix>((string)o);
		((IBlackboard)null).GetValue<LayerMask>((string)o);
	}

	private void CustomSpoof()
	{
	}
}
