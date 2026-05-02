using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[DebuggerDisplay("\\{Name = {Name} FullName = {FullName} {m_typeRef}\\}")]
internal class MetadataOnlyTypeReference : TypeProxy, ITypeReference, ITypeProxy
{
	private Token m_typeRef;

	public Module DeclaringScope => m_resolver;

	public Token TypeRefToken => m_typeRef;

	public Token ResolutionScope
	{
		get
		{
			m_resolver.RawImport.GetTypeRefProps(m_typeRef.Value, out var ptkResolutionScope, null, 0, out var _);
			return new Token(ptkResolutionScope);
		}
	}

	public virtual string RawName
	{
		get
		{
			int value = m_typeRef.Value;
			m_resolver.RawImport.GetTypeRefProps(value, out var ptkResolutionScope, null, 0, out var pchName);
			StringBuilder builder = StringBuilderPool.Get(pchName);
			m_resolver.RawImport.GetTypeRefProps(value, out ptkResolutionScope, builder, builder.Capacity, out pchName);
			string result = builder.ToString();
			StringBuilderPool.Release(ref builder);
			return result;
		}
	}

	public override string Namespace
	{
		get
		{
			if (DeclaringType != null)
			{
				return DeclaringType.Namespace;
			}
			return Utility.GetNamespaceHelper(FullName);
		}
	}

	public override string Name => Utility.GetTypeNameFromFullNameHelper(FullName, base.IsNested);

	public override string FullName
	{
		get
		{
			int value = m_typeRef.Value;
			string empty = string.Empty;
			string text = string.Empty;
			StringBuilder builder;
			while (true)
			{
				m_resolver.RawImport.GetTypeRefProps(value, out var ptkResolutionScope, null, 0, out var pchName);
				builder = StringBuilderPool.Get(pchName);
				Token token = new Token(ptkResolutionScope);
				m_resolver.RawImport.GetTypeRefProps(value, out ptkResolutionScope, builder, builder.Capacity, out pchName);
				if (!token.IsType(TokenType.TypeRef))
				{
					break;
				}
				text = "+" + builder.ToString() + text;
				value = token.Value;
			}
			builder.Append(text);
			empty = builder.ToString();
			StringBuilderPool.Release(ref builder);
			return empty;
		}
	}

	private AssemblyName RequestedAssemblyName
	{
		get
		{
			Token resolutionScope = ResolutionScope;
			switch (resolutionScope.TokenType)
			{
			case TokenType.TypeRef:
			{
				MetadataOnlyTypeReference metadataOnlyTypeReference = new MetadataOnlyTypeReference(m_resolver, resolutionScope);
				return metadataOnlyTypeReference.RequestedAssemblyName;
			}
			case TokenType.AssemblyRef:
				return m_resolver.GetAssemblyNameFromAssemblyRef(resolutionScope);
			case TokenType.Module:
			case TokenType.ModuleRef:
				return m_resolver.Assembly.GetName();
			default:
				throw new InvalidOperationException(Resources.InvalidMetadata);
			}
		}
	}

	public override Assembly Assembly
	{
		get
		{
			AssemblyName requestedAssemblyName = RequestedAssemblyName;
			return new AssemblyRef(requestedAssemblyName, base.TypeUniverse);
		}
	}

	public override string AssemblyQualifiedName
	{
		get
		{
			string assemblyName = RequestedAssemblyName.ToString();
			string fullName = FullName;
			return Assembly.CreateQualifiedName(assemblyName, fullName);
		}
	}

	public override bool IsGenericParameter => false;

	public override Type DeclaringType
	{
		get
		{
			int value = m_typeRef.Value;
			m_resolver.RawImport.GetTypeRefProps(value, out var ptkResolutionScope, null, 0, out var _);
			Token tokenTypeRef = new Token(ptkResolutionScope);
			if (tokenTypeRef.IsType(TokenType.TypeRef))
			{
				return m_resolver.Factory.CreateTypeRef(m_resolver, tokenTypeRef);
			}
			return null;
		}
	}

	public MetadataOnlyTypeReference(MetadataOnlyModule resolver, Token typeRef)
		: base(resolver)
	{
		m_typeRef = typeRef;
	}

	protected override Type GetResolvedTypeWorker()
	{
		return m_resolver.ResolveTypeRef(this);
	}

	protected override bool IsByRefImpl()
	{
		return false;
	}

	protected override bool IsArrayImpl()
	{
		return false;
	}

	protected override bool IsPointerImpl()
	{
		return false;
	}

	protected override bool IsPrimitiveImpl()
	{
		if (!base.TypeUniverse.WouldResolveToAssembly(RequestedAssemblyName, base.TypeUniverse.GetSystemAssembly()))
		{
			return false;
		}
		return GetResolvedType().IsPrimitive;
	}
}
