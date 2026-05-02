using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal class ChecksumHelper
{
	private static ChecksumHelper instance;

	internal const int ChecksumLength = 64;

	private HashAlgorithm hashProvider;

	internal static ChecksumHelper Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ChecksumHelper();
			}
			return instance;
		}
	}

	private ChecksumHelper()
	{
		hashProvider = new SHA256CryptoServiceProvider();
	}

	internal string ComputeCheckSumForXamlFile(string fullXamlFilePath)
	{
		byte[] array = null;
		using (FileStream inputStream = File.Open(fullXamlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			array = hashProvider.ComputeHash(inputStream);
		}
		StringBuilder stringBuilder = new StringBuilder(64);
		int num = 0;
		while (stringBuilder.Length < 64 && num < array.Length)
		{
			stringBuilder.Append(array[num].ToString("X2"));
			num++;
		}
		return stringBuilder.ToString();
	}
}
