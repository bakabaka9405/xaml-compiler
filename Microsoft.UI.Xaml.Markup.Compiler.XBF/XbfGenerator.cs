using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.UI.Xaml.Markup.Compiler.Core;
using Microsoft.UI.Xaml.Markup.Compiler.FileIO;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

internal class XbfGenerator
{
	private static Dictionary<Version, IntPtr> s_GenXbfDllHandles = new InstanceCache<Version, IntPtr>(ClearCache);

	private List<IXbfFileNameInfo> xamlFiles = new List<IXbfFileNameInfo>();

	private IXbfMetadataProvider xbfMetadataProvider;

	private XamlProjectInfo projectInfo;

	private Lazy<List<XamlCompileWarning>> warningMessages = new Lazy<List<XamlCompileWarning>>(() => new List<XamlCompileWarning>());

	private Lazy<List<XamlCompileError>> errorMessages = new Lazy<List<XamlCompileError>>(() => new List<XamlCompileError>());

	public List<XamlCompileWarning> XbfWarnings => warningMessages.Value;

	public List<XamlCompileError> XbfErrors => errorMessages.Value;

	public XbfGenerator(XamlProjectInfo projectInfo, IXbfMetadataProvider xbfMetadataProvider)
	{
		this.projectInfo = projectInfo;
		this.xbfMetadataProvider = xbfMetadataProvider;
	}

	public void SetXamlInputFilesFromTaskItems(IEnumerable<TaskItemFilename> tifs, bool isSdk = false)
	{
		foreach (TaskItemFilename tif in tifs)
		{
			xamlFiles.Add(new XbfFileNameInfo(tif.SourceXamlFullPath, tif.XamlGivenPath, isSdk ? tif.XamlGivenPath : tif.XamlOutputFilename, tif.XbfOutputFilename));
		}
	}

	public void SetXamlInputFiles(IEnumerable<IXbfFileNameInfo> xamlFiles)
	{
		this.xamlFiles.AddRange(xamlFiles);
	}

	public bool GenerateXbfFiles(uint xbfGenerationFlags, bool v80Compat = false)
	{
		IntPtr value = IntPtr.Zero;
		string empty = string.Empty;
		empty = RuntimeInformation.ProcessArchitecture switch
		{
			Architecture.Arm64 => projectInfo.GenXbfArm64Path, 
			Architecture.X64 => projectInfo.GenXbf64Path, 
			_ => projectInfo.GenXbf32Path, 
		};
		if (string.IsNullOrWhiteSpace(projectInfo.GenXbf32Path))
		{
			XbfErrors.Add(new XbfGeneration_NoWindowsSdk());
			return false;
		}
		if (!s_GenXbfDllHandles.TryGetValue(projectInfo.TargetPlatformMinVersion, out value))
		{
			value = NativeMethods.LoadLibraryEx(empty, IntPtr.Zero, NativeMethods.LOAD_WITH_ALTERED_SEARCH_PATH);
			s_GenXbfDllHandles.Add(projectInfo.TargetPlatformMinVersion, value);
		}
		if (value == IntPtr.Zero)
		{
			XbfErrors.Add(new XbfGeneration_CouldNotLoadXbfGenerator(empty));
			return false;
		}
		foreach (IXbfFileNameInfo xamlFile in xamlFiles)
		{
			if (!xamlFile.GivenXamlName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
			{
				XbfErrors.Add(new XamlFileMustEndInDotXaml(xamlFile.GivenXamlName));
				return false;
			}
		}
		bool setAirityOnGenericTypeNames = XamlSchemaCodeInfo.SetAirityOnGenericTypeNames;
		XamlSchemaCodeInfo.SetAirityOnGenericTypeNames = !v80Compat;
		bool result = GenerateAll(value, xbfGenerationFlags);
		XamlSchemaCodeInfo.SetAirityOnGenericTypeNames = setAirityOnGenericTypeNames;
		return result;
	}

	internal virtual List<IStream> GetInputOutputStreams()
	{
		List<IStream> list = new List<IStream>();
		foreach (XbfFileNameInfo xamlFile in xamlFiles)
		{
			string inputXamlName = xamlFile.InputXamlName;
			try
			{
				list.Add(new StreamXamlInput(inputXamlName));
			}
			catch (Exception ex)
			{
				XbfErrors.Add(new XbfInputFileOpenFailure(inputXamlName, ex.Message));
				return null;
			}
			string outputXbfName = xamlFile.OutputXbfName;
			try
			{
				string directoryName = Path.GetDirectoryName(outputXbfName);
				Directory.CreateDirectory(directoryName);
				list.Add(new StreamXbfOutput(outputXbfName));
			}
			catch (Exception ex2)
			{
				XbfErrors.Add(new XbfOutputFileOpenFailure(outputXbfName, ex2.Message));
				return null;
			}
		}
		return list;
	}

	internal virtual bool GenerateXbfFromStreams(IntPtr dllHandle, IStream[] inputStreams, IStream[] outputStreams, uint xbfGenerationFlags, string[] checksums, TargetOSVersion targetOS, out int errorCode, out int errorFile, out int errorLine, out int errorPosition)
	{
		errorCode = (errorFile = (errorLine = (errorPosition = 0)));
		try
		{
			int num = NativeMethodsHelper.Write(dllHandle, inputStreams, inputStreams.Count(), checksums, 64, xbfMetadataProvider, targetOS, xbfGenerationFlags, outputStreams, out errorCode, out errorFile, out errorLine, out errorPosition);
		}
		catch (Exception ex)
		{
			XbfErrors.Add(new XbfGenerationGeneralFailure(ex.Message));
			return false;
		}
		return true;
	}

	internal virtual string GetTextLine(string fileName, int line)
	{
		using TextReader textReader = File.OpenText(fileName);
		string result = string.Empty;
		for (int i = 0; i < line; i++)
		{
			result = textReader.ReadLine();
		}
		return result;
	}

	private string[] GetAllXamlFilesChecksums()
	{
		IList<string> list = new List<string>();
		foreach (IXbfFileNameInfo xamlFile in xamlFiles)
		{
			list.Add(xamlFile.XamlFileChecksum);
		}
		return list.ToArray();
	}

	private bool GenerateAll(IntPtr dllHandle, uint xbfGenerationFlags)
	{
		List<IStream> inputOutputStreams = GetInputOutputStreams();
		if (inputOutputStreams == null || inputOutputStreams.Count == 0)
		{
			return false;
		}
		try
		{
			IEnumerable<IStream> source = inputOutputStreams.Where((IStream s) => s is IXamlStream && ((IXamlStream)s).StreamType == StreamType.Input);
			IEnumerable<IStream> source2 = inputOutputStreams.Where((IStream s) => s is IXamlStream && ((IXamlStream)s).StreamType == StreamType.Output);
			if (!GenerateXbfFromStreams(dllHandle, source.ToArray(), source2.ToArray(), xbfGenerationFlags, GetAllXamlFilesChecksums(), projectInfo.TargetPlatformMinVersion.ToTargetOSVersion(), out var errorCode, out var errorFile, out var errorLine, out var errorPosition))
			{
				return false;
			}
			if (errorCode != 0)
			{
				LogXbfError(errorCode, errorFile, errorLine, errorPosition);
				return false;
			}
		}
		finally
		{
			foreach (StreamImpl item in inputOutputStreams.OfType<StreamImpl>())
			{
				item.Dispose();
			}
		}
		return true;
	}

	private void LogXbfError(int errorCode, int errorFile, int errorLine, int errorPosition)
	{
		string givenXamlName = xamlFiles[errorFile].GivenXamlName;
		if (IsMarkupExtensionAssignmentError(givenXamlName, errorLine, errorPosition, out var additional))
		{
			XbfErrors.Add(new XbfGeneration_NonMeInCurlyBraces(givenXamlName, errorLine, errorPosition, additional, errorCode));
		}
		else if (errorCode == 2500)
		{
			XbfErrors.Add(new XbfGenerationPropertyNotFoundError(givenXamlName, errorLine, errorPosition));
		}
		else
		{
			XbfErrors.Add(new XbfGenerationParseError(givenXamlName, errorLine, errorPosition, errorCode));
		}
	}

	private bool IsMarkupExtensionAssignmentError(string fileName, int line, int pos, out string additional)
	{
		string textLine = GetTextLine(fileName, line);
		if (LookslikeMarkupExtensionAssigment(textLine, pos, out var meName))
		{
			additional = meName;
			return true;
		}
		additional = string.Empty;
		return false;
	}

	private bool LookslikeMarkupExtensionAssigment(string textLine, int pos, out string meName)
	{
		if (pos < textLine.Length)
		{
			int num = textLine.IndexOf('\'', pos);
			int num2 = textLine.IndexOf('"', pos);
			int num3 = ((num == -1) ? num2 : ((num2 == -1) ? num : ((num < num2) ? num : num2)));
			if (num3 != -1 && num3 < textLine.Length - 3 && textLine[num3 + 1] == '{')
			{
				meName = ScanIdentifierToken(textLine, num3 + 2);
				if (!string.IsNullOrWhiteSpace(meName))
				{
					return true;
				}
			}
		}
		meName = string.Empty;
		return false;
	}

	private string ScanIdentifierToken(string textLine, int start)
	{
		string text = textLine.Substring(start);
		XamlDomValidator.IsValidIdentifierName(text, out var idx);
		return text.Substring(0, idx);
	}

	private static void ClearCache()
	{
		foreach (IntPtr value in s_GenXbfDllHandles.Values)
		{
			NativeMethods.FreeLibrary(value);
		}
		s_GenXbfDllHandles.Clear();
	}
}
