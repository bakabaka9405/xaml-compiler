using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Context;
using System.Xaml.MS.Impl;

namespace MS.Internal.Xaml.Context;

[DebuggerDisplay("{ToString()}")]
internal class ObjectWriterFrame : XamlCommonFrame
{
	[Flags]
	private enum ObjectWriterFrameFlags : byte
	{
		None = 0,
		WasAssignedAtCreation = 1,
		IsObjectFromMember = 2,
		IsPropertyValueSet = 4,
		IsKeySet = 8,
		IsTypeConvertedObject = 0x10,
		KeyIsUnconverted = 0x20,
		ShouldConvertChildKeys = 0x40,
		ShouldNotConvertChildKeys = 0x80
	}

	private ObjectWriterFrameFlags _flags;

	private Dictionary<XamlMember, object> _preconstructionPropertyValues;

	private System.Xaml.Context.HashSet<XamlMember> _assignedProperties;

	private object _key;

	public object Instance { get; set; }

	public object Collection { get; set; }

	public bool WasAssignedAtCreation
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.WasAssignedAtCreation);
		}
		set
		{
			SetFlag(ObjectWriterFrameFlags.WasAssignedAtCreation, value);
		}
	}

	public bool IsObjectFromMember
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.IsObjectFromMember);
		}
		set
		{
			SetFlag(ObjectWriterFrameFlags.IsObjectFromMember, value);
		}
	}

	public bool IsPropertyValueSet
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.IsPropertyValueSet);
		}
		set
		{
			SetFlag(ObjectWriterFrameFlags.IsPropertyValueSet, value);
		}
	}

	public bool IsKeySet
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.IsKeySet);
		}
		private set
		{
			SetFlag(ObjectWriterFrameFlags.IsKeySet, value);
		}
	}

	public bool IsTypeConvertedObject
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.IsTypeConvertedObject);
		}
		set
		{
			SetFlag(ObjectWriterFrameFlags.IsTypeConvertedObject, value);
		}
	}

	public bool KeyIsUnconverted
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.KeyIsUnconverted);
		}
		set
		{
			SetFlag(ObjectWriterFrameFlags.KeyIsUnconverted, value);
		}
	}

	public bool ShouldConvertChildKeys
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.ShouldConvertChildKeys);
		}
		set
		{
			SetFlag(ObjectWriterFrameFlags.ShouldConvertChildKeys, value);
		}
	}

	public bool ShouldNotConvertChildKeys
	{
		get
		{
			return GetFlag(ObjectWriterFrameFlags.ShouldNotConvertChildKeys);
		}
		set
		{
			SetFlag(ObjectWriterFrameFlags.ShouldNotConvertChildKeys, value);
		}
	}

	public INameScopeDictionary NameScopeDictionary { get; set; }

	public object[] PositionalCtorArgs { get; set; }

	public object Key
	{
		get
		{
			if (_key is FixupTargetKeyHolder fixupTargetKeyHolder)
			{
				return fixupTargetKeyHolder.Key;
			}
			return _key;
		}
		set
		{
			_key = value;
			IsKeySet = true;
		}
	}

	public string InstanceRegisteredName { get; set; }

	public Dictionary<XamlMember, object> PreconstructionPropertyValues
	{
		get
		{
			if (_preconstructionPropertyValues == null)
			{
				_preconstructionPropertyValues = new Dictionary<XamlMember, object>();
			}
			return _preconstructionPropertyValues;
		}
	}

	public bool HasPreconstructionPropertyValuesDictionary => _preconstructionPropertyValues != null;

	public System.Xaml.Context.HashSet<XamlMember> AssignedProperties
	{
		get
		{
			if (_assignedProperties == null)
			{
				_assignedProperties = new System.Xaml.Context.HashSet<XamlMember>();
			}
			return _assignedProperties;
		}
	}

	public ObjectWriterFrame()
	{
	}

	public ObjectWriterFrame(ObjectWriterFrame source)
		: base(source)
	{
		if (source._preconstructionPropertyValues != null)
		{
			_preconstructionPropertyValues = new Dictionary<XamlMember, object>(source.PreconstructionPropertyValues);
		}
		if (source._assignedProperties != null)
		{
			_assignedProperties = new System.Xaml.Context.HashSet<XamlMember>(source.AssignedProperties);
		}
		_key = source._key;
		_flags = source._flags;
		Instance = source.Instance;
		Collection = source.Collection;
		NameScopeDictionary = source.NameScopeDictionary;
		PositionalCtorArgs = source.PositionalCtorArgs;
		InstanceRegisteredName = source.InstanceRegisteredName;
	}

	public override void Reset()
	{
		base.Reset();
		_preconstructionPropertyValues = null;
		_assignedProperties = null;
		Instance = null;
		Collection = null;
		NameScopeDictionary = null;
		PositionalCtorArgs = null;
		InstanceRegisteredName = null;
		_flags = ObjectWriterFrameFlags.None;
		_key = null;
	}

	public override XamlFrame Clone()
	{
		return new ObjectWriterFrame(this);
	}

	public override string ToString()
	{
		string text = ((base.XamlType == null) ? string.Empty : base.XamlType.Name);
		string text2 = ((base.Member == null) ? "-" : base.Member.Name);
		string text3 = ((Instance == null) ? "-" : ((Instance is string) ? Instance.ToString() : "*"));
		string text4 = ((Collection == null) ? "-" : "*");
		return KS.Fmt("{0}.{1} inst={2} coll={3}", text, text2, text3, text4);
	}

	private bool GetFlag(ObjectWriterFrameFlags flag)
	{
		return (_flags & flag) != 0;
	}

	private void SetFlag(ObjectWriterFrameFlags flag, bool value)
	{
		if (value)
		{
			_flags |= flag;
		}
		else
		{
			_flags &= (ObjectWriterFrameFlags)(byte)(~(int)flag);
		}
	}
}
