using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection.Adds;

[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
internal class MetadataFile : IDisposable
{
	private IntPtr m_rawPointer;

	public IntPtr RawPtr
	{
		get
		{
			EnsureNotDispose();
			return m_rawPointer;
		}
	}

	public virtual string FilePath => null;

	public MetadataFile(object importer)
	{
		if (importer == null)
		{
			throw new ArgumentNullException("importer");
		}
		m_rawPointer = Marshal.GetIUnknownForObject(importer);
	}

	internal MetadataFile(IntPtr rawImporter)
	{
		if (rawImporter == IntPtr.Zero)
		{
			throw new ArgumentNullException("rawImporter");
		}
		Marshal.AddRef(rawImporter);
		m_rawPointer = rawImporter;
	}

	~MetadataFile()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_rawPointer != IntPtr.Zero)
		{
			Marshal.Release(m_rawPointer);
		}
		m_rawPointer = IntPtr.Zero;
	}

	public virtual byte[] ReadRva(long rva, int countBytes)
	{
		throw new NotSupportedException(Resources.RVAUnsupported);
	}

	public virtual byte[] ReadResource(long offset)
	{
		throw new NotSupportedException(Resources.RVAUnsupported);
	}

	public byte[] ReadEmbeddedBlob(EmbeddedBlobPointer pointer, int countBytes)
	{
		EnsureNotDispose();
		if (countBytes == 0)
		{
			return new byte[0];
		}
		IntPtr getDangerousLivePointer = pointer.GetDangerousLivePointer;
		ValidateRange(getDangerousLivePointer, countBytes);
		byte[] array = new byte[countBytes];
		Marshal.Copy(getDangerousLivePointer, array, 0, countBytes);
		return array;
	}

	protected virtual void ValidateRange(IntPtr ptr, int countBytes)
	{
	}

	public virtual Token ReadEntryPointToken()
	{
		throw new NotSupportedException(Resources.RVAUnsupported);
	}

	public virtual T ReadRvaStruct<T>(long rva) where T : new()
	{
		EnsureNotDispose();
		int countBytes = Marshal.SizeOf(typeof(T));
		byte[] value = ReadRva(rva, countBytes);
		GCHandle gCHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
		try
		{
			IntPtr ptr = gCHandle.AddrOfPinnedObject();
			return (T)Marshal.PtrToStructure(ptr, typeof(T));
		}
		finally
		{
			gCHandle.Free();
		}
	}

	protected void EnsureNotDispose()
	{
		if (m_rawPointer == IntPtr.Zero)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
	}
}
