using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

internal struct _InternalControlPropertyUpdateData
{
	internal Dictionary<string, _InternalControlPropertyMetaData> properties;

	public _InternalControlPropertyUpdateData(string name, _KnownControlPropertyPrimitiveTypes type, object value)
	{
		properties = new Dictionary<string, _InternalControlPropertyMetaData>();
		_InternalControlPropertyMetaData value2 = new _InternalControlPropertyMetaData
		{
			type = type
		};
		try
		{
			switch (type)
			{
			case _KnownControlPropertyPrimitiveTypes.Boolean:
				value2.boolValue = (bool)value;
				break;
			case _KnownControlPropertyPrimitiveTypes.Number:
				value2.numberValue = (double)value;
				break;
			default:
				value2.stringValue = value.ToString();
				break;
			}
		}
		catch (Exception ex)
		{
			InteractivityManager.SingletonInstance._LogError("Failed to cast the value to a known type. Exception: " + ex.Message);
		}
		properties.Add(name, value2);
	}
}
