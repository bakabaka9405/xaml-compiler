using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyMethodBodyWorker : MetadataOnlyMethodBody
{
	private class ExceptionHandlingClauseWorker : ExceptionHandlingClause
	{
		private readonly MethodInfo m_method;

		private readonly IEHClause m_data;

		public override Type CatchType
		{
			get
			{
				Token classToken = m_data.ClassToken;
				Module module = m_method.Module;
				return module.ResolveType(classToken, m_method.DeclaringType.GetGenericArguments(), m_method.GetGenericArguments());
			}
		}

		public override int FilterOffset => m_data.FilterOffset;

		public override ExceptionHandlingClauseOptions Flags => m_data.Flags;

		public override int HandlerLength => m_data.HandlerLength;

		public override int HandlerOffset => m_data.HandlerOffset;

		public override int TryLength => m_data.TryLength;

		public override int TryOffset => m_data.TryOffset;

		public ExceptionHandlingClauseWorker(MethodInfo method, IEHClause data)
		{
			m_method = method;
			m_data = data;
		}
	}

	internal interface IMethodHeader
	{
		int MaxStack { get; }

		int CodeSize { get; }

		Token LocalVarSigTok { get; }

		MethodHeaderFlags Flags { get; }

		int HeaderSizeBytes { get; }
	}

	[Flags]
	internal enum MethodHeaderFlags
	{
		FatFormat = 3,
		TinyFormat = 2,
		MoreSects = 8,
		InitLocals = 0x10
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class TinyHeader : IMethodHeader
	{
		private readonly byte FlagsAndSize;

		public MethodHeaderFlags Flags => (MethodHeaderFlags)(FlagsAndSize & 3);

		public int CodeSize => (FlagsAndSize >> 2) & 0x3F;

		public int MaxStack => 8;

		public Token LocalVarSigTok => Token.Nil;

		public int HeaderSizeBytes => 1;

		public TinyHeader()
		{
		}

		public TinyHeader(byte data)
		{
			FlagsAndSize = data;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class FatHeader : IMethodHeader
	{
		private readonly short m_FlagsAndSize;

		private readonly short m_MaxStack;

		private readonly uint m_CodeSize;

		private readonly uint m_LocalVarSigTok;

		public MethodHeaderFlags Flags => (MethodHeaderFlags)(m_FlagsAndSize & 0xFFF);

		public int MaxStack => m_MaxStack;

		public int CodeSize => (int)m_CodeSize;

		public Token LocalVarSigTok => new Token(m_LocalVarSigTok);

		public int HeaderSizeBytes
		{
			get
			{
				int num = (m_FlagsAndSize >> 12) & 0xF;
				return num * 4;
			}
		}
	}

	[Flags]
	private enum CorILMethod_Sect
	{
		EHTable = 1,
		OptILTable = 2,
		FatFormat = 0x40,
		MoreSects = 0x80
	}

	private interface IEHClause
	{
		ExceptionHandlingClauseOptions Flags { get; }

		int TryOffset { get; }

		int TryLength { get; }

		int HandlerOffset { get; }

		int HandlerLength { get; }

		Token ClassToken { get; }

		int FilterOffset { get; }
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class EHSmall : IEHClause
	{
		private readonly ushort m_Flags;

		private readonly ushort m_TryOffset;

		private readonly byte m_TryLength;

		private readonly byte m_HandlerOffset1;

		private readonly byte m_HandlerOffset2;

		private readonly byte m_HandlerLength;

		private readonly uint m_ClassToken;

		private readonly int m_FilterOffset;

		ExceptionHandlingClauseOptions IEHClause.Flags => (ExceptionHandlingClauseOptions)m_Flags;

		int IEHClause.TryOffset => m_TryOffset;

		int IEHClause.TryLength => m_TryLength;

		int IEHClause.HandlerOffset => m_HandlerOffset2 * 256 + m_HandlerOffset1;

		int IEHClause.HandlerLength => m_HandlerLength;

		Token IEHClause.ClassToken => new Token(m_ClassToken);

		int IEHClause.FilterOffset => m_FilterOffset;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class EHFat : IEHClause
	{
		private readonly uint m_Flags;

		private readonly int m_TryOffset;

		private readonly int m_TryLength;

		private readonly int m_HandlerOffset;

		private readonly int m_HandlerLength;

		private readonly uint m_ClassToken;

		private readonly int m_FilterOffset;

		ExceptionHandlingClauseOptions IEHClause.Flags => (ExceptionHandlingClauseOptions)m_Flags;

		int IEHClause.TryOffset => m_TryOffset;

		int IEHClause.TryLength => m_TryLength;

		int IEHClause.HandlerOffset => m_HandlerOffset;

		int IEHClause.HandlerLength => m_HandlerLength;

		Token IEHClause.ClassToken => new Token(m_ClassToken);

		int IEHClause.FilterOffset => m_FilterOffset;
	}

	private static readonly byte[] s_EmptyByteArray = new byte[0];

	private readonly IMethodHeader m_header;

	public override IList<ExceptionHandlingClause> ExceptionHandlingClauses
	{
		get
		{
			if ((m_header.Flags & MethodHeaderFlags.MoreSects) == 0)
			{
				return new ExceptionHandlingClause[0];
			}
			MetadataOnlyModule resolver = base.Method.Resolver;
			uint methodRva = resolver.GetMethodRva(base.Method.MetadataToken);
			long num = methodRva + m_header.HeaderSizeBytes + m_header.CodeSize;
			num = ((num - 1) & -4) + 4;
			byte b = resolver.RawMetadata.ReadRvaStruct<byte>(num);
			CorILMethod_Sect corILMethod_Sect = (CorILMethod_Sect)b;
			if ((corILMethod_Sect & ~(CorILMethod_Sect.EHTable | CorILMethod_Sect.FatFormat)) != 0)
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Resources.UnsupportedExceptionFlags, corILMethod_Sect.ToString()));
			}
			bool flag = (corILMethod_Sect & CorILMethod_Sect.FatFormat) != 0;
			int num2;
			int num3;
			if (flag)
			{
				byte[] array = resolver.RawMetadata.ReadRva(num + 1, 3);
				num2 = array[0] + array[1] * 256 + array[2] * 65536;
				num3 = 24;
			}
			else
			{
				num2 = resolver.RawMetadata.ReadRvaStruct<byte>(num + 1);
				num3 = 12;
			}
			int num4 = (num2 - 4) / num3;
			ExceptionHandlingClause[] array2 = new ExceptionHandlingClauseWorker[num4];
			ExceptionHandlingClause[] array3 = array2;
			long num5 = num + 4;
			for (int i = 0; i < num4; i++)
			{
				IEHClause iEHClause2;
				if (!flag)
				{
					IEHClause iEHClause = resolver.RawMetadata.ReadRvaStruct<EHSmall>(num5);
					iEHClause2 = iEHClause;
				}
				else
				{
					IEHClause iEHClause = resolver.RawMetadata.ReadRvaStruct<EHFat>(num5);
					iEHClause2 = iEHClause;
				}
				IEHClause data = iEHClause2;
				num5 += num3;
				array3[i] = new ExceptionHandlingClauseWorker(base.Method, data);
			}
			return Array.AsReadOnly(array3);
		}
	}

	public override int MaxStackSize => m_header.MaxStack;

	public override bool InitLocals => (m_header.Flags & MethodHeaderFlags.InitLocals) != 0;

	public override int LocalSignatureMetadataToken => m_header.LocalVarSigTok.Value;

	public MetadataOnlyMethodBodyWorker(MetadataOnlyMethodInfo method, IMethodHeader header)
		: base(method)
	{
		m_header = header;
	}

	internal static MethodBody Create(MetadataOnlyMethodInfo method)
	{
		MetadataOnlyModule resolver = method.Resolver;
		uint methodRva = resolver.GetMethodRva(method.MetadataToken);
		if (methodRva == 0)
		{
			return null;
		}
		IMethodHeader methodHeader = GetMethodHeader(methodRva, resolver);
		return new MetadataOnlyMethodBodyWorker(method, methodHeader);
	}

	public static IMethodHeader GetMethodHeader(uint rva, MetadataOnlyModule scope)
	{
		byte[] array = scope.RawMetadata.ReadRva(rva, 1);
		return (MethodHeaderFlags)(array[0] & 3) switch
		{
			MethodHeaderFlags.FatFormat => scope.RawMetadata.ReadRvaStruct<FatHeader>(rva), 
			MethodHeaderFlags.TinyFormat => new TinyHeader(array[0]), 
			_ => throw new InvalidOperationException(Resources.InvalidMetadata), 
		};
	}

	public override byte[] GetILAsByteArray()
	{
		if (m_header.CodeSize == 0)
		{
			return s_EmptyByteArray;
		}
		MetadataOnlyModule resolver = base.Method.Resolver;
		uint methodRva = resolver.GetMethodRva(base.Method.MetadataToken);
		return resolver.RawMetadata.ReadRva(methodRva + m_header.HeaderSizeBytes, m_header.CodeSize);
	}
}
