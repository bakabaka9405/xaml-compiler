using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.UI.Xaml.Markup.Compiler.FileIO;

internal class StreamImpl : IStream, IDisposable
{
	private bool disposedValue;

	protected Stream _underlyingStream;

	public void Close()
	{
		_underlyingStream.Close();
	}

	public void Read(byte[] pv, int cb, IntPtr pcbRead)
	{
		if (!_underlyingStream.CanRead)
		{
			throw new InvalidOperationException();
		}
		int val = _underlyingStream.Read(pv, 0, cb);
		Marshal.WriteInt32(pcbRead, val);
	}

	public void Write(byte[] pv, int cb, IntPtr pcbWritten)
	{
		if (!_underlyingStream.CanWrite)
		{
			throw new InvalidOperationException();
		}
		_underlyingStream.Write(pv, 0, cb);
		Marshal.WriteInt32(pcbWritten, cb);
	}

	public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
	{
		if (!_underlyingStream.CanSeek)
		{
			throw new InvalidOperationException();
		}
		SeekOrigin origin = dwOrigin switch
		{
			0 => SeekOrigin.Begin, 
			1 => SeekOrigin.Current, 
			2 => SeekOrigin.End, 
			_ => throw new ArgumentException("dwOrigin"), 
		};
		long val = _underlyingStream.Seek(dlibMove, origin);
		Marshal.WriteInt64(plibNewPosition, val);
	}

	public void SetSize(long libNewSize)
	{
		_underlyingStream.SetLength(libNewSize);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
	{
		throw new NotImplementedException();
	}

	public void Clone(out IStream ppstm)
	{
		throw new NotImplementedException();
	}

	public void Commit(int grfCommitFlags)
	{
		throw new NotImplementedException();
	}

	public void LockRegion(long libOffset, long cb, int dwLockType)
	{
		throw new NotImplementedException();
	}

	public void Revert()
	{
		throw new NotImplementedException();
	}

	public void UnlockRegion(long libOffset, long cb, int dwLockType)
	{
		throw new NotImplementedException();
	}

	public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
	{
		throw new NotImplementedException();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_underlyingStream.Dispose();
				_underlyingStream = null;
			}
			disposedValue = true;
		}
	}
}
