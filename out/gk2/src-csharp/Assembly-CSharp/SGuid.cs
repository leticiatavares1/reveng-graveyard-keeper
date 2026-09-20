using System;
using UnityEngine;

[Serializable]
public class SGuid : IEquatable<SGuid>
{
	[SerializeField]
	private string id;

	private Guid guid;

	public string Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
			guid = new Guid(id);
		}
	}

	public Guid Guid
	{
		get
		{
			if (guid == Guid.Empty)
			{
				guid = new Guid(id);
			}
			return guid;
		}
	}

	public static SGuid Empty => new SGuid(Guid.Empty);

	public bool IsEmpty => this == Empty;

	public SGuid()
	{
		id = Guid.NewGuid().ToString();
	}

	public SGuid(Guid newGuid)
	{
		id = newGuid.ToString();
		guid = newGuid;
	}

	public SGuid(string strRepresentation)
	{
		guid = Guid.Parse(strRepresentation);
		id = strRepresentation;
	}

	public void SetGuid(SGuid newGuid)
	{
		id = newGuid.id;
		guid = newGuid.Guid;
	}

	public static SGuid Parse(string strRepresentation)
	{
		return new SGuid(strRepresentation);
	}

	public static bool operator ==(SGuid left, SGuid right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		return left.Guid == right.Guid;
	}

	public static bool operator !=(SGuid left, SGuid right)
	{
		return !(left == right);
	}

	public override string ToString()
	{
		return id;
	}

	public bool Equals(SGuid other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (id == other.id)
		{
			return object.Equals(Guid, other.Guid);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SGuid)obj);
	}

	public override int GetHashCode()
	{
		return id.GetHashCode();
	}

	public static bool IsNullOrEmpty(SGuid sGuid)
	{
		if (!(sGuid == null))
		{
			return sGuid.IsEmpty;
		}
		return true;
	}
}
