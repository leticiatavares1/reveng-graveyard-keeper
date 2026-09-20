using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using LinqTools;
using UnityEngine;

public static class CommandFactory
{
	private class CommandTypeData
	{
		public Type type;

		public ushort id;

		public List<FieldInfo> fields = new List<FieldInfo>();

		public Dictionary<ushort, FieldInfo> fieldsCache = new Dictionary<ushort, FieldInfo>();

		public CommandTypeData(Type type, ushort id)
		{
			this.type = type;
			this.id = id;
		}

		public void AddField(FieldInfo fieldInfo)
		{
			fields.Add(fieldInfo);
			fieldsCache.Add((ushort)fields.Count, fieldInfo);
		}

		public int GetFieldsSizeInByte<T>(T obj) where T : Command
		{
			return CommandFieldAttributeHelper.GetCommandFieldsSerializedSize(obj, fields);
		}
	}

	private static ushort typeUId;

	private static readonly Dictionary<string, CommandTypeData> types;

	private static readonly Dictionary<ushort, Type> typesById;

	static CommandFactory()
	{
		types = new Dictionary<string, CommandTypeData>();
		typesById = new Dictionary<ushort, Type>();
		typeUId = 0;
		RegisterCommands();
	}

	private static void RegisterCommands()
	{
		Type commandType = typeof(ICommand);
		Type commandAttributeType = typeof(CommandAttribute);
		foreach (Type item in from p in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly s) => s.GetTypes())
			where commandType.IsAssignableFrom(p) && p.IsDefined(commandAttributeType, inherit: false)
			select p)
		{
			CommandTypeData commandTypeData = new CommandTypeData(item, ++typeUId);
			types.Add(item.Name, commandTypeData);
			typesById.Add(commandTypeData.id, commandTypeData.type);
			foreach (FieldInfo commandSerializedField in CommandFieldAttributeHelper.GetCommandSerializedFields(item))
			{
				commandTypeData.AddField(commandSerializedField);
			}
		}
		Debug.Log($"Command types registered count: {types.Count()}");
	}

	public static byte[] SerializeCommand(ICommand data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(types[data.GetType().Name].id);
		byte[] array = data.Serialize();
		binaryWriter.Write(array.Length);
		binaryWriter.Write(array);
		return memoryStream.ToArray();
	}

	public static ICommand DeserializeCommand(byte[] data)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader binaryReader = new BinaryReader(input);
		ushort key = binaryReader.ReadUInt16();
		int count = binaryReader.ReadInt32();
		byte[] data2 = binaryReader.ReadBytes(count);
		ICommand obj = (ICommand)Activator.CreateInstance(typesById[key]);
		obj.Deserialize(data2);
		return obj;
	}

	public static int GetCommandTypeFieldsSize<T>(T obj) where T : Command
	{
		return types[obj.GetType().Name].GetFieldsSizeInByte(obj);
	}

	public static ReadOnlyCollection<FieldInfo> GetCommandTypeSerializedFields<T>(T obj) where T : Command
	{
		return types[obj.GetType().Name].fields.AsReadOnly();
	}
}
