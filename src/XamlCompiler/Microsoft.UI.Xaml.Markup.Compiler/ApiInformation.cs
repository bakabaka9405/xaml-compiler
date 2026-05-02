using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class ApiInformation
{
	private static IReadOnlyDictionary<string, ApiInformationMethod> SupportedApiInformation = new Dictionary<string, ApiInformationMethod>
	{
		{
			"IsApiContractPresent",
			new ApiInformationMethod("IsApiContractPresent", condition: true)
		},
		{
			"IsApiContractNotPresent",
			new ApiInformationMethod("IsApiContractPresent", condition: false)
		},
		{
			"IsPropertyPresent",
			new ApiInformationMethod("IsPropertyPresent", condition: true)
		},
		{
			"IsPropertyNotPresent",
			new ApiInformationMethod("IsPropertyPresent", condition: false)
		},
		{
			"IsTypePresent",
			new ApiInformationMethod("IsTypePresent", condition: true)
		},
		{
			"IsTypeNotPresent",
			new ApiInformationMethod("IsTypePresent", condition: false)
		}
	};

	private static IReadOnlyDictionary<string, List<ApiInformationParameter>> SupportedApiInformationParameters = new Dictionary<string, List<ApiInformationParameter>>
	{
		{
			"IsApiContractPresent2",
			new List<ApiInformationParameter>
			{
				new ApiInformationParameter(typeof(string)),
				new ApiInformationParameter(typeof(ushort))
			}
		},
		{
			"IsApiContractPresent3",
			new List<ApiInformationParameter>
			{
				new ApiInformationParameter(typeof(string)),
				new ApiInformationParameter(typeof(ushort)),
				new ApiInformationParameter(typeof(ushort))
			}
		},
		{
			"IsTypePresent1",
			new List<ApiInformationParameter>
			{
				new ApiInformationParameter(typeof(string))
			}
		},
		{
			"IsPropertyPresent2",
			new List<ApiInformationParameter>
			{
				new ApiInformationParameter(typeof(string)),
				new ApiInformationParameter(typeof(string))
			}
		}
	};

	public ApiInformationMethod Method { get; }

	public IEnumerable<ApiInformationParameter> Parameters { get; private set; }

	public string MemberFriendlyName => UniqueName.GetMemberFriendlyName();

	public string UniqueName => string.Format("{0}_{1}", Method.UniqueName, string.Join("_", Parameters.Select((ApiInformationParameter p) => p.UniqueName)));

	public ApiInformation(string methodName)
	{
		if (!SupportedApiInformation.ContainsKey(methodName))
		{
			throw new ArgumentException("methodName");
		}
		Method = SupportedApiInformation[methodName];
	}

	internal void SetParameters(List<ApiInformationParameter> parameters)
	{
		if (!SupportedApiInformationParameters.ContainsKey(Method.MethodName + parameters.Count))
		{
			throw new ArgumentException("parameters");
		}
		List<ApiInformationParameter> list = SupportedApiInformationParameters[Method.MethodName + parameters.Count];
		if (list.Count != parameters.Count)
		{
			throw new ArgumentException("parameters");
		}
		for (int i = 0; i < parameters.Count; i++)
		{
			parameters[i].ParameterType = list[i].ParameterType;
		}
		Parameters = parameters;
	}

	public override bool Equals(object obj)
	{
		if (obj is ApiInformation apiInformation)
		{
			return apiInformation.UniqueName == UniqueName;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return UniqueName.GetHashCode();
	}
}
