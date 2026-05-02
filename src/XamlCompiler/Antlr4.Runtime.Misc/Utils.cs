using System;
using System.Collections.Generic;
using System.Text;

namespace Antlr4.Runtime.Misc;

public class Utils
{
	public static string Join<T>(string separator, IEnumerable<T> items)
	{
		return string.Join(separator, items);
	}

	public static int NumNonnull(object[] data)
	{
		int num = 0;
		if (data == null)
		{
			return num;
		}
		foreach (object obj in data)
		{
			if (obj != null)
			{
				num++;
			}
		}
		return num;
	}

	public static void RemoveAllElements<T>(ICollection<T> data, T value)
	{
		if (data != null)
		{
			while (data.Contains(value))
			{
				data.Remove(value);
			}
		}
	}

	public static string EscapeWhitespace(string s, bool escapeSpaces)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = s.ToCharArray();
		foreach (char c in array)
		{
			if (c == ' ' && escapeSpaces)
			{
				stringBuilder.Append('·');
				continue;
			}
			switch (c)
			{
			case '\t':
				stringBuilder.Append("\\t");
				break;
			case '\n':
				stringBuilder.Append("\\n");
				break;
			case '\r':
				stringBuilder.Append("\\r");
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static void RemoveAll<T>(IList<T> list, Predicate<T> predicate)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			T val = list[i];
			if (!predicate(val))
			{
				if (num != i)
				{
					list[num] = val;
				}
				num++;
			}
		}
		while (num < list.Count)
		{
			list.RemoveAt(list.Count - 1);
		}
	}

	public static IDictionary<string, int> ToMap(string[] keys)
	{
		IDictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 0; i < keys.Length; i++)
		{
			dictionary[keys[i]] = i;
		}
		return dictionary;
	}

	public static char[] ToCharArray(List<int> data)
	{
		if (data == null)
		{
			return null;
		}
		char[] array = new char[data.Count];
		for (int i = 0; i < data.Count; i++)
		{
			array[i] = (char)data[i];
		}
		return array;
	}
}
