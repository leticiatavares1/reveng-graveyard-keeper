using System;
using System.Reflection;
using LazyBearTechnology;
using Sirenix.Serialization;

public class LazySerializationPolicy : CustomSerializationPolicy
{
	private static readonly LazySerializationPolicy instance = new LazySerializationPolicy("LazySerializationPolicy", allowNonSerializableTypes: true, delegate(MemberInfo member)
	{
		if (member is FieldInfo element && Attribute.IsDefined(element, typeof(LazySerialize)))
		{
			return true;
		}
		return SerializationPolicies.Unity.ShouldSerializeMember(member) ? true : false;
	});

	public static LazySerializationPolicy Instance => instance;

	private LazySerializationPolicy(string id, bool allowNonSerializableTypes, Func<MemberInfo, bool> shouldSerializeFunc)
		: base(id, allowNonSerializableTypes, shouldSerializeFunc)
	{
	}

	public void TestSerialization()
	{
		SerializationContext serializationContext = new SerializationContext();
		DeserializationContext deserializationContext = new DeserializationContext();
		serializationContext.Config.SerializationPolicy = Instance;
		serializationContext.IndexReferenceResolver = new UnityReferenceResolver();
		deserializationContext.Config.SerializationPolicy = Instance;
		deserializationContext.IndexReferenceResolver = new UnityReferenceResolver();
		SerializationUtility.SerializeValue(new MainGame(), DataFormat.Binary, serializationContext);
	}
}
