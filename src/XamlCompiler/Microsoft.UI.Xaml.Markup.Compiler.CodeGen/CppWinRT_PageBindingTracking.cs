using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[GeneratedCode("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
internal class CppWinRT_PageBindingTracking : CppWinRT_CodeGenerator<PageDefinition>
{
	public override string TransformText()
	{
		BindUniverse bindUniverse = base.Arguments[0] as BindUniverse;
		Write("    struct ");
		Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.BindingsTrackingClassName));
		Write(" : public ");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection(base.ProjectInfo.RootNamespace)));
		Write("::implementation::XamlBindingTrackingBase\r\n    {\r\n        ");
		Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.BindingsTrackingClassName));
		Write("()\r\n        {}\r\n\r\n        // Event handlers\r\n");
		IEnumerable<XamlType> enumerable = (from s in bindUniverse.BindPathSteps.Values
			where s.ValueType.ImplementsIObservableVector()
			select s.ValueType.ItemType).Distinct();
		IEnumerable<XamlType> enumerable2 = (from s in bindUniverse.BindPathSteps.Values
			where s.ValueType.ImplementsIObservableMap()
			select s.ValueType.ItemType).Distinct();
		foreach (XamlType item in enumerable)
		{
			Write("\r\n        void VectorChanged_");
			Write(base.ToStringHelper.ToStringWithCulture(item.MemberFriendlyName()));
			Write("(\r\n            ::winrt::Windows::Foundation::Collections::IObservableVector<");
			Write(base.ToStringHelper.ToStringWithCulture(item));
			Write("> const& sender,\r\n            ::winrt::Windows::Foundation::Collections::IVectorChangedEventArgs const& e)\r\n        {\r\n            VectorChanged(sender, e);\r\n        }\r\n");
		}
		foreach (XamlType item2 in enumerable2)
		{
			Write("\r\n        void MapChanged_");
			Write(base.ToStringHelper.ToStringWithCulture(item2.MemberFriendlyName()));
			Write("(\r\n            ::winrt::Windows::Foundation::Collections::IObservableMap<::winrt::hstring, ");
			Write(base.ToStringHelper.ToStringWithCulture(item2));
			Write("> const& sender,\r\n            ::winrt::Windows::Foundation::Collections::IMapChangedEventArgs<::winrt::hstring> const& e)\r\n        {\r\n            MapChanged(sender, e);\r\n        }\r\n");
		}
		Write("\r\n        // Listener update functions\r\n");
		foreach (XamlType item3 in enumerable)
		{
			Write("\r\n        void UpdateVectorChangedListener_");
			Write(base.ToStringHelper.ToStringWithCulture(item3.MemberFriendlyName()));
			Write("(\r\n            ::winrt::Windows::Foundation::Collections::IObservableVector<");
			Write(base.ToStringHelper.ToStringWithCulture(item3));
			Write("> const& obj,\r\n            ::winrt::Windows::Foundation::Collections::IObservableVector<");
			Write(base.ToStringHelper.ToStringWithCulture(item3));
			Write("> & cache,\r\n            ::winrt::event_token & token)\r\n        {\r\n            if (cache && cache != obj)\r\n            {\r\n                cache.VectorChanged(token);\r\n                cache = nullptr;\r\n            }\r\n            if (!cache && obj)\r\n            {\r\n                cache = obj;\r\n                token = obj.VectorChanged({this, &");
			Write(base.ToStringHelper.ToStringWithCulture(GetBindingTrackingClassName(bindUniverse, base.Model.CodeInfo)));
			Write("::VectorChanged_");
			Write(base.ToStringHelper.ToStringWithCulture(item3.MemberFriendlyName()));
			Write("});\r\n            }\r\n        }\r\n");
		}
		foreach (XamlType item4 in enumerable2)
		{
			Write("\r\n        void UpdateMapChangedListener_");
			Write(base.ToStringHelper.ToStringWithCulture(item4.MemberFriendlyName()));
			Write("(\r\n            ::winrt::Windows::Foundation::Collections::IObservableMap<::winrt::hstring, ");
			Write(base.ToStringHelper.ToStringWithCulture(item4));
			Write("> const& obj,\r\n            ::winrt::Windows::Foundation::Collections::IObservableMap<::winrt::hstring, ");
			Write(base.ToStringHelper.ToStringWithCulture(item4));
			Write("> & cache,\r\n            ::winrt::event_token & token)\r\n        {\r\n            if (cache && cache != obj)\r\n            {\r\n                cache.MapChanged(token);\r\n                cache = nullptr;\r\n            }\r\n            if (!cache && obj)\r\n            {\r\n                cache = obj;\r\n                token = obj.MapChanged({this, &");
			Write(base.ToStringHelper.ToStringWithCulture(GetBindingTrackingClassName(bindUniverse, base.Model.CodeInfo)));
			Write("::MapChanged_");
			Write(base.ToStringHelper.ToStringWithCulture(item4.MemberFriendlyName()));
			Write("});\r\n            }\r\n        }\r\n");
		}
		Write("    };\r\n");
		return base.GenerationEnvironment.ToString();
	}
}
