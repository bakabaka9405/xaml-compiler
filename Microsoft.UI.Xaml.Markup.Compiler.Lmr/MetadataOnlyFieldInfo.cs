using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyFieldInfo : FieldInfo, IFieldInfo2
{
	private readonly MetadataOnlyModule m_resolver;

	private readonly int m_fieldDefToken;

	private readonly FieldAttributes m_attrib;

	private readonly int m_declaringClassToken;

	private Type m_fieldType;

	private GenericContext m_context;

	private string m_name;

	private int m_nameLength;

	private CustomModifiers m_customModifiers;

	private bool m_initialized;

	public override FieldAttributes Attributes => m_attrib;

	public override MemberTypes MemberType => MemberTypes.Field;

	public override string Name
	{
		get
		{
			InitializeName();
			return m_name;
		}
	}

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override Type FieldType
	{
		get
		{
			Initialize();
			return m_fieldType;
		}
	}

	public override Type DeclaringType
	{
		get
		{
			Initialize();
			return m_resolver.GetGenericType(new Token(m_declaringClassToken), m_context);
		}
	}

	public override RuntimeFieldHandle FieldHandle
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override int MetadataToken => m_fieldDefToken;

	public override Module Module => m_resolver;

	public MetadataOnlyFieldInfo(MetadataOnlyModule resolver, Token fieldDefToken, Type[] typeArgs, Type[] methodArgs)
	{
		m_resolver = resolver;
		m_fieldDefToken = fieldDefToken;
		if (typeArgs != null || methodArgs != null)
		{
			m_context = new GenericContext(typeArgs, methodArgs);
		}
		IMetadataImport rawImport = m_resolver.RawImport;
		rawImport.GetFieldProps(m_fieldDefToken, out m_declaringClassToken, null, 0, out m_nameLength, out var pdwAttr, out var _, out var _, out var _, out var _, out var _);
		m_attrib = pdwAttr;
	}

	private void InitializeName()
	{
		if (string.IsNullOrEmpty(m_name))
		{
			IMetadataImport rawImport = m_resolver.RawImport;
			StringBuilder builder = StringBuilderPool.Get(m_nameLength);
			rawImport.GetFieldProps(m_fieldDefToken, out var _, builder, builder.Capacity, out var _, out var _, out var _, out var _, out var _, out var _, out var _);
			m_name = builder.ToString();
			StringBuilderPool.Release(ref builder);
		}
	}

	private void Initialize()
	{
		if (m_initialized)
		{
			return;
		}
		IMetadataImport rawImport = m_resolver.RawImport;
		rawImport.GetFieldProps(m_fieldDefToken, out var _, null, 0, out var _, out var _, out var ppvSigBlob, out var pcbSigBlob, out var _, out var _, out var _);
		byte[] sig = m_resolver.ReadEmbeddedBlob(ppvSigBlob, pcbSigBlob);
		int index = 0;
		CorCallingConvention corCallingConvention = SignatureUtil.ExtractCallingConvention(sig, ref index);
		m_customModifiers = SignatureUtil.ExtractCustomModifiers(sig, ref index, m_resolver, m_context);
		if (m_resolver.RawImport.IsValidToken((uint)m_declaringClassToken))
		{
			Type type = m_resolver.ResolveType(m_declaringClassToken);
			if (type.IsGenericType && (m_context == null || m_context.TypeArgs == null || m_context.TypeArgs.Length == 0))
			{
				if (m_context == null)
				{
					m_context = new GenericContext(type.GetGenericArguments(), null);
				}
				else
				{
					m_context = new GenericContext(type.GetGenericArguments(), m_context.MethodArgs);
				}
			}
		}
		m_fieldType = SignatureUtil.ExtractType(sig, ref index, m_resolver, m_context);
		m_initialized = true;
	}

	public override string ToString()
	{
		return MetadataOnlyCommonType.TypeSigToString(FieldType) + " " + Name;
	}

	private object ParseDefaultValue()
	{
		Initialize();
		IMetadataImport rawImport = m_resolver.RawImport;
		rawImport.GetFieldProps(m_fieldDefToken, out var _, null, 0, out var _, out var _, out var ppvSigBlob, out var pcbSigBlob, out var pdwCPlusTypeFlab, out var ppValue, out var pcchValue);
		if (ppValue == IntPtr.Zero)
		{
			throw new InvalidOperationException();
		}
		byte[] sig = m_resolver.ReadEmbeddedBlob(ppvSigBlob, pcbSigBlob);
		int index = 0;
		CorCallingConvention corCallingConvention = SignatureUtil.ExtractCallingConvention(sig, ref index);
		CorElementType corElementType = SignatureUtil.ExtractElementType(sig, ref index);
		switch (corElementType)
		{
		case CorElementType.ValueType:
			SignatureUtil.ExtractToken(sig, ref index);
			corElementType = (CorElementType)pdwCPlusTypeFlab;
			break;
		case CorElementType.GenericInstantiation:
		{
			Type type = SignatureUtil.ExtractType(sig, ref index, m_resolver, m_context);
			corElementType = (CorElementType)pdwCPlusTypeFlab;
			break;
		}
		}
		switch (corElementType)
		{
		case CorElementType.Bool:
			if (Marshal.ReadByte(ppValue) == 0)
			{
				return false;
			}
			return true;
		case CorElementType.Char:
			return (char)Marshal.ReadInt16(ppValue);
		case CorElementType.SByte:
			return (sbyte)Marshal.ReadByte(ppValue);
		case CorElementType.Byte:
			return Marshal.ReadByte(ppValue);
		case CorElementType.Short:
			return Marshal.ReadInt16(ppValue);
		case CorElementType.UShort:
			return (ushort)Marshal.ReadInt16(ppValue);
		case CorElementType.Int:
			return Marshal.ReadInt32(ppValue);
		case CorElementType.UInt:
			return (uint)Marshal.ReadInt32(ppValue);
		case CorElementType.Long:
			return Marshal.ReadInt64(ppValue);
		case CorElementType.ULong:
			return (ulong)Marshal.ReadInt64(ppValue);
		case CorElementType.IntPtr:
			return Marshal.ReadIntPtr(ppValue);
		case CorElementType.String:
			return Marshal.PtrToStringAuto(ppValue, pcchValue);
		case CorElementType.Class:
			return null;
		case CorElementType.Float:
		{
			float[] array2 = new float[1];
			Marshal.Copy(ppValue, array2, 0, 1);
			return array2[0];
		}
		case CorElementType.Double:
		{
			double[] array = new double[1];
			Marshal.Copy(ppValue, array, 0, 1);
			return array[0];
		}
		default:
			throw new InvalidOperationException(Resources.IncorrectElementTypeValue);
		}
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		throw new NotSupportedException();
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		throw new NotSupportedException();
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		throw new NotSupportedException();
	}

	public override Type[] GetOptionalCustomModifiers()
	{
		Initialize();
		if (m_customModifiers == null)
		{
			return Type.EmptyTypes;
		}
		return m_customModifiers.OptionalCustomModifiers;
	}

	public override Type[] GetRequiredCustomModifiers()
	{
		Initialize();
		if (m_customModifiers == null)
		{
			return Type.EmptyTypes;
		}
		return m_customModifiers.RequiredCustomModifiers;
	}

	public override object GetValue(object obj)
	{
		throw new NotSupportedException();
	}

	public virtual byte[] GetRvaField()
	{
		if ((Attributes & FieldAttributes.HasFieldRVA) == 0)
		{
			throw new InvalidOperationException(Resources.OperationValidOnRVAFieldsOnly);
		}
		StructLayoutAttribute structLayoutAttribute = FieldType.StructLayoutAttribute;
		if (structLayoutAttribute.Value == LayoutKind.Auto)
		{
			throw new InvalidOperationException(Resources.OperationInvalidOnAutoLayoutFields);
		}
		m_resolver.RawImport.GetRVA(MetadataToken, out var rva, out var _);
		int num = structLayoutAttribute.Size;
		if (num == 0)
		{
			switch (Type.GetTypeCode(FieldType))
			{
			case TypeCode.Int32:
				num = 4;
				break;
			case TypeCode.Int64:
				num = 8;
				break;
			}
		}
		return m_resolver.RawMetadata.ReadRva(rva, num);
	}

	public override object GetRawConstantValue()
	{
		if (!base.IsLiteral)
		{
			throw new InvalidOperationException(Resources.OperationValidOnLiteralFieldsOnly);
		}
		return ParseDefaultValue();
	}

	public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override bool Equals(object obj)
	{
		MetadataOnlyFieldInfo metadataOnlyFieldInfo = obj as MetadataOnlyFieldInfo;
		if (metadataOnlyFieldInfo != null)
		{
			if (metadataOnlyFieldInfo.m_resolver.Equals(m_resolver) && metadataOnlyFieldInfo.m_fieldDefToken.Equals(m_fieldDefToken))
			{
				return DeclaringType.Equals(metadataOnlyFieldInfo.DeclaringType);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_resolver.GetHashCode() * 32767 + m_fieldDefToken.GetHashCode();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return m_resolver.GetCustomAttributeData(MetadataToken);
	}
}
