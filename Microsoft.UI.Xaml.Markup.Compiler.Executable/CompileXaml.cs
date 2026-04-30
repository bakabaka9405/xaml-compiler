using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

namespace Microsoft.UI.Xaml.Markup.Compiler.Executable;

[Description("Generates code for a given set of XAML files.")]
internal sealed class CompileXaml
{
	private ConsoleLogger Log { get; } = new ConsoleLogger();

	public int Run(string[] args)
	{
		Options options = ParseCommandLineParameters(args);
		if (options == null)
		{
			PrintUsage();
			return 0;
		}
		if (!File.Exists(options.JsonInputFile))
		{
			Console.WriteLine("Xaml compiler error: Input JSON file \"" + options.JsonInputFile + "\" doesn't exist!");
			return 1;
		}
		return ExecuteCompilation(options);
	}

	private Options ParseCommandLineParameters(string[] args)
	{
		if (args.Length != 2)
		{
			return null;
		}
		Options options = new Options();
		options.JsonInputFile = args[0];
		options.JsonOutputFile = args[1];
		return options;
	}

	private void PrintUsage()
	{
		Console.WriteLine("Usage: XamlCompiler.exe <input JSON file> <output JSON file>");
		Console.WriteLine("Example: XamlCompiler.exe input.json output.json");
	}

	private int ExecuteCompilation(Options parsedOptions)
	{
		bool flag = false;
		CompilerInputs compilerInputs = DeserializeCompilerInputs(parsedOptions.JsonInputFile);
		CompileXamlInternal compileXamlInternal = CreateCore(compilerInputs);
		try
		{
			compileXamlInternal.SaveState = SavedStateManager.Load(compilerInputs.SavedStateFile);
			flag = compileXamlInternal.DoExecute();
			if (flag)
			{
				compileXamlInternal.SaveStateBeforeFinishing();
				SaveResults(compileXamlInternal, parsedOptions.JsonOutputFile);
			}
		}
		catch (Exception e)
		{
			flag = false;
			compileXamlInternal.LogError_XamlInternalError(e, null);
		}
		return (!flag) ? 1 : 0;
	}

	private CompileXamlInternal CreateCore(CompilerInputs compilerInputs)
	{
		CompileXamlInternal compileXamlInternal = new CompileXamlInternal();
		compileXamlInternal.Log = Log;
		compileXamlInternal.TaskFileService = new BuildTaskFileService(compileXamlInternal.LanguageSourceExtension);
		compileXamlInternal.PopulateFromCompilerInputs(compilerInputs);
		return compileXamlInternal;
	}

	private CompilerInputs DeserializeCompilerInputs(string inputJsonFile)
	{
		using FileStream utf8Json = new FileStream(inputJsonFile, FileMode.Open, FileAccess.Read);
		return JsonSerializer.Deserialize<CompilerInputs>(utf8Json);
	}

	private void SaveResults(CompileXamlInternal core, string outputFile)
	{
		CompilerOutputs compilerOutputs = new CompilerOutputs();
		compilerOutputs.GeneratedCodeFiles = core.GeneratedCodeFiles;
		compilerOutputs.GeneratedXamlFiles = core.GeneratedXamlFiles;
		compilerOutputs.GeneratedXamlPagesFiles = core.GeneratedXamlPagesFiles;
		compilerOutputs.GeneratedXbfFiles = core.GeneratedXbfFiles;
		compilerOutputs.MSBuildLogEntries = Log.Entries;
		using FileStream utf8Json = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
		JsonSerializer.Serialize(utf8Json, compilerOutputs);
	}
}
