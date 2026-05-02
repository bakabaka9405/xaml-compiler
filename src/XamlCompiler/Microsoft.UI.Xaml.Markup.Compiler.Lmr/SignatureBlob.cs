using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class SignatureBlob
{
	private readonly byte[] m_signature;

	private SignatureBlob(byte[] data)
	{
		m_signature = data;
	}

	public static SignatureBlob ReadSignature(MetadataFile storage, EmbeddedBlobPointer pointer, int countBytes)
	{
		return new SignatureBlob(storage.ReadEmbeddedBlob(pointer, countBytes));
	}

	public byte[] GetSignatureAsByteArray()
	{
		return m_signature;
	}
}
