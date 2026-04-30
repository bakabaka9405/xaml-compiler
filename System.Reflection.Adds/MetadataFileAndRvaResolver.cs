using System.Runtime.InteropServices;

namespace System.Reflection.Adds;

internal class MetadataFileAndRvaResolver : MetadataFile
{
	private FileMapping m_file;

	private bool m_disableRangeValidation;

	public override string FilePath => m_file.Path;

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		m_file.Dispose();
	}

	public MetadataFileAndRvaResolver(IntPtr importer, FileMapping file)
		: base(importer)
	{
		m_file = file;
	}

	public MetadataFileAndRvaResolver(IntPtr importer, FileMapping file, bool disableRangeValidation)
		: base(importer)
	{
		m_file = file;
		m_disableRangeValidation = disableRangeValidation;
	}

	public MetadataFileAndRvaResolver(object importer, FileMapping file)
		: base(importer)
	{
		m_file = file;
	}

	private IntPtr ResolveRva(long rva)
	{
		ImageHelper imageHelper = new ImageHelper(m_file.BaseAddress, m_file.Length);
		IntPtr intPtr = imageHelper.ResolveRva(rva);
		if (intPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException(Resources.CannotResolveRVA);
		}
		return intPtr;
	}

	public override byte[] ReadRva(long rva, int countBytes)
	{
		IntPtr source = ResolveRva(rva);
		byte[] array = new byte[countBytes];
		Marshal.Copy(source, array, 0, array.Length);
		return array;
	}

	protected override void ValidateRange(IntPtr ptr, int countBytes)
	{
		if (!m_disableRangeValidation)
		{
			long num = m_file.BaseAddress.ToInt64();
			long num2 = num + m_file.Length;
			long num3 = ptr.ToInt64();
			if (num3 < num || num3 + countBytes >= num2)
			{
				throw new InvalidOperationException();
			}
		}
	}

	public override byte[] ReadResource(long offset)
	{
		ImageHelper imageHelper = new ImageHelper(m_file.BaseAddress, m_file.Length);
		IntPtr ptr = new IntPtr(imageHelper.GetResourcesSectionStart().ToInt64() + offset);
		uint num = (uint)Marshal.ReadInt32(ptr);
		ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(num));
		byte[] array = new byte[num];
		Marshal.Copy(ptr, array, 0, array.Length);
		return array;
	}

	public override Token ReadEntryPointToken()
	{
		ImageHelper imageHelper = new ImageHelper(m_file.BaseAddress, m_file.Length);
		return imageHelper.GetEntryPointToken();
	}

	public override T ReadRvaStruct<T>(long rva)
	{
		IntPtr ptr = ResolveRva(rva);
		return (T)Marshal.PtrToStructure(ptr, typeof(T));
	}
}
