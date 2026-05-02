using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[GeneratedCode("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
internal class CppWinRT_PageBinding : CppWinRT_CodeGenerator<PageDefinition>
{
	public override string TransformText()
	{
		BindUniverse bindUniverse = base.Arguments[0] as BindUniverse;
		Write("    template <typename D, typename ... I>\r\n    struct ");
		Write(base.ToStringHelper.ToStringWithCulture(base.Model.CodeInfo.ClassName.ShortName));
		Write("T<D, I...>::");
		Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.BindingsClassName));
		Write("\r\n        : public ");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection(base.ProjectInfo.RootNamespace)));
		Write("::implementation::");
		Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.DataRootType.NeedsBoxUnbox() ? "ValueTypeXamlBindings" : "ReferenceTypeXamlBindings"));
		Write("<");
		Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.DataRootType));
		Write(", ");
		Write(base.ToStringHelper.ToStringWithCulture(GetBindingTrackingClassName(bindUniverse, base.Model.CodeInfo)));
		Write(">\r\n        , public std::enable_shared_from_this<");
		Write(base.ToStringHelper.ToStringWithCulture(base.Model.CodeInfo.ClassName.ShortName));
		Write("T<D, I...>::");
		Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.BindingsClassName));
		Write(">\r\n");
		if (bindUniverse.NeedsBindingsTracking)
		{
			Write("        , public ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection(base.ProjectInfo.RootNamespace)));
			Write("::implementation::IXamlBindingTracking\r\n");
		}
		Write("{\r\n        ");
		Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.BindingsClassName));
		Write("()\r\n        {\r\n");
		if (bindUniverse.NeedsBindingsTracking)
		{
			Write("            InitializeTracking(this);\r\n");
		}
		Write("        }\r\n\r\n");
		if (bindUniverse.NeedsBindingsTracking)
		{
			Write("        ~");
			Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.BindingsClassName));
			Write("()\r\n        {\r\n            ReleaseAllListeners();\r\n        }\r\n\r\n");
		}
		Write("        void Connect(int32_t connectionId, IInspectable const& target) override\r\n        {\r\n            switch(connectionId)\r\n            {\r\n");
		foreach (ConnectionIdElement item in bindUniverse.ElementsWithConnectCase)
		{
			Write("            case ");
			Write(base.ToStringHelper.ToStringWithCulture(item.ConnectionId));
			Write(": // ");
			Write(base.ToStringHelper.ToStringWithCulture(item.LineNumberAndXamlFile));
			Write("\r\n                {\r\n");
			if (item.Type.IsDerivedFromControlTemplate())
			{
				Write("                    Update(); // Template children have been connected, initialize bindings\r\n");
			}
			else
			{
				Output_ConnectionId_Case(item);
			}
			Write("                }\r\n                break;\r\n");
		}
		Write("            }\r\n        }\r\n\r\n        void DisconnectUnloadedObject(int connectionId) override\r\n        {\r\n");
		if (bindUniverse.ElementsWithDisconnectCase.Count() == 0)
		{
			Write("            throw ::winrt::hresult_invalid_argument { L\"No unloadable elements to disconnect.\" };\r\n");
		}
		else
		{
			Write("            switch (connectionId)\r\n            {\r\n");
			foreach (ConnectionIdElement item2 in bindUniverse.ElementsWithDisconnectCase)
			{
				Write("                case ");
				Write(base.ToStringHelper.ToStringWithCulture(item2.ConnectionId));
				Write(": // ");
				Write(base.ToStringHelper.ToStringWithCulture(item2.LineNumberAndXamlFile));
				Write("\r\n                    {\r\n");
				if (bindUniverse.BoundElements.Contains(item2))
				{
					Output_ApiInformationCall_Push(item2.ApiInformation, Indent.OneTab);
					Write("                        if (");
					Write(base.ToStringHelper.ToStringWithCulture(item2.ObjectCodeName));
					Write(")\r\n                        {\r\n");
					foreach (BindAssignment item3 in item2.BindAssignments.Where((BindAssignment ba) => ba.HasDeferredValueProxy))
					{
						Write("                            ");
						Write(base.ToStringHelper.ToStringWithCulture(item3.ObjectDeferredAssignmentCodeName));
						Write(" = ");
						Write(base.ToStringHelper.ToStringWithCulture(item2.GetMemberGetExpression(item3)));
						Write(";\r\n");
					}
					Write("                            ");
					Write(base.ToStringHelper.ToStringWithCulture(item2.ObjectCodeName));
					Write(" = nullptr;\r\n                        }\r\n");
					foreach (ConnectionIdElement item4 in item2.Children.Where((ConnectionIdElement c) => bindUniverse.ElementsWithDisconnectCase.Contains(c)))
					{
						Write("                        DisconnectUnloadedObject(");
						Write(base.ToStringHelper.ToStringWithCulture(item4.ConnectionId));
						Write(");\r\n");
					}
					Output_ApiInformationCall_Pop(item2.ApiInformation, Indent.OneTab);
				}
				if (item2.HasRootNamedElementStep)
				{
					Write("                    this->UnloadableBindingSourcesToUpdate.push_back([this]()\r\n                    {\r\n");
					PushIndent(Indent.TwoTabs);
					Output_Custom_Update_Call(item2.RootNamedElementStep, "nullptr", "NOT_PHASED");
					PopIndent();
					Write("                            });\r\n");
				}
				Write("                }\r\n                break;\r\n");
			}
			Write("                default:\r\n                    throw hresult_invalid_argument(L\"Invalid connectionId.\");\r\n            }\r\n");
		}
		Write("        }\r\n");
		if (bindUniverse.ElementsWithBoundLoadAssignments.Any())
		{
			Write("\r\n        void UpdateUnloadedElement(int connectionId)\r\n        {\r\n            switch (connectionId)\r\n            {\r\n");
			foreach (ConnectionIdElement elementsWithBoundLoadAssignment in bindUniverse.ElementsWithBoundLoadAssignments)
			{
				Write("            case ");
				Write(base.ToStringHelper.ToStringWithCulture(elementsWithBoundLoadAssignment.ConnectionId));
				Write(": // ");
				Write(base.ToStringHelper.ToStringWithCulture(elementsWithBoundLoadAssignment.LineNumberAndXamlFile));
				Write("\r\n                {\r\n");
				foreach (BoundLoadAssignment item5 in elementsWithBoundLoadAssignment.BindAssignments.OfType<BoundLoadAssignment>())
				{
					PushIndent(Indent.TwoTabs);
					Output_Binding_SetValue_Non_Function_Call(item5, item5.ObjectDeferredAssignmentCodeName.CppWinRTName(), includeDeferredSet: false);
					PopIndent();
				}
				Write("                    }\r\n                    break;\r\n");
			}
			Write("            default:\r\n                throw hresult_invalid_argument(L\"Invalid connectionId.\");\r\n            }\r\n        }\r\n");
		}
		if (bindUniverse.NeedsIDataTemplateExtension)
		{
			Write("\r\n        void Recycle() override\r\n        {\r\n");
			if (bindUniverse.NeedsBindingsTracking)
			{
				Write("            ReleaseAllListeners();\r\n");
			}
			foreach (KeyValuePair<int, List<PhaseAssignment>> item6 in from kvp in bindUniverse.PhaseAssignments
				where kvp.Key != 0
				orderby kvp.Key
				select kvp)
			{
				foreach (PhaseAssignment item7 in item6.Value)
				{
					if (item7.ConnectionIdElement.CanBeInstantiatedLater)
					{
						Write("            if (");
						Write(base.ToStringHelper.ToStringWithCulture(item7.ConnectionIdElement.ReferenceExpression));
						Write(")\r\n            {\r\n");
						PushIndent();
					}
					Write("            ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Markup")));
					Write("::XamlBindingHelper::SuspendRendering(");
					Write(base.ToStringHelper.ToStringWithCulture(item7.ConnectionIdElement.ReferenceExpression));
					Write(");\r\n");
					if (item7.ConnectionIdElement.CanBeInstantiatedLater)
					{
						PopIndent();
						Write("            }\r\n");
					}
				}
			}
			Write("        }\r\n\r\n        void ProcessBindings(IInspectable const& item, int itemIndex, int phase, int32_t& nextPhase)\r\n        {\r\n            nextPhase = -1;\r\n            switch (phase)\r\n            {\r\n            case 0:\r\n                {\r\n                    nextPhase = ");
			Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.GetNextPhase(0)));
			Write(";\r\n                    SetDataRoot(item);\r\n                    if (_dataContextChangedToken.value != 0)\r\n                    {\r\n                        auto rootElement = ");
			Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.RootElement.ReferenceExpression));
			Write(";\r\n                        if (rootElement != nullptr)\r\n                        {\r\n                            rootElement.DataContextChanged(_dataContextChangedToken);\r\n                        }\r\n                        _dataContextChangedToken.value = 0;\r\n                    }\r\n                    _isInitialized = true;\r\n                 }\r\n                 break;\r\n");
			foreach (KeyValuePair<int, List<PhaseAssignment>> item8 in from kvp in bindUniverse.PhaseAssignments
				where kvp.Key != 0
				orderby kvp.Key
				select kvp)
			{
				Write("            case ");
				Write(base.ToStringHelper.ToStringWithCulture(item8.Key));
				Write(":\r\n                {\r\n");
				foreach (PhaseAssignment item9 in item8.Value)
				{
					if (item9.ConnectionIdElement.CanBeInstantiatedLater)
					{
						Write("                    if (");
						Write(base.ToStringHelper.ToStringWithCulture(item9.ConnectionIdElement.ReferenceExpression));
						Write(")\r\n                    {\r\n");
						PushIndent();
					}
					Write("                    ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Markup")));
					Write("::XamlBindingHelper::ResumeRendering(");
					Write(base.ToStringHelper.ToStringWithCulture(item9.ConnectionIdElement.ReferenceExpression));
					Write(");\r\n");
					if (item9.ConnectionIdElement.CanBeInstantiatedLater)
					{
						PopIndent();
						Write("                    }\r\n");
					}
				}
				Write("                    nextPhase = ");
				Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.GetNextPhase(item8.Key)));
				Write(";\r\n                }\r\n                break;\r\n");
			}
			Write("            }\r\n            Update_(::winrt::unbox_value<");
			Write(base.ToStringHelper.ToStringWithCulture(bindUniverse.RootStep.ValueType));
			Write(">(item) , 1 << phase);\r\n        }\r\n");
		}
		else if (bindUniverse.NeedsIDataTemplateComponent)
		{
			Write("        void Recycle() override\r\n        {\r\n            return;\r\n        }\r\n\r\n        void ProcessBindings(IInspectable const& item, int itemIndex, int phase, int32_t& nextPhase)\r\n        {\r\n            nextPhase = 1;\r\n        }\r\n");
		}
		Write("\r\n");
		if (bindUniverse.DistinctConvertersUsed.Count() > 0)
		{
			if (base.Model.CodeInfo.IsResourceDictionary)
			{
				Write("        void SetConverterLookupRoot(");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml")));
				Write("::ResourceDictionary const& resources)\r\n        {\r\n            localResources = resources;\r\n        }\r\n");
			}
			else
			{
				Write("        void SetConverterLookupRoot(");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml")));
				Write("::FrameworkElement const& rootElement)\r\n        {\r\n            converterLookupRoot = rootElement;\r\n        }\r\n");
			}
			Write("\r\n        ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Data")));
			Write("::IValueConverter LookupConverter(::winrt::hstring const& key)\r\n        {\r\n");
			if (!base.Model.CodeInfo.IsResourceDictionary)
			{
				Write("            if (!localResources)\r\n            {\r\n                localResources = converterLookupRoot.get().Resources();\r\n                converterLookupRoot = nullptr;\r\n            }\r\n");
			}
			Write("            auto boxedKey = ::winrt::box_value(key);\r\n            return (localResources.HasKey(boxedKey) ? localResources.Lookup(boxedKey) : ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml")));
			Write("::Application::Current().Resources().Lookup(boxedKey))\r\n                .as<");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Data")));
			Write("::IValueConverter>();\r\n        }\r\n");
		}
		Write("\r\n    private:\r\n");
		if (bindUniverse.BoundElements != null && bindUniverse.BoundElements.Any())
		{
			Write("        // Fields for each control that has bindings.\r\n");
		}
		foreach (ConnectionIdElement boundElement in bindUniverse.BoundElements)
		{
			if (boundElement.IsWeakRef)
			{
				Write("        ::winrt::weak_ref<");
				Write(base.ToStringHelper.ToStringWithCulture(boundElement.Type));
				Write("> ");
				Write(base.ToStringHelper.ToStringWithCulture(boundElement.ObjectCodeName));
				Write(";\r\n");
			}
			else if (!boundElement.Type.IsDerivedFromControlTemplate())
			{
				Write("        ");
				Write(base.ToStringHelper.ToStringWithCulture(boundElement.Type));
				Write(" ");
				Write(base.ToStringHelper.ToStringWithCulture(boundElement.ObjectCodeName));
				Write(" { nullptr };\r\n");
			}
			if (!boundElement.CanBeInstantiatedLater)
			{
				continue;
			}
			foreach (BindAssignment bindAssignment in boundElement.BindAssignments)
			{
				Write("        ");
				Write(base.ToStringHelper.ToStringWithCulture(bindAssignment.MemberType));
				Write(" ");
				Write(base.ToStringHelper.ToStringWithCulture(bindAssignment.ObjectDeferredAssignmentCodeName));
				Write(" ");
				Write(base.ToStringHelper.ToStringWithCulture(bindAssignment.MemberType.IsNullable ? "{nullptr}" : "{}"));
				Write(";\r\n");
			}
		}
		if (bindUniverse.UnloadableBindingSourceElements.Any())
		{
			Write("        std::list<std::function<void ()>> UnloadableBindingSourcesToUpdate;\r\n");
		}
		if (bindUniverse.ElementsWithBoundLoadAssignments.Any())
		{
			Write("        std::list<int> UnloadedElementsToUpdate;\r\n");
		}
		if (bindUniverse.NeedsBindingsTracking)
		{
			Write("\r\n        // Fields for binding tracking.\r\n");
			Output_Listener_Tracking_Fields(bindUniverse);
		}
		if (bindUniverse.DistinctConvertersUsed.Count() > 0)
		{
			Write("        ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml")));
			Write("::ResourceDictionary localResources { nullptr };\r\n");
			if (!base.Model.CodeInfo.IsResourceDictionary)
			{
				Write("        ::winrt::weak_ref<");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml")));
				Write("::FrameworkElement> converterLookupRoot;\r\n");
			}
		}
		if (bindUniverse.HasBindings)
		{
			if (bindUniverse.HasFunctionBindings)
			{
				Output_TryGetValueFunctions(bindUniverse);
				Output_FunctionBindings(bindUniverse);
			}
			if (bindUniverse.NeedsCompleteUpdate)
			{
				Output_CompleteUpdate(bindUniverse);
			}
			Write("\r\n        // Update methods for each path node used in binding steps.\r\n");
		}
		foreach (BindPathStep item10 in bindUniverse.BindPathSteps.Values.Where((BindPathStep bindStep) => bindStep.IsIncludedInUpdate))
		{
			Write("\r\n");
			if (item10 is FunctionStep)
			{
				Write("        void Update_");
				Write(base.ToStringHelper.ToStringWithCulture(item10.CodeName));
				Write("(int32_t phase)\r\n");
			}
			else
			{
				Write("        void Update_");
				Write(base.ToStringHelper.ToStringWithCulture(item10.CodeName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(item10.ValueType));
				Write(" obj, int32_t phase)\r\n");
			}
			Write("        {\r\n");
			Output_UpdateChildListeners_Call(item10, "obj");
			Output_Update_Steps(item10.ValueType.IsNullable, item10.Children, checkPhaseCondition: true, "phase");
			Output_Update_Steps(checkForNull: false, item10.Dependents, checkPhaseCondition: false, "phase");
			foreach (int distinctPhase in item10.DistinctPhases)
			{
				Output_Binding_Phased_SetValue(distinctPhase, isTracking: true, item10, isFunctionResult: false);
				Output_Binding_Phased_SetValue(distinctPhase, isTracking: false, item10, isFunctionResult: false);
			}
			if (item10 is RootStep)
			{
				Output_Update_Steps(checkForNull: false, bindUniverse.BindPathSteps.Values.Where((BindPathStep s) => s.Parent is StaticRootStep), checkPhaseCondition: false, "phase");
				if (bindUniverse.NeedsCompleteUpdate)
				{
					Write("            CompleteUpdate(phase);\r\n");
				}
			}
			Write("        }\r\n");
		}
		foreach (BindPathStep item11 in bindUniverse.BindPathSteps.Values.Where((BindPathStep bindStep) => bindStep.IsIncludedInUpdate))
		{
			if (item11.Parent == null || !item11.BindStatus.HasFlag(BindStatus.HasFallbackValue))
			{
				continue;
			}
			Write("\r\n        void UpdateFallback_");
			Write(base.ToStringHelper.ToStringWithCulture(item11.CodeName));
			Write("(int phase)\r\n        {\r\n");
			foreach (BindPathStep item12 in item11.Children.Concat(item11.Dependents))
			{
				if (item12.BindStatus.HasFlag(BindStatus.HasFallbackValue))
				{
					Write("            UpdateFallback_");
					Write(base.ToStringHelper.ToStringWithCulture(item12.CodeName));
					Write("(phase);\r\n");
				}
			}
			foreach (int distinctPhase2 in item11.DistinctPhases)
			{
				Output_Binding_Phased_Fallback_SetValue(distinctPhase2, isTracking: true, item11);
				Output_Binding_Phased_Fallback_SetValue(distinctPhase2, isTracking: false, item11);
			}
			Write("        }\r\n");
		}
		if (bindUniverse.NeedsBindingsTracking)
		{
			Write("\r\n        virtual void ReleaseAllListeners() override\r\n        {\r\n");
			foreach (BindPathStep item13 in bindUniverse.BindPathSteps.Values.Where((BindPathStep step) => step.NeedsUpdateChildListeners))
			{
				Output_UpdateChildListeners_Call(item13, "nullptr");
			}
			Write("        }\r\n\r\n        virtual void PropertyChanged(IInspectable const& sender, ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Data")));
			Write("::PropertyChangedEventArgs const& e) override\r\n        {\r\n");
			foreach (BindPathStep item14 in bindUniverse.BindPathSteps.Values.Where((BindPathStep step) => step.IsIncludedInUpdate))
			{
				if (!item14.RequiresChildNotification || (!(item14 is PropertyStep) && !(item14 is CastStep) && !(item14 is RootStep) && !(item14 is RootNamedElementStep) && !(item14 is ArrayIndexStep) && !(item14 is MapIndexStep)) || !item14.ImplementsINPC)
				{
					continue;
				}
				string objectToConvert = "GetDataRoot()";
				if (item14.RequiresChildNotification && !(item14 is RootStep))
				{
					objectToConvert = "cachePC_" + item14.CodeName;
				}
				Write("            if (");
				Write(base.ToStringHelper.ToStringWithCulture(objectToConvert));
				Write(" && ");
				Write(base.ToStringHelper.ToStringWithCulture(objectToConvert));
				Write(" == sender)\r\n            {\r\n                auto propName = e.PropertyName();\r\n                auto obj = sender.as<");
				Write(base.ToStringHelper.ToStringWithCulture(item14.ValueType));
				Write(">();\r\n                if (propName.empty())\r\n                {\r\n");
				PushIndent(Indent.TwoTabs);
				Output_Update_Steps(item14.ValueType.IsNullable, item14.TrackingSteps, checkPhaseCondition: false, "DATA_CHANGED");
				PopIndent();
				Write("                }\r\n");
				foreach (string propertyName in (from p in item14.TrackingSteps.OfType<PropertyStep>()
					select p.PropertyName).Distinct())
				{
					Write("                else if (propName == L\"");
					Write(base.ToStringHelper.ToStringWithCulture(propertyName));
					Write("\")\r\n                {\r\n");
					PushIndent(Indent.TwoTabs);
					foreach (PropertyStep item15 in from p in item14.TrackingSteps.OfType<PropertyStep>()
						where p.PropertyName == propertyName
						select p)
					{
						Output_Update_DataChanged_Step(item15);
					}
					PopIndent();
					Write("                }\r\n");
				}
				foreach (string functionName in (from p in item14.TrackingSteps.OfType<FunctionStep>()
					select p.Method.MethodName).Distinct())
				{
					Write("                else if (propName == L\"");
					Write(base.ToStringHelper.ToStringWithCulture(functionName));
					Write("\")\r\n                {\r\n");
					PushIndent(Indent.TwoTabs);
					Output_Update_Steps(item14.ValueType.IsNullable, from p in item14.TrackingSteps.OfType<FunctionStep>()
						where p.Method.MethodName == functionName
						select p, checkPhaseCondition: false, "DATA_CHANGED");
					PopIndent();
					Write("                }\r\n");
				}
				Write("            }\r\n");
			}
			if (bindUniverse.NeedsCompleteUpdate)
			{
				Write("            CompleteUpdate(DATA_CHANGED);\r\n");
			}
			Write("        }\r\n\r\n        void CollectionChanged(IInspectable const& sender, ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Interop")));
			Write("::NotifyCollectionChangedEventArgs const& e) override\r\n        {\r\n");
			foreach (BindPathStep item16 in bindUniverse.BindPathSteps.Values.Where((BindPathStep step) => step.IsIncludedInUpdate))
			{
				if (item16.RequiresChildNotification && (item16 is PropertyStep || item16 is CastStep || item16 is RootStep || item16 is RootNamedElementStep) && item16.ImplementsINCC && !item16.ImplementsIObservableVector && !item16.ImplementsIObservableMap)
				{
					string objectToConvert2 = "GetDataRoot()";
					if (item16.RequiresChildNotification && !(item16 is RootStep))
					{
						objectToConvert2 = "cacheCC_" + item16.CodeName;
					}
					Write("            if (");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert2));
					Write(" && ");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert2));
					Write(" == sender)\r\n            {\r\n                ");
					Write(base.ToStringHelper.ToStringWithCulture(item16.ValueType));
					Write(" obj = sender.as<");
					Write(base.ToStringHelper.ToStringWithCulture(item16.ValueType));
					Write(">();\r\n");
					PushIndent();
					Output_Update_Steps(item16.ValueType.IsNullable, item16.TrackingSteps, checkPhaseCondition: false, "DATA_CHANGED");
					PopIndent();
					Write("            }\r\n");
				}
			}
			if (bindUniverse.NeedsCompleteUpdate)
			{
				Write("            CompleteUpdate(DATA_CHANGED);\r\n");
			}
			Write("        }\r\n\r\n        void VectorChanged(IInspectable const& sender, ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Windows.Foundation.Collections")));
			Write("::IVectorChangedEventArgs const& e) override\r\n        {\r\n");
			foreach (BindPathStep item17 in bindUniverse.BindPathSteps.Values.Where((BindPathStep step) => step.IsIncludedInUpdate))
			{
				if (item17.RequiresChildNotification && (item17 is PropertyStep || item17 is CastStep || item17 is RootStep) && item17.ImplementsIObservableVector)
				{
					string objectToConvert3 = "GetDataRoot()";
					if (item17.RequiresChildNotification && !(item17 is RootStep))
					{
						objectToConvert3 = "cacheVC_" + item17.CodeName;
					}
					Write("            if (");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert3));
					Write(" && ");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert3));
					Write(" == sender)\r\n            {\r\n                ");
					Write(base.ToStringHelper.ToStringWithCulture(item17.ValueType));
					Write(" obj = sender.as<");
					Write(base.ToStringHelper.ToStringWithCulture(item17.ValueType));
					Write(">();\r\n");
					PushIndent();
					Output_Update_Steps(item17.ValueType.IsNullable, item17.TrackingSteps, checkPhaseCondition: false, "DATA_CHANGED");
					PopIndent();
					Write("            }\r\n");
				}
			}
			if (bindUniverse.NeedsCompleteUpdate)
			{
				Write("            CompleteUpdate(DATA_CHANGED);\r\n");
			}
			Write("        }\r\n\r\n        void MapChanged(IInspectable const& sender, ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Windows.Foundation.Collections")));
			Write("::IMapChangedEventArgs<::winrt::hstring> const& e) override\r\n        {\r\n");
			foreach (BindPathStep item18 in bindUniverse.BindPathSteps.Values.Where((BindPathStep step) => step.IsIncludedInUpdate))
			{
				if (item18.RequiresChildNotification && (item18 is PropertyStep || item18 is CastStep || item18 is RootStep) && item18.ImplementsIObservableMap)
				{
					string objectToConvert4 = "GetDataRoot()";
					if (item18.RequiresChildNotification && !(item18 is RootStep))
					{
						objectToConvert4 = "cacheMC_" + item18.CodeName;
					}
					Write("            if (");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert4));
					Write(" && ");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert4));
					Write(" == sender)\r\n            {\r\n                ");
					Write(base.ToStringHelper.ToStringWithCulture(item18.ValueType));
					Write(" obj = sender.as<");
					Write(base.ToStringHelper.ToStringWithCulture(item18.ValueType));
					Write(">();\r\n");
					PushIndent();
					Output_Update_Steps(item18.ValueType.IsNullable, item18.TrackingSteps, checkPhaseCondition: false, "DATA_CHANGED");
					PopIndent();
					Write("            }\r\n");
				}
			}
			if (bindUniverse.NeedsCompleteUpdate)
			{
				Write("            CompleteUpdate(DATA_CHANGED);\r\n");
			}
			Write("        }\r\n\r\n        void DependencyPropertyChanged(DependencyObject const& sender, DependencyProperty const& prop) override\r\n        {\r\n            if (sender)\r\n            {\r\n");
			foreach (BindPathStep item19 in bindUniverse.BindPathSteps.Values.Where((BindPathStep step) => step.IsIncludedInUpdate))
			{
				foreach (DependencyPropertyStep item20 in item19.TrackingSteps.OfType<DependencyPropertyStep>())
				{
					Write("                if (sender == cacheDPC_");
					Write(base.ToStringHelper.ToStringWithCulture(item20.CodeName));
					Write(base.ToStringHelper.ToStringWithCulture((item19 is RootStep) ? ".get()" : ""));
					Write(" && ");
					Write(base.ToStringHelper.ToStringWithCulture(item20.OwnerType.CppWinRTLocalElseRef()));
					Write("::");
					Write(base.ToStringHelper.ToStringWithCulture(item20.PropertyName));
					Write("Property() == prop)\r\n                {\r\n                    auto obj = sender.as<");
					Write(base.ToStringHelper.ToStringWithCulture(item19.ValueType));
					Write(">();\r\n");
					PushIndent(Indent.TwoTabs);
					Output_Update_DataChanged_Step(item20);
					PopIndent();
					Write("                }\r\n");
				}
			}
			if (bindUniverse.HasFunctionBindings)
			{
				Write("                CompleteUpdate(DATA_CHANGED);\r\n");
			}
			Write("            }\r\n        }\r\n\r\n");
			if (base.ProjectInfo.IsInputValidationEnabled)
			{
				Write("        virtual void ErrorsChanged(IInspectable const& sender, ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Data")));
				Write("::DataErrorsChangedEventArgs const& e) override\r\n        {\r\n");
				foreach (BindPathStep iNDEIPathStep in bindUniverse.INDEIPathSteps)
				{
					string objectToConvert5 = "GetDataRoot()";
					if (!(iNDEIPathStep is RootStep))
					{
						objectToConvert5 = "cacheEC_" + iNDEIPathStep.CodeName;
					}
					Write("            if (");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert5));
					Write(" != nullptr && ");
					Write(base.ToStringHelper.ToStringWithCulture(objectToConvert5));
					Write(" == sender)\r\n            {\r\n                auto errorInfo= sender.as<");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Data")));
					Write("::INotifyDataErrorInfo>();\r\n                auto propertyName = e.PropertyName();\r\n                if (propertyName.empty())\r\n                {\r\n");
					foreach (PropertyStep item21 in iNDEIPathStep.TrackingSteps.OfType<PropertyStep>())
					{
						Output_UpdateErrorsCall(item21);
					}
					Write("                }\r\n");
					foreach (string propertyName2 in (from p in iNDEIPathStep.TrackingSteps.OfType<PropertyStep>()
						select p.PropertyName).Distinct())
					{
						Write("                else if (propertyName == L\"");
						Write(base.ToStringHelper.ToStringWithCulture(propertyName2));
						Write("\")\r\n                {\r\n");
						foreach (PropertyStep item22 in from p in iNDEIPathStep.TrackingSteps.OfType<PropertyStep>()
							where p.PropertyName == propertyName2
							select p)
						{
							Output_UpdateErrorsCall(item22);
						}
						Write("                }\r\n            }\r\n");
					}
				}
				Write("        }\r\n");
				if (bindUniverse.INDEIPathSteps.Any())
				{
					Write("\r\n        void UpdateErrors(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Controls")));
					Write("::Control const& control, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Data")));
					Write("::INotifyDataErrorInfo const& sender, ::winrt::hstring propertyName)\r\n        {\r\n            if (auto validationControl = control.try_as");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Controls")));
					Write("::IInputValidationControl>())\r\n            {\r\n                auto validationErrors =  validationControl.ValidationErrors();\r\n                validationErrors.Clear();\r\n                for (const auto& error : sender.GetErrors(propertyName))\r\n                {\r\n                    auto inputValidationError = ::winrt::unbox_value<");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Controls")));
					Write("::InputValidationError>(error);\r\n                    if (inputValidationError == nullptr)\r\n                    {\r\n                        auto errorMessage = ::winrt::unbox_value<::winrt::hstring>(error);\r\n                        if (errorMessage.empty())\r\n                        {\r\n                            if (auto stringable = ::winrt::unbox_value<");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Windows.Foundation")));
					Write("::IStringable>(error))\r\n                            {\r\n                                errorMessage = stringable.ToString();\r\n                            }\r\n                            else\r\n                            {\r\n                                errorMessage = ::winrt::get_class_name(error);\r\n                            }\r\n                        }\r\n                        inputValidationError = ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Controls")));
					Write("::InputValidationError(errorMessage);\r\n                    }\r\n                    validationErrors.Append(inputValidationError);\r\n                }\r\n            }\r\n        }\r\n");
				}
			}
		}
		Output_BindingSetters(bindUniverse);
		Write("    }; ");
		Write("\r\n");
		return base.GenerationEnvironment.ToString();
		void Output_UpdateErrorsCall(PropertyStep step)
		{
			foreach (IBindAssignment item23 in step.AssociatedBindAssignments.Where((IBindAssignment ba) => ba.IsTrackingTarget))
			{
				Write("                    UpdateErrors(");
				Write(base.ToStringHelper.ToStringWithCulture((item23 as BindAssignmentBase).ConnectionIdElement.ObjectCodeName));
				Write(", errorInfo, L\"");
				Write(base.ToStringHelper.ToStringWithCulture(step.PropertyName));
				Write("\");\r\n");
			}
		}
	}

	private void Output_ConnectionId_Case(ConnectionIdElement element)
	{
		if (element.BindUniverse.BoundElements.Contains(element))
		{
			Output_ApiInformationCall_Push(element.ApiInformation, Indent.ThreeTabs);
			Write("                    auto targetElement = target.as<");
			Write(base.ToStringHelper.ToStringWithCulture(element.Type));
			Write(">();\r\n                    ");
			Write(base.ToStringHelper.ToStringWithCulture(element.ObjectCodeName));
			Write(" = targetElement;\r\n");
			foreach (BoundEventAssignment boundEventAssignment in element.BoundEventAssignments)
			{
				Output_ApiInformationCall_Push(boundEventAssignment.ApiInformation, Indent.ThreeTabs);
				Output_NullCheckedEventAssignment(boundEventAssignment);
				Output_ApiInformationCall_Pop(boundEventAssignment.ApiInformation, Indent.ThreeTabs);
			}
			if (element.CanBeInstantiatedLater && (element.HasBindAssignments || element.HasBoundEventAssignments))
			{
				foreach (BindAssignment item in element.BindAssignments.Where((BindAssignment ba) => !(ba is BoundLoadAssignment)))
				{
					PushIndent(Indent.TwoTabs);
					Output_Binding_SetValue_Non_Function_Call(item, item.ObjectDeferredAssignmentCodeName.CppWinRTName(), includeDeferredSet: false);
					PopIndent();
				}
			}
			foreach (BindAssignment item2 in element.BindAssignments.Where((BindAssignment bindAssignment) => bindAssignment.IsTrackingTarget))
			{
				PushIndent();
				Output_Connect_TwoWayBinding(item2);
				PopIndent();
			}
			Output_ApiInformationCall_Pop(element.ApiInformation, Indent.ThreeTabs);
		}
		if (!element.IsBindingRoot)
		{
			foreach (ConnectionIdElement item3 in element.Children.Intersect(element.BindUniverse.ElementsWithBoundLoadAssignments))
			{
				Write("                if (std::find(UnloadedElementsToUpdate.begin(), UnloadedElementsToUpdate.end(), ");
				Write(base.ToStringHelper.ToStringWithCulture(item3.ConnectionId));
				Write(") == UnloadedElementsToUpdate.end())\r\n                {\r\n                    UnloadedElementsToUpdate.push_back(");
				Write(base.ToStringHelper.ToStringWithCulture(item3.ConnectionId));
				Write(");\r\n                }\r\n");
			}
		}
		if (element.CanBeInstantiatedLater && element.HasRootNamedElementStep)
		{
			Write("                    this->UnloadableBindingSourcesToUpdate.push_back([this]()\r\n                    {\r\n");
			PushIndent(Indent.TwoTabs);
			Output_Custom_Update_Call(element.RootNamedElementStep, element.RootNamedElementStep.CodeGen().PathExpression.CppWinRTName(), "NOT_PHASED");
			PopIndent();
			Write("                    });\r\n");
		}
		if (element.TryGetValidationContextStep(out var bindStep) && base.ProjectInfo.IsInputValidationEnabled && base.ProjectInfo.EnableDefaultValidationContextGeneration)
		{
			Output_ApiInformationCall_Push(bindStep.ApiInformation, Indent.ThreeTabs);
			Write("                    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Controls")));
			Write("::InputValidationContext context(L\"");
			Write(base.ToStringHelper.ToStringWithCulture(bindStep.PropertyName));
			Write("\", ");
			Write(base.ToStringHelper.ToStringWithCulture(bindStep.IsValueRequired));
			Write(");\r\n                    target.as<");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Controls")));
			Write("::IInputValidationControl>().ValidationContext(context);\r\n");
			Output_ApiInformationCall_Pop(bindStep.ApiInformation, Indent.ThreeTabs);
		}
	}

	private void Output_Listener_Tracking_Fields(BindUniverse bindUniverse)
	{
		foreach (string cacheDeclaration in GetCacheDeclarations(bindUniverse))
		{
			Write("        ");
			Write(base.ToStringHelper.ToStringWithCulture(cacheDeclaration));
			Write("\r\n");
		}
		foreach (string tokenDeclaration in GetTokenDeclarations(bindUniverse))
		{
			Write("        ");
			Write(base.ToStringHelper.ToStringWithCulture(tokenDeclaration));
			Write("\r\n");
		}
	}

	private void Output_Connect_TwoWayBinding(BindAssignment ba)
	{
		Output_ApiInformationCall_Push(ba.ApiInformation, Indent.None);
		if (ba.NeedsLostFocusForTwoWay)
		{
			Write("                ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.ConnectionIdElement.ReferenceExpression));
			Write(".LostFocus(\r\n                    [weakThis{ this->weak_from_this() }, this] (IInspectable const& sender, ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml")));
			Write("::RoutedEventArgs const& e)\r\n                    {\r\n");
		}
		else
		{
			Write("                ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.ConnectionIdElement.ReferenceExpression));
			Write(".RegisterPropertyChangedCallback(");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberDeclaringType.CppWinRTLocalElseRef()));
			Write("::");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberName));
			Write("Property(),\r\n                    [weakThis{ this->weak_from_this() }, this] (DependencyObject const& sender, DependencyProperty const& prop)\r\n                    {\r\n");
		}
		Write("                        if (auto strongThis{ weakThis.lock() })\r\n                        {\r\n                            if (IsInitialized())\r\n                            {\r\n                                // Update Two Way binding\r\n");
		if (ba.BindBackStep is MethodStep methodStep)
		{
			Parameter parameter = methodStep.Parameters[0];
			Write("                                ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberType));
			Write(" ");
			Write(base.ToStringHelper.ToStringWithCulture(parameter.Name));
			Write(" = ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.ReverseAssignmentExpression));
			Write(";\r\n");
			Output_NullCheckedAssignment(methodStep, null);
		}
		else
		{
			Output_NullCheckedAssignment(ba.PathStep, ba.ReverseAssignmentExpression);
		}
		Write("                            }\r\n                        }\r\n                    });\r\n");
		Output_ApiInformationCall_Pop(ba.ApiInformation, Indent.None);
	}

	private void Output_NullCheckedAssignment(BindPathStep step, LanguageSpecificString value)
	{
		PushIndent(Indent.FourTabs);
		foreach (BindPathStep item in step.Parents.Where((BindPathStep parent) => parent.NeedsCheckForNull))
		{
			Write("                if (");
			Write(base.ToStringHelper.ToStringWithCulture(item.CodeGen().PathExpression));
			Write(" != nullptr)\r\n                {\r\n");
			PushIndent();
		}
		if (value != null)
		{
			Write("                ");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeGen().PathSetExpression(value)));
			Write(";\r\n");
		}
		else
		{
			Write("                ");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeGen().PathExpression));
			Write(";\r\n");
		}
		foreach (BindPathStep item2 in step.Parents.Where((BindPathStep parent) => parent.NeedsCheckForNull))
		{
			PopIndent();
			Write("                }\r\n");
		}
		PopIndent();
	}

	private void Output_NullCheckedEventAssignment(BoundEventAssignment evt)
	{
		Write("                    targetElement.");
		Write(base.ToStringHelper.ToStringWithCulture(evt.MemberName));
		Write("([weakThis{ this->weak_from_this() }, this](");
		Write(base.ToStringHelper.ToStringWithCulture(evt.Parameters.Declaration()));
		Write(")\r\n                    {\r\n");
		PushIndent(Indent.ThreeTabs);
		Write("            if (auto strongThis{ weakThis.lock() })\r\n            {\r\n");
		foreach (BindPathStep item in evt.PathStep.Parents.Where((BindPathStep parent) => parent.NeedsCheckForNull))
		{
			Write("                if (");
			Write(base.ToStringHelper.ToStringWithCulture(item.CodeGen().PathExpression));
			Write(" != nullptr)\r\n                {\r\n");
			PushIndent();
		}
		if (!evt.PathStep.ValueType.IsDelegate())
		{
			Write("                ");
			Write(base.ToStringHelper.ToStringWithCulture(evt.PathStep.CodeGen().PathExpression));
			Write(";\r\n");
		}
		else
		{
			Write("                ");
			Write(base.ToStringHelper.ToStringWithCulture(evt.PathStep.CodeGen().PathExpression));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(evt.Parameters.ForCall()));
			Write(");\r\n");
		}
		foreach (BindPathStep item2 in evt.PathStep.Parents.Where((BindPathStep parent) => parent.NeedsCheckForNull))
		{
			PopIndent();
			Write("                }\r\n");
		}
		Write("            }\r\n");
		PopIndent();
		Write("                    });\r\n");
	}

	private void Output_UpdateChildListeners_Call(BindPathStep step, string parameter)
	{
		if (!step.NeedsUpdateChildListeners)
		{
			return;
		}
		Output_ApiInformationCall_Push(step.ApiInformation, Indent.OneTab);
		if (step.ImplementsINPC)
		{
			Write("            _bindingsTracking->UpdatePropertyChangedListener(");
			Write(base.ToStringHelper.ToStringWithCulture(parameter));
			Write(", cachePC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(", tokenPC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(");\r\n");
		}
		if (step.ImplementsINDEI && base.ProjectInfo.IsInputValidationEnabled)
		{
			Write("            _bindingsTracking->UpdateErrorsChangedListener(");
			Write(base.ToStringHelper.ToStringWithCulture(parameter));
			Write(", cacheEC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(", tokenEC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(");\r\n");
		}
		if (step.ImplementsIObservableVector)
		{
			Write("            _bindingsTracking->UpdateVectorChangedListener_");
			Write(base.ToStringHelper.ToStringWithCulture(step.ValueType.ItemType.MemberFriendlyName()));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(parameter));
			Write(", cacheVC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(", tokenVC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(");\r\n");
		}
		if (step.ImplementsIObservableMap)
		{
			Write("            _bindingsTracking->UpdateMapChangedListener_");
			Write(base.ToStringHelper.ToStringWithCulture(step.ValueType.ItemType.MemberFriendlyName()));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(parameter));
			Write(", cacheMC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(", tokenMC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(");\r\n");
		}
		else if (step.ImplementsINCC)
		{
			Write("            _bindingsTracking->UpdateCollectionChangedListener(");
			Write(base.ToStringHelper.ToStringWithCulture(parameter));
			Write(", cacheCC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(", tokenCC_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write(");\r\n");
		}
		foreach (DependencyPropertyStep item in step.TrackingSteps.OfType<DependencyPropertyStep>())
		{
			Write("            _bindingsTracking->UpdateDependencyPropertyChangedListener(");
			Write(base.ToStringHelper.ToStringWithCulture(parameter));
			Write(", ");
			Write(base.ToStringHelper.ToStringWithCulture(item.OwnerType.CppWinRTLocalElseRef()));
			Write("::");
			Write(base.ToStringHelper.ToStringWithCulture(item.PropertyName));
			Write("Property(), cacheDPC_");
			Write(base.ToStringHelper.ToStringWithCulture(item.CodeName));
			Write(", tokenDPC_");
			Write(base.ToStringHelper.ToStringWithCulture(item.CodeName));
			Write(");\r\n");
		}
		Output_ApiInformationCall_Pop(step.ApiInformation, Indent.OneTab);
	}

	private void Output_Binding_Phased_SetValue(int phase, bool isTracking, BindPathStep bindStep, bool isFunctionResult)
	{
		IEnumerable<IBindAssignment> enumerable = bindStep.BindAssignments.Where((IBindAssignment ba) => ba.ComputedPhase == phase && ba.IsTrackingSource == isTracking);
		if (enumerable.Count() <= 0)
		{
			return;
		}
		string directPhaseCondition = GetDirectPhaseCondition(phase, isTracking);
		Write("            if(");
		Write(base.ToStringHelper.ToStringWithCulture(directPhaseCondition));
		Write(")\r\n            {\r\n");
		PushIndent();
		foreach (BindAssignment item in enumerable)
		{
			Output_Binding_SetValue(item, isFunctionResult);
		}
		PopIndent();
		Write("            }\r\n");
	}

	private void Output_Binding_Phased_Fallback_SetValue(int phase, bool isTracking, BindPathStep bindStep)
	{
		IEnumerable<IBindAssignment> enumerable = bindStep.BindAssignments.Where((IBindAssignment ba) => ba.ComputedPhase == phase && ba.IsTrackingSource == isTracking && ba.BindStatus.HasFlag(BindStatus.HasFallbackValue));
		if (enumerable.Count() <= 0)
		{
			return;
		}
		string directPhaseCondition = GetDirectPhaseCondition(phase, isTracking);
		Write("            if(");
		Write(base.ToStringHelper.ToStringWithCulture(directPhaseCondition));
		Write(")\r\n            {\r\n");
		PushIndent();
		foreach (BindAssignment item in enumerable)
		{
			Output_Binding_SetValue_Non_Function_Call(item, item.FallbackValueExpression.CppWinRTName(), includeDeferredSet: true);
		}
		PopIndent();
		Write("            }\r\n");
	}

	private void Output_Binding_SetValue(BindAssignment bindAssignment, bool isFunctionResult)
	{
		if (bindAssignment.PathStep is FunctionStep && !isFunctionResult)
		{
			Write("            PendingFunctionBindings[L\"");
			Write(base.ToStringHelper.ToStringWithCulture(bindAssignment.PathStep.CodeName));
			Write("\"] = std::bind(&");
			Write(base.ToStringHelper.ToStringWithCulture(bindAssignment.BindUniverse.BindingsClassName));
			Write("::Invoke_");
			Write(base.ToStringHelper.ToStringWithCulture(bindAssignment.PathStep.CodeName));
			Write(", this, std::placeholders::_1);\r\n");
		}
		else
		{
			string value = bindAssignment.DirectAssignmentExpression(isFunctionResult ? "result" : "obj").CppWinRTName();
			Output_Binding_SetValue_Non_Function_Call(bindAssignment, value, includeDeferredSet: true);
		}
	}

	private void Output_Binding_SetValue_Non_Function_Call(BindAssignment ba, string value, bool includeDeferredSet)
	{
		ConnectionIdElement connectionIdElement = ba.ConnectionIdElement;
		Write("            // ");
		Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.LineNumberAndXamlFile));
		Write("\r\n");
		Output_ApiInformationCall_Push(ba.ApiInformation, Indent.OneTab);
		if (ba is BoundLoadAssignment)
		{
			if (includeDeferredSet)
			{
				Write("            ");
				Write(base.ToStringHelper.ToStringWithCulture(ba.ObjectDeferredAssignmentCodeName));
				Write(" = ");
				Write(base.ToStringHelper.ToStringWithCulture(value));
				Write(";\r\n\r\n");
			}
			Write("            if (");
			Write(base.ToStringHelper.ToStringWithCulture(value));
			Write(")\r\n            {\r\n");
			if (ba.BindUniverse.IsFileRoot)
			{
				Write("                GetDataRoot().FindName(L\"");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ElementName));
				Write("\");\r\n");
			}
			else
			{
				Write("                ");
				Write(base.ToStringHelper.ToStringWithCulture(ba.BindUniverse.RootElement.ReferenceExpression));
				Write(".FindName(L\"");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ElementName));
				Write("\");\r\n");
			}
			Write("            }\r\n            else\r\n            {\r\n");
			if (ba.BindUniverse.IsFileRoot)
			{
				Write("                ::winrt::get_self<");
				Write(base.ToStringHelper.ToStringWithCulture(ba.BindUniverse.DataRootType.CppWinRTLocalElseRef()));
				Write(">(GetDataRoot())->UnloadObject(");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ReferenceExpression));
				Write(");\r\n");
			}
			else
			{
				Write("                if (");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ReferenceExpression));
				Write(") \r\n                {\r\n                    ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<PageDefinition>.Projection("Microsoft.UI.Xaml.Markup")));
				Write("::XamlMarkupHelper::UnloadObject(");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ReferenceExpression));
				Write(");\r\n                }\r\n                DisconnectUnloadedObject(");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ConnectionId));
				Write(");\r\n");
			}
			Write("            }\r\n");
		}
		else
		{
			if (connectionIdElement.NeedsNullCheckBeforeSetValue)
			{
				Write("            if (");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ObjectCodeName));
				Write(")\r\n            {\r\n");
				PushIndent();
			}
			string objectToConvert = (ba.NeedsBox ? ("::winrt::box_value(" + value + ")") : value);
			if (ba.MemberType.IsNullable)
			{
				Write("            Set_");
				Write(base.ToStringHelper.ToStringWithCulture(ba.MemberDeclaringType.MemberFriendlyName()));
				Write("_");
				Write(base.ToStringHelper.ToStringWithCulture(ba.MemberName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ReferenceExpression));
				Write(", ");
				Write(base.ToStringHelper.ToStringWithCulture(objectToConvert));
				Write(", ");
				Write(base.ToStringHelper.ToStringWithCulture(ba.TargetNullValueExpression));
				Write(");\r\n");
			}
			else
			{
				Write("            Set_");
				Write(base.ToStringHelper.ToStringWithCulture(ba.MemberDeclaringType.MemberFriendlyName()));
				Write("_");
				Write(base.ToStringHelper.ToStringWithCulture(ba.MemberName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(connectionIdElement.ReferenceExpression));
				Write(", ");
				Write(base.ToStringHelper.ToStringWithCulture(objectToConvert));
				Write(");\r\n");
			}
			if (connectionIdElement.NeedsNullCheckBeforeSetValue)
			{
				PopIndent();
				Write("            }\r\n");
				if (includeDeferredSet && connectionIdElement.CanBeInstantiatedLater)
				{
					Write("            else\r\n            {\r\n                ");
					Write(base.ToStringHelper.ToStringWithCulture(ba.ObjectDeferredAssignmentCodeName));
					Write(" = ");
					Write(base.ToStringHelper.ToStringWithCulture(value));
					Write(";\r\n            }\r\n");
				}
			}
		}
		Output_ApiInformationCall_Pop(ba.ApiInformation, Indent.OneTab);
	}

	private void Output_Update_DataChanged_Step(BindPathStep step)
	{
		if (step.Parent.ValueType.IsNullable)
		{
			Write("            if (obj)\r\n            {\r\n");
		}
		Output_UpdateCall(step, "DATA_CHANGED");
		if (step.Parent.ValueType.IsNullable)
		{
			Write("            }\r\n");
		}
		if (step.BindStatus.HasFlag(BindStatus.HasFallbackValue))
		{
			Write("            else\r\n            {\r\n                UpdateFallback_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write("(DATA_CHANGED);\r\n            }\r\n");
		}
	}

	private void Output_Update_Steps(bool checkForNull, IEnumerable<BindPathStep> steps, bool checkPhaseCondition, string phaseParam)
	{
		if (steps.Count() <= 0)
		{
			return;
		}
		if (checkForNull)
		{
			Write("            if (obj)\r\n            {\r\n");
		}
		List<BindPathStep> list = new List<BindPathStep>();
		string text = null;
		foreach (BindPathStep item in from step in steps
			where step.IsIncludedInUpdate
			orderby step.PhaseList
			select step)
		{
			if (checkPhaseCondition)
			{
				string phaseCondition = GetPhaseCondition(item);
				if (phaseCondition != text)
				{
					if (text != null)
					{
						Write("                }\r\n");
					}
					text = phaseCondition;
					Write("                if (");
					Write(base.ToStringHelper.ToStringWithCulture(phaseCondition));
					Write(")\r\n                {\r\n");
				}
				PushIndent();
				Output_UpdateCall(item, phaseParam);
				PopIndent();
			}
			else
			{
				if (text != null)
				{
					Write("                }\r\n");
				}
				text = null;
				Output_UpdateCall(item, phaseParam);
			}
			if (item.BindStatus.HasFlag(BindStatus.HasFallbackValue))
			{
				list.Add(item);
			}
		}
		if (text != null)
		{
			Write("                }\r\n");
		}
		if (!checkForNull)
		{
			return;
		}
		Write("            }\r\n");
		if (list.Count <= 0)
		{
			return;
		}
		Write("            else\r\n            {\r\n");
		foreach (BindPathStep item2 in list)
		{
			if (checkPhaseCondition)
			{
				Write("                if (");
				Write(base.ToStringHelper.ToStringWithCulture(GetPhaseCondition(item2)));
				Write(")\r\n                {\r\n                    UpdateFallback_");
				Write(base.ToStringHelper.ToStringWithCulture(item2.CodeName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(phaseParam));
				Write(");\r\n                }\r\n");
			}
			else
			{
				Write("                UpdateFallback_");
				Write(base.ToStringHelper.ToStringWithCulture(item2.CodeName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(phaseParam));
				Write(");\r\n");
			}
		}
		Write("            }\r\n");
	}

	private void Output_Custom_Update_Call(BindPathStep step, string firstArgument, string phaseParam)
	{
		if (string.IsNullOrEmpty(firstArgument))
		{
			Write("                Update_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(phaseParam));
			Write(");\r\n");
		}
		else
		{
			Output_ApiInformationCall_Push(step.ApiInformation, Indent.TwoTabs);
			Write("                Update_");
			Write(base.ToStringHelper.ToStringWithCulture(step.CodeName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(firstArgument));
			Write(", ");
			Write(base.ToStringHelper.ToStringWithCulture(phaseParam));
			Write(");\r\n");
			Output_ApiInformationCall_Pop(step.ApiInformation, Indent.TwoTabs);
		}
	}

	private void Output_UpdateCall(BindPathStep step, string phaseParam)
	{
		string firstArgument = step.CodeGen().UpdateCallParam.CppWinRTName();
		Output_Custom_Update_Call(step, firstArgument, phaseParam);
	}

	private void Output_CompleteUpdate(BindUniverse bindUniverse)
	{
		List<string> list = new List<string>();
		bool flag = bindUniverse.UnloadableBindingSourceElements.Any();
		Write("\r\n        void CompleteUpdate(int phase)\r\n        {\r\n");
		if (flag)
		{
			Write("            do\r\n            {\r\n");
			PushIndent();
		}
		if (bindUniverse.HasFunctionBindings)
		{
			Write("        auto copiedBindings = std::move(PendingFunctionBindings);\r\n        PendingFunctionBindings = std::map<::std::wstring, std::function<void (int)>>();\r\n\r\n        for(auto& pair : copiedBindings)\r\n        {\r\n            pair.second(phase);\r\n        }\r\n");
			list.Add("!this->PendingFunctionBindings.empty()");
		}
		if (bindUniverse.UnloadableBindingSourceElements.Any())
		{
			Write("            while (!this->UnloadableBindingSourcesToUpdate.empty())\r\n            {\r\n                this->UnloadableBindingSourcesToUpdate.front()();\r\n                this->UnloadableBindingSourcesToUpdate.pop_front();\r\n            }\r\n");
			list.Add("!this->UnloadableBindingSourcesToUpdate.empty()");
		}
		if (bindUniverse.ElementsWithBoundLoadAssignments.Any())
		{
			Write("            while (!UnloadedElementsToUpdate.empty())\r\n            {\r\n                UpdateUnloadedElement(UnloadedElementsToUpdate.front());\r\n                UnloadedElementsToUpdate.pop_front();\r\n            }\r\n");
			list.Add("!this->UnloadedElementsToUpdate.empty()");
		}
		if (flag)
		{
			PopIndent();
			Write("            }\r\n            while (");
			Write(base.ToStringHelper.ToStringWithCulture(string.Join(" || ", list)));
			Write(");\r\n");
		}
		Write("        }\r\n");
	}

	private void Output_FunctionBindings(BindUniverse bindUniverse)
	{
		Write("        std::map<::std::wstring, std::function<void (int)>> PendingFunctionBindings;\r\n\r\n");
		foreach (FunctionStep item in bindUniverse.BindPathSteps.Values.Where((BindPathStep s) => s is FunctionStep))
		{
			Write("\r\n        void Invoke_");
			Write(base.ToStringHelper.ToStringWithCulture(item.CodeName));
			Write("(int phase)\r\n        {\r\n");
			foreach (FunctionParam item2 in item.Parameters.OrderBy((FunctionParam p) => p.HasTryGetValue))
			{
				if (item2.HasTryGetValue && item.RequiresSafeParameterRetrieval)
				{
					if (item2.AssignmentType.IsNullable)
					{
						Write("            ");
						Write(base.ToStringHelper.ToStringWithCulture(item2.AssignmentType));
						Write(" ");
						Write(base.ToStringHelper.ToStringWithCulture(item2.Name));
						Write(" = nullptr;\r\n");
					}
					else
					{
						Write("            ");
						Write(base.ToStringHelper.ToStringWithCulture(item2.AssignmentType));
						Write(" ");
						Write(base.ToStringHelper.ToStringWithCulture(item2.Name));
						Write(";\r\n");
					}
					Write("            if (!");
					Write(base.ToStringHelper.ToStringWithCulture(item2.TryGetValueCodeName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(item2.Name));
					Write(")) { return; }\r\n");
				}
				else
				{
					Write("            ");
					Write(base.ToStringHelper.ToStringWithCulture(item2.ValueType));
					Write(" ");
					Write(base.ToStringHelper.ToStringWithCulture(item2.Name));
					Write(" = ");
					Write(base.ToStringHelper.ToStringWithCulture(item2.CodeGen().PathExpression));
					Write(";\r\n");
				}
			}
			Write("            ");
			Write(base.ToStringHelper.ToStringWithCulture(item.ValueType));
			Write(" result = ");
			Write(base.ToStringHelper.ToStringWithCulture(item.CodeGen().PathExpression));
			Write(";\r\n");
			foreach (int distinctPhase in item.DistinctPhases)
			{
				Output_Binding_Phased_SetValue(distinctPhase, isTracking: true, item, isFunctionResult: true);
				Output_Binding_Phased_SetValue(distinctPhase, isTracking: false, item, isFunctionResult: true);
			}
			Write("        }\r\n");
		}
	}

	private void Output_TryGetValueFunctions(BindUniverse bindUniverse)
	{
		foreach (BindPathStep tryGetValueStep in bindUniverse.TryGetValueSteps)
		{
			Write("\r\n        bool ");
			Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.TryGetValueCodeName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.ValueType));
			Write("& val)\r\n        {\r\n");
			if (tryGetValueStep is RootStep || !tryGetValueStep.Parent.IsIncludedInUpdate)
			{
				Write("            val = ");
				Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.CodeGen().PathExpression));
				Write(";\r\n            return true;\r\n");
			}
			else
			{
				if (tryGetValueStep.Parent.ValueType.IsNullable)
				{
					Write("            ");
					Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.Parent.ValueType));
					Write(" obj{nullptr};\r\n            if (");
					Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.Parent.TryGetValueCodeName));
					Write("(obj) && obj)\r\n");
				}
				else
				{
					Write("            ");
					Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.Parent.ValueType));
					Write(" obj;\r\n            if (");
					Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.Parent.TryGetValueCodeName));
					Write("(obj))\r\n");
				}
				Write("            {\r\n                val = ");
				Write(base.ToStringHelper.ToStringWithCulture(tryGetValueStep.CodeGen().UpdateCallParam));
				Write(";\r\n                return true;\r\n            }\r\n            else\r\n            {\r\n                return false;\r\n            }\r\n");
			}
			Write("        }\r\n");
		}
	}

	private void Output_BindingSetValueFunction(BindAssignment ba)
	{
		if (ba.MemberType.IsNullable)
		{
			Write("\r\n        static void Set_");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberDeclaringType.MemberFriendlyName()));
			Write("_");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberTargetType));
			Write(" const& obj, ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberType));
			Write(" value, std::optional<::winrt::hstring> const& targetNullValue)\r\n        {\r\n            if (!value && targetNullValue)\r\n            {\r\n                value = ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberType.GetStringToThing("targetNullValue.value()")));
			Write(";\r\n            }\r\n");
		}
		else
		{
			Write("\r\n        static void Set_");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberDeclaringType.MemberFriendlyName()));
			Write("_");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberTargetType));
			Write(" const& obj, ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberType));
			Write(" const& value)\r\n        {\r\n");
		}
		if (ba.IsAttachable)
		{
			Write("            ");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberDeclaringType));
			Write("::Set");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberName));
			Write("(obj, value);\r\n");
		}
		else
		{
			Write("            obj.");
			Write(base.ToStringHelper.ToStringWithCulture(ba.MemberName));
			Write("(value);\r\n");
		}
		Write("        }\r\n");
	}

	private string GetCacheArgumentStatement(BindPathStep step, string prefix, BindPathStep child)
	{
		return $"{prefix}{child.CodeName}";
	}

	private void Output_BindingSetters(BindUniverse bu)
	{
		IEnumerable<IGrouping<string, BindAssignment>> enumerable = from ba in bu.BindAssignments
			where ba.HasSetValueHelper
			group ba by ba.MemberFullName;
		foreach (IGrouping<string, BindAssignment> item in enumerable)
		{
			Output_BindingSetValueFunction(item.First());
		}
	}

	private void Output_ApiInformationCall_Push(ApiInformation apiInformation, Indent indent)
	{
		if (apiInformation != null)
		{
			PushIndent(indent);
			Write("        if (");
			Write(base.ToStringHelper.ToStringWithCulture(base.Model.CodeInfo.ClassName.ShortName));
			Write("_");
			Write(base.ToStringHelper.ToStringWithCulture(apiInformation.MemberFriendlyName));
			Write(")\r\n        {\r\n");
			PopIndent();
			PushIndent();
		}
	}

	private void Output_ApiInformationCall_Pop(ApiInformation apiInformation, Indent indent)
	{
		if (apiInformation != null)
		{
			PopIndent();
			PushIndent(indent);
			Write("        }\r\n");
			PopIndent();
		}
	}
}
