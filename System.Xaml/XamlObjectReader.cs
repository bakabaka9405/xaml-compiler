using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows.Markup;
using System.Xaml.Schema;
using System.Xml;
using System.Xml.Serialization;
using MS.Internal.Xaml.Runtime;

namespace System.Xaml;

public class XamlObjectReader : XamlReader
{
	private class NameScopeMarkupInfo : ObjectMarkupInfo
	{
		public ReferenceTable ParentTable { get; set; }

		public object SourceObject { get; set; }

		public void Resume(SerializerContext context)
		{
			context.ReferenceTable = new ReferenceTable(ParentTable);
			AddRecordMembers(SourceObject, context);
		}

		public override void EnsureNoDuplicateNames(Stack<HashSet<string>> namesInCurrentScope)
		{
			namesInCurrentScope.Push(new HashSet<string>());
			base.EnsureNoDuplicateNames(namesInCurrentScope);
			namesInCurrentScope.Pop();
		}
	}

	private class ObjectReferenceEqualityComparer : IEqualityComparer<object>
	{
		public new bool Equals(object x, object y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return obj?.GetHashCode() ?? 0;
		}
	}

	private class ValueMarkupInfo : ObjectOrValueMarkupInfo
	{
	}

	private class MemberMarkupInfo : MarkupInfo
	{
		private List<MarkupInfo> children = new List<MarkupInfo>();

		public bool IsContent { get; set; }

		public bool IsFactoryMethod { get; set; }

		public List<MarkupInfo> Children => children;

		public bool IsAtomic
		{
			get
			{
				if (children.Count == 1)
				{
					return children[0] is ValueMarkupInfo;
				}
				return false;
			}
		}

		public bool IsAttributableMarkupExtension
		{
			get
			{
				if (children.Count != 1)
				{
					return false;
				}
				if (children[0] is ObjectMarkupInfo objectMarkupInfo)
				{
					return objectMarkupInfo.IsAttributableMarkupExtension;
				}
				return false;
			}
		}

		public bool IsAttributable
		{
			get
			{
				if (base.XamlNode.Member == XamlLanguage.PositionalParameters)
				{
					foreach (MarkupInfo child in children)
					{
						if (child is ObjectMarkupInfo { IsAttributableMarkupExtension: false })
						{
							return false;
						}
					}
					return true;
				}
				if (Children.Count > 1)
				{
					return false;
				}
				if (Children.Count == 0 || Children[0] is ValueMarkupInfo)
				{
					return true;
				}
				if (!(Children[0] is ObjectMarkupInfo objectMarkupInfo2))
				{
					throw new InvalidOperationException(SR.Get("ExpectedObjectMarkupInfo"));
				}
				return objectMarkupInfo2.IsAttributableMarkupExtension;
			}
		}

		public override List<MarkupInfo> Decompose()
		{
			children.Add(EndMemberMarkupInfo.Instance);
			return children;
		}

		public override void FindNamespace(SerializerContext context)
		{
			XamlMember member = base.XamlNode.Member;
			if (MemberRequiresNamespaceHoisting(member))
			{
				context.FindPrefix(member.PreferredXamlNamespace);
			}
			foreach (MarkupInfo child in Children)
			{
				child.FindNamespace(context);
			}
		}

		private bool MemberRequiresNamespaceHoisting(XamlMember member)
		{
			if (member.IsAttachable || (member.IsDirective && !XamlXmlWriter.IsImplicit(member)))
			{
				return member.PreferredXamlNamespace != "http://www.w3.org/XML/1998/namespace";
			}
			return false;
		}

		public static XamlTemplateMarkupInfo ConvertToXamlReader(object propertyValue, XamlValueConverter<XamlDeferringLoader> deferringLoader, SerializerContext context)
		{
			XamlDeferringLoader converterInstance = deferringLoader.ConverterInstance;
			if (converterInstance == null)
			{
				throw new XamlObjectReaderException(SR.Get("DeferringLoaderInstanceNull", deferringLoader));
			}
			context.Instance = propertyValue;
			XamlReader xamlReader = context.Runtime.DeferredSave(context.TypeDescriptorContext, deferringLoader, propertyValue);
			context.Instance = null;
			using (xamlReader)
			{
				return new XamlTemplateMarkupInfo(xamlReader, context);
			}
		}

		public static MemberMarkupInfo ForAttachedProperty(object source, XamlMember attachedProperty, object value, SerializerContext context)
		{
			if (GetSerializationVisibility(attachedProperty) == DesignerSerializationVisibility.Hidden)
			{
				return null;
			}
			if (ShouldWriteProperty(source, attachedProperty, context))
			{
				if (context.IsPropertyWriteVisible(attachedProperty))
				{
					return new MemberMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.StartMember, attachedProperty),
						Children = { (MarkupInfo)GetPropertyValueInfo(value, attachedProperty, context) }
					};
				}
				if (attachedProperty.Type.IsDictionary)
				{
					return ForDictionary(value, attachedProperty, context, isAttachable: true);
				}
				if (attachedProperty.Type.IsCollection)
				{
					return ForSequence(value, attachedProperty, context, isAttachable: true);
				}
			}
			return null;
		}

		public static MemberMarkupInfo ForDictionaryItems(object sourceOrValue, XamlMember property, XamlType propertyType, SerializerContext context)
		{
			object obj;
			if (property != null)
			{
				obj = context.Runtime.GetValue(sourceOrValue, property);
				if (obj == null)
				{
					return null;
				}
			}
			else
			{
				obj = sourceOrValue;
			}
			XamlType keyType = propertyType.KeyType;
			MemberMarkupInfo memberMarkupInfo = new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
			};
			foreach (DictionaryEntry dictionaryItem in context.Runtime.GetDictionaryItems(obj, propertyType))
			{
				ObjectMarkupInfo objectMarkupInfo = ObjectMarkupInfo.ForObject(dictionaryItem.Value, context);
				XamlType xamlType = null;
				if (dictionaryItem.Key != null)
				{
					xamlType = context.GetXamlType(dictionaryItem.Key.GetType());
				}
				ObjectOrValueMarkupInfo item;
				if (dictionaryItem.Key != null && xamlType != keyType)
				{
					TypeConverter converterInstance = TypeConverterExtensions.GetConverterInstance(xamlType.TypeConverter);
					item = ObjectMarkupInfo.ForObject(dictionaryItem.Key, context, converterInstance);
				}
				else
				{
					ValueSerializer converterInstance2 = TypeConverterExtensions.GetConverterInstance(keyType.ValueSerializer);
					TypeConverter converterInstance3 = TypeConverterExtensions.GetConverterInstance(keyType.TypeConverter);
					item = GetPropertyValueInfoInternal(dictionaryItem.Key, converterInstance2, converterInstance3, isXamlTemplate: false, null, context);
				}
				if (!ShouldOmitKey(dictionaryItem, context))
				{
					objectMarkupInfo.Properties.Insert(0, new MemberMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Key),
						Children = { (MarkupInfo)item }
					});
				}
				memberMarkupInfo.Children.Add(objectMarkupInfo);
			}
			return memberMarkupInfo;
		}

		public static bool ShouldOmitKey(DictionaryEntry entry, SerializerContext context)
		{
			if (entry.Value != null)
			{
				XamlType xamlType = context.GetXamlType(entry.Value.GetType());
				XamlMember aliasedProperty = xamlType.GetAliasedProperty(XamlLanguage.Key);
				if (aliasedProperty != null && ObjectMarkupInfo.CanPropertyXamlRoundtrip(aliasedProperty, context))
				{
					object value = context.Runtime.GetValue(entry.Value, aliasedProperty);
					if (value == null)
					{
						return entry.Key == null;
					}
					if (value.Equals(entry.Key))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static MemberMarkupInfo ForProperty(object source, XamlMember property, SerializerContext context)
		{
			if (ShouldWriteProperty(source, property, context))
			{
				if (context.IsPropertyWriteVisible(property))
				{
					return ForReadWriteProperty(source, property, context);
				}
				if (property.Type.IsXData)
				{
					return ForXmlSerializable(source, property, context);
				}
				if (property.Type.IsDictionary)
				{
					return ForDictionary(source, property, context, isAttachable: false);
				}
				if (property.Type.IsCollection)
				{
					return ForSequence(source, property, context, isAttachable: false);
				}
			}
			return null;
		}

		private static MemberMarkupInfo ForSequence(object source, XamlMember property, SerializerContext context, bool isAttachable)
		{
			MemberMarkupInfo memberMarkupInfo = ForSequenceItems(source, isAttachable ? null : property, property.Type, context, allowReadOnly: false);
			if (memberMarkupInfo != null && memberMarkupInfo.Children.Count != 0)
			{
				return new MemberMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartMember, property),
					Children = { (MarkupInfo)new ObjectMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.GetObject),
						Properties = { (MarkupInfo)memberMarkupInfo }
					} }
				};
			}
			return null;
		}

		private static MemberMarkupInfo ForDictionary(object source, XamlMember property, SerializerContext context, bool isAttachable)
		{
			MemberMarkupInfo memberMarkupInfo = ForDictionaryItems(source, isAttachable ? null : property, property.Type, context);
			if (memberMarkupInfo != null && memberMarkupInfo.Children.Count != 0)
			{
				return new MemberMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartMember, property),
					Children = { (MarkupInfo)new ObjectMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.GetObject),
						Properties = { (MarkupInfo)memberMarkupInfo }
					} }
				};
			}
			return null;
		}

		private static MemberMarkupInfo ForXmlSerializable(object source, XamlMember property, SerializerContext context)
		{
			IXmlSerializable xmlSerializable = (IXmlSerializable)context.Runtime.GetValue(source, property);
			if (xmlSerializable == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Auto,
				Indent = true,
				OmitXmlDeclaration = true
			};
			using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings))
			{
				xmlSerializable.WriteXml(writer);
			}
			if (stringBuilder.Length > 0)
			{
				return new MemberMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartMember, property),
					Children = { (MarkupInfo)new ObjectMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.XData),
						Properties = { (MarkupInfo)new MemberMarkupInfo
						{
							XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.XData.GetMember("Text")),
							Children = { (MarkupInfo)new ValueMarkupInfo
							{
								XamlNode = new XamlNode(XamlNodeType.Value, stringBuilder.ToString())
							} }
						} }
					} }
				};
			}
			return null;
		}

		private static MemberMarkupInfo ForReadWriteProperty(object source, XamlMember xamlProperty, SerializerContext context)
		{
			object value = context.Runtime.GetValue(source, xamlProperty);
			XamlType declaringType = xamlProperty.DeclaringType;
			MemberMarkupInfo memberMarkupInfo = ((!(xamlProperty == declaringType.GetAliasedProperty(XamlLanguage.Lang)) || !(value is string)) ? new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, xamlProperty),
				Children = { (MarkupInfo)GetPropertyValueInfo(value, xamlProperty, context) }
			} : new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Lang),
				Children = { (MarkupInfo)GetPropertyValueInfo(value, xamlProperty, context) }
			});
			RemoveObjectNodesForCollectionOrDictionary(memberMarkupInfo);
			return memberMarkupInfo;
		}

		private static void RemoveObjectNodesForCollectionOrDictionary(MemberMarkupInfo memberInfo)
		{
			XamlType type = memberInfo.XamlNode.Member.Type;
			if ((type.IsCollection || type.IsDictionary) && memberInfo.Children.Count == 1 && memberInfo.Children[0] is ObjectMarkupInfo objectMarkupInfo && objectMarkupInfo.Properties.Count == 1 && type == objectMarkupInfo.XamlNode.XamlType && objectMarkupInfo.Properties[0].XamlNode.Member == XamlLanguage.Items && objectMarkupInfo.Properties[0] is MemberMarkupInfo memberMarkupInfo && memberMarkupInfo.Children.Count > 0 && (!(memberMarkupInfo.Children[0] is ObjectMarkupInfo { XamlNode: var xamlNode } objectMarkupInfo2) || xamlNode.XamlType == null || !objectMarkupInfo2.XamlNode.XamlType.IsMarkupExtension))
			{
				objectMarkupInfo.XamlNode = new XamlNode(XamlNodeType.GetObject);
			}
		}

		public static MemberMarkupInfo ForSequenceItems(object sourceOrValue, XamlMember property, XamlType xamlType, SerializerContext context, bool allowReadOnly)
		{
			object obj;
			if (property != null)
			{
				obj = context.Runtime.GetValue(sourceOrValue, property);
				if (obj == null)
				{
					return null;
				}
			}
			else
			{
				obj = sourceOrValue;
			}
			if (!allowReadOnly && xamlType.IsReadOnlyMethod != null && (bool)xamlType.IsReadOnlyMethod.Invoke(obj, null))
			{
				return null;
			}
			MemberMarkupInfo memberMarkupInfo = new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
			};
			bool flag = false;
			IList<object> collectionItems = context.Runtime.GetCollectionItems(obj, xamlType);
			for (int i = 0; i < collectionItems.Count; i++)
			{
				object value = collectionItems[i];
				ObjectMarkupInfo objectMarkupInfo = ObjectMarkupInfo.ForObject(value, context);
				ObjectOrValueMarkupInfo objectOrValueMarkupInfo = null;
				if (xamlType.ContentWrappers != null && objectMarkupInfo.Properties != null && objectMarkupInfo.Properties.Count == 1)
				{
					MemberMarkupInfo memberMarkupInfo2 = (MemberMarkupInfo)objectMarkupInfo.Properties[0];
					if (memberMarkupInfo2.XamlNode.Member == objectMarkupInfo.XamlNode.XamlType.ContentProperty)
					{
						foreach (XamlType contentWrapper in xamlType.ContentWrappers)
						{
							if (contentWrapper == objectMarkupInfo.XamlNode.XamlType && memberMarkupInfo2.Children.Count == 1)
							{
								ObjectOrValueMarkupInfo objectOrValueMarkupInfo2 = (ObjectOrValueMarkupInfo)memberMarkupInfo2.Children[0];
								if (!(objectOrValueMarkupInfo2 is ValueMarkupInfo))
								{
									objectOrValueMarkupInfo = objectOrValueMarkupInfo2;
									break;
								}
								bool isFirstElementOfCollection = i == 0;
								bool isLastElementOfCollection = i == collectionItems.Count - 1;
								if (!flag && !ShouldUnwrapDueToWhitespace((string)objectOrValueMarkupInfo2.XamlNode.Value, xamlType, isFirstElementOfCollection, isLastElementOfCollection))
								{
									objectOrValueMarkupInfo = objectOrValueMarkupInfo2;
									flag = true;
									break;
								}
							}
						}
					}
				}
				if (objectOrValueMarkupInfo == null || !(objectOrValueMarkupInfo is ValueMarkupInfo))
				{
					flag = false;
				}
				memberMarkupInfo.Children.Add(objectOrValueMarkupInfo ?? objectMarkupInfo);
			}
			return memberMarkupInfo;
		}

		private static bool ShouldUnwrapDueToWhitespace(string value, XamlType xamlType, bool isFirstElementOfCollection, bool isLastElementOfCollection)
		{
			if (XamlXmlWriter.HasSignificantWhitespace(value))
			{
				if (xamlType.IsWhitespaceSignificantCollection)
				{
					if (XamlXmlWriter.ContainsConsecutiveInnerSpaces(value) || XamlXmlWriter.ContainsWhitespaceThatIsNotSpace(value))
					{
						return true;
					}
					if (XamlXmlWriter.ContainsTrailingSpace(value) && isLastElementOfCollection)
					{
						return true;
					}
					if (XamlXmlWriter.ContainsLeadingSpace(value) && isFirstElementOfCollection)
					{
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		private static ObjectOrValueMarkupInfo GetPropertyValueInfo(object propertyValue, XamlMember xamlProperty, SerializerContext context)
		{
			return GetPropertyValueInfoInternal(propertyValue, TypeConverterExtensions.GetConverterInstance(xamlProperty.ValueSerializer), TypeConverterExtensions.GetConverterInstance(xamlProperty.TypeConverter), xamlProperty != null && xamlProperty.DeferringLoader != null, xamlProperty, context);
		}

		private static ObjectOrValueMarkupInfo GetPropertyValueInfoInternal(object propertyValue, ValueSerializer propertyValueSerializer, TypeConverter propertyConverter, bool isXamlTemplate, XamlMember xamlProperty, SerializerContext context)
		{
			context.Instance = propertyValue;
			if (isXamlTemplate && propertyValue != null)
			{
				return ConvertToXamlReader(propertyValue, xamlProperty.DeferringLoader, context);
			}
			if (context.TryValueSerializeToString(propertyValueSerializer, propertyConverter, context, ref propertyValue))
			{
				ThrowIfPropertiesAreAttached(context.Instance, xamlProperty, context);
				context.Instance = null;
				return new ValueMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.Value, propertyValue)
				};
			}
			if (propertyConverter != null && context.TryConvertToMarkupExtension(propertyConverter, ref propertyValue))
			{
				context.Instance = null;
				return ObjectMarkupInfo.ForObject(propertyValue, context);
			}
			if (propertyConverter != null && context.TryTypeConvertToString(propertyConverter, ref propertyValue))
			{
				ThrowIfPropertiesAreAttached(context.Instance, xamlProperty, context);
				context.Instance = null;
				return new ValueMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.Value, propertyValue ?? string.Empty)
				};
			}
			if (propertyValue is string)
			{
				ThrowIfPropertiesAreAttached(propertyValue, xamlProperty, context);
				context.Instance = null;
				return new ValueMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.Value, propertyValue)
				};
			}
			context.Instance = null;
			return ObjectMarkupInfo.ForObject(propertyValue, context, propertyConverter);
		}

		private static void ThrowIfPropertiesAreAttached(object value, XamlMember property, SerializerContext context)
		{
			KeyValuePair<AttachableMemberIdentifier, object>[] attachedProperties = context.Runtime.GetAttachedProperties(value);
			if (attachedProperties != null)
			{
				if (property != null)
				{
					throw new InvalidOperationException(SR.Get("AttachedPropertyOnTypeConvertedOrStringProperty", property.Name, value.ToString(), attachedProperties[0].Key.ToString()));
				}
				throw new InvalidOperationException(SR.Get("AttachedPropertyOnDictionaryKey", value.ToString(), attachedProperties[0].Key.ToString()));
			}
		}

		private static bool ShouldWriteProperty(object source, XamlMember property, SerializerContext context)
		{
			bool flag = !context.IsPropertyWriteVisible(property);
			if (!flag && GetDefaultValue(property, out var value))
			{
				object value2 = context.Runtime.GetValue(source, property);
				return !object.Equals(value, value2);
			}
			ShouldSerializeResult shouldSerializeResult = context.Runtime.ShouldSerialize(property, source);
			if (shouldSerializeResult != ShouldSerializeResult.Default)
			{
				return shouldSerializeResult == ShouldSerializeResult.True;
			}
			if (!flag)
			{
				return true;
			}
			if (!context.Settings.RequireExplicitContentVisibility)
			{
				return true;
			}
			return GetSerializationVisibility(property) == DesignerSerializationVisibility.Content;
		}
	}

	private abstract class MarkupInfo
	{
		public XamlNode XamlNode { get; set; }

		public virtual void FindNamespace(SerializerContext context)
		{
		}

		public virtual List<MarkupInfo> Decompose()
		{
			return null;
		}
	}

	private class EndObjectMarkupInfo : MarkupInfo
	{
		private static EndObjectMarkupInfo instance = new EndObjectMarkupInfo();

		public static EndObjectMarkupInfo Instance => instance;

		private EndObjectMarkupInfo()
		{
			base.XamlNode = new XamlNode(XamlNodeType.EndObject);
		}
	}

	private class EndMemberMarkupInfo : MarkupInfo
	{
		private static EndMemberMarkupInfo instance = new EndMemberMarkupInfo();

		public static EndMemberMarkupInfo Instance => instance;

		private EndMemberMarkupInfo()
		{
			base.XamlNode = new XamlNode(XamlNodeType.EndMember);
		}
	}

	private class NamespaceMarkupInfo : MarkupInfo
	{
	}

	private abstract class ObjectOrValueMarkupInfo : MarkupInfo
	{
		public virtual void EnsureNoDuplicateNames(Stack<HashSet<string>> namesInCurrentScope)
		{
		}
	}

	private class ObjectMarkupInfo : ObjectOrValueMarkupInfo
	{
		private class PropertySorterForXmlSyntax : IComparer<MarkupInfo>
		{
			private const int Equal = 0;

			private const int XFirst = -1;

			private const int YFirst = 1;

			public static readonly PropertySorterForXmlSyntax Instance = new PropertySorterForXmlSyntax();

			public int Compare(MarkupInfo x, MarkupInfo y)
			{
				MemberMarkupInfo memberMarkupInfo = (MemberMarkupInfo)x;
				MemberMarkupInfo memberMarkupInfo2 = (MemberMarkupInfo)y;
				XamlMember member = x.XamlNode.Member;
				XamlMember member2 = y.XamlNode.Member;
				bool isFactoryMethod = memberMarkupInfo.IsFactoryMethod;
				bool isFactoryMethod2 = memberMarkupInfo2.IsFactoryMethod;
				if (isFactoryMethod && !isFactoryMethod2)
				{
					return -1;
				}
				if (isFactoryMethod2 && !isFactoryMethod)
				{
					return 1;
				}
				bool flag = memberMarkupInfo.IsContent || member == XamlLanguage.Items;
				bool flag2 = memberMarkupInfo2.IsContent || member2 == XamlLanguage.Items;
				if (flag && !flag2)
				{
					return 1;
				}
				if (flag2 && !flag)
				{
					return -1;
				}
				bool isAttributableMarkupExtension = memberMarkupInfo.IsAttributableMarkupExtension;
				bool isAttributableMarkupExtension2 = memberMarkupInfo2.IsAttributableMarkupExtension;
				if (isAttributableMarkupExtension && !isAttributableMarkupExtension2)
				{
					return -1;
				}
				if (isAttributableMarkupExtension2 && !isAttributableMarkupExtension)
				{
					return 1;
				}
				bool isAtomic = memberMarkupInfo.IsAtomic;
				bool isAtomic2 = memberMarkupInfo2.IsAtomic;
				bool flag3 = member == XamlLanguage.Initialization;
				bool flag4 = member2 == XamlLanguage.Initialization;
				bool flag5 = isAtomic && !flag3;
				bool flag6 = isAtomic2 && !flag4;
				if (flag5 && !flag6)
				{
					return -1;
				}
				if (flag6 && !flag5)
				{
					return 1;
				}
				if (isAtomic && !isAtomic2)
				{
					return -1;
				}
				if (isAtomic2 && !isAtomic)
				{
					return 1;
				}
				if (flag3 && !flag4)
				{
					return -1;
				}
				if (flag4 && !flag3)
				{
					return 1;
				}
				bool flag7 = member == XamlLanguage.Arguments;
				bool flag8 = member2 == XamlLanguage.Arguments;
				if (flag7 && !flag8)
				{
					return -1;
				}
				if (flag8 && !flag7)
				{
					return 1;
				}
				bool isDirective = member.IsDirective;
				bool isDirective2 = member2.IsDirective;
				if (isDirective && !isDirective2)
				{
					return -1;
				}
				if (isDirective2 && !isDirective)
				{
					return 1;
				}
				return string.CompareOrdinal(member.Name, member2.Name);
			}
		}

		private class PropertySorterForCurlySyntax : IComparer<MarkupInfo>
		{
			private const int Equal = 0;

			private const int XFirst = -1;

			private const int YFirst = 1;

			public static readonly PropertySorterForCurlySyntax Instance = new PropertySorterForCurlySyntax();

			public int Compare(MarkupInfo x, MarkupInfo y)
			{
				MemberMarkupInfo memberMarkupInfo = (MemberMarkupInfo)x;
				MemberMarkupInfo memberMarkupInfo2 = (MemberMarkupInfo)y;
				XamlMember member = x.XamlNode.Member;
				XamlMember member2 = y.XamlNode.Member;
				bool flag = member == XamlLanguage.PositionalParameters;
				bool flag2 = member2 == XamlLanguage.PositionalParameters;
				if (flag && !flag2)
				{
					return -1;
				}
				if (flag2 && !flag)
				{
					return 1;
				}
				bool isFactoryMethod = memberMarkupInfo.IsFactoryMethod;
				bool isFactoryMethod2 = memberMarkupInfo2.IsFactoryMethod;
				if (isFactoryMethod && !isFactoryMethod2)
				{
					return -1;
				}
				if (isFactoryMethod2 && !isFactoryMethod)
				{
					return 1;
				}
				bool isAttributableMarkupExtension = memberMarkupInfo.IsAttributableMarkupExtension;
				bool isAttributableMarkupExtension2 = memberMarkupInfo2.IsAttributableMarkupExtension;
				if (isAttributableMarkupExtension && !isAttributableMarkupExtension2)
				{
					return -1;
				}
				if (isAttributableMarkupExtension2 && !isAttributableMarkupExtension)
				{
					return 1;
				}
				bool isAtomic = memberMarkupInfo.IsAtomic;
				bool isAtomic2 = memberMarkupInfo2.IsAtomic;
				if (isAtomic && !isAtomic2)
				{
					return -1;
				}
				if (isAtomic2 && !isAtomic)
				{
					return 1;
				}
				bool isDirective = member.IsDirective;
				bool isDirective2 = member2.IsDirective;
				if (isDirective && !isDirective2)
				{
					return -1;
				}
				if (isDirective2 && !isDirective2)
				{
					return 1;
				}
				return string.CompareOrdinal(member.Name, member2.Name);
			}
		}

		private List<MarkupInfo> properties = new List<MarkupInfo>();

		private bool? isAttributableMarkupExtension;

		public List<MarkupInfo> Properties => properties;

		public string Name { get; set; }

		public object Object { get; set; }

		public virtual bool IsAttributableMarkupExtension
		{
			get
			{
				if (isAttributableMarkupExtension.HasValue)
				{
					return isAttributableMarkupExtension.Value;
				}
				if ((base.XamlNode.NodeType == XamlNodeType.StartObject && !base.XamlNode.XamlType.IsMarkupExtension) || base.XamlNode.NodeType == XamlNodeType.GetObject)
				{
					isAttributableMarkupExtension = false;
					return false;
				}
				foreach (MarkupInfo property in Properties)
				{
					if (!((MemberMarkupInfo)property).IsAttributable)
					{
						isAttributableMarkupExtension = false;
						return false;
					}
				}
				isAttributableMarkupExtension = true;
				return true;
			}
		}

		public override List<MarkupInfo> Decompose()
		{
			SortProperties();
			Properties.Add(EndObjectMarkupInfo.Instance);
			return properties;
		}

		private void SortProperties()
		{
			if (IsAttributableMarkupExtension)
			{
				Properties.Sort(PropertySorterForCurlySyntax.Instance);
			}
			else
			{
				Properties.Sort(PropertySorterForXmlSyntax.Instance);
			}
			ReorderPropertiesWithDO();
		}

		private void ReorderPropertiesWithDO()
		{
			SelectAndRemovePropertiesWithDO(out var removedProperties);
			if (removedProperties != null)
			{
				InsertPropertiesWithDO(removedProperties);
			}
		}

		private void InsertPropertiesWithDO(List<MarkupInfo> propertiesWithDO)
		{
			int posOfFirstNonAttributableProperty;
			HashSet<string> hashSet = FindAllAttributableProperties(out posOfFirstNonAttributableProperty);
			foreach (MarkupInfo item in propertiesWithDO)
			{
				MemberMarkupInfo memberMarkupInfo = (MemberMarkupInfo)item;
				if (IsMemberOnlyDependentOnAttributableMembers(memberMarkupInfo.XamlNode.Member, hashSet) && (memberMarkupInfo.IsAtomic || memberMarkupInfo.IsAttributableMarkupExtension))
				{
					properties.Insert(posOfFirstNonAttributableProperty, item);
					hashSet.Add(memberMarkupInfo.XamlNode.Member.Name);
					posOfFirstNonAttributableProperty++;
				}
				else
				{
					Properties.Add(item);
				}
			}
		}

		private bool IsMemberOnlyDependentOnAttributableMembers(XamlMember member, HashSet<string> namesOfAttributableProperties)
		{
			foreach (XamlMember item in member.DependsOn)
			{
				if (!namesOfAttributableProperties.Contains(item.Name))
				{
					return false;
				}
			}
			return true;
		}

		private HashSet<string> FindAllAttributableProperties(out int posOfFirstNonAttributableProperty)
		{
			HashSet<string> hashSet = new HashSet<string>();
			int i;
			for (i = 0; i < Properties.Count; i++)
			{
				MemberMarkupInfo memberMarkupInfo = (MemberMarkupInfo)Properties[i];
				if (!memberMarkupInfo.IsAtomic && !memberMarkupInfo.IsAttributableMarkupExtension)
				{
					break;
				}
				hashSet.Add(memberMarkupInfo.XamlNode.Member.Name);
			}
			posOfFirstNonAttributableProperty = i;
			return hashSet;
		}

		private void SelectAndRemovePropertiesWithDO(out List<MarkupInfo> removedProperties)
		{
			removedProperties = null;
			PartiallyOrderedList<string, MarkupInfo> partiallyOrderedList = null;
			int num = 0;
			while (num < properties.Count)
			{
				MarkupInfo markupInfo = properties[num];
				if (markupInfo.XamlNode.Member.DependsOn.Count > 0)
				{
					if (partiallyOrderedList == null)
					{
						partiallyOrderedList = new PartiallyOrderedList<string, MarkupInfo>();
					}
					string name = markupInfo.XamlNode.Member.Name;
					partiallyOrderedList.Add(name, markupInfo);
					foreach (XamlMember item in markupInfo.XamlNode.Member.DependsOn)
					{
						partiallyOrderedList.SetOrder(item.Name, name);
					}
					properties.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			if (partiallyOrderedList != null)
			{
				removedProperties = new List<MarkupInfo>(partiallyOrderedList);
			}
		}

		public override void FindNamespace(SerializerContext context)
		{
			if (base.XamlNode.NodeType == XamlNodeType.StartObject)
			{
				context.FindPrefix(base.XamlNode.XamlType.PreferredXamlNamespace);
				XamlType xamlType = base.XamlNode.XamlType;
				if (xamlType.IsGeneric)
				{
					context.FindPrefix(XamlLanguage.TypeArguments.PreferredXamlNamespace);
					FindNamespaceForTypeArguments(xamlType.TypeArguments, context);
				}
			}
			foreach (MarkupInfo property in Properties)
			{
				property.FindNamespace(context);
			}
		}

		private void FindNamespaceForTypeArguments(IList<XamlType> types, SerializerContext context)
		{
			if (types == null || types.Count == 0)
			{
				return;
			}
			foreach (XamlType type in types)
			{
				context.FindPrefix(type.PreferredXamlNamespace);
				FindNamespaceForTypeArguments(type.TypeArguments, context);
			}
		}

		private void AddItemsProperty(object value, SerializerContext context, XamlType xamlType)
		{
			MemberMarkupInfo memberMarkupInfo = null;
			if (xamlType.IsDictionary)
			{
				memberMarkupInfo = MemberMarkupInfo.ForDictionaryItems(value, null, xamlType, context);
			}
			else if (xamlType.IsCollection)
			{
				memberMarkupInfo = MemberMarkupInfo.ForSequenceItems(value, null, xamlType, context, allowReadOnly: true);
			}
			if (memberMarkupInfo != null && memberMarkupInfo.Children.Count != 0)
			{
				properties.Add(memberMarkupInfo);
			}
		}

		private ParameterInfo[] GetMethodParams(MemberInfo memberInfo)
		{
			ParameterInfo[] result = null;
			MethodBase methodBase = memberInfo as MethodBase;
			if (methodBase != null)
			{
				result = methodBase.GetParameters();
			}
			return result;
		}

		private void AddFactoryMethodAndValidateArguments(Type valueType, MemberInfo memberInfo, ICollection arguments, SerializerContext context, out ParameterInfo[] methodParams)
		{
			methodParams = null;
			if (memberInfo == null)
			{
				methodParams = new ParameterInfo[0];
			}
			else if (memberInfo is ConstructorInfo)
			{
				ConstructorInfo constructorInfo = (ConstructorInfo)memberInfo;
				methodParams = constructorInfo.GetParameters();
			}
			else
			{
				if (!(memberInfo is MethodInfo))
				{
					if (valueType.IsValueType)
					{
						if (arguments != null && arguments.Count > 0)
						{
							throw new XamlObjectReaderException(SR.Get("ObjectReaderInstanceDescriptorIncompatibleArguments"));
						}
						return;
					}
					throw new XamlObjectReaderException(SR.Get("ObjectReaderInstanceDescriptorInvalidMethod"));
				}
				MethodInfo methodInfo = (MethodInfo)memberInfo;
				methodParams = methodInfo.GetParameters();
				string text = memberInfo.Name;
				Type declaringType = memberInfo.DeclaringType;
				if (declaringType != valueType)
				{
					text = ConvertTypeAndMethodToString(declaringType, text, context);
				}
				Properties.Add(new MemberMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.FactoryMethod),
					IsFactoryMethod = true,
					Children = { (MarkupInfo)new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.Value, text)
					} }
				});
			}
			if (arguments == null)
			{
				return;
			}
			if (arguments.Count != methodParams.Length)
			{
				throw new XamlObjectReaderException(SR.Get("ObjectReaderInstanceDescriptorIncompatibleArguments"));
			}
			int num = 0;
			foreach (object argument in arguments)
			{
				ParameterInfo parameterInfo = methodParams[num++];
				if (argument == null)
				{
					if (parameterInfo.ParameterType.IsValueType && (!parameterInfo.ParameterType.IsGenericType || !(parameterInfo.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))))
					{
						throw new XamlObjectReaderException(SR.Get("ObjectReaderInstanceDescriptorIncompatibleArgumentTypes", "null", parameterInfo.ParameterType));
					}
				}
				else if (!parameterInfo.ParameterType.IsAssignableFrom(argument.GetType()))
				{
					throw new XamlObjectReaderException(SR.Get("ObjectReaderInstanceDescriptorIncompatibleArgumentTypes", argument.GetType(), parameterInfo.ParameterType));
				}
			}
		}

		private void AddArgumentsMembers(ICollection arguments, SerializerContext context)
		{
			if (arguments == null || arguments.Count <= 0)
			{
				return;
			}
			MemberMarkupInfo memberMarkupInfo = new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
			};
			MemberMarkupInfo memberMarkupInfo2 = new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Arguments)
			};
			foreach (object argument in arguments)
			{
				memberMarkupInfo2.Children.Add(ForObject(argument, context));
			}
			Properties.Add(memberMarkupInfo2);
		}

		private bool TryAddPositionalParameters(XamlType xamlType, MemberInfo member, ICollection arguments, SerializerContext context)
		{
			if (arguments != null && arguments.Count > 0)
			{
				ParameterInfo[] methodParams = GetMethodParams(member);
				MemberMarkupInfo memberMarkupInfo = new MemberMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.PositionalParameters)
				};
				int num = 0;
				foreach (object argument in arguments)
				{
					XamlType xamlType2 = context.GetXamlType(methodParams[num++].ParameterType);
					ValueSerializer converterInstance = TypeConverterExtensions.GetConverterInstance(xamlType2.ValueSerializer);
					TypeConverter converterInstance2 = TypeConverterExtensions.GetConverterInstance(xamlType2.TypeConverter);
					ObjectMarkupInfo objectMarkupInfo = null;
					object value = argument;
					context.Instance = argument;
					if (converterInstance2 != null && converterInstance != null && context.CanRoundtripUsingValueSerializer(converterInstance, converterInstance2, argument))
					{
						string data = context.ConvertToString(converterInstance, argument);
						context.Instance = null;
						memberMarkupInfo.Children.Add(new ValueMarkupInfo
						{
							XamlNode = new XamlNode(XamlNodeType.Value, data)
						});
						continue;
					}
					if ((converterInstance2 != null && context.TryConvertToMarkupExtension(converterInstance2, ref value)) || value is MarkupExtension)
					{
						context.Instance = null;
						objectMarkupInfo = ForObject(value, context);
						if (!objectMarkupInfo.IsAttributableMarkupExtension)
						{
							return false;
						}
						memberMarkupInfo.Children.Add(objectMarkupInfo);
						continue;
					}
					if (converterInstance2 != null && context.CanRoundTripString(converterInstance2))
					{
						string data2 = context.ConvertTo<string>(converterInstance2, argument);
						context.Instance = null;
						memberMarkupInfo.Children.Add(new ValueMarkupInfo
						{
							XamlNode = new XamlNode(XamlNodeType.Value, data2)
						});
						continue;
					}
					if (argument is string)
					{
						context.Instance = null;
						memberMarkupInfo.Children.Add(new ValueMarkupInfo
						{
							XamlNode = new XamlNode(XamlNodeType.Value, argument)
						});
						continue;
					}
					context.Instance = null;
					return false;
				}
				Properties.Add(memberMarkupInfo);
				return true;
			}
			return false;
		}

		protected void AddRecordMembers(object value, SerializerContext context)
		{
			AddRecordMembers(value, context, null);
		}

		private bool TryGetInstanceDescriptorInfo(object value, SerializerContext context, TypeConverter converter, out MemberInfo member, out ICollection arguments, out bool isComplete)
		{
			bool result = false;
			member = null;
			arguments = null;
			isComplete = false;
			context.Instance = value;
			if (converter != null && context.CanConvertTo(converter, typeof(InstanceDescriptor)))
			{
				ConvertToInstanceDescriptor(context, value, converter, out member, out arguments, out isComplete);
				result = true;
			}
			return result;
		}

		[SecuritySafeCritical]
		[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
		private void ConvertToInstanceDescriptor(SerializerContext context, object instance, TypeConverter converter, out MemberInfo member, out ICollection arguments, out bool isComplete)
		{
			InstanceDescriptor instanceDescriptor = context.ConvertTo<InstanceDescriptor>(converter, instance);
			context.Instance = null;
			member = instanceDescriptor.MemberInfo;
			arguments = instanceDescriptor.Arguments;
			isComplete = instanceDescriptor.IsComplete;
		}

		private bool TryGetDefaultConstructorInfo(XamlType type, out MemberInfo member, out ICollection arguments, out bool isComplete)
		{
			arguments = null;
			isComplete = false;
			member = null;
			if (type.IsConstructible)
			{
				return !type.ConstructionRequiresArguments;
			}
			return false;
		}

		protected void AddRecordMembers(object value, SerializerContext context, TypeConverter converter)
		{
			Type type = value.GetType();
			XamlType xamlType = context.GetXamlType(type);
			context.Instance = value;
			if (converter == null || !context.CanConvertTo(converter, typeof(InstanceDescriptor)))
			{
				context.Instance = null;
				converter = TypeConverterExtensions.GetConverterInstance(xamlType.TypeConverter);
			}
			AddRecordConstructionMembers(value, xamlType, context, converter, out var isComplete, out var methodParams);
			if (!isComplete || xamlType.GetAliasedProperty(XamlLanguage.Name) != null || context.Runtime.AttachedPropertyCount(value) > 0)
			{
				AddRecordMembers(value, context, methodParams, xamlType);
			}
		}

		private void AddRecordMembers(object value, SerializerContext context, ParameterInfo[] methodParameters, XamlType xamlType)
		{
			List<XamlMember> xamlSerializableProperties = GetXamlSerializableProperties(xamlType, context);
			foreach (XamlMember item in xamlSerializableProperties)
			{
				if (GetSerializationVisibility(item) == DesignerSerializationVisibility.Hidden || PropertyUsedInMethodSignature(item, methodParameters))
				{
					continue;
				}
				MemberMarkupInfo memberMarkupInfo = MemberMarkupInfo.ForProperty(value, item, context);
				if (memberMarkupInfo == null)
				{
					continue;
				}
				if (item == xamlType.GetAliasedProperty(XamlLanguage.Name))
				{
					if (IsNull(memberMarkupInfo, context) || IsEmptyString(memberMarkupInfo))
					{
						continue;
					}
					Name = ValidateNamePropertyAndFindName(memberMarkupInfo);
				}
				memberMarkupInfo.IsContent = IsPropertyContent(memberMarkupInfo, xamlType);
				Properties.Add(memberMarkupInfo);
			}
			AddItemsProperty(value, context, xamlType);
			AddAttachedProperties(value, this, context);
		}

		private void AddRecordConstructionMembers(object value, XamlType valueXamlType, SerializerContext context, TypeConverter converter, out bool isComplete, out ParameterInfo[] methodParams)
		{
			MemberInfo member = null;
			ICollection arguments = null;
			isComplete = false;
			if (valueXamlType.IsMarkupExtension)
			{
				if (!TryGetInstanceDescriptorInfo(value, context, converter, out member, out arguments, out isComplete))
				{
					if (!TryGetDefaultConstructorInfo(valueXamlType, out member, out arguments, out isComplete))
					{
						GetConstructorInfo(value, valueXamlType, context, out member, out arguments, out isComplete);
						if (!TryAddPositionalParameters(valueXamlType, member, arguments, context))
						{
							AddArgumentsMembers(arguments, context);
						}
					}
				}
				else if (!TryAddPositionalParameters(valueXamlType, member, arguments, context))
				{
					MemberInfo memberInfo = member;
					ICollection collection = arguments;
					bool flag = isComplete;
					if (!TryGetDefaultConstructorInfo(valueXamlType, out member, out arguments, out isComplete))
					{
						member = memberInfo;
						arguments = collection;
						isComplete = flag;
						AddArgumentsMembers(arguments, context);
					}
				}
			}
			else if (!TryGetDefaultConstructorInfo(valueXamlType, out member, out arguments, out isComplete))
			{
				if (!TryGetInstanceDescriptorInfo(value, context, converter, out member, out arguments, out isComplete))
				{
					GetConstructorInfo(value, valueXamlType, context, out member, out arguments, out isComplete);
				}
				AddArgumentsMembers(arguments, context);
			}
			AddFactoryMethodAndValidateArguments(value.GetType(), member, arguments, context, out methodParams);
		}

		private bool IsPropertyContent(MemberMarkupInfo propertyInfo, XamlType containingType)
		{
			XamlMember member = propertyInfo.XamlNode.Member;
			if (member != containingType.ContentProperty)
			{
				return false;
			}
			if (propertyInfo.IsAtomic)
			{
				return XamlLanguage.String.CanAssignTo(member.Type);
			}
			return true;
		}

		private void GetConstructorInfo(object value, XamlType valueXamlType, SerializerContext context, out MemberInfo member, out ICollection arguments, out bool isComplete)
		{
			member = null;
			arguments = null;
			isComplete = false;
			ICollection<XamlMember> allMembers = valueXamlType.GetAllMembers();
			ICollection<XamlMember> allExcludedReadOnlyMembers = valueXamlType.GetAllExcludedReadOnlyMembers();
			List<XamlMember> list = new List<XamlMember>();
			foreach (XamlMember item in allMembers)
			{
				if (context.IsPropertyReadVisible(item) && !string.IsNullOrEmpty(GetConstructorArgument(item)))
				{
					list.Add(item);
				}
			}
			foreach (XamlMember item2 in allExcludedReadOnlyMembers)
			{
				if (context.IsPropertyReadVisible(item2) && !string.IsNullOrEmpty(GetConstructorArgument(item2)))
				{
					list.Add(item2);
				}
			}
			foreach (ConstructorInfo constructor in valueXamlType.GetConstructors())
			{
				ParameterInfo[] parameters = constructor.GetParameters();
				if (parameters.Length != list.Count)
				{
					continue;
				}
				IList list2 = new List<object>(parameters.Length);
				foreach (ParameterInfo parameterInfo in parameters)
				{
					XamlMember xamlMember = null;
					foreach (XamlMember item3 in list)
					{
						if (item3.Type.UnderlyingType == parameterInfo.ParameterType && GetConstructorArgument(item3) == parameterInfo.Name)
						{
							xamlMember = item3;
							break;
						}
					}
					if (xamlMember == null)
					{
						break;
					}
					list2.Add(context.Runtime.GetValue(value, xamlMember));
				}
				if (list2.Count == list.Count)
				{
					member = constructor;
					arguments = list2;
					if (list2.Count == allMembers.Count && !valueXamlType.IsCollection && !valueXamlType.IsDictionary)
					{
						isComplete = true;
					}
					break;
				}
			}
			if (member == null && !valueXamlType.UnderlyingType.IsValueType)
			{
				if (list.Count == 0)
				{
					throw new XamlObjectReaderException(SR.Get("ObjectReaderNoDefaultConstructor", value.GetType()));
				}
				throw new XamlObjectReaderException(SR.Get("ObjectReaderNoMatchingConstructor", value.GetType()));
			}
		}

		private static void CheckTypeCanRoundtrip(ObjectMarkupInfo objInfo)
		{
			XamlType xamlType = objInfo.XamlNode.XamlType;
			if (xamlType.IsConstructible)
			{
				return;
			}
			foreach (MarkupInfo property in objInfo.Properties)
			{
				if (((MemberMarkupInfo)property).IsFactoryMethod && !xamlType.UnderlyingType.IsNested)
				{
					return;
				}
			}
			if (xamlType.UnderlyingType.IsNested)
			{
				throw new XamlObjectReaderException(SR.Get("ObjectReaderTypeIsNested", xamlType.Name));
			}
			throw new XamlObjectReaderException(SR.Get("ObjectReaderTypeCannotRoundtrip", xamlType.Name));
		}

		public void AssignName(SerializerContext context)
		{
			if (Name == null)
			{
				Name = context.AllocateIdentifier();
				AddNameProperty(context);
			}
		}

		public void AssignName(string name, SerializerContext context)
		{
			if (Name == null)
			{
				Name = name;
				if (name.StartsWith("__ReferenceID", StringComparison.Ordinal))
				{
					AddNameProperty(context);
				}
			}
		}

		public void AddNameProperty(SerializerContext context)
		{
			Properties.Add(new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Name),
				Children = { (MarkupInfo)new ValueMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.Value, Name)
				} }
			});
			if (base.XamlNode.NodeType == XamlNodeType.GetObject)
			{
				XamlType data = context.LocalAssemblyAwareGetXamlType(Object.GetType());
				base.XamlNode = new XamlNode(XamlNodeType.StartObject, data);
			}
		}

		public override void EnsureNoDuplicateNames(Stack<HashSet<string>> namesInCurrentScope)
		{
			if (!string.IsNullOrEmpty(Name) && !namesInCurrentScope.Peek().Add(Name))
			{
				throw new XamlObjectReaderException(SR.Get("ObjectReaderXamlNamedElementAlreadyRegistered", Name));
			}
			foreach (MarkupInfo property in Properties)
			{
				MemberMarkupInfo memberMarkupInfo = (MemberMarkupInfo)property;
				foreach (MarkupInfo child in memberMarkupInfo.Children)
				{
					((ObjectOrValueMarkupInfo)child).EnsureNoDuplicateNames(namesInCurrentScope);
				}
			}
		}

		private static string ConvertTypeAndMethodToString(Type type, string methodName, SerializerContext context)
		{
			string text = context.ConvertXamlTypeToString(context.LocalAssemblyAwareGetXamlType(type));
			return text + "." + methodName;
		}

		private static ObjectMarkupInfo ForArray(Array value, SerializerContext context)
		{
			if (value.Rank > 1)
			{
				throw new XamlObjectReaderException(SR.Get("ObjectReaderMultidimensionalArrayNotSupported"));
			}
			XamlType xamlType = context.LocalAssemblyAwareGetXamlType(value.GetType());
			XamlType itemType = xamlType.ItemType;
			MemberMarkupInfo memberMarkupInfo = new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
			};
			foreach (object item3 in value)
			{
				memberMarkupInfo.Children.Add(ForObject(item3, context));
			}
			ObjectMarkupInfo objectMarkupInfo = new ObjectMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.Array),
				Object = value,
				Properties = { (MarkupInfo)new MemberMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Array.GetMember("Type")),
					Children = { (MarkupInfo)new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.Value, context.ConvertXamlTypeToString(itemType))
					} }
				} }
			};
			if (memberMarkupInfo.Children.Count != 0)
			{
				ObjectMarkupInfo item = new ObjectMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.GetObject),
					Properties = { (MarkupInfo)memberMarkupInfo }
				};
				MemberMarkupInfo item2 = new MemberMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Array.ContentProperty),
					Children = { (MarkupInfo)item }
				};
				objectMarkupInfo.Properties.Add(item2);
			}
			AddAttachedProperties(value, objectMarkupInfo, context);
			return objectMarkupInfo;
		}

		private static void AddAttachedProperties(object value, ObjectMarkupInfo objectInfo, SerializerContext context)
		{
			KeyValuePair<AttachableMemberIdentifier, object>[] attachedProperties = context.Runtime.GetAttachedProperties(value);
			if (attachedProperties == null)
			{
				return;
			}
			KeyValuePair<AttachableMemberIdentifier, object>[] array = attachedProperties;
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<AttachableMemberIdentifier, object> keyValuePair = array[i];
				XamlType xamlType = context.GetXamlType(keyValuePair.Key.DeclaringType);
				if (!xamlType.IsVisibleTo(context.LocalAssembly))
				{
					continue;
				}
				XamlMember attachableMember = xamlType.GetAttachableMember(keyValuePair.Key.MemberName);
				if (attachableMember == null)
				{
					throw new XamlObjectReaderException(SR.Get("ObjectReaderAttachedPropertyNotFound", xamlType, keyValuePair.Key.MemberName));
				}
				if (CanPropertyXamlRoundtrip(attachableMember, context))
				{
					MemberMarkupInfo memberMarkupInfo = MemberMarkupInfo.ForAttachedProperty(value, attachableMember, keyValuePair.Value, context);
					if (memberMarkupInfo != null)
					{
						objectInfo.Properties.Add(memberMarkupInfo);
					}
				}
			}
		}

		private static ObjectMarkupInfo ForNull()
		{
			return new ObjectMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.Null)
			};
		}

		public static ObjectMarkupInfo ForObject(object value, SerializerContext context, TypeConverter instanceConverter = null, bool isRoot = false)
		{
			if (value == null)
			{
				return ForNull();
			}
			ObjectMarkupInfo objectMarkupInfo = context.ReferenceTable.Find(value);
			if (objectMarkupInfo != null)
			{
				objectMarkupInfo.AssignName(context);
				return new ReferenceMarkupInfo(objectMarkupInfo);
			}
			context.IsRoot = isRoot;
			if (value is Array value2)
			{
				return ForArray(value2, context);
			}
			XamlType xamlType = context.GetXamlType(value.GetType());
			ValueSerializer valueSerializer = null;
			TypeConverter typeConverter = null;
			if (xamlType.ContentProperty == null || (xamlType.ContentProperty.TypeConverter != BuiltInValueConverter.String && xamlType.ContentProperty.TypeConverter != BuiltInValueConverter.Object))
			{
				valueSerializer = TypeConverterExtensions.GetConverterInstance(xamlType.ValueSerializer);
				typeConverter = TypeConverterExtensions.GetConverterInstance(xamlType.TypeConverter);
			}
			context.Instance = value;
			ObjectMarkupInfo objectMarkupInfo2;
			if (xamlType.DeferringLoader != null)
			{
				objectMarkupInfo2 = MemberMarkupInfo.ConvertToXamlReader(value, xamlType.DeferringLoader, context);
			}
			else if (typeConverter != null && valueSerializer != null && context.CanRoundtripUsingValueSerializer(valueSerializer, typeConverter, value))
			{
				if (isRoot)
				{
					context.ReserveDefaultPrefixForRootObject(value);
				}
				string value3 = context.ConvertToString(valueSerializer, value);
				context.Instance = null;
				objectMarkupInfo2 = ForTypeConverted(value3, value, context);
			}
			else if (typeConverter != null && context.TryConvertToMarkupExtension(typeConverter, ref value))
			{
				context.Instance = null;
				if (isRoot)
				{
					context.ReserveDefaultPrefixForRootObject(value);
				}
				objectMarkupInfo2 = ForObject(value, context);
			}
			else if (value is Type)
			{
				context.Instance = null;
				objectMarkupInfo2 = ForObject(new TypeExtension((Type)value), context);
			}
			else if (typeConverter != null && context.CanRoundTripString(typeConverter))
			{
				if (isRoot)
				{
					context.ReserveDefaultPrefixForRootObject(value);
				}
				string value4 = context.ConvertTo<string>(typeConverter, value);
				context.Instance = null;
				objectMarkupInfo2 = ForTypeConverted(value4, value, context);
			}
			else if (value is string)
			{
				context.Instance = null;
				objectMarkupInfo2 = ForTypeConverted((string)value, value, context);
			}
			else
			{
				if (isRoot)
				{
					context.ReserveDefaultPrefixForRootObject(value);
				}
				context.Instance = null;
				objectMarkupInfo2 = ForObjectInternal(value, context, instanceConverter);
			}
			string text = context.ReferenceTable.FindInServiceProviderTable(value);
			if (text != null)
			{
				objectMarkupInfo2.AssignName(text, context);
			}
			CheckTypeCanRoundtrip(objectMarkupInfo2);
			return objectMarkupInfo2;
		}

		private static ObjectMarkupInfo ForObjectInternal(object value, SerializerContext context, TypeConverter converter)
		{
			XamlType data = context.LocalAssemblyAwareGetXamlType(value.GetType());
			ObjectMarkupInfo objectMarkupInfo;
			if (value is INameScope)
			{
				NameScopeMarkupInfo nameScopeMarkupInfo = new NameScopeMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartObject, data),
					Object = value,
					SourceObject = value,
					ParentTable = context.ReferenceTable
				};
				context.PendingNameScopes.Enqueue(nameScopeMarkupInfo);
				AddReference(value, nameScopeMarkupInfo, context);
				objectMarkupInfo = nameScopeMarkupInfo;
			}
			else
			{
				objectMarkupInfo = new ObjectMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.StartObject, data),
					Object = value
				};
				AddReference(value, objectMarkupInfo, context);
				objectMarkupInfo.AddRecordMembers(value, context, converter);
			}
			return objectMarkupInfo;
		}

		private static void AddReference(object value, ObjectMarkupInfo objectInfo, SerializerContext context)
		{
			context.ReferenceTable.Add(value, objectInfo);
		}

		private static ObjectMarkupInfo ForTypeConverted(string value, object originalValue, SerializerContext context)
		{
			XamlType data = context.LocalAssemblyAwareGetXamlType(originalValue.GetType());
			ObjectMarkupInfo objectMarkupInfo = new ObjectMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartObject, data),
				Object = originalValue
			};
			value = value ?? string.Empty;
			objectMarkupInfo.Properties.Add(new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Initialization),
				Children = { (MarkupInfo)new ValueMarkupInfo
				{
					XamlNode = new XamlNode(XamlNodeType.Value, value)
				} }
			});
			AddAttachedProperties(originalValue, objectMarkupInfo, context);
			return objectMarkupInfo;
		}

		private static bool IsEmptyString(MemberMarkupInfo propertyInfo)
		{
			if (propertyInfo.Children.Count == 1 && propertyInfo.Children[0] is ValueMarkupInfo { XamlNode: var xamlNode })
			{
				return object.Equals(xamlNode.Value, string.Empty);
			}
			return false;
		}

		private static bool IsNull(MemberMarkupInfo propertyInfo, SerializerContext context)
		{
			if (propertyInfo.Children.Count == 1 && propertyInfo.Children[0] is ObjectMarkupInfo { XamlNode: var xamlNode })
			{
				return xamlNode.XamlType == XamlLanguage.Null;
			}
			return false;
		}

		private static bool PropertyUsedInMethodSignature(XamlMember property, ParameterInfo[] methodParameters)
		{
			if (methodParameters != null && !string.IsNullOrEmpty(GetConstructorArgument(property)))
			{
				foreach (ParameterInfo parameterInfo in methodParameters)
				{
					if (parameterInfo.Name == GetConstructorArgument(property) && property.Type.UnderlyingType == parameterInfo.ParameterType)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static string ValidateNamePropertyAndFindName(MemberMarkupInfo propertyInfo)
		{
			if (propertyInfo.Children.Count == 1 && propertyInfo.Children[0] is ValueMarkupInfo { XamlNode: { Value: string value } })
			{
				return value;
			}
			XamlMember member = propertyInfo.XamlNode.Member;
			throw new XamlObjectReaderException(SR.Get("ObjectReaderXamlNamePropertyMustBeString", member.Name, member.DeclaringType));
		}

		internal static bool CanPropertyXamlRoundtrip(XamlMember property, SerializerContext context)
		{
			if (!property.IsEvent && context.IsPropertyReadVisible(property))
			{
				if (!context.IsPropertyWriteVisible(property))
				{
					return property.Type.IsUsableAsReadOnly;
				}
				return true;
			}
			return false;
		}

		private static List<XamlMember> GetXamlSerializableProperties(XamlType type, SerializerContext context)
		{
			List<XamlMember> list = new List<XamlMember>();
			foreach (XamlMember allMember in type.GetAllMembers())
			{
				if (CanPropertyXamlRoundtrip(allMember, context))
				{
					list.Add(allMember);
				}
			}
			return list;
		}
	}

	private class ReferenceMarkupInfo : ObjectMarkupInfo
	{
		private MemberMarkupInfo nameProperty;

		public ObjectMarkupInfo Target { get; set; }

		public ReferenceMarkupInfo(ObjectMarkupInfo target)
		{
			base.XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.Reference);
			nameProperty = new MemberMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.PositionalParameters)
			};
			base.Properties.Add(nameProperty);
			Target = target;
			base.Object = target.Object;
		}

		public override List<MarkupInfo> Decompose()
		{
			nameProperty.Children.Add(new ValueMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.Value, Target.Name)
			});
			return base.Decompose();
		}
	}

	private class ReferenceTable
	{
		private ReferenceTable parent;

		private Dictionary<object, ObjectMarkupInfo> objectGraphTable;

		private Dictionary<object, string> serviceProviderTable;

		public ReferenceTable(ReferenceTable parent)
		{
			this.parent = parent;
			objectGraphTable = new Dictionary<object, ObjectMarkupInfo>(new ObjectReferenceEqualityComparer());
		}

		public void Add(object value, ObjectMarkupInfo info)
		{
			objectGraphTable.Add(value, info);
		}

		public void AddToServiceProviderTable(object value, string name)
		{
			if (serviceProviderTable == null)
			{
				serviceProviderTable = new Dictionary<object, string>(new ObjectReferenceEqualityComparer());
			}
			serviceProviderTable.Add(value, name);
		}

		public ObjectMarkupInfo Find(object value)
		{
			if (!objectGraphTable.TryGetValue(value, out var value2) && parent != null)
			{
				return parent.Find(value);
			}
			return value2;
		}

		public string FindInServiceProviderTable(object value)
		{
			string value2 = null;
			if (serviceProviderTable != null)
			{
				serviceProviderTable.TryGetValue(value, out value2);
			}
			return value2;
		}
	}

	private class SerializerContext
	{
		private int lastIdentifier;

		private Queue<NameScopeMarkupInfo> pendingNameScopes;

		private ITypeDescriptorContext typeDescriptorContext;

		private IValueSerializerContext valueSerializerContext;

		private Dictionary<string, string> namespaceToPrefixMap;

		private Dictionary<string, string> prefixToNamespaceMap;

		private XamlSchemaContext schemaContext;

		private ClrObjectRuntime runtime;

		private XamlObjectReaderSettings settings;

		public XamlObjectReaderSettings Settings => settings;

		public object Instance { get; set; }

		public ClrObjectRuntime Runtime => runtime;

		public Queue<NameScopeMarkupInfo> PendingNameScopes => pendingNameScopes;

		public ReferenceTable ReferenceTable { get; set; }

		public IValueSerializerContext ValueSerializerContext => valueSerializerContext;

		public ITypeDescriptorContext TypeDescriptorContext => typeDescriptorContext;

		public XamlSchemaContext SchemaContext => schemaContext;

		public Assembly LocalAssembly => Settings.LocalAssembly;

		public bool IsRoot { get; set; }

		public Type RootType { get; set; }

		public SerializerContext(XamlSchemaContext schemaContext, XamlObjectReaderSettings settings)
		{
			pendingNameScopes = new Queue<NameScopeMarkupInfo>();
			valueSerializerContext = (IValueSerializerContext)(typeDescriptorContext = new TypeDescriptorAndValueSerializerContext(this));
			namespaceToPrefixMap = new Dictionary<string, string>();
			prefixToNamespaceMap = new Dictionary<string, string>();
			ReferenceTable = new ReferenceTable(null);
			this.schemaContext = schemaContext;
			runtime = new ClrObjectRuntime(null, isWriter: false);
			this.settings = settings;
		}

		public void ReserveDefaultPrefixForRootObject(object obj)
		{
			string preferredXamlNamespace = GetXamlType(obj.GetType()).PreferredXamlNamespace;
			if (preferredXamlNamespace != "http://schemas.microsoft.com/winfx/2006/xaml")
			{
				namespaceToPrefixMap.Add(preferredXamlNamespace, string.Empty);
				prefixToNamespaceMap.Add(string.Empty, preferredXamlNamespace);
			}
		}

		public List<XamlNode> GetSortedNamespaceNodes()
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, string> item in namespaceToPrefixMap)
			{
				list.Add(item);
			}
			list.Sort(CompareByValue);
			return list.ConvertAll((KeyValuePair<string, string> pair) => new XamlNode(XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration(pair.Key, pair.Value)));
		}

		private static int CompareByValue(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
		{
			return string.Compare(y.Value, x.Value, ignoreCase: false, TypeConverterHelper.InvariantEnglishUS);
		}

		public string AllocateIdentifier()
		{
			return "__ReferenceID" + lastIdentifier++;
		}

		public bool TryHoistNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
		{
			if (prefixToNamespaceMap.TryGetValue(namespaceDeclaration.Prefix, out var value))
			{
				if (value == namespaceDeclaration.Namespace)
				{
					return true;
				}
				return false;
			}
			namespaceToPrefixMap.Add(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
			prefixToNamespaceMap.Add(namespaceDeclaration.Prefix, namespaceDeclaration.Namespace);
			return true;
		}

		public string FindPrefix(string ns)
		{
			string value = null;
			if (namespaceToPrefixMap.TryGetValue(ns, out value))
			{
				return value;
			}
			string preferredPrefix = SchemaContext.GetPreferredPrefix(ns);
			if (preferredPrefix != "x" && !namespaceToPrefixMap.ContainsValue(string.Empty))
			{
				value = string.Empty;
			}
			if (value == null)
			{
				value = preferredPrefix;
				int num = 0;
				while (namespaceToPrefixMap.ContainsValue(value))
				{
					num++;
					value = preferredPrefix + num.ToString(TypeConverterHelper.InvariantEnglishUS);
				}
				if (value != string.Empty)
				{
					XmlConvert.VerifyNCName(value);
				}
			}
			namespaceToPrefixMap.Add(ns, value);
			prefixToNamespaceMap.Add(value, ns);
			return value;
		}

		public XamlType GetXamlType(Type clrType)
		{
			XamlType xamlType = schemaContext.GetXamlType(clrType);
			if (xamlType == null)
			{
				throw new XamlObjectReaderException(SR.Get("ObjectReaderTypeNotAllowed", schemaContext.GetType(), clrType));
			}
			return xamlType;
		}

		public XamlType LocalAssemblyAwareGetXamlType(Type clrType)
		{
			XamlType xamlType = GetXamlType(clrType);
			if (!xamlType.IsVisibleTo(LocalAssembly) && !typeof(Type).IsAssignableFrom(clrType))
			{
				throw new XamlObjectReaderException(SR.Get("ObjectReader_TypeNotVisible", clrType.FullName));
			}
			return xamlType;
		}

		public bool CanConvertTo(TypeConverter converter, Type type)
		{
			return Runtime.CanConvertTo(TypeDescriptorContext, converter, type);
		}

		public bool CanRoundTripString(TypeConverter converter)
		{
			if (converter is ReferenceConverter)
			{
				return false;
			}
			if (Runtime.CanConvertFrom<string>(TypeDescriptorContext, converter))
			{
				return Runtime.CanConvertTo(TypeDescriptorContext, converter, typeof(string));
			}
			return false;
		}

		public bool CanRoundtripUsingValueSerializer(ValueSerializer valueSerializer, TypeConverter typeConverter, object value)
		{
			if (valueSerializer != null && typeConverter != null && Runtime.CanConvertToString(ValueSerializerContext, valueSerializer, value))
			{
				return Runtime.CanConvertFrom<string>(TypeDescriptorContext, typeConverter);
			}
			return false;
		}

		public string ConvertToString(ValueSerializer valueSerializer, object value)
		{
			return Runtime.ConvertToString(valueSerializerContext, valueSerializer, value);
		}

		public T ConvertTo<T>(TypeConverter converter, object value)
		{
			return Runtime.ConvertToValue<T>(TypeDescriptorContext, converter, value);
		}

		public bool TryValueSerializeToString(ValueSerializer valueSerializer, TypeConverter propertyConverter, SerializerContext context, ref object value)
		{
			if (value == null)
			{
				return false;
			}
			if (value is string)
			{
				return true;
			}
			XamlType xamlType = context.GetXamlType(value.GetType());
			TypeConverter converterInstance = TypeConverterExtensions.GetConverterInstance(xamlType.TypeConverter);
			if (!CanRoundtripUsingValueSerializer(valueSerializer, propertyConverter, value) && !CanRoundtripUsingValueSerializer(valueSerializer, converterInstance, value))
			{
				return false;
			}
			value = Runtime.ConvertToString(ValueSerializerContext, valueSerializer, value);
			return true;
		}

		public bool TryTypeConvertToString(TypeConverter converter, ref object value)
		{
			if (value == null)
			{
				return false;
			}
			if (value is string)
			{
				return true;
			}
			if (!CanRoundTripString(converter))
			{
				return false;
			}
			value = ConvertTo<string>(converter, value);
			return true;
		}

		public bool TryConvertToMarkupExtension(TypeConverter converter, ref object value)
		{
			if (value == null)
			{
				return false;
			}
			if (!Runtime.CanConvertTo(TypeDescriptorContext, converter, typeof(MarkupExtension)))
			{
				return false;
			}
			value = ConvertTo<MarkupExtension>(converter, value);
			return true;
		}

		public string ConvertXamlTypeToString(XamlType type)
		{
			XamlTypeName xamlTypeName = new XamlTypeName(type);
			return xamlTypeName.ConvertToStringInternal(FindPrefix);
		}

		public string GetName(object objectToName)
		{
			string text = null;
			XamlType xamlType = GetXamlType(objectToName.GetType());
			XamlMember aliasedProperty = xamlType.GetAliasedProperty(XamlLanguage.Name);
			if (aliasedProperty != null)
			{
				text = Runtime.GetValue(objectToName, aliasedProperty) as string;
			}
			if (text != null)
			{
				return text;
			}
			ObjectMarkupInfo objectMarkupInfo = ReferenceTable.Find(objectToName);
			if (objectMarkupInfo != null)
			{
				objectMarkupInfo.AssignName(this);
				return objectMarkupInfo.Name;
			}
			string text2 = null;
			text2 = ReferenceTable.FindInServiceProviderTable(objectToName);
			if (text2 == null)
			{
				text2 = AllocateIdentifier();
				ReferenceTable.AddToServiceProviderTable(objectToName, text2);
			}
			return text2;
		}

		public bool IsPropertyReadVisible(XamlMember property)
		{
			Type accessingType = null;
			if (Settings.AllowProtectedMembersOnRoot && IsRoot)
			{
				accessingType = RootType;
			}
			return property.IsReadVisibleTo(LocalAssembly, accessingType);
		}

		public bool IsPropertyWriteVisible(XamlMember property)
		{
			Type accessingType = null;
			if (Settings.AllowProtectedMembersOnRoot && IsRoot)
			{
				accessingType = RootType;
			}
			return property.IsWriteVisibleTo(LocalAssembly, accessingType);
		}
	}

	private class TypeDescriptorAndValueSerializerContext : IValueSerializerContext, ITypeDescriptorContext, IServiceProvider, INamespacePrefixLookup, IXamlSchemaContextProvider, IXamlNameProvider
	{
		private SerializerContext context;

		public IContainer Container => null;

		public object Instance => context.Instance;

		public PropertyDescriptor PropertyDescriptor => null;

		public XamlSchemaContext SchemaContext => context.SchemaContext;

		public TypeDescriptorAndValueSerializerContext(SerializerContext context)
		{
			this.context = context;
		}

		public object GetService(Type serviceType)
		{
			if (serviceType == typeof(IValueSerializerContext))
			{
				return this;
			}
			if (serviceType == typeof(ITypeDescriptorContext))
			{
				return this;
			}
			if (serviceType == typeof(INamespacePrefixLookup))
			{
				return this;
			}
			if (serviceType == typeof(IXamlSchemaContextProvider))
			{
				return this;
			}
			if (serviceType == typeof(IXamlNameProvider))
			{
				return this;
			}
			return null;
		}

		public void OnComponentChanged()
		{
		}

		public bool OnComponentChanging()
		{
			return false;
		}

		public string LookupPrefix(string ns)
		{
			return context.FindPrefix(ns);
		}

		public ValueSerializer GetValueSerializerFor(PropertyDescriptor propertyDescriptor)
		{
			return ValueSerializer.GetSerializerFor(propertyDescriptor);
		}

		public ValueSerializer GetValueSerializerFor(Type type)
		{
			return ValueSerializer.GetSerializerFor(type);
		}

		public string GetName(object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return context.GetName(value);
		}
	}

	private class XamlTemplateMarkupInfo : ObjectMarkupInfo
	{
		private List<MarkupInfo> nodes = new List<MarkupInfo>();

		private int objectPosition;

		public XamlTemplateMarkupInfo(XamlReader reader, SerializerContext context)
		{
			while (reader.Read() && reader.NodeType != XamlNodeType.StartObject)
			{
				if (reader.NodeType != XamlNodeType.NamespaceDeclaration)
				{
					throw new XamlObjectReaderException(SR.Get("XamlFactoryInvalidXamlNode", reader.NodeType));
				}
				if (!context.TryHoistNamespaceDeclaration(reader.Namespace))
				{
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.NamespaceDeclaration, reader.Namespace)
					});
				}
			}
			if (reader.NodeType != XamlNodeType.StartObject)
			{
				throw new XamlObjectReaderException(SR.Get("XamlFactoryInvalidXamlNode", reader.NodeType));
			}
			nodes.Add(new ValueMarkupInfo
			{
				XamlNode = new XamlNode(XamlNodeType.StartObject, reader.Type)
			});
			objectPosition = nodes.Count;
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
				case XamlNodeType.NamespaceDeclaration:
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.NamespaceDeclaration, reader.Namespace)
					});
					break;
				case XamlNodeType.StartObject:
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.StartObject, reader.Type)
					});
					break;
				case XamlNodeType.GetObject:
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.GetObject)
					});
					break;
				case XamlNodeType.EndObject:
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.EndObject)
					});
					break;
				case XamlNodeType.StartMember:
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.StartMember, reader.Member)
					});
					break;
				case XamlNodeType.EndMember:
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.EndMember)
					});
					break;
				case XamlNodeType.Value:
					nodes.Add(new ValueMarkupInfo
					{
						XamlNode = new XamlNode(XamlNodeType.Value, reader.Value)
					});
					break;
				default:
					throw new InvalidOperationException(SR.Get("XamlFactoryInvalidXamlNode", reader.NodeType));
				}
			}
			base.XamlNode = ((ValueMarkupInfo)nodes[0]).XamlNode;
			nodes.RemoveAt(0);
		}

		public override List<MarkupInfo> Decompose()
		{
			foreach (MarkupInfo property in base.Properties)
			{
				nodes.Insert(objectPosition, property);
			}
			return nodes;
		}

		public override void FindNamespace(SerializerContext context)
		{
			foreach (MarkupInfo property in base.Properties)
			{
				property.FindNamespace(context);
			}
		}
	}

	private class HashSet<T>
	{
		private Dictionary<T, bool> dictionary;

		public HashSet()
		{
			dictionary = new Dictionary<T, bool>();
		}

		public HashSet(IEqualityComparer<T> comparer)
		{
			dictionary = new Dictionary<T, bool>(comparer);
		}

		public bool Contains(T member)
		{
			return dictionary.ContainsKey(member);
		}

		public bool Add(T member)
		{
			if (Contains(member))
			{
				return false;
			}
			dictionary.Add(member, value: true);
			return true;
		}
	}

	private class PartiallyOrderedList<TKey, TValue> : IEnumerable<TValue>, IEnumerable where TValue : class
	{
		private class Entry
		{
			public readonly TKey Key;

			public readonly TValue Value;

			public List<int> Predecessors;

			public int Link;

			public const int UNSEEN = -1;

			public const int INDFS = -2;

			public Entry(TKey key, TValue value)
			{
				Key = key;
				Value = value;
				Predecessors = null;
				Link = 0;
			}

			public override bool Equals(object obj)
			{
				if (obj is Entry { Key: var key })
				{
					return key.Equals(Key);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode();
			}
		}

		private List<Entry> _entries = new List<Entry>();

		private int _firstIndex = -1;

		private int _lastIndex;

		public void Add(TKey key, TValue value)
		{
			Entry entry = new Entry(key, value);
			int num = _entries.IndexOf(entry);
			if (num >= 0)
			{
				entry.Predecessors = _entries[num].Predecessors;
				_entries[num] = entry;
			}
			else
			{
				_entries.Add(entry);
			}
		}

		private int GetEntryIndex(TKey key)
		{
			Entry item = new Entry(key, null);
			int num = _entries.IndexOf(item);
			if (num < 0)
			{
				num = _entries.Count;
				_entries.Add(item);
			}
			return num;
		}

		public void SetOrder(TKey predecessor, TKey key)
		{
			int entryIndex = GetEntryIndex(predecessor);
			Entry entry = _entries[entryIndex];
			int entryIndex2 = GetEntryIndex(key);
			Entry entry2 = _entries[entryIndex2];
			if (entry2.Predecessors == null)
			{
				entry2.Predecessors = new List<int>();
			}
			entry2.Predecessors.Add(entryIndex);
			_firstIndex = -1;
		}

		private void TopologicalSort()
		{
			_firstIndex = -1;
			_lastIndex = -1;
			for (int i = 0; i < _entries.Count; i++)
			{
				_entries[i].Link = -1;
			}
			for (int j = 0; j < _entries.Count; j++)
			{
				DepthFirstSearch(j);
			}
		}

		private void DepthFirstSearch(int index)
		{
			if (_entries[index].Link != -1)
			{
				return;
			}
			_entries[index].Link = -2;
			if (_entries[index].Predecessors != null)
			{
				foreach (int predecessor in _entries[index].Predecessors)
				{
					DepthFirstSearch(predecessor);
				}
			}
			if (_lastIndex == -1)
			{
				_firstIndex = index;
			}
			else
			{
				_entries[_lastIndex].Link = index;
			}
			_lastIndex = index;
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			if (_firstIndex < 0)
			{
				TopologicalSort();
			}
			int num = _firstIndex;
			while (num >= 0)
			{
				Entry entry = _entries[num];
				if (entry.Value != null)
				{
					yield return entry.Value;
				}
				num = entry.Link;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			using IEnumerator<TValue> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}
	}

	internal static class TypeConverterExtensions
	{
		public static TConverter GetConverterInstance<TConverter>(XamlValueConverter<TConverter> converter) where TConverter : class
		{
			if (!(converter == null))
			{
				return converter.ConverterInstance;
			}
			return null;
		}
	}

	private static class XamlMemberExtensions
	{
		internal enum GetNearestBaseMemberCriterion
		{
			HasSerializationVisibility,
			HasDefaultValue,
			HasConstructorArgument
		}

		internal static XamlMember GetNearestMember(XamlMember member, GetNearestBaseMemberCriterion criterion)
		{
			if (member.IsAttachable || member.IsDirective || MeetsCriterion(member, criterion))
			{
				return member;
			}
			MethodInfo methodInfo = member.Getter ?? member.Setter;
			if (methodInfo == null || !methodInfo.IsVirtual)
			{
				return member;
			}
			Type declaringType = methodInfo.GetBaseDefinition().DeclaringType;
			if (member.DeclaringType.UnderlyingType == declaringType)
			{
				return member;
			}
			XamlType baseType = member.DeclaringType.BaseType;
			while (baseType != null && baseType != XamlLanguage.Object)
			{
				XamlMember xamlMember = baseType.GetMember(member.Name);
				if (xamlMember == null)
				{
					xamlMember = GetExcludedReadOnlyMember(baseType, member.Name);
					if (xamlMember == null)
					{
						break;
					}
				}
				if (MeetsCriterion(xamlMember, criterion))
				{
					return xamlMember;
				}
				if (baseType.UnderlyingType == declaringType)
				{
					break;
				}
				baseType = xamlMember.DeclaringType.BaseType;
			}
			return member;
		}

		private static XamlMember GetExcludedReadOnlyMember(XamlType type, string name)
		{
			foreach (XamlMember allExcludedReadOnlyMember in type.GetAllExcludedReadOnlyMembers())
			{
				if (allExcludedReadOnlyMember.Name == name)
				{
					return allExcludedReadOnlyMember;
				}
			}
			return null;
		}

		private static bool MeetsCriterion(XamlMember member, GetNearestBaseMemberCriterion criterion)
		{
			return criterion switch
			{
				GetNearestBaseMemberCriterion.HasConstructorArgument => member.ConstructorArgument != null, 
				GetNearestBaseMemberCriterion.HasDefaultValue => member.HasDefaultValue, 
				GetNearestBaseMemberCriterion.HasSerializationVisibility => member.HasSerializationVisibility, 
				_ => false, 
			};
		}
	}

	private XamlObjectReaderSettings settings;

	private XamlSchemaContext schemaContext;

	private XamlNode currentXamlNode;

	private object currentInstance;

	private Stack<MarkupInfo> nodes;

	public override XamlNodeType NodeType => currentXamlNode.NodeType;

	public override NamespaceDeclaration Namespace => currentXamlNode.NamespaceDeclaration;

	public override XamlType Type => currentXamlNode.XamlType;

	public override XamlMember Member => currentXamlNode.Member;

	public override object Value => currentXamlNode.Value;

	public override XamlSchemaContext SchemaContext => schemaContext;

	public override bool IsEof => currentXamlNode.IsEof;

	public virtual object Instance
	{
		get
		{
			if (currentXamlNode.NodeType == XamlNodeType.StartObject)
			{
				return currentInstance;
			}
			return null;
		}
	}

	public XamlObjectReader(object instance)
		: this(instance, (XamlObjectReaderSettings)null)
	{
	}

	public XamlObjectReader(object instance, XamlObjectReaderSettings settings)
		: this(instance, new XamlSchemaContext(), settings)
	{
	}

	public XamlObjectReader(object instance, XamlSchemaContext schemaContext)
		: this(instance, schemaContext, null)
	{
	}

	public XamlObjectReader(object instance, XamlSchemaContext schemaContext, XamlObjectReaderSettings settings)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		this.schemaContext = schemaContext;
		this.settings = settings ?? new XamlObjectReaderSettings();
		nodes = new Stack<MarkupInfo>();
		currentXamlNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
		SerializerContext serializerContext = new SerializerContext(schemaContext, this.settings)
		{
			RootType = instance?.GetType()
		};
		ObjectMarkupInfo objectMarkupInfo = ObjectMarkupInfo.ForObject(instance, serializerContext, null, isRoot: true);
		while (serializerContext.PendingNameScopes.Count > 0)
		{
			NameScopeMarkupInfo nameScopeMarkupInfo = serializerContext.PendingNameScopes.Dequeue();
			nameScopeMarkupInfo.Resume(serializerContext);
		}
		Stack<HashSet<string>> stack = new Stack<HashSet<string>>();
		stack.Push(new HashSet<string>());
		objectMarkupInfo.EnsureNoDuplicateNames(stack);
		objectMarkupInfo.FindNamespace(serializerContext);
		nodes.Push(objectMarkupInfo);
		List<XamlNode> sortedNamespaceNodes = serializerContext.GetSortedNamespaceNodes();
		foreach (XamlNode item in sortedNamespaceNodes)
		{
			nodes.Push(new NamespaceMarkupInfo
			{
				XamlNode = item
			});
		}
	}

	public override bool Read()
	{
		if (nodes.Count == 0)
		{
			if (currentXamlNode.NodeType != XamlNodeType.None)
			{
				currentXamlNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
			}
			return false;
		}
		MarkupInfo markupInfo = nodes.Pop();
		currentXamlNode = markupInfo.XamlNode;
		currentInstance = (markupInfo as ObjectMarkupInfo)?.Object;
		List<MarkupInfo> list = markupInfo.Decompose();
		if (list != null)
		{
			list.Reverse();
			foreach (MarkupInfo item in list)
			{
				nodes.Push(item);
			}
		}
		return true;
	}

	internal static DesignerSerializationVisibility GetSerializationVisibility(XamlMember member)
	{
		XamlMember nearestMember = XamlMemberExtensions.GetNearestMember(member, XamlMemberExtensions.GetNearestBaseMemberCriterion.HasSerializationVisibility);
		return nearestMember.SerializationVisibility;
	}

	internal static string GetConstructorArgument(XamlMember member)
	{
		XamlMember nearestMember = XamlMemberExtensions.GetNearestMember(member, XamlMemberExtensions.GetNearestBaseMemberCriterion.HasConstructorArgument);
		return nearestMember.ConstructorArgument;
	}

	internal static bool GetDefaultValue(XamlMember member, out object value)
	{
		XamlMember nearestMember = XamlMemberExtensions.GetNearestMember(member, XamlMemberExtensions.GetNearestBaseMemberCriterion.HasDefaultValue);
		if (nearestMember.HasDefaultValue)
		{
			value = nearestMember.DefaultValue;
			return true;
		}
		value = null;
		return false;
	}
}
