using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Markup;

public interface INameScopeDictionary : INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
}
