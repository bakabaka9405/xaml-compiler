using System.Collections.Generic;

namespace System.Xaml;

public interface IAmbientProvider
{
	AmbientPropertyValue GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties);

	object GetFirstAmbientValue(params XamlType[] types);

	IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties);

	IEnumerable<object> GetAllAmbientValues(params XamlType[] types);

	IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember[] properties);
}
