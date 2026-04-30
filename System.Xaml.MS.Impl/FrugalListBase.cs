namespace System.Xaml.MS.Impl;

internal abstract class FrugalListBase<T>
{
	internal class Compacter
	{
		protected FrugalListBase<T> _store;

		protected int _validItemCount;

		protected int _previousEnd;

		private int _newCount;

		public Compacter(FrugalListBase<T> store, int newCount)
		{
			_store = store;
			_newCount = newCount;
		}

		public void Include(int start, int end)
		{
			IncludeOverride(start, end);
			_previousEnd = end;
		}

		protected virtual void IncludeOverride(int start, int end)
		{
			for (int i = start; i < end; i++)
			{
				_store.SetAt(_validItemCount++, _store.EntryAt(i));
			}
		}

		public virtual FrugalListBase<T> Finish()
		{
			T value = default(T);
			int i = _validItemCount;
			for (int count = _store._count; i < count; i++)
			{
				_store.SetAt(i, value);
			}
			_store._count = _validItemCount;
			return _store;
		}
	}

	protected int _count;

	public int Count => _count;

	public abstract int Capacity { get; }

	internal void TrustedSetCount(int newCount)
	{
		_count = newCount;
	}

	public abstract FrugalListStoreState Add(T value);

	public abstract void Clear();

	public abstract bool Contains(T value);

	public abstract int IndexOf(T value);

	public abstract void Insert(int index, T value);

	public abstract void SetAt(int index, T value);

	public abstract bool Remove(T value);

	public abstract void RemoveAt(int index);

	public abstract T EntryAt(int index);

	public abstract void Promote(FrugalListBase<T> newList);

	public abstract T[] ToArray();

	public abstract void CopyTo(T[] array, int index);

	public abstract object Clone();

	public virtual Compacter NewCompacter(int newCount)
	{
		return new Compacter(this, newCount);
	}
}
