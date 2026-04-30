using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyVectorType : MetadataOnlyCommonArrayType
{
	public override string FullName
	{
		get
		{
			string fullName = GetElementType().FullName;
			if (fullName == null || GetElementType().IsGenericTypeDefinition)
			{
				return null;
			}
			return fullName + "[]";
		}
	}

	public override string Name => GetElementType().Name + "[]";

	public MetadataOnlyVectorType(MetadataOnlyCommonType elementType)
		: base(elementType)
	{
	}

	public override int GetArrayRank()
	{
		return 1;
	}

	protected override bool IsArrayImpl()
	{
		return true;
	}

	public override bool Equals(Type t)
	{
		if (t == null)
		{
			return false;
		}
		if (t is MetadataOnlyVectorType && t.GetArrayRank() == 1)
		{
			return GetElementType().Equals(t.GetElementType());
		}
		return false;
	}

	public override bool IsAssignableFrom(Type c)
	{
		if (c == null)
		{
			return false;
		}
		if (c.IsArray && c.GetArrayRank() == 1 && c is MetadataOnlyVectorType)
		{
			Type elementType = c.GetElementType();
			if (elementType.IsValueType)
			{
				return GetElementType().Equals(elementType);
			}
			return GetElementType().IsAssignableFrom(elementType);
		}
		return MetadataOnlyTypeDef.IsAssignableFromHelper(this, c);
	}

	public override string ToString()
	{
		return GetElementType().ToString() + "[]";
	}
}
