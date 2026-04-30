namespace Microsoft.UI.Xaml.Markup.Compiler;

internal interface IXamlTypeMeta
{
	bool ImplementsINotifyPropertyChanged { get; }

	bool ImplementsINotifyCollectionChanged { get; }

	bool ImplementsIObservableVector { get; }

	bool ImplementsIObservableMap { get; }

	bool ImplementsINotifyDataErrorInfo { get; }

	bool HasApiInformation { get; }
}
