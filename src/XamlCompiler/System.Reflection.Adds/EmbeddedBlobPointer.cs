namespace System.Reflection.Adds;

#pragma warning disable CS0649 // Field is assigned via interop/reflection
internal struct EmbeddedBlobPointer
{
	private IntPtr m_data;
#pragma warning restore CS0649

	internal IntPtr GetDangerousLivePointer => m_data;
}
