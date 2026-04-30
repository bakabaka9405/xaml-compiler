using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class SignatureUtil
{
	public static class TypeMapForAttributes
	{
		private static readonly Dictionary<string, CorElementType> s_typeNameMapForAttributes = new Dictionary<string, CorElementType>
		{
			{
				"System.Boolean",
				CorElementType.Bool
			},
			{
				"System.Char",
				CorElementType.Char
			},
			{
				"System.SByte",
				CorElementType.SByte
			},
			{
				"System.Byte",
				CorElementType.Byte
			},
			{
				"System.Int16",
				CorElementType.Short
			},
			{
				"System.UInt16",
				CorElementType.UShort
			},
			{
				"System.Int32",
				CorElementType.Int
			},
			{
				"System.UInt32",
				CorElementType.UInt
			},
			{
				"System.Int64",
				CorElementType.Long
			},
			{
				"System.UInt64",
				CorElementType.ULong
			},
			{
				"System.Single",
				CorElementType.Float
			},
			{
				"System.Double",
				CorElementType.Double
			},
			{
				"System.IntPtr",
				CorElementType.IntPtr
			},
			{
				"System.UIntPtr",
				CorElementType.UIntPtr
			},
			{
				"System.Array",
				CorElementType.SzArray
			},
			{
				"System.String",
				CorElementType.String
			},
			{
				"System.Type",
				CorElementType.Type
			},
			{
				"System.Object",
				CorElementType.Object
			}
		};

		public static bool IsValidCustomAttributeElementType(CorElementType elementType)
		{
			return s_typeNameMapForAttributes.ContainsValue(elementType);
		}

		public static bool LookupPrimitive(Type type, out CorElementType result)
		{
			result = CorElementType.End;
			ITypeUniverse typeUniverse = Helpers.Universe(type);
			if (typeUniverse != null && type is ITypeReference typeReference)
			{
				type = typeReference.GetResolvedType();
				if (!typeUniverse.GetSystemAssembly().Equals(type.Assembly) && (!(typeUniverse.GetSystemRuntimeAssembly() != null) || !typeUniverse.GetSystemRuntimeAssembly().Equals(type.Assembly)))
				{
					return false;
				}
			}
			return s_typeNameMapForAttributes.TryGetValue(type.FullName, out result);
		}
	}

	private static readonly uint[] s_tkCorEncodeToken = new uint[4] { 33554432u, 16777216u, 452984832u, 1912602624u };

	private const byte FieldId = 83;

	private const byte PropertyId = 84;

	private const CorElementType BoxedValue = (CorElementType)81;

	internal static CorElementType ExtractElementType(byte[] sig, ref int index)
	{
		return (CorElementType)ExtractInt(sig, ref index);
	}

	internal static UnmanagedType ExtractUnmanagedType(byte[] sig, ref int index)
	{
		return (UnmanagedType)ExtractInt(sig, ref index);
	}

	internal static CorCallingConvention ExtractCallingConvention(byte[] sig, ref int index)
	{
		return (CorCallingConvention)ExtractInt(sig, ref index);
	}

	internal static CustomModifiers ExtractCustomModifiers(byte[] sig, ref int index, MetadataOnlyModule resolver, GenericContext context)
	{
		int num = index;
		CorElementType corElementType = ExtractElementType(sig, ref index);
		List<Type> list = null;
		List<Type> list2 = null;
		if (corElementType == CorElementType.CModOpt || corElementType == CorElementType.CModReqd)
		{
			list = new List<Type>();
			list2 = new List<Type>();
			while (corElementType == CorElementType.CModOpt || corElementType == CorElementType.CModReqd)
			{
				Token token = ExtractToken(sig, ref index);
				Type item = resolver.ResolveTypeTokenInternal(token, context);
				if (corElementType == CorElementType.CModOpt)
				{
					list.Add(item);
				}
				else
				{
					list2.Add(item);
				}
				num = index;
				corElementType = ExtractElementType(sig, ref index);
			}
			index = num;
			return new CustomModifiers(list, list2);
		}
		index = num;
		return null;
	}

	internal static Type ExtractType(byte[] sig, ref int index, MetadataOnlyModule resolver, GenericContext context)
	{
		TypeSignatureDescriptor typeSignatureDescriptor = ExtractType(sig, ref index, resolver, context, fAllowPinned: false);
		return typeSignatureDescriptor.Type;
	}

	internal static TypeSignatureDescriptor ExtractType(byte[] sig, ref int index, MetadataOnlyModule resolver, GenericContext context, bool fAllowPinned)
	{
		TypeSignatureDescriptor typeSignatureDescriptor = new TypeSignatureDescriptor();
		typeSignatureDescriptor.IsPinned = false;
		CorElementType corElementType = ExtractElementType(sig, ref index);
		switch (corElementType)
		{
		case CorElementType.Void:
		case CorElementType.Bool:
		case CorElementType.Char:
		case CorElementType.SByte:
		case CorElementType.Byte:
		case CorElementType.Short:
		case CorElementType.UShort:
		case CorElementType.Int:
		case CorElementType.UInt:
		case CorElementType.Long:
		case CorElementType.ULong:
		case CorElementType.Float:
		case CorElementType.Double:
		case CorElementType.String:
		case CorElementType.IntPtr:
		case CorElementType.UIntPtr:
		case CorElementType.Object:
			typeSignatureDescriptor.Type = resolver.AssemblyResolver.GetBuiltInType(corElementType);
			break;
		case CorElementType.Array:
		{
			Type type2 = ExtractType(sig, ref index, resolver, context);
			int rank = ExtractInt(sig, ref index);
			int num6 = ExtractInt(sig, ref index);
			for (int k = 0; k < num6; k++)
			{
				ExtractInt(sig, ref index);
			}
			int num7 = ExtractInt(sig, ref index);
			for (int l = 0; l < num7; l++)
			{
				ExtractInt(sig, ref index);
			}
			typeSignatureDescriptor.Type = type2.MakeArrayType(rank);
			break;
		}
		case CorElementType.Byref:
			typeSignatureDescriptor.Type = ExtractType(sig, ref index, resolver, context).MakeByRefType();
			break;
		case CorElementType.Pointer:
			typeSignatureDescriptor.Type = ExtractType(sig, ref index, resolver, context).MakePointerType();
			break;
		case CorElementType.SzArray:
			typeSignatureDescriptor.Type = ExtractType(sig, ref index, resolver, context).MakeArrayType();
			break;
		case CorElementType.ValueType:
		case CorElementType.Class:
		{
			Token token3 = ExtractToken(sig, ref index);
			typeSignatureDescriptor.Type = resolver.ResolveTypeTokenInternal(token3, corElementType, context);
			break;
		}
		case CorElementType.CModOpt:
		{
			Token token2 = ExtractToken(sig, ref index);
			resolver.ResolveTypeTokenInternal(token2, context);
			typeSignatureDescriptor.Type = ExtractType(sig, ref index, resolver, context);
			break;
		}
		case CorElementType.CModReqd:
		{
			Token token = ExtractToken(sig, ref index);
			resolver.ResolveTypeTokenInternal(token, context);
			typeSignatureDescriptor.Type = ExtractType(sig, ref index, resolver, context);
			break;
		}
		case CorElementType.GenericInstantiation:
		{
			int num4 = index;
			Type type = ExtractType(sig, ref index, resolver, null);
			int num5 = ExtractInt(sig, ref index);
			Type[] array = new Type[num5];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = ExtractType(sig, ref index, resolver, context);
			}
			typeSignatureDescriptor.Type = type.MakeGenericType(array);
			break;
		}
		case CorElementType.MethodVar:
		{
			int num3 = ExtractInt(sig, ref index);
			if (GenericContext.IsNullOrEmptyMethodArgs(context))
			{
				throw new ArgumentException(Resources.TypeArgumentCannotBeResolved);
			}
			typeSignatureDescriptor.Type = context.MethodArgs[num3];
			break;
		}
		case CorElementType.TypeVar:
		{
			int num2 = ExtractInt(sig, ref index);
			if (GenericContext.IsNullOrEmptyTypeArgs(context))
			{
				throw new ArgumentException(Resources.TypeArgumentCannotBeResolved);
			}
			typeSignatureDescriptor.Type = context.TypeArgs[num2];
			break;
		}
		case CorElementType.FnPtr:
		{
			ExtractCallingConvention(sig, ref index);
			int num = ExtractInt(sig, ref index);
			ExtractType(sig, ref index, resolver, context);
			for (int i = 0; i < num; i++)
			{
				ExtractType(sig, ref index, resolver, context);
			}
			typeSignatureDescriptor.Type = resolver.AssemblyResolver.GetBuiltInType(CorElementType.IntPtr);
			break;
		}
		case CorElementType.TypedByRef:
			typeSignatureDescriptor.Type = resolver.AssemblyResolver.GetTypeXFromName("System.TypedReference");
			break;
		case CorElementType.Pinned:
			typeSignatureDescriptor.IsPinned = true;
			typeSignatureDescriptor.Type = ExtractType(sig, ref index, resolver, context);
			break;
		default:
			throw new ArgumentException(Resources.IncorrectElementTypeValue);
		}
		return typeSignatureDescriptor;
	}

	internal static void ExtractCustomAttributeArgumentType(ITypeUniverse universe, Module module, byte[] customAttributeBlob, ref int index, out CorElementType argumentTypeId, out Type argumentType)
	{
		argumentTypeId = ExtractElementType(customAttributeBlob, ref index);
		VerifyElementType(argumentTypeId);
		if (argumentTypeId == CorElementType.SzArray)
		{
			CorElementType corElementType = ExtractElementType(customAttributeBlob, ref index);
			VerifyElementType(corElementType);
			switch (corElementType)
			{
			case (CorElementType)81:
				argumentType = universe.GetBuiltInType(CorElementType.Object).MakeArrayType();
				break;
			case CorElementType.Enum:
				argumentType = ExtractTypeValue(universe, module, customAttributeBlob, ref index);
				argumentType = argumentType.MakeArrayType();
				break;
			default:
				argumentType = universe.GetBuiltInType(corElementType).MakeArrayType();
				break;
			}
		}
		else if (argumentTypeId == CorElementType.Enum)
		{
			argumentType = ExtractTypeValue(universe, module, customAttributeBlob, ref index);
			if (argumentType == null)
			{
				throw new ArgumentException(Resources.InvalidCustomAttributeFormatForEnum);
			}
		}
		else if (argumentTypeId == (CorElementType)81)
		{
			argumentType = null;
		}
		else
		{
			argumentType = universe.GetBuiltInType(argumentTypeId);
		}
	}

	internal static bool IsVarArg(CorCallingConvention conv)
	{
		CorCallingConvention corCallingConvention = conv & CorCallingConvention.Mask;
		return corCallingConvention == CorCallingConvention.VarArg;
	}

	internal static int ExtractInt(byte[] sig, ref int index)
	{
		int result;
		if ((sig[index] & 0x80) == 0)
		{
			result = sig[index];
			index++;
		}
		else if ((sig[index] & 0xC0) == 128)
		{
			result = ((sig[index] & 0x3F) << 8) | sig[index + 1];
			index += 2;
		}
		else
		{
			if ((sig[index] & 0xE0) != 192)
			{
				throw new ArgumentException(Resources.InvalidMetadataSignature);
			}
			result = ((sig[index] & 0x1F) << 24) | (sig[index + 1] << 16) | (sig[index + 2] << 8) | sig[index + 3];
			index += 4;
		}
		return result;
	}

	internal static Token ExtractToken(byte[] sig, ref int index)
	{
		uint num = (uint)ExtractInt(sig, ref index);
		uint tkType = s_tkCorEncodeToken[num & 3];
		uint value = TokenFromRid(num >> 2, tkType);
		return new Token(value);
	}

	internal static CorElementType GetTypeId(Type type)
	{
		if (type.IsEnum)
		{
			return GetTypeId(MetadataOnlyModule.GetUnderlyingType(type));
		}
		if (type.IsArray)
		{
			return CorElementType.SzArray;
		}
		if (TypeMapForAttributes.LookupPrimitive(type, out var result))
		{
			return result;
		}
		throw new ArgumentException(Resources.UnsupportedTypeInAttributeSignature);
	}

	internal static string ExtractStringValue(byte[] blob, ref int index)
	{
		return (string)ExtractValue(CorElementType.String, blob, ref index);
	}

	internal static uint ExtractUIntValue(byte[] blob, ref int index)
	{
		return (uint)ExtractValue(CorElementType.UInt, blob, ref index);
	}

	internal static Type ExtractTypeValue(ITypeUniverse universe, Module module, byte[] blob, ref int index)
	{
		Type result = null;
		string text = ExtractStringValue(blob, ref index);
		if (!string.IsNullOrEmpty(text))
		{
			bool throwOnError = true;
			bool useSystemAssemblyToResolveTypes = true;
			result = TypeNameParser.ParseTypeName(universe, module, text, useSystemAssemblyToResolveTypes, MetadataOnlyModule.IsWindowsRuntime(module), throwOnError);
		}
		return result;
	}

	internal static object ExtractValue(CorElementType typeId, byte[] blob, ref int index)
	{
		switch (typeId)
		{
		case CorElementType.Bool:
		{
			object result = BitConverter.ToBoolean(blob, index);
			index++;
			return result;
		}
		case CorElementType.Byte:
		{
			object result = blob[index];
			index++;
			return result;
		}
		case CorElementType.SByte:
		{
			object result = (sbyte)blob[index];
			index++;
			return result;
		}
		case CorElementType.Short:
		{
			object result = BitConverter.ToInt16(blob, index);
			index += 2;
			return result;
		}
		case CorElementType.UShort:
		{
			object result = BitConverter.ToUInt16(blob, index);
			index += 2;
			return result;
		}
		case CorElementType.Int:
		{
			object result = BitConverter.ToInt32(blob, index);
			index += 4;
			return result;
		}
		case CorElementType.UInt:
		{
			object result = BitConverter.ToUInt32(blob, index);
			index += 4;
			return result;
		}
		case CorElementType.Long:
		{
			object result = BitConverter.ToInt64(blob, index);
			index += 8;
			return result;
		}
		case CorElementType.ULong:
		{
			object result = BitConverter.ToUInt64(blob, index);
			index += 8;
			return result;
		}
		case CorElementType.Char:
		{
			object result = BitConverter.ToChar(blob, index);
			index += 2;
			return result;
		}
		case CorElementType.Float:
		{
			object result = BitConverter.ToSingle(blob, index);
			index += 4;
			return result;
		}
		case CorElementType.Double:
		{
			object result = BitConverter.ToDouble(blob, index);
			index += 8;
			return result;
		}
		case CorElementType.String:
		{
			object result;
			if (blob[index] == byte.MaxValue)
			{
				index++;
				result = null;
			}
			else
			{
				int num = ExtractInt(blob, ref index);
				result = Encoding.UTF8.GetString(blob, index, num);
				index += num;
			}
			return result;
		}
		default:
			throw new InvalidOperationException(Resources.IncorrectElementTypeValue);
		}
	}

	internal static IList<CustomAttributeTypedArgument> ExtractListOfValues(Type elementType, ITypeUniverse universe, Module module, uint size, byte[] blob, ref int index)
	{
		CorElementType typeId = GetTypeId(elementType);
		List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>((int)size);
		switch (typeId)
		{
		case CorElementType.Object:
		{
			for (int j = 0; j < size; j++)
			{
				CorElementType corElementType = ExtractElementType(blob, ref index);
				VerifyElementType(corElementType);
				Type type = null;
				object obj = null;
				switch (corElementType)
				{
				case CorElementType.SzArray:
					throw new NotImplementedException(Resources.ArrayInsideArrayInAttributeNotSupported);
				case CorElementType.Enum:
					type = ExtractTypeValue(universe, module, blob, ref index);
					if (type != null)
					{
						Type underlyingType = MetadataOnlyModule.GetUnderlyingType(type);
						CorElementType typeId2 = GetTypeId(underlyingType);
						obj = ExtractValue(typeId2, blob, ref index);
						break;
					}
					throw new ArgumentException(Resources.InvalidCustomAttributeFormatForEnum);
				default:
					type = universe.GetBuiltInType(corElementType);
					obj = ExtractValue(corElementType, blob, ref index);
					break;
				}
				list.Add(new CustomAttributeTypedArgument(type, obj));
			}
			break;
		}
		case CorElementType.Type:
		{
			for (int k = 0; k < size; k++)
			{
				object value2 = ExtractTypeValue(universe, module, blob, ref index);
				list.Add(new CustomAttributeTypedArgument(elementType, value2));
			}
			break;
		}
		case CorElementType.SzArray:
			throw new ArgumentException(Resources.JaggedArrayInAttributeNotSupported);
		default:
		{
			for (int i = 0; i < size; i++)
			{
				object value = ExtractValue(typeId, blob, ref index);
				list.Add(new CustomAttributeTypedArgument(elementType, value));
			}
			break;
		}
		}
		return list.AsReadOnly();
	}

	internal static bool IsValidCustomAttributeElementType(CorElementType elementType)
	{
		return TypeMapForAttributes.IsValidCustomAttributeElementType(elementType);
	}

	internal static void VerifyElementType(CorElementType elementType)
	{
		if (elementType == CorElementType.Bool || elementType == CorElementType.Char || elementType == CorElementType.SByte || elementType == CorElementType.Byte || elementType == CorElementType.Short || elementType == CorElementType.UShort || elementType == CorElementType.Int || elementType == CorElementType.UInt || elementType == CorElementType.Long || elementType == CorElementType.ULong || elementType == CorElementType.Float || elementType == CorElementType.Double || elementType == CorElementType.String || elementType == CorElementType.Type || elementType == CorElementType.SzArray || elementType == CorElementType.Enum || elementType == (CorElementType)81)
		{
			return;
		}
		throw new ArgumentException(Resources.InvalidElementTypeInAttribute);
	}

	internal static NamedArgumentType ExtractNamedArgumentType(byte[] customAttributeBlob, ref int index)
	{
		return (byte)ExtractValue(CorElementType.Byte, customAttributeBlob, ref index) switch
		{
			84 => NamedArgumentType.Property, 
			83 => NamedArgumentType.Field, 
			_ => throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} {1}", Resources.InvalidCustomAttributeFormat, Resources.ExpectedPropertyOrFieldId)), 
		};
	}

	internal static MethodSignatureDescriptor ExtractMethodSignature(SignatureBlob methodSignatureBlob, MetadataOnlyModule resolver, GenericContext context)
	{
		byte[] signatureAsByteArray = methodSignatureBlob.GetSignatureAsByteArray();
		int index = 0;
		MethodSignatureDescriptor methodSignatureDescriptor = new MethodSignatureDescriptor();
		methodSignatureDescriptor.ReturnParameter = new TypeSignatureDescriptor();
		methodSignatureDescriptor.GenericParameterCount = 0;
		methodSignatureDescriptor.CallingConvention = ExtractCallingConvention(signatureAsByteArray, ref index);
		bool flag = (methodSignatureDescriptor.CallingConvention & CorCallingConvention.ExplicitThis) != 0;
		if ((methodSignatureDescriptor.CallingConvention & CorCallingConvention.Generic) != CorCallingConvention.Default)
		{
			int num = ExtractInt(signatureAsByteArray, ref index);
			if (num <= 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} {1}", Resources.InvalidMetadataSignature, Resources.ExpectedPositiveNumberOfGenericParameters));
			}
			context = context.VerifyAndUpdateMethodArguments(num);
			methodSignatureDescriptor.GenericParameterCount = num;
		}
		int num2 = ExtractInt(signatureAsByteArray, ref index);
		bool fAllowPinned = false;
		CustomModifiers customModifiers = ExtractCustomModifiers(signatureAsByteArray, ref index, resolver, context);
		methodSignatureDescriptor.ReturnParameter = ExtractType(signatureAsByteArray, ref index, resolver, context, fAllowPinned);
		methodSignatureDescriptor.ReturnParameter.CustomModifiers = customModifiers;
		if (flag)
		{
			ExtractType(signatureAsByteArray, ref index, resolver, context);
			num2--;
		}
		methodSignatureDescriptor.Parameters = new TypeSignatureDescriptor[num2];
		for (int i = 0; i < num2; i++)
		{
			customModifiers = ExtractCustomModifiers(signatureAsByteArray, ref index, resolver, context);
			methodSignatureDescriptor.Parameters[i] = ExtractType(signatureAsByteArray, ref index, resolver, context, fAllowPinned);
			methodSignatureDescriptor.Parameters[i].CustomModifiers = customModifiers;
		}
		if (index != signatureAsByteArray.Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} {1}", Resources.InvalidMetadataSignature, Resources.ExtraInformationAfterLastParameter));
		}
		return methodSignatureDescriptor;
	}

	internal static CallingConventions GetReflectionCallingConvention(CorCallingConvention callConvention)
	{
		CallingConventions callingConventions = (CallingConventions)0;
		if ((callConvention & CorCallingConvention.Mask) == CorCallingConvention.HasThis)
		{
			callingConventions |= CallingConventions.HasThis;
		}
		else if ((callConvention & CorCallingConvention.Mask) == CorCallingConvention.ExplicitThis)
		{
			callingConventions |= CallingConventions.ExplicitThis;
		}
		if (IsVarArg(callConvention))
		{
			return callingConventions | CallingConventions.VarArgs;
		}
		return callingConventions | CallingConventions.Standard;
	}

	internal static bool IsCallingConventionMatch(MethodBase method, CallingConventions callConvention)
	{
		if ((callConvention & CallingConventions.Any) == 0)
		{
			if ((callConvention & CallingConventions.VarArgs) != 0 && (method.CallingConvention & CallingConventions.VarArgs) == 0)
			{
				return false;
			}
			if ((callConvention & CallingConventions.Standard) != 0 && (method.CallingConvention & CallingConventions.Standard) == 0)
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsGenericParametersCountMatch(MethodInfo method, int expectedGenericParameterCount)
	{
		int num = 0;
		if (method.IsGenericMethod)
		{
			num = method.GetGenericArguments().Length;
		}
		return num == expectedGenericParameterCount;
	}

	internal static bool IsParametersTypeMatch(MethodBase method, Type[] parameterTypes)
	{
		if (parameterTypes == null)
		{
			return true;
		}
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length != parameterTypes.Length)
		{
			return false;
		}
		int num = parameters.Length;
		for (int i = 0; i < num; i++)
		{
			if (!parameters[i].ParameterType.Equals(parameterTypes[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static uint TokenFromRid(uint rid, uint tkType)
	{
		return rid | tkType;
	}

	internal static StringComparison GetStringComparison(BindingFlags flags)
	{
		if ((flags & BindingFlags.IgnoreCase) != BindingFlags.Default)
		{
			return StringComparison.OrdinalIgnoreCase;
		}
		return StringComparison.Ordinal;
	}
}
