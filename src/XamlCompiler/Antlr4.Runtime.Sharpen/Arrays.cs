using System;
using System.Collections.Generic;
using System.Text;

namespace Antlr4.Runtime.Sharpen;

internal static class Arrays
{
	public static T[] CopyOf<T>(T[] array, int newSize)
	{
		if (array.Length == newSize)
		{
			return (T[])array.Clone();
		}
		Array.Resize(ref array, newSize);
		return array;
	}

	public static IList<T> AsList<T>(params T[] array)
	{
		return array;
	}

	public static void Fill<T>(T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = value;
		}
	}

	public static int HashCode<T>(T[] array)
	{
		if (array == null)
		{
			return 0;
		}
		int num = 1;
		for (int i = 0; i < array.Length; i++)
		{
			num = 31 * num + (((object)array[i])?.GetHashCode() ?? 0);
		}
		return num;
	}

	public static bool Equals<T>(T[] left, T[] right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null)
		{
			return false;
		}
		if (left.Length != right.Length)
		{
			return false;
		}
		for (int i = 0; i < left.Length; i++)
		{
			if (!object.Equals(left[i], right[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static string ToString<T>(T[] array)
	{
		if (array == null)
		{
			return "null";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('[');
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			T val = array[i];
			if (val == null)
			{
				stringBuilder.Append("null");
			}
			else
			{
				stringBuilder.Append(val);
			}
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}
}
