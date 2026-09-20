using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Rewired.Utils.Libraries.CLZF2;
using Rewired.Utils.Libraries.TinyJson;
using UnityEngine;

namespace Rewired.Data;

public class UserDataStore_File : UserDataStore_KeyValue
{
	private sealed class DataStore : IDataStore
	{
		private Dictionary<string, object> _data;

		private readonly string _absFilePath;

		private IDataHandler _dataHandler;

		public DataStore(string fileName, string absDirectory, IDataHandler dataHandler)
		{
			_absFilePath = Path.Combine(absDirectory, fileName);
			if (dataHandler == null)
			{
				throw new ArgumentNullException("dataHandler");
			}
			_dataHandler = dataHandler;
			_data = new Dictionary<string, object>();
			Load();
		}

		public bool TryGetValue(string key, out object value)
		{
			if (string.IsNullOrEmpty(key))
			{
				value = null;
				return false;
			}
			return _data.TryGetValue(key, out value);
		}

		public bool SetValue(string key, object value)
		{
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}
			_data[key] = value;
			return true;
		}

		public bool Save()
		{
			try
			{
				return _dataHandler.Save(_absFilePath, JsonWriter.ToJson(_data));
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				return false;
			}
		}

		public bool Load()
		{
			try
			{
				string data;
				bool num = _dataHandler.Load(_absFilePath, out data);
				if (num)
				{
					Dictionary<string, object> dictionary = JsonParser.FromJson<Dictionary<string, object>>(data);
					if (dictionary == null)
					{
						dictionary = new Dictionary<string, object>();
					}
					_data = dictionary;
				}
				return num;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				return false;
			}
		}

		public bool Clear()
		{
			bool result;
			try
			{
				result = _dataHandler.Clear(_absFilePath);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				result = false;
			}
			_data.Clear();
			return result;
		}
	}

	private sealed class LocalFileDataHandler : IDataHandler
	{
		private readonly Func<DataFormat> _dataFormatDelegate;

		private readonly Codec _codec;

		public LocalFileDataHandler(Func<DataFormat> dataFormatDelegate, Codec codec)
		{
			if (dataFormatDelegate == null)
			{
				throw new ArgumentNullException("dataFormatDelegate");
			}
			_dataFormatDelegate = dataFormatDelegate;
			if (codec == null)
			{
				codec = new UTF8Text();
			}
			_codec = codec;
		}

		public bool Load(string absoluteFilePath, out string data)
		{
			data = null;
			if (string.IsNullOrEmpty(absoluteFilePath))
			{
				return false;
			}
			if (!File.Exists(absoluteFilePath))
			{
				return false;
			}
			try
			{
				switch (_dataFormatDelegate())
				{
				case DataFormat.Binary:
				{
					byte[] array = File.ReadAllBytes(absoluteFilePath);
					data = _codec.Decode(array);
					return array != null && array.Length != 0;
				}
				case DataFormat.Text:
					data = File.ReadAllText(absoluteFilePath);
					return !string.IsNullOrEmpty(data);
				default:
					throw new NotImplementedException();
				}
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				return false;
			}
		}

		public bool Save(string absoluteFilePath, string data)
		{
			if (string.IsNullOrEmpty(absoluteFilePath))
			{
				return false;
			}
			try
			{
				string directoryName = Path.GetDirectoryName(absoluteFilePath);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				switch (_dataFormatDelegate())
				{
				case DataFormat.Binary:
					File.WriteAllBytes(absoluteFilePath, _codec.Encode(data));
					break;
				case DataFormat.Text:
					File.WriteAllText(absoluteFilePath, data);
					break;
				default:
					throw new NotImplementedException();
				}
				return true;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				return false;
			}
		}

		public bool Clear(string absoluteFilePath)
		{
			if (string.IsNullOrEmpty(absoluteFilePath))
			{
				return false;
			}
			try
			{
				if (File.Exists(absoluteFilePath))
				{
					File.Delete(absoluteFilePath);
					return true;
				}
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			return false;
		}
	}

	private abstract class Codec
	{
		public abstract byte[] Encode(string @string);

		public abstract string Decode(byte[] data);
	}

	private sealed class UTF8Text : Codec
	{
		public override byte[] Encode(string @string)
		{
			return Encoding.UTF8.GetBytes(@string);
		}

		public override string Decode(byte[] data)
		{
			return Encoding.UTF8.GetString(data);
		}
	}

	private sealed class CLZF2 : Codec
	{
		private readonly Rewired.Utils.Libraries.CLZF2.CLZF2 _cLZF2;

		public CLZF2()
		{
			_cLZF2 = new Rewired.Utils.Libraries.CLZF2.CLZF2();
		}

		public override byte[] Encode(string @string)
		{
			return _cLZF2.Compress(Encoding.UTF8.GetBytes(@string));
		}

		public override string Decode(byte[] data)
		{
			return Encoding.UTF8.GetString(_cLZF2.Decompress(data));
		}
	}

	public interface IDataHandler
	{
		bool Load(string absoluteFilePath, out string data);

		bool Save(string absoluteFilePath, string data);

		bool Clear(string absoluteFilePath);
	}

	public enum DataFormat
	{
		Text,
		Binary
	}

	private static readonly string thisScriptName = typeof(UserDataStore_File).Name;

	private const string logPrefix = "Rewired: ";

	private const string defaultExtensionText = ".json";

	private const string defaultExtensionBinary = ".bin";

	private const string defaultFileName = "RewiredSaveData.json";

	[Tooltip("The data file name. Changing this will make saved data already stored with the old file name no longer accessible.")]
	[SerializeField]
	private string _fileName = "RewiredSaveData.json";

	[Tooltip("Determines if the file should be stored as binary or text. Changing this will make saved data already stored no longer accessible.")]
	[SerializeField]
	private DataFormat _dataFormat;

	[NonSerialized]
	private string __directory;

	[NonSerialized]
	private DataStore _dataStore;

	[NonSerialized]
	private IDataHandler __dataHandler;

	[NonSerialized]
	private bool _initialized;

	public string directory
	{
		get
		{
			if (string.IsNullOrEmpty(__directory))
			{
				return __directory = Application.persistentDataPath;
			}
			return __directory;
		}
		set
		{
			__directory = value;
			if (_initialized)
			{
				OnDataSourceChanged();
			}
		}
	}

	public string fileName
	{
		get
		{
			return _fileName;
		}
		set
		{
			_fileName = value;
			if (_initialized)
			{
				OnDataSourceChanged();
			}
		}
	}

	public DataFormat dataFormat
	{
		get
		{
			return _dataFormat;
		}
		set
		{
			_dataFormat = value;
			if (_initialized)
			{
				OnDataSourceChanged();
			}
		}
	}

	protected IDataHandler dataHandler
	{
		get
		{
			if (__dataHandler == null)
			{
				return __dataHandler = new LocalFileDataHandler(() => _dataFormat, new CLZF2());
			}
			return __dataHandler;
		}
		set
		{
			__dataHandler = value;
			if (_initialized)
			{
				OnDataSourceChanged();
			}
		}
	}

	protected override IDataStore dataStore => _dataStore;

	protected virtual void SetInitialValues()
	{
	}

	protected override void OnInitialize()
	{
		SetInitialValues();
		_initialized = true;
		OnDataSourceChanged();
		base.OnInitialize();
	}

	private void OnDataSourceChanged()
	{
		_dataStore = new DataStore((!string.IsNullOrEmpty(_fileName)) ? _fileName : "RewiredSaveData.json", directory, dataHandler);
	}
}
