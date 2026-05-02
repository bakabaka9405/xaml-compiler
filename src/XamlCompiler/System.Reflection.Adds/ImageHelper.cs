using System.Runtime.InteropServices;

namespace System.Reflection.Adds;

internal class ImageHelper
{
	[StructLayout(LayoutKind.Explicit)]
	public class IMAGE_DOS_HEADER
	{
		[FieldOffset(0)]
		public short e_magic;

		[FieldOffset(60)]
		public uint e_lfanew;

		public bool IsValid => e_magic == 23117;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_FILE_HEADER
	{
		public short Machine;

		public short NumberOfSections;

		public uint TimeDateStamp;

		public uint PointerToSymbolTable;

		public uint NumberOfSymbols;

		public short SizeOfOptionalHeader;

		public short Characteristics;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_NT_HEADERS_HELPER
	{
		public uint Signature;

		public IMAGE_FILE_HEADER FileHeader;

		public ushort Magic;

		public bool IsValid => Signature == 17744;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_NT_HEADERS_32
	{
		public uint Signature;

		public IMAGE_FILE_HEADER FileHeader;

		public IMAGE_OPTIONAL_HEADER_32 OptionalHeader;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_NT_HEADERS_64
	{
		public uint Signature;

		public IMAGE_FILE_HEADER FileHeader;

		public IMAGE_OPTIONAL_HEADER_64 OptionalHeader;
	}

	[StructLayout(LayoutKind.Sequential)]
	private class IMAGE_SECTION_HEADER
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
		public string name;

		public uint union;

		public uint VirtualAddress;

		public uint SizeOfRawData;

		public uint PointerToRawData;

		public uint PointerToRelocations;

		public uint PointerToLinenumbers;

		public ushort NumberOfRelocations;

		public ushort NumberOfLinenumbers;

		public uint Characteristics;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_OPTIONAL_HEADER_32
	{
		public ushort Magic;

		public byte MajorLinkerVersion;

		public byte MinorLinkerVersion;

		public uint SizeOfCode;

		public uint SizeOfInitializedData;

		public uint SizeOfUninitializedData;

		public uint AddressOfEntryPoint;

		public uint BaseOfCode;

		public uint BaseOfData;

		public uint ImageBase;

		public uint SectionAlignment;

		public uint FileAlignment;

		public ushort MajorOperatingSystemVersion;

		public ushort MinorOperatingSystemVersion;

		public ushort MajorImageVersion;

		public ushort MinorImageVersion;

		public ushort MajorSubsystemVersion;

		public ushort MinorSubsystemVersion;

		public uint Win32VersionValue;

		public uint SizeOfImage;

		public uint SizeOfHeaders;

		public uint CheckSum;

		public ushort Subsystem;

		public ushort DllCharacteristics;

		public uint SizeOfStackReserve;

		public uint SizeOfStackCommit;

		public uint SizeOfHeapReserve;

		public uint SizeOfHeapCommit;

		public uint LoaderFlags;

		public uint NumberOfRvaAndSizes;

		public IMAGE_DATA_DIRECTORY ExportTable;

		public IMAGE_DATA_DIRECTORY ImportTable;

		public IMAGE_DATA_DIRECTORY ResourceTable;

		public IMAGE_DATA_DIRECTORY ExceptionTable;

		public IMAGE_DATA_DIRECTORY CertificateTable;

		public IMAGE_DATA_DIRECTORY BaseRelocationTable;

		public IMAGE_DATA_DIRECTORY DebugData;

		public IMAGE_DATA_DIRECTORY ArchitectureData;

		public IMAGE_DATA_DIRECTORY GlobalPointer;

		public IMAGE_DATA_DIRECTORY TlsTable;

		public IMAGE_DATA_DIRECTORY LoadConfigTable;

		public IMAGE_DATA_DIRECTORY BoundImportTable;

		public IMAGE_DATA_DIRECTORY ImportAddressTable;

		public IMAGE_DATA_DIRECTORY DelayImportTable;

		public IMAGE_DATA_DIRECTORY ClrHeaderTable;

		public IMAGE_DATA_DIRECTORY Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_OPTIONAL_HEADER_64
	{
		public ushort Magic;

		public byte MajorLinkerVersion;

		public byte MinorLinkerVersion;

		public uint SizeOfCode;

		public uint SizeOfInitializedData;

		public uint SizeOfUninitializedData;

		public uint AddressOfEntryPoint;

		public uint BaseOfCode;

		public ulong ImageBase;

		public uint SectionAlignment;

		public uint FileAlignment;

		public ushort MajorOperatingSystemVersion;

		public ushort MinorOperatingSystemVersion;

		public ushort MajorImageVersion;

		public ushort MinorImageVersion;

		public ushort MajorSubsystemVersion;

		public ushort MinorSubsystemVersion;

		public uint Win32VersionValue;

		public uint SizeOfImage;

		public uint SizeOfHeaders;

		public uint CheckSum;

		public ushort Subsystem;

		public ushort DllCharacteristics;

		public ulong SizeOfStackReserve;

		public ulong SizeOfStackCommit;

		public ulong SizeOfHeapReserve;

		public ulong SizeOfHeapCommit;

		public uint LoaderFlags;

		public uint NumberOfRvaAndSizes;

		public IMAGE_DATA_DIRECTORY ExportTable;

		public IMAGE_DATA_DIRECTORY ImportTable;

		public IMAGE_DATA_DIRECTORY ResourceTable;

		public IMAGE_DATA_DIRECTORY ExceptionTable;

		public IMAGE_DATA_DIRECTORY CertificateTable;

		public IMAGE_DATA_DIRECTORY BaseRelocationTable;

		public IMAGE_DATA_DIRECTORY DebugData;

		public IMAGE_DATA_DIRECTORY ArchitectureData;

		public IMAGE_DATA_DIRECTORY GlobalPointer;

		public IMAGE_DATA_DIRECTORY TlsTable;

		public IMAGE_DATA_DIRECTORY LoadConfigTable;

		public IMAGE_DATA_DIRECTORY BoundImportTable;

		public IMAGE_DATA_DIRECTORY ImportAddressTable;

		public IMAGE_DATA_DIRECTORY DelayImportTable;

		public IMAGE_DATA_DIRECTORY ClrHeaderTable;

		public IMAGE_DATA_DIRECTORY Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_DATA_DIRECTORY
	{
		public uint VirtualAddress;

		public uint Size;
	}

	internal enum CorHdrNumericDefines : uint
	{
		COMIMAGE_FLAGS_ILONLY = 1u,
		COMIMAGE_FLAGS_32BITREQUIRED = 2u,
		COMIMAGE_FLAGS_IL_LIBRARY = 4u,
		COMIMAGE_FLAGS_STRONGNAMESIGNED = 8u,
		COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 0x10u,
		COMIMAGE_FLAGS_TRACKDEBUGDATA = 0x10000u,
		COMIMAGE_FLAGS_ISIBCOPTIMIZED = 0x20000u
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IMAGE_COR20_HEADER
	{
		public uint cb;

		public ushort MajorRuntimeVersion;

		public ushort MinorRuntimeVersion;

		public IMAGE_DATA_DIRECTORY MetaData;

		public CorHdrNumericDefines Flags;

		public uint EntryPoint;

		public IMAGE_DATA_DIRECTORY Resources;

		public IMAGE_DATA_DIRECTORY StrongNameSignature;

		public IMAGE_DATA_DIRECTORY CodeManagerTable;

		public IMAGE_DATA_DIRECTORY VTableFixups;

		public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;

		public IMAGE_DATA_DIRECTORY ManagedNativeHeader;
	}

	private readonly IntPtr m_baseAddress;

	private readonly long m_lengthBytes;

	private readonly uint m_idx;

	private readonly uint m_idxSectionStart;

	private readonly uint m_numSections;

	private readonly uint m_clrHeaderRva;

	public ImageType ImageType { get; private set; }

	public ImageHelper(IntPtr baseAddress, long lengthBytes)
	{
		m_baseAddress = baseAddress;
		m_lengthBytes = lengthBytes;
		IMAGE_DOS_HEADER iMAGE_DOS_HEADER = MarshalAt<IMAGE_DOS_HEADER>(0u);
		if (!iMAGE_DOS_HEADER.IsValid)
		{
			throw new ArgumentException(Resources.InvalidFileFormat);
		}
		m_idx = iMAGE_DOS_HEADER.e_lfanew;
		IMAGE_NT_HEADERS_HELPER iMAGE_NT_HEADERS_HELPER = MarshalAt<IMAGE_NT_HEADERS_HELPER>(m_idx);
		if (!iMAGE_NT_HEADERS_HELPER.IsValid)
		{
			throw new ArgumentException(Resources.InvalidFileFormat);
		}
		if (iMAGE_NT_HEADERS_HELPER.Magic == 267)
		{
			ImageType = ImageType.Pe32bit;
			IMAGE_NT_HEADERS_32 iMAGE_NT_HEADERS_ = MarshalAt<IMAGE_NT_HEADERS_32>(m_idx);
			m_idxSectionStart = m_idx + (uint)Marshal.SizeOf(typeof(IMAGE_NT_HEADERS_32));
			m_numSections = (uint)iMAGE_NT_HEADERS_.FileHeader.NumberOfSections;
			m_clrHeaderRva = iMAGE_NT_HEADERS_.OptionalHeader.ClrHeaderTable.VirtualAddress;
			return;
		}
		if (iMAGE_NT_HEADERS_HELPER.Magic == 523)
		{
			ImageType = ImageType.Pe64bit;
			IMAGE_NT_HEADERS_64 iMAGE_NT_HEADERS_2 = MarshalAt<IMAGE_NT_HEADERS_64>(m_idx);
			m_idxSectionStart = m_idx + (uint)Marshal.SizeOf(typeof(IMAGE_NT_HEADERS_64));
			m_numSections = (uint)iMAGE_NT_HEADERS_2.FileHeader.NumberOfSections;
			m_clrHeaderRva = iMAGE_NT_HEADERS_2.OptionalHeader.ClrHeaderTable.VirtualAddress;
			return;
		}
		throw new ArgumentException(Resources.UnsupportedImageType);
	}

	public IntPtr GetResourcesSectionStart()
	{
		IMAGE_COR20_HEADER cor20Header = GetCor20Header();
		uint virtualAddress = cor20Header.Resources.VirtualAddress;
		return ResolveRva(virtualAddress);
	}

	internal IMAGE_COR20_HEADER GetCor20Header()
	{
		IntPtr ptr = ResolveRva(m_clrHeaderRva);
		return (IMAGE_COR20_HEADER)Marshal.PtrToStructure(ptr, typeof(IMAGE_COR20_HEADER));
	}

	public Token GetEntryPointToken()
	{
		IMAGE_COR20_HEADER cor20Header = GetCor20Header();
		if ((cor20Header.Flags & CorHdrNumericDefines.COMIMAGE_FLAGS_NATIVE_ENTRYPOINT) != 0)
		{
			return Token.Nil;
		}
		uint entryPoint = cor20Header.EntryPoint;
		return new Token(entryPoint);
	}

	public IntPtr ResolveRva(long rva)
	{
		uint num = m_idxSectionStart;
		for (int i = 0; i < m_numSections; i++)
		{
			IMAGE_SECTION_HEADER iMAGE_SECTION_HEADER = MarshalAt<IMAGE_SECTION_HEADER>(num);
			if (rva >= iMAGE_SECTION_HEADER.VirtualAddress && rva < iMAGE_SECTION_HEADER.VirtualAddress + iMAGE_SECTION_HEADER.SizeOfRawData)
			{
				long value = m_baseAddress.ToInt64() + rva - iMAGE_SECTION_HEADER.VirtualAddress + iMAGE_SECTION_HEADER.PointerToRawData;
				return new IntPtr(value);
			}
			num += (uint)Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER));
		}
		return IntPtr.Zero;
	}

	internal T MarshalAt<T>(uint offset)
	{
		long num = m_baseAddress.ToInt64();
		long num2 = num + m_lengthBytes;
		int num3 = Marshal.SizeOf(typeof(T));
		if (offset + num3 > num2)
		{
			throw new InvalidOperationException(Resources.CorruptImage);
		}
		IntPtr ptr = new IntPtr(num + offset);
		return (T)Marshal.PtrToStructure(ptr, typeof(T));
	}
}
