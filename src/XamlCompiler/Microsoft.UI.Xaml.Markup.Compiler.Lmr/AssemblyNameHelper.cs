using System;
using System.Configuration.Assemblies;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;
using Microsoft.UI.Xaml.Markup.Compiler.Core;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class AssemblyNameHelper
{
	private abstract class AssemblyNameBuilder : IDisposable
	{
		private readonly MetadataFile m_storage;

		protected readonly IMetadataAssemblyImport m_assemblyImport;

		protected EmbeddedBlobPointer m_publicKey;

		protected int m_cbPublicKey;

		protected int m_hashAlgId;

		protected StringBuilder m_szName;

		protected int m_chName;

		protected AssemblyNameFlags m_flags;

		protected AssemblyMetaData m_metadata;

		public AssemblyNameFlags AssemblyNameFlags => m_flags;

		protected AssemblyNameBuilder(MetadataFile storage, IMetadataAssemblyImport assemblyImport)
		{
			m_storage = storage;
			m_assemblyImport = assemblyImport;
		}

		protected abstract void Fetch();

		public AssemblyName CalculateName()
		{
			AssemblyName assemblyName = new AssemblyName();
			m_metadata = default(AssemblyMetaData);
			m_metadata.Init();
			m_szName = null;
			m_chName = 0;
			Fetch();
			m_szName = new StringBuilder();
			m_szName.Capacity = m_chName;
			int countBytes = (int)(m_metadata.cbLocale * 2);
			m_metadata.szLocale = new UnmanagedStringMemoryHandle(countBytes);
			m_metadata.ulProcessor = 0u;
			m_metadata.ulOS = 0u;
			Fetch();
			assemblyName.CultureInfo = m_metadata.Locale;
			byte[] array = m_storage.ReadEmbeddedBlob(m_publicKey, m_cbPublicKey);
			assemblyName.HashAlgorithm = (System.Configuration.Assemblies.AssemblyHashAlgorithm)m_hashAlgId;
			assemblyName.Name = m_szName.ToString();
			assemblyName.Version = m_metadata.Version;
			SetAssemblyNameFlags(assemblyName, m_flags);
			if ((m_flags & AssemblyNameFlags.PublicKey) != AssemblyNameFlags.None)
			{
				assemblyName.SetPublicKey(array);
			}
			else
			{
				assemblyName.SetPublicKeyToken(array);
			}
			return assemblyName;
		}

		private static void SetAssemblyNameFlags(AssemblyName assemblyName, AssemblyNameFlags flags)
		{
			assemblyName.Flags = flags;
			int num = (int)(flags & (AssemblyNameFlags)112) >> 4;
			if (num > 5)
			{
				num = 0;
			}
			assemblyName.ProcessorArchitecture = (ProcessorArchitecture)num;
			int num2 = (int)(flags & (AssemblyNameFlags)3584) >> 9;
			if (num2 > 1)
			{
				num2 = 0;
			}
			assemblyName.ContentType = (AssemblyContentType)num2;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_metadata.szLocale.Dispose();
			}
		}
	}

	private class AssemblyNameFromDefitionBuilder : AssemblyNameBuilder
	{
		private Token assemblyToken;

		public AssemblyNameFromDefitionBuilder(Token assemblyToken, MetadataFile storage, IMetadataAssemblyImport assemblyImport)
			: base(storage, assemblyImport)
		{
			this.assemblyToken = assemblyToken;
		}

		protected override void Fetch()
		{
			m_assemblyImport.GetAssemblyProps(assemblyToken, out m_publicKey, out m_cbPublicKey, out m_hashAlgId, m_szName, m_chName, out m_chName, ref m_metadata, out m_flags);
		}
	}

	private class AssemblyNameFromRefBuilder : AssemblyNameBuilder
	{
		private Token assemblyRefToken;

		public AssemblyNameFromRefBuilder(Token assemblyRefToken, MetadataFile storage, IMetadataAssemblyImport assemblyImport)
			: base(storage, assemblyImport)
		{
			if (assemblyRefToken.TokenType != TokenType.AssemblyRef)
			{
				throw new ArgumentException(Resources.AssemblyRefTokenExpected);
			}
			this.assemblyRefToken = assemblyRefToken;
		}

		protected override void Fetch()
		{
			m_assemblyImport.GetAssemblyRefProps(assemblyRefToken, out m_publicKey, out m_cbPublicKey, m_szName, m_chName, out m_chName, ref m_metadata, out var _, out var _, out m_flags);
		}
	}

	private const int ProcessorArchitectureMask = 240;

	private const int ReferenceAssembly = 112;

	private static InstanceCache<Tuple<Token, MetadataFile, IMetadataAssemblyImport>, AssemblyName> _assemblyNameCache = new InstanceCache<Tuple<Token, MetadataFile, IMetadataAssemblyImport>, AssemblyName>();

	public static AssemblyName GetAssemblyName(MetadataOnlyModule module)
	{
		Token assemblyToken = MetadataOnlyAssembly.GetAssemblyToken(module);
		IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)module.RawImport;
		AssemblyNameFromDefitionBuilder assemblyNameFromDefitionBuilder = new AssemblyNameFromDefitionBuilder(assemblyToken, module.RawMetadata, assemblyImport);
		AssemblyName assemblyName = assemblyNameFromDefitionBuilder.CalculateName();
		assemblyName.CodeBase = MetadataOnlyAssembly.GetCodeBaseFromManifestModule(module);
		if (!HasV1Metadata(module))
		{
			module.GetPEKind(out var peKind, out var machine);
			ProcessorArchitecture processorArchitecture = CalculateProcArchIndex(peKind, machine, assemblyNameFromDefitionBuilder.AssemblyNameFlags);
			assemblyName.ProcessorArchitecture = processorArchitecture;
		}
		else
		{
			assemblyName.ProcessorArchitecture = ProcessorArchitecture.None;
		}
		return assemblyName;
	}

	public static bool HasV1Metadata(MetadataOnlyModule module)
	{
		string runtimeVersion = module.GetRuntimeVersion();
		if (runtimeVersion.Length >= 2 && runtimeVersion[1] == '1')
		{
			return true;
		}
		return false;
	}

	public static AssemblyName GetAssemblyNameFromRef(Token assemblyRefToken, MetadataOnlyModule module, IMetadataAssemblyImport assemblyImport)
	{
		Tuple<Token, MetadataFile, IMetadataAssemblyImport> key = new Tuple<Token, MetadataFile, IMetadataAssemblyImport>(assemblyRefToken, module.RawMetadata, assemblyImport);
		if (!_assemblyNameCache.TryGetValue(key, out var value))
		{
			AssemblyNameFromRefBuilder assemblyNameFromRefBuilder = new AssemblyNameFromRefBuilder(assemblyRefToken, module.RawMetadata, assemblyImport);
			value = assemblyNameFromRefBuilder.CalculateName();
			_assemblyNameCache[key] = value;
		}
		return value;
	}

	private static ProcessorArchitecture CalculateProcArchIndex(PortableExecutableKinds pek, ImageFileMachine ifm, AssemblyNameFlags flags)
	{
		if ((flags & (AssemblyNameFlags)240) == (AssemblyNameFlags)112)
		{
			return ProcessorArchitecture.None;
		}
		if ((pek & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
		{
			switch (ifm)
			{
			case ImageFileMachine.I386:
				if ((pek & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
				{
					return ProcessorArchitecture.MSIL;
				}
				break;
			case ImageFileMachine.IA64:
				return ProcessorArchitecture.IA64;
			case ImageFileMachine.AMD64:
				return ProcessorArchitecture.Amd64;
			}
		}
		else if (ifm == ImageFileMachine.I386)
		{
			if ((pek & PortableExecutableKinds.Required32Bit) != PortableExecutableKinds.Required32Bit && (pek & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
			{
				return ProcessorArchitecture.MSIL;
			}
			return ProcessorArchitecture.X86;
		}
		return ProcessorArchitecture.None;
	}
}
