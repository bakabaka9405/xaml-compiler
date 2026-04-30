using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyEventInfo : EventInfo
{
	private MetadataOnlyModule m_resolver;

	private int m_eventToken;

	private EventAttributes m_attrib;

	private int m_declaringClassToken;

	private int m_eventHandlerTypeToken;

	private GenericContext m_context;

	private string m_name;

	private int m_nameLength;

	private Token m_addMethodToken;

	private Token m_removeMethodToken;

	private Token m_raiseMethodToken;

	public override EventAttributes Attributes => m_attrib;

	public override MemberTypes MemberType => MemberTypes.Event;

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

	public override Type EventHandlerType => m_resolver.GetGenericType(new Token(m_eventHandlerTypeToken), m_context);

	public override Type DeclaringType => m_resolver.GetGenericType(new Token(m_declaringClassToken), m_context);

	public override int MetadataToken => m_eventToken;

	public override Module Module => m_resolver;

	public MetadataOnlyEventInfo(MetadataOnlyModule resolver, Token eventToken, Type[] typeArgs, Type[] methodArgs)
	{
		m_resolver = resolver;
		m_eventToken = eventToken;
		m_context = new GenericContext(typeArgs, methodArgs);
		IMetadataImport rawImport = m_resolver.RawImport;
		rawImport.GetEventProps(m_eventToken, out m_declaringClassToken, null, 0, out m_nameLength, out var pdwEventFlags, out m_eventHandlerTypeToken, out var pmdAddOn, out var pmdRemoveOn, out var pmdFire, out var _, 1u, out var _);
		m_attrib = (EventAttributes)pdwEventFlags;
		m_addMethodToken = new Token(pmdAddOn);
		m_removeMethodToken = new Token(pmdRemoveOn);
		m_raiseMethodToken = new Token(pmdFire);
	}

	public override string ToString()
	{
		return DeclaringType.ToString() + "." + Name;
	}

	private void InitializeName()
	{
		if (string.IsNullOrEmpty(m_name))
		{
			IMetadataImport rawImport = m_resolver.RawImport;
			StringBuilder builder = StringBuilderPool.Get(m_nameLength);
			rawImport.GetEventProps(m_eventToken, out m_declaringClassToken, builder, builder.Capacity, out var _, out var _, out m_eventHandlerTypeToken, out var _, out var _, out var _, out var _, 1u, out var _);
			m_name = builder.ToString();
			StringBuilderPool.Release(ref builder);
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

	public override MethodInfo GetAddMethod(bool nonPublic)
	{
		if (m_addMethodToken.IsNil)
		{
			return null;
		}
		MethodInfo genericMethodInfo = m_resolver.GetGenericMethodInfo(m_addMethodToken, m_context);
		if (nonPublic || genericMethodInfo.IsPublic)
		{
			return genericMethodInfo;
		}
		return null;
	}

	public override MethodInfo GetRemoveMethod(bool nonPublic)
	{
		if (m_removeMethodToken.IsNil)
		{
			return null;
		}
		MethodInfo genericMethodInfo = m_resolver.GetGenericMethodInfo(m_removeMethodToken, m_context);
		if (nonPublic || genericMethodInfo.IsPublic)
		{
			return genericMethodInfo;
		}
		return null;
	}

	public override MethodInfo GetRaiseMethod(bool nonPublic)
	{
		if (m_raiseMethodToken.IsNil)
		{
			return null;
		}
		MethodInfo genericMethodInfo = m_resolver.GetGenericMethodInfo(m_raiseMethodToken, m_context);
		if (nonPublic || genericMethodInfo.IsPublic)
		{
			return genericMethodInfo;
		}
		return null;
	}

	public override bool Equals(object obj)
	{
		MetadataOnlyEventInfo metadataOnlyEventInfo = obj as MetadataOnlyEventInfo;
		if (metadataOnlyEventInfo != null)
		{
			if (metadataOnlyEventInfo.m_resolver.Equals(m_resolver) && metadataOnlyEventInfo.m_eventToken.Equals(m_eventToken))
			{
				return DeclaringType.Equals(metadataOnlyEventInfo.DeclaringType);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_resolver.GetHashCode() * 32767 + m_eventToken.GetHashCode();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return m_resolver.GetCustomAttributeData(MetadataToken);
	}
}
