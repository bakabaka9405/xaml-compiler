using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;
using System.Security.Permissions;

namespace System.Xaml.Permissions;

[Serializable]
public sealed class XamlLoadPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private static class XmlConstants
	{
		public const string IPermission = "IPermission";

		public const string Class = "class";

		public const string Version = "version";

		public const string VersionNumber = "1";

		public const string Unrestricted = "Unrestricted";
	}

	private static IList<XamlAccessLevel> s_emptyAccessLevel;

	private bool _isUnrestricted;

	public IList<XamlAccessLevel> AllowedAccess { get; private set; }

	public XamlLoadPermission(PermissionState state)
	{
		Init(state == PermissionState.Unrestricted, null);
	}

	public XamlLoadPermission(XamlAccessLevel allowedAccess)
	{
		if (allowedAccess == null)
		{
			throw new ArgumentNullException("allowedAccess");
		}
		Init(isUnrestricted: false, new XamlAccessLevel[1] { allowedAccess });
	}

	public XamlLoadPermission(IEnumerable<XamlAccessLevel> allowedAccess)
	{
		if (allowedAccess == null)
		{
			throw new ArgumentNullException("allowedAccess");
		}
		List<XamlAccessLevel> list = new List<XamlAccessLevel>(allowedAccess);
		foreach (XamlAccessLevel item in allowedAccess)
		{
			if (item == null)
			{
				throw new ArgumentException(SR.Get("CollectionCannotContainNulls", "allowedAccess"));
			}
			list.Add(item);
		}
		Init(isUnrestricted: false, list);
	}

	private XamlLoadPermission(XamlLoadPermission other)
	{
		_isUnrestricted = other._isUnrestricted;
		AllowedAccess = other.AllowedAccess;
	}

	private void Init(bool isUnrestricted, IList<XamlAccessLevel> allowedAccess)
	{
		_isUnrestricted = isUnrestricted;
		if (allowedAccess == null)
		{
			if (s_emptyAccessLevel == null)
			{
				s_emptyAccessLevel = new ReadOnlyCollection<XamlAccessLevel>(new XamlAccessLevel[0]);
			}
			AllowedAccess = s_emptyAccessLevel;
		}
		else
		{
			AllowedAccess = new ReadOnlyCollection<XamlAccessLevel>(allowedAccess);
		}
	}

	public override IPermission Copy()
	{
		return new XamlLoadPermission(this);
	}

	public override void FromXml(SecurityElement elem)
	{
		if (elem == null)
		{
			throw new ArgumentNullException("elem");
		}
		if (elem.Tag != "IPermission")
		{
			throw new ArgumentException(SR.Get("SecurityXmlUnexpectedTag", elem.Tag, "IPermission"), "elem");
		}
		string text = elem.Attribute("class");
		if (!text.StartsWith(GetType().FullName, ignoreCase: false, TypeConverterHelper.InvariantEnglishUS))
		{
			throw new ArgumentException(SR.Get("SecurityXmlUnexpectedValue", text, "class", GetType().FullName), "elem");
		}
		string text2 = elem.Attribute("version");
		if (text2 != null && text2 != "1")
		{
			throw new ArgumentException(SR.Get("SecurityXmlUnexpectedValue", text, "version", "1"), "elem");
		}
		string text3 = elem.Attribute("Unrestricted");
		if (text3 != null && bool.Parse(text3))
		{
			Init(isUnrestricted: true, null);
			return;
		}
		List<XamlAccessLevel> list = null;
		if (elem.Children != null)
		{
			list = new List<XamlAccessLevel>(elem.Children.Count);
			foreach (SecurityElement child in elem.Children)
			{
				list.Add(XamlAccessLevel.FromXml(child));
			}
		}
		Init(isUnrestricted: false, list);
	}

	public bool Includes(XamlAccessLevel requestedAccess)
	{
		if (requestedAccess == null)
		{
			throw new ArgumentNullException("requestedAccess");
		}
		if (_isUnrestricted)
		{
			return true;
		}
		foreach (XamlAccessLevel item in AllowedAccess)
		{
			if (item.Includes(requestedAccess))
			{
				return true;
			}
		}
		return false;
	}

	public override IPermission Intersect(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		XamlLoadPermission xamlLoadPermission = CastPermission(target, "target");
		if (xamlLoadPermission.IsUnrestricted())
		{
			return Copy();
		}
		if (IsUnrestricted())
		{
			return xamlLoadPermission.Copy();
		}
		List<XamlAccessLevel> list = new List<XamlAccessLevel>();
		foreach (XamlAccessLevel item in AllowedAccess)
		{
			if (xamlLoadPermission.Includes(item))
			{
				list.Add(item);
			}
			else if (item.PrivateAccessToTypeName != null)
			{
				XamlAccessLevel xamlAccessLevel = item.AssemblyOnly();
				if (xamlLoadPermission.Includes(xamlAccessLevel))
				{
					list.Add(xamlAccessLevel);
				}
			}
		}
		return new XamlLoadPermission(list);
	}

	public override bool IsSubsetOf(IPermission target)
	{
		if (target == null)
		{
			return !IsUnrestricted() && AllowedAccess.Count == 0;
		}
		XamlLoadPermission xamlLoadPermission = CastPermission(target, "target");
		if (xamlLoadPermission.IsUnrestricted())
		{
			return true;
		}
		if (IsUnrestricted())
		{
			return false;
		}
		foreach (XamlAccessLevel item in AllowedAccess)
		{
			if (!xamlLoadPermission.Includes(item))
			{
				return false;
			}
		}
		return true;
	}

	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("IPermission");
		securityElement.AddAttribute("class", GetType().AssemblyQualifiedName);
		securityElement.AddAttribute("version", "1");
		if (IsUnrestricted())
		{
			securityElement.AddAttribute("Unrestricted", bool.TrueString);
		}
		else
		{
			foreach (XamlAccessLevel item in AllowedAccess)
			{
				securityElement.AddChild(item.ToXml());
			}
		}
		return securityElement;
	}

	public override IPermission Union(IPermission other)
	{
		if (other == null)
		{
			return Copy();
		}
		XamlLoadPermission xamlLoadPermission = CastPermission(other, "other");
		if (IsUnrestricted() || xamlLoadPermission.IsUnrestricted())
		{
			return new XamlLoadPermission(PermissionState.Unrestricted);
		}
		List<XamlAccessLevel> list = new List<XamlAccessLevel>(AllowedAccess);
		foreach (XamlAccessLevel item in xamlLoadPermission.AllowedAccess)
		{
			if (Includes(item))
			{
				continue;
			}
			list.Add(item);
			if (item.PrivateAccessToTypeName == null)
			{
				continue;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].PrivateAccessToTypeName == null && list[i].AssemblyNameString == item.AssemblyNameString)
				{
					list.RemoveAt(i);
					break;
				}
			}
		}
		return new XamlLoadPermission(list);
	}

	public bool IsUnrestricted()
	{
		return _isUnrestricted;
	}

	private static XamlLoadPermission CastPermission(IPermission other, string argName)
	{
		if (!(other is XamlLoadPermission result))
		{
			throw new ArgumentException(SR.Get("ExpectedLoadPermission"), argName);
		}
		return result;
	}
}
