using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;
using Microsoft.UI.Xaml.Markup.Compiler.Tracing;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class FingerPrinter
{
	private static bool s_VcMetaIsLoaded;

	private string[] _ignorePathsList;

	private string _localAssemblyPath;

	private string[] _nonSystemReferenceAssemblies;

	private readonly bool _useVcMetaManaged = true;

	public string LocalAssemblyPath => _localAssemblyPath;

	public string[] ReferenceAssemblyPaths => _nonSystemReferenceAssemblies;

	public string[] IgnorePathsList
	{
		get
		{
			return _ignorePathsList;
		}
		private set
		{
			List<string> list = new List<string>();
			if (value != null)
			{
				foreach (string text in value)
				{
					string item = text.ToLowerInvariant();
					list.Add(item);
				}
			}
			_ignorePathsList = list.ToArray();
		}
	}

	public FingerPrinter(IAssemblyItem localAssembly, IEnumerable<IAssemblyItem> referenceAssemblies, string[] ignorePaths, string vcInstallDir, string vcInstallPath32, string vcInstallPath64, bool useVCMetaManaged)
	{
		IgnorePathsList = ignorePaths;
		SetLocalAssembly(localAssembly);
		SetReferenceAssemblies(referenceAssemblies, IgnorePathsList);
		_useVcMetaManaged = useVCMetaManaged;
		if (!_useVcMetaManaged)
		{
			s_VcMetaIsLoaded = NativeMethodsHelper.EnsureVcMetaIsLoaded(vcInstallDir, vcInstallPath32, vcInstallPath64);
		}
	}

	public void SetLocalAssembly(IAssemblyItem localAssembly)
	{
		_localAssemblyPath = null;
		if (localAssembly != null)
		{
			_localAssemblyPath = ToLowerFullFilePath(localAssembly.ItemSpec);
		}
	}

	public void SetReferenceAssemblies(IEnumerable<IAssemblyItem> referenceAssemblies, string[] ignoreList)
	{
		List<string> list = new List<string>();
		foreach (IAssemblyItem referenceAssembly in referenceAssemblies)
		{
			string assemblyPath = ToLowerFullFilePath(referenceAssembly.ItemSpec);
			if (!(_localAssemblyPath == assemblyPath) && !referenceAssembly.IsNuGetReference && !ignoreList.Any((string x) => assemblyPath.StartsWith(x)))
			{
				list.Add(assemblyPath);
			}
		}
		_nonSystemReferenceAssemblies = list.ToArray();
	}

	public bool HasAssemblyFileListChanged(HashSet<string> asmFileNames)
	{
		bool flag = false;
		if (asmFileNames != null)
		{
			if (asmFileNames.Count != ReferenceAssemblyPaths.Length)
			{
				PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "Number of assemblies changed");
				flag = true;
			}
			else
			{
				string[] referenceAssemblyPaths = ReferenceAssemblyPaths;
				foreach (string text in referenceAssemblyPaths)
				{
					if (!asmFileNames.Contains(text))
					{
						PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "New2: " + text);
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			asmFileNames.Clear();
			string[] referenceAssemblyPaths2 = ReferenceAssemblyPaths;
			foreach (string item in referenceAssemblyPaths2)
			{
				asmFileNames.Add(item);
			}
		}
		return flag;
	}

	public bool HasLocalAssemblyHashChanged(Dictionary<string, Guid> dictionaryOfGuidHashs)
	{
		if (LocalAssemblyPath == null)
		{
			return false;
		}
		if (!_useVcMetaManaged && !s_VcMetaIsLoaded)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "vcmeta.dll is not loaded");
			return true;
		}
		bool result = false;
		try
		{
			if (HasAssemblyChanged(LocalAssemblyPath, dictionaryOfGuidHashs))
			{
				result = true;
			}
		}
		catch (Exception ex)
		{
			result = true;
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "HashForWinMD threw an Exception: " + ex);
		}
		return result;
	}

	public bool HaveReferenceAssembliesHashesChanged(Dictionary<string, Guid> dictionaryOfGuidHashs)
	{
		if (ReferenceAssemblyPaths == null || ReferenceAssemblyPaths.Length == 0)
		{
			return false;
		}
		if (!_useVcMetaManaged && !s_VcMetaIsLoaded)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "vcmeta.dll is not loaded");
			return true;
		}
		bool result = false;
		try
		{
			string[] referenceAssemblyPaths = ReferenceAssemblyPaths;
			foreach (string asmPath in referenceAssemblyPaths)
			{
				if (HasAssemblyChanged(asmPath, dictionaryOfGuidHashs))
				{
					result = true;
				}
			}
		}
		catch (Exception)
		{
			result = true;
		}
		return result;
	}

	private string ToLowerFullFilePath(string filename)
	{
		return Path.GetFullPath(filename).ToLowerInvariant();
	}

	private bool HasAssemblyChanged(string asmPath, Dictionary<string, Guid> dictionaryOfGuidHashs)
	{
		int num = 0;
		Guid hash;
		if (_useVcMetaManaged)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "Using managed HashForWinMD");
			hash = VCMetaManaged.HashForWinMD(asmPath);
		}
		else
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "Using HashForWinMD from vcmeta.dll");
			num = NativeMethods.HashForWinMD(asmPath, out hash);
		}
		if (num != 0 || hash == Guid.Empty)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "API failure");
			return true;
		}
		if (dictionaryOfGuidHashs.TryGetValue(asmPath, out var value))
		{
			if (value == hash)
			{
				return false;
			}
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "Differ: " + asmPath);
			dictionaryOfGuidHashs[asmPath] = hash;
		}
		else
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "New1: " + asmPath);
			dictionaryOfGuidHashs.Add(asmPath, hash);
		}
		return true;
	}
}
