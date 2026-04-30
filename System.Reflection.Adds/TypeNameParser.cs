using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Reflection.Adds;

internal static class TypeNameParser
{
	private enum TokenType
	{
		Id,
		LeftBracket,
		RightBracket,
		Comma,
		Plus,
		Equals,
		Reference,
		Pointer,
		EndOfInput
	}

	private class Token
	{
		internal static readonly Token Plus = new Token(TokenType.Plus, null);

		internal static readonly Token LeftBracket = new Token(TokenType.LeftBracket, null);

		internal static readonly Token RightBracket = new Token(TokenType.RightBracket, null);

		internal static readonly Token Comma = new Token(TokenType.Comma, null);

		internal new static readonly Token Equals = new Token(TokenType.Equals, null);

		internal static readonly Token Reference = new Token(TokenType.Reference, null);

		internal static readonly Token Pointer = new Token(TokenType.Pointer, null);

		internal TokenType TokenType { get; private set; }

		internal string Value { get; private set; }

		private Token(TokenType tokenType, string value)
		{
			TokenType = tokenType;
			Value = value;
		}

		internal static Token MakeIdToken(string value)
		{
			return new Token(TokenType.Id, value);
		}
	}

	private static readonly char[] compoundTypeNameCharacters = new char[5] { '+', ',', '[', '*', '&' };

	public static Type ParseTypeName(ITypeUniverse universe, Module module, string input)
	{
		bool throwOnError = true;
		return ParseTypeName(universe, module, input, throwOnError);
	}

	public static Type ParseTypeName(ITypeUniverse universe, Module module, string input, bool throwOnError)
	{
		return ParseTypeName(universe, module, input, useSystemAssemblyToResolveTypes: false, useWindowsRuntimeResolution: false, throwOnError);
	}

	public static Type ParseTypeName(ITypeUniverse universe, Module module, string input, bool useSystemAssemblyToResolveTypes, bool useWindowsRuntimeResolution, bool throwOnError)
	{
		Module systemModule = universe.GetSystemAssembly().ManifestModule;
		Func<AssemblyName, Assembly> assemblyResolver = (AssemblyName assemblyName) => DetermineAssembly(assemblyName, module, universe, throwOnError);
		Func<Assembly, string, bool, Type> typeResolver = delegate(Assembly assembly, string simpleTypeName, bool ignoreCase)
		{
			bool throwOnError2 = false;
			Type type = null;
			if (assembly != null)
			{
				type = assembly.GetType(simpleTypeName, throwOnError2, ignoreCase);
			}
			else
			{
				if (null == type)
				{
					type = module.GetType(simpleTypeName, throwOnError2, ignoreCase);
				}
				if (null == type && useSystemAssemblyToResolveTypes)
				{
					type = systemModule.GetType(simpleTypeName, throwOnError2, ignoreCase);
				}
				if (null == type && useWindowsRuntimeResolution)
				{
					type = universe.ResolveWindowsRuntimeType(simpleTypeName, throwOnError2, ignoreCase);
				}
			}
			return type;
		};
		return Type.GetType(input, assemblyResolver, typeResolver, throwOnError);
	}

	private static Type ParseTypeName(ITypeUniverse universe, Module defaultTokenResolver, string input, ref int idx, bool throwOnError, bool isGenericArgument, bool expectAssemblyName)
	{
		List<string> list = new List<string>();
		List<Type> list2 = new List<Type>();
		AssemblyName assemblyName = null;
		while (true)
		{
			list.Add(ReadIdWithoutLeadingSpaces(input, ref idx));
			if (PeekNextToken(input, idx) != TokenType.Plus)
			{
				break;
			}
			ReadSpecialToken(input, TokenType.Plus, ref idx);
		}
		if (IsGenericType(input, idx))
		{
			ReadSpecialToken(input, TokenType.LeftBracket, ref idx);
			while (true)
			{
				bool flag = false;
				if (PeekNextToken(input, idx) == TokenType.LeftBracket)
				{
					flag = true;
					ReadSpecialToken(input, TokenType.LeftBracket, ref idx);
				}
				Type type = ParseTypeName(universe, defaultTokenResolver, input, ref idx, throwOnError, isGenericArgument: true, flag);
				if (type == null)
				{
					return null;
				}
				list2.Add(type);
				if (flag)
				{
					ReadSpecialToken(input, TokenType.RightBracket, ref idx);
				}
				if (PeekNextToken(input, idx) != TokenType.Comma)
				{
					break;
				}
				ReadSpecialToken(input, TokenType.Comma, ref idx);
			}
			ReadSpecialToken(input, TokenType.RightBracket, ref idx);
		}
		int idx2 = idx;
		ReadModifiers(null, input, ref idx);
		int num = idx;
		if ((!isGenericArgument || expectAssemblyName) && PeekNextToken(input, idx) == TokenType.Comma)
		{
			ReadSpecialToken(input, TokenType.Comma, ref idx);
			assemblyName = ParseAssemblyInfo(input, ref idx);
		}
		Assembly assembly = DetermineAssembly(assemblyName, defaultTokenResolver, universe);
		Type type2 = Resolve(list, list2, assembly);
		if (type2 == null)
		{
			if (throwOnError)
			{
				throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, Resources.CannotFindTypeInModule, input, assembly.ToString()));
			}
			return null;
		}
		return ReadModifiers(type2, input, ref idx2);
	}

	private static bool IsGenericType(string input, int idx)
	{
		if (PeekNextToken(input, idx) == TokenType.LeftBracket && PeekSecondToken(input, idx) != TokenType.RightBracket && PeekSecondToken(input, idx) != TokenType.Comma && PeekSecondToken(input, idx) != TokenType.Pointer)
		{
			return true;
		}
		return false;
	}

	private static Type ReadModifiers(Type type, string input, ref int idx)
	{
		while (true)
		{
			if (PeekNextToken(input, idx) == TokenType.LeftBracket)
			{
				if (PeekSecondToken(input, idx) == TokenType.RightBracket)
				{
					ReadSpecialToken(input, TokenType.LeftBracket, ref idx);
					ReadSpecialToken(input, TokenType.RightBracket, ref idx);
					if (type != null)
					{
						type = type.MakeArrayType();
					}
					continue;
				}
				if (PeekSecondToken(input, idx) == TokenType.Comma)
				{
					ReadSpecialToken(input, TokenType.LeftBracket, ref idx);
					int num = 1;
					while (PeekNextToken(input, idx) == TokenType.Comma)
					{
						ReadSpecialToken(input, TokenType.Comma, ref idx);
						num++;
					}
					if (PeekNextToken(input, idx) == TokenType.RightBracket)
					{
						ReadSpecialToken(input, TokenType.RightBracket, ref idx);
						if (type != null)
						{
							type = type.MakeArrayType(num);
						}
						continue;
					}
					throw new ArgumentException(Resources.UnexpectedCharacterFound);
				}
			}
			if (PeekNextToken(input, idx) == TokenType.Reference)
			{
				ReadSpecialToken(input, TokenType.Reference, ref idx);
				if (type != null)
				{
					type = type.MakeByRefType();
				}
				continue;
			}
			if (PeekNextToken(input, idx) != TokenType.Pointer)
			{
				break;
			}
			ReadSpecialToken(input, TokenType.Pointer, ref idx);
			if (type != null)
			{
				type = type.MakePointerType();
			}
		}
		return type;
	}

	private static AssemblyName ParseAssemblyInfo(string input, ref int idx)
	{
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = ReadIdWithoutLeadingAndEndingSpaces(input, ref idx);
		while (true)
		{
			TokenType tokenType = PeekNextToken(input, idx);
			if (tokenType != TokenType.Comma)
			{
				break;
			}
			ReadSpecialToken(input, TokenType.Comma, ref idx);
			string text = ReadIdWithoutLeadingAndEndingSpaces(input, ref idx);
			ReadSpecialToken(input, TokenType.Equals, ref idx);
			string text2 = ReadIdWithoutLeadingAndEndingSpaces(input, ref idx);
			switch (text)
			{
			case "Version":
				if (assemblyName.Version != null)
				{
					throw new ArgumentException(Resources.VersionAlreadyDefined);
				}
				assemblyName.Version = new Version(text2);
				break;
			case "Culture":
				if (!text2.Equals("neutral"))
				{
					assemblyName.CultureInfo = CultureInfo.GetCultureInfo(text2);
				}
				else
				{
					assemblyName.CultureInfo = CultureInfo.InvariantCulture;
				}
				break;
			case "PublicKeyToken":
				if (!text2.Equals("null"))
				{
					if ((text2.Length & 1) != 0)
					{
						throw new ArgumentException(Resources.InvalidPublicKeyTokenLength);
					}
					byte[] array = new byte[text2.Length / 2];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = byte.Parse(text2.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					}
					assemblyName.SetPublicKeyToken(array);
				}
				else
				{
					assemblyName.SetPublicKeyToken(new byte[0]);
				}
				break;
			default:
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.UnrecognizedAssemblyAttribute, text));
			}
		}
		return assemblyName;
	}

	private static Assembly DetermineAssembly(AssemblyName assemblyName, Module defaultTokenResolver, ITypeUniverse universe, bool throwOnError = true)
	{
		if (assemblyName != null)
		{
			if (universe == null)
			{
				throw new ArgumentException(Resources.HostSpecifierMissing);
			}
			Assembly assembly = universe.ResolveAssembly(assemblyName, throwOnError);
			if (assembly == null && throwOnError)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.UniverseCannotResolveAssembly, assemblyName));
			}
			return assembly;
		}
		if (defaultTokenResolver == null)
		{
			throw new ArgumentException(Resources.DefaultTokenResolverRequired);
		}
		return defaultTokenResolver.Assembly;
	}

	private static Type Resolve(List<string> path, List<Type> genericTypeArgs, Assembly assembly)
	{
		Type type = assembly.GetType(path[0], throwOnError: false);
		if (type == null)
		{
			return null;
		}
		for (int i = 1; i < path.Count; i++)
		{
			Type nestedType = type.GetNestedType(path[i], BindingFlags.Public | BindingFlags.NonPublic);
			if (nestedType == null)
			{
				return null;
			}
			type = nestedType;
		}
		if (genericTypeArgs.Count > 0)
		{
			type = type.MakeGenericType(genericTypeArgs.ToArray());
		}
		return type;
	}

	private static void ReadSpecialToken(string input, TokenType expected, ref int idx)
	{
		Token token = ReadToken(input, ref idx);
		if (token == null || token.TokenType != expected)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ExpectedTokenType, expected));
		}
	}

	private static string ReadIdToken(string input, ref int idx)
	{
		Token token = ReadToken(input, ref idx);
		if (token == null || token.TokenType != TokenType.Id)
		{
			throw new ArgumentException(Resources.IdTokenTypeExpected);
		}
		return token.Value;
	}

	private static string ReadIdWithoutLeadingSpaces(string input, ref int idx)
	{
		return TrimLeadingSpaces(ReadIdToken(input, ref idx));
	}

	private static string ReadIdWithoutLeadingAndEndingSpaces(string input, ref int idx)
	{
		return ReadIdToken(input, ref idx).Trim();
	}

	private static TokenType PeekNextToken(string input, int idx)
	{
		return ReadToken(input, ref idx)?.TokenType ?? TokenType.EndOfInput;
	}

	private static TokenType PeekSecondToken(string input, int idx)
	{
		Token token = ReadToken(input, ref idx);
		if (token == null)
		{
			throw new ArgumentException(Resources.UnexpectedEndOfInput);
		}
		return PeekNextToken(input, idx);
	}

	private static Token ReadToken(string input, ref int idx)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		int i;
		for (i = idx; i < input.Length; i++)
		{
			char c = input[i];
			if (flag)
			{
				stringBuilder.Append(c);
				flag = false;
				continue;
			}
			switch (c)
			{
			case '\'':
				flag = true;
				break;
			case '&':
			case '*':
			case '+':
			case ',':
			case '=':
			case '[':
			case ']':
				if (stringBuilder.Length > 0)
				{
					idx = i;
					return Token.MakeIdToken(stringBuilder.ToString());
				}
				idx = i + 1;
				return c switch
				{
					'+' => Token.Plus, 
					'[' => Token.LeftBracket, 
					']' => Token.RightBracket, 
					',' => Token.Comma, 
					'=' => Token.Equals, 
					'&' => Token.Reference, 
					'*' => Token.Pointer, 
					_ => throw new InvalidOperationException(Resources.UnexpectedCharacterFound), 
				};
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		if (flag)
		{
			throw new ArgumentException(Resources.EscapeSequenceMissingCharacter);
		}
		idx = i;
		if (stringBuilder.Length > 0)
		{
			return Token.MakeIdToken(stringBuilder.ToString());
		}
		return null;
	}

	private static string TrimLeadingSpaces(string str)
	{
		int i;
		for (i = 0; i < str.Length && char.IsWhiteSpace(str, i); i++)
		{
		}
		return str.Substring(i);
	}

	public static bool IsCompoundType(string name)
	{
		return name.IndexOfAny(compoundTypeNameCharacters) > 0;
	}
}
