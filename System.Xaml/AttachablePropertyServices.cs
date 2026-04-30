using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Xaml;

public static class AttachablePropertyServices
{
	private sealed class DefaultAttachedPropertyStore
	{
		private Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>> instanceStorage = new Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>>();

		public void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value))
			{
				lock (value)
				{
					((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>)value).CopyTo(array, index);
				}
			}
		}

		public int GetPropertyCount(object instance)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value))
			{
				lock (value)
				{
					return value.Count;
				}
			}
			return 0;
		}

		public bool RemoveProperty(object instance, AttachableMemberIdentifier name)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value))
			{
				lock (value)
				{
					return value.Remove(name);
				}
			}
			return false;
		}

		public void SetProperty(object instance, AttachableMemberIdentifier name, object value)
		{
			if (!instanceStorage.Value.TryGetValue(instance, out var value2))
			{
				value2 = new Dictionary<AttachableMemberIdentifier, object>();
				try
				{
					instanceStorage.Value.Add(instance, value2);
				}
				catch (ArgumentException)
				{
					if (!instanceStorage.Value.TryGetValue(instanceStorage, out value2))
					{
						throw new InvalidOperationException(SR.Get("DefaultAttachablePropertyStoreCannotAddInstance"));
					}
				}
			}
			lock (value2)
			{
				value2[name] = value;
			}
		}

		public bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value2))
			{
				lock (value2)
				{
					if (value2.TryGetValue(name, out var value3) && value3 is T)
					{
						value = (T)value3;
						return true;
					}
				}
			}
			value = default(T);
			return false;
		}
	}

	private static DefaultAttachedPropertyStore attachedProperties = new DefaultAttachedPropertyStore();

	public static int GetAttachedPropertyCount(object instance)
	{
		if (instance == null)
		{
			return 0;
		}
		if (instance is IAttachedPropertyStore attachedPropertyStore)
		{
			return attachedPropertyStore.PropertyCount;
		}
		return attachedProperties.GetPropertyCount(instance);
	}

	public static void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
	{
		if (instance != null)
		{
			if (instance is IAttachedPropertyStore attachedPropertyStore)
			{
				attachedPropertyStore.CopyPropertiesTo(array, index);
			}
			else
			{
				attachedProperties.CopyPropertiesTo(instance, array, index);
			}
		}
	}

	public static bool RemoveProperty(object instance, AttachableMemberIdentifier name)
	{
		if (instance == null)
		{
			return false;
		}
		if (instance is IAttachedPropertyStore attachedPropertyStore)
		{
			return attachedPropertyStore.RemoveProperty(name);
		}
		return attachedProperties.RemoveProperty(instance, name);
	}

	public static void SetProperty(object instance, AttachableMemberIdentifier name, object value)
	{
		if (instance != null)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (instance is IAttachedPropertyStore attachedPropertyStore)
			{
				attachedPropertyStore.SetProperty(name, value);
			}
			else
			{
				attachedProperties.SetProperty(instance, name, value);
			}
		}
	}

	public static bool TryGetProperty(object instance, AttachableMemberIdentifier name, out object value)
	{
		return AttachablePropertyServices.TryGetProperty<object>(instance, name, out value);
	}

	public static bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
	{
		if (instance == null)
		{
			value = default(T);
			return false;
		}
		if (instance is IAttachedPropertyStore attachedPropertyStore)
		{
			if (attachedPropertyStore.TryGetProperty(name, out var value2) && value2 is T)
			{
				value = (T)value2;
				return true;
			}
			value = default(T);
			return false;
		}
		return attachedProperties.TryGetProperty<T>(instance, name, out value);
	}
}
