using System.Collections.Generic;

namespace Antlr4.Runtime.Misc;

public interface IIntSet
{
	int Count { get; }

	bool IsNil { get; }

	int SingleElement { get; }

	void Add(int el);

	[return: NotNull]
	IIntSet AddAll(IIntSet set);

	[return: Nullable]
	IIntSet And(IIntSet a);

	[return: Nullable]
	IIntSet Complement(IIntSet elements);

	[return: Nullable]
	IIntSet Or(IIntSet a);

	[return: Nullable]
	IIntSet Subtract(IIntSet a);

	bool Equals(object obj);

	bool Contains(int el);

	void Remove(int el);

	[return: NotNull]
	IList<int> ToList();

	string ToString();
}
