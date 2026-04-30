using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class ATNConfigSet : IEnumerable<ATNConfig>, IEnumerable
{
	private sealed class _IComparer_475 : IComparer<ATNConfig>
	{
		public int Compare(ATNConfig o1, ATNConfig o2)
		{
			if (o1.Alt != o2.Alt)
			{
				return o1.Alt - o2.Alt;
			}
			if (o1.State.stateNumber != o2.State.stateNumber)
			{
				return o1.State.stateNumber - o2.State.stateNumber;
			}
			return string.CompareOrdinal(o1.SemanticContext.ToString(), o2.SemanticContext.ToString());
		}
	}

	private readonly Dictionary<long, ATNConfig> mergedConfigs;

	private readonly List<ATNConfig> unmerged;

	private readonly List<ATNConfig> configs;

	private int uniqueAlt;

	private ConflictInfo conflictInfo;

	private bool hasSemanticContext;

	private bool dipsIntoOuterContext;

	private bool outermostConfigSet;

	private int cachedHashCode = -1;

	[NotNull]
	public virtual BitSet RepresentedAlternatives
	{
		get
		{
			if (conflictInfo != null)
			{
				return conflictInfo.ConflictedAlts.Clone();
			}
			BitSet bitSet = new BitSet();
			using IEnumerator<ATNConfig> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				ATNConfig current = enumerator.Current;
				bitSet.Set(current.Alt);
			}
			return bitSet;
		}
	}

	public bool IsReadOnly => mergedConfigs == null;

	public virtual bool IsOutermostConfigSet
	{
		get
		{
			return outermostConfigSet;
		}
		set
		{
			bool flag = value;
			if (outermostConfigSet && !flag)
			{
				throw new InvalidOperationException();
			}
			outermostConfigSet = flag;
		}
	}

	public virtual HashSet<ATNState> States
	{
		get
		{
			HashSet<ATNState> hashSet = new HashSet<ATNState>();
			foreach (ATNConfig config in configs)
			{
				hashSet.Add(config.State);
			}
			return hashSet;
		}
	}

	public virtual int Count => configs.Count;

	public virtual int UniqueAlt => uniqueAlt;

	public virtual bool HasSemanticContext => hasSemanticContext;

	public virtual ConflictInfo ConflictInformation
	{
		get
		{
			return conflictInfo;
		}
		set
		{
			EnsureWritable();
			conflictInfo = value;
		}
	}

	public virtual BitSet ConflictingAlts
	{
		get
		{
			if (conflictInfo == null)
			{
				return null;
			}
			return conflictInfo.ConflictedAlts;
		}
	}

	public virtual bool IsExactConflict
	{
		get
		{
			if (conflictInfo == null)
			{
				return false;
			}
			return conflictInfo.IsExact;
		}
	}

	public virtual bool DipsIntoOuterContext => dipsIntoOuterContext;

	public virtual ATNConfig this[int index] => configs[index];

	public ATNConfigSet()
	{
		mergedConfigs = new Dictionary<long, ATNConfig>();
		unmerged = new List<ATNConfig>();
		configs = new List<ATNConfig>();
		uniqueAlt = 0;
	}

	protected internal ATNConfigSet(ATNConfigSet set, bool @readonly)
	{
		if (@readonly)
		{
			mergedConfigs = null;
			unmerged = null;
		}
		else if (!set.IsReadOnly)
		{
			mergedConfigs = new Dictionary<long, ATNConfig>(set.mergedConfigs);
			unmerged = new List<ATNConfig>(set.unmerged);
		}
		else
		{
			mergedConfigs = new Dictionary<long, ATNConfig>(set.configs.Count);
			unmerged = new List<ATNConfig>();
		}
		configs = new List<ATNConfig>(set.configs);
		dipsIntoOuterContext = set.dipsIntoOuterContext;
		hasSemanticContext = set.hasSemanticContext;
		outermostConfigSet = set.outermostConfigSet;
		if (@readonly || !set.IsReadOnly)
		{
			uniqueAlt = set.uniqueAlt;
			conflictInfo = set.conflictInfo;
		}
	}

	public virtual void OptimizeConfigs(ATNSimulator interpreter)
	{
		if (configs.Count != 0)
		{
			for (int i = 0; i < configs.Count; i++)
			{
				ATNConfig aTNConfig = configs[i];
				aTNConfig.Context = interpreter.atn.GetCachedContext(aTNConfig.Context);
			}
		}
	}

	public virtual ATNConfigSet Clone(bool @readonly)
	{
		ATNConfigSet aTNConfigSet = new ATNConfigSet(this, @readonly);
		if (!@readonly && IsReadOnly)
		{
			aTNConfigSet.AddAll(configs);
		}
		return aTNConfigSet;
	}

	public virtual bool IsEmpty()
	{
		return configs.Count == 0;
	}

	public virtual bool Contains(object o)
	{
		if (!(o is ATNConfig))
		{
			return false;
		}
		ATNConfig aTNConfig = (ATNConfig)o;
		long key = GetKey(aTNConfig);
		if (mergedConfigs.TryGetValue(key, out var value) && CanMerge(aTNConfig, key, value))
		{
			return value.Contains(aTNConfig);
		}
		foreach (ATNConfig item in unmerged)
		{
			if (item.Contains(aTNConfig))
			{
				return true;
			}
		}
		return false;
	}

	public virtual IEnumerator<ATNConfig> GetEnumerator()
	{
		return configs.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public virtual object[] ToArray()
	{
		return configs.ToArray();
	}

	public virtual bool Add(ATNConfig e)
	{
		return Add(e, null);
	}

	public virtual bool Add(ATNConfig e, PredictionContextCache contextCache)
	{
		EnsureWritable();
		if (contextCache == null)
		{
			contextCache = PredictionContextCache.Uncached;
		}
		long key = GetKey(e);
		ATNConfig value;
		bool flag = !mergedConfigs.TryGetValue(key, out value);
		if (value != null && CanMerge(e, key, value))
		{
			value.OuterContextDepth = Math.Max(value.OuterContextDepth, e.OuterContextDepth);
			if (e.PrecedenceFilterSuppressed)
			{
				value.PrecedenceFilterSuppressed = true;
			}
			PredictionContext predictionContext = PredictionContext.Join(value.Context, e.Context, contextCache);
			UpdatePropertiesForMergedConfig(e);
			if (value.Context == predictionContext)
			{
				return false;
			}
			value.Context = predictionContext;
			return true;
		}
		for (int i = 0; i < unmerged.Count; i++)
		{
			ATNConfig aTNConfig = unmerged[i];
			if (CanMerge(e, key, aTNConfig))
			{
				aTNConfig.OuterContextDepth = Math.Max(aTNConfig.OuterContextDepth, e.OuterContextDepth);
				if (e.PrecedenceFilterSuppressed)
				{
					aTNConfig.PrecedenceFilterSuppressed = true;
				}
				PredictionContext predictionContext2 = PredictionContext.Join(aTNConfig.Context, e.Context, contextCache);
				UpdatePropertiesForMergedConfig(e);
				if (aTNConfig.Context == predictionContext2)
				{
					return false;
				}
				aTNConfig.Context = predictionContext2;
				if (flag)
				{
					mergedConfigs[key] = aTNConfig;
					unmerged.RemoveAt(i);
				}
				return true;
			}
		}
		configs.Add(e);
		if (flag)
		{
			mergedConfigs[key] = e;
		}
		else
		{
			unmerged.Add(e);
		}
		UpdatePropertiesForAddedConfig(e);
		return true;
	}

	private void UpdatePropertiesForMergedConfig(ATNConfig config)
	{
		dipsIntoOuterContext |= config.ReachesIntoOuterContext;
	}

	private void UpdatePropertiesForAddedConfig(ATNConfig config)
	{
		if (configs.Count == 1)
		{
			uniqueAlt = config.Alt;
		}
		else if (uniqueAlt != config.Alt)
		{
			uniqueAlt = 0;
		}
		hasSemanticContext |= !SemanticContext.None.Equals(config.SemanticContext);
		dipsIntoOuterContext |= config.ReachesIntoOuterContext;
	}

	protected internal virtual bool CanMerge(ATNConfig left, long leftKey, ATNConfig right)
	{
		if (left.State.stateNumber != right.State.stateNumber)
		{
			return false;
		}
		if (leftKey != GetKey(right))
		{
			return false;
		}
		return left.SemanticContext.Equals(right.SemanticContext);
	}

	protected internal virtual long GetKey(ATNConfig e)
	{
		long num = e.State.stateNumber;
		return (num << 12) | (long)((ulong)e.Alt & 0xFFFuL);
	}

	public virtual bool Remove(object o)
	{
		EnsureWritable();
		throw new NotSupportedException("Not supported yet.");
	}

	public virtual bool ContainsAll(IEnumerable<ATNConfig> c)
	{
		foreach (ATNConfig item in c)
		{
			if (!Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool AddAll(IEnumerable<ATNConfig> c)
	{
		return AddAll(c, null);
	}

	public virtual bool AddAll(IEnumerable<ATNConfig> c, PredictionContextCache contextCache)
	{
		EnsureWritable();
		bool flag = false;
		foreach (ATNConfig item in c)
		{
			flag |= Add(item, contextCache);
		}
		return flag;
	}

	public virtual bool RetainAll<_T0>(ICollection<_T0> c)
	{
		EnsureWritable();
		throw new NotSupportedException("Not supported yet.");
	}

	public virtual bool RemoveAll<_T0>(ICollection<_T0> c)
	{
		EnsureWritable();
		throw new NotSupportedException("Not supported yet.");
	}

	public virtual void Clear()
	{
		EnsureWritable();
		mergedConfigs.Clear();
		unmerged.Clear();
		configs.Clear();
		dipsIntoOuterContext = false;
		hasSemanticContext = false;
		uniqueAlt = 0;
		conflictInfo = null;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is ATNConfigSet))
		{
			return false;
		}
		ATNConfigSet aTNConfigSet = (ATNConfigSet)obj;
		if (outermostConfigSet == aTNConfigSet.outermostConfigSet && object.Equals(conflictInfo, aTNConfigSet.conflictInfo))
		{
			return configs.SequenceEqual(aTNConfigSet.configs);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (IsReadOnly && cachedHashCode != -1)
		{
			return cachedHashCode;
		}
		int num = 1;
		num = (5 * num) ^ (outermostConfigSet ? 1 : 0);
		num = (5 * num) ^ SequenceEqualityComparer<ATNConfig>.Default.GetHashCode(configs);
		if (IsReadOnly)
		{
			cachedHashCode = num;
		}
		return num;
	}

	public override string ToString()
	{
		return ToString(showContext: false);
	}

	public virtual string ToString(bool showContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<ATNConfig> list = new List<ATNConfig>(configs);
		list.Sort(new _IComparer_475());
		stringBuilder.Append("[");
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(list[i].ToString(null, showAlt: true, showContext));
		}
		stringBuilder.Append("]");
		if (hasSemanticContext)
		{
			stringBuilder.Append(",hasSemanticContext=").Append(hasSemanticContext);
		}
		if (uniqueAlt != 0)
		{
			stringBuilder.Append(",uniqueAlt=").Append(uniqueAlt);
		}
		if (conflictInfo != null)
		{
			stringBuilder.Append(",conflictingAlts=").Append(conflictInfo.ConflictedAlts);
			if (!conflictInfo.IsExact)
			{
				stringBuilder.Append("*");
			}
		}
		if (dipsIntoOuterContext)
		{
			stringBuilder.Append(",dipsIntoOuterContext");
		}
		return stringBuilder.ToString();
	}

	public virtual void ClearExplicitSemanticContext()
	{
		EnsureWritable();
		hasSemanticContext = false;
	}

	public virtual void MarkExplicitSemanticContext()
	{
		EnsureWritable();
		hasSemanticContext = true;
	}

	public virtual void Remove(int index)
	{
		EnsureWritable();
		ATNConfig aTNConfig = configs[index];
		configs.Remove(aTNConfig);
		long key = GetKey(aTNConfig);
		if (mergedConfigs.TryGetValue(key, out var value) && value == aTNConfig)
		{
			mergedConfigs.Remove(key);
			return;
		}
		for (int i = 0; i < unmerged.Count; i++)
		{
			if (unmerged[i] == aTNConfig)
			{
				unmerged.RemoveAt(i);
				break;
			}
		}
	}

	protected internal void EnsureWritable()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException("This ATNConfigSet is read only.");
		}
	}
}
