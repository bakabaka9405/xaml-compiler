using System;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyArrayType : MetadataOnlyCommonArrayType
{
	private readonly int m_rank;

	public override string FullName
	{
		get
		{
			string fullName = GetElementType().FullName;
			if (fullName == null || GetElementType().IsGenericTypeDefinition)
			{
				return null;
			}
			return fullName + "[" + GetDimensionString(m_rank) + "]";
		}
	}

	public override string Name => GetElementType().Name + "[" + GetDimensionString(m_rank) + "]";

	public MetadataOnlyArrayType(MetadataOnlyCommonType elementType, int rank)
		: base(elementType)
	{
		m_rank = rank;
	}

	private static string GetDimensionString(int rank)
	{
		if (rank == 1)
		{
			return "*";
		}
		StringBuilder builder = StringBuilderPool.Get();
		for (int i = 1; i < rank; i++)
		{
			builder.Append(',');
		}
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}

	public override int GetArrayRank()
	{
		return m_rank;
	}

	public override bool Equals(Type t)
	{
		if (t == null)
		{
			return false;
		}
		if (t is MetadataOnlyArrayType && t.GetArrayRank() == GetArrayRank())
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
		if (c.IsArray)
		{
			if (c.GetArrayRank() != m_rank)
			{
				return false;
			}
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
		return GetElementType().ToString() + "[" + GetDimensionString(m_rank) + "]";
	}
}
