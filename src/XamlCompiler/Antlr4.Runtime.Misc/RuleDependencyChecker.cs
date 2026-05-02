using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Misc;

public class RuleDependencyChecker
{
	private sealed class RuleRelations
	{
		public readonly BitSet[] parents;

		public readonly BitSet[] children;

		public RuleRelations(int ruleCount)
		{
			parents = new BitSet[ruleCount];
			for (int i = 0; i < ruleCount; i++)
			{
				parents[i] = new BitSet();
			}
			children = new BitSet[ruleCount];
			for (int j = 0; j < ruleCount; j++)
			{
				children[j] = new BitSet();
			}
		}

		public bool AddRuleInvocation(int caller, int callee)
		{
			if (caller < 0)
			{
				return false;
			}
			if (children[caller].Get(callee))
			{
				return false;
			}
			children[caller].Set(callee);
			parents[callee].Set(caller);
			return true;
		}

		public BitSet GetAncestors(int rule)
		{
			BitSet bitSet = new BitSet();
			bitSet.Or(parents[rule]);
			int num;
			do
			{
				num = bitSet.Cardinality();
				for (int num2 = bitSet.NextSetBit(0); num2 >= 0; num2 = bitSet.NextSetBit(num2 + 1))
				{
					bitSet.Or(parents[num2]);
				}
			}
			while (bitSet.Cardinality() != num);
			return bitSet;
		}

		public BitSet GetDescendants(int rule)
		{
			BitSet bitSet = new BitSet();
			bitSet.Or(children[rule]);
			int num;
			do
			{
				num = bitSet.Cardinality();
				for (int num2 = bitSet.NextSetBit(0); num2 >= 0; num2 = bitSet.NextSetBit(num2 + 1))
				{
					bitSet.Or(children[num2]);
				}
			}
			while (bitSet.Cardinality() != num);
			return bitSet;
		}
	}

	private const BindingFlags AllDeclaredStaticMembers = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private const BindingFlags AllDeclaredMembers = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly HashSet<string> checkedAssemblies = new HashSet<string>();

	private static readonly Dependents ImplementedDependents = Dependents.Self | Dependents.Parents | Dependents.Children | Dependents.Ancestors | Dependents.Descendants;

	public static void CheckDependencies(Assembly assembly)
	{
		if (IsChecked(assembly))
		{
			return;
		}
		IList<Type> typesToCheck = GetTypesToCheck(assembly);
		List<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> list = new List<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>();
		foreach (Type item in typesToCheck)
		{
			list.AddRange(GetDependencies(item));
		}
		if (list.Count > 0)
		{
			IDictionary<Type, IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>> dictionary = new Dictionary<Type, IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>>();
			foreach (Tuple<RuleDependencyAttribute, ICustomAttributeProvider> item2 in list)
			{
				Type recognizer = item2.Item1.Recognizer;
				if (!dictionary.TryGetValue(recognizer, out var value))
				{
					value = (dictionary[recognizer] = new List<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>());
				}
				value.Add(item2);
			}
			foreach (KeyValuePair<Type, IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>> item3 in dictionary)
			{
				CheckDependencies(item3.Value, item3.Key);
			}
		}
		MarkChecked(assembly);
	}

	private static IList<Type> GetTypesToCheck(Assembly assembly)
	{
		return assembly.GetTypes();
	}

	private static bool IsChecked(Assembly assembly)
	{
		lock (checkedAssemblies)
		{
			return checkedAssemblies.Contains(assembly.FullName);
		}
	}

	private static void MarkChecked(Assembly assembly)
	{
		lock (checkedAssemblies)
		{
			checkedAssemblies.Add(assembly.FullName);
		}
	}

	private static void CheckDependencies(IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> dependencies, Type recognizerType)
	{
		string[] ruleNames = GetRuleNames(recognizerType);
		int[] ruleVersions = GetRuleVersions(recognizerType, ruleNames);
		RuleRelations ruleRelations = ExtractRuleRelations(recognizerType);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Tuple<RuleDependencyAttribute, ICustomAttributeProvider> dependency in dependencies)
		{
			if (!dependency.Item1.Recognizer.IsAssignableFrom(recognizerType))
			{
				continue;
			}
			int rule = dependency.Item1.Rule;
			if (rule < 0 || rule >= ruleVersions.Length)
			{
				string value = $"Rule dependency on unknown rule {dependency.Item1.Rule}@{dependency.Item1.Version} in {dependency.Item1.Recognizer.ToString()}";
				stringBuilder.AppendLine(dependency.Item2.ToString());
				stringBuilder.AppendLine(value);
				continue;
			}
			Dependents dependents = Dependents.Self | dependency.Item1.Dependents;
			ReportUnimplementedDependents(stringBuilder, dependency, dependents);
			BitSet bitSet = new BitSet();
			int num = CheckDependencyVersion(stringBuilder, dependency, ruleNames, ruleVersions, rule, null);
			if ((dependents & Dependents.Parents) != Dependents.None)
			{
				BitSet bitSet2 = ruleRelations.parents[dependency.Item1.Rule];
				for (int num2 = bitSet2.NextSetBit(0); num2 >= 0; num2 = bitSet2.NextSetBit(num2 + 1))
				{
					if (num2 >= 0 && num2 < ruleVersions.Length && !bitSet.Get(num2))
					{
						bitSet.Set(num2);
						int val = CheckDependencyVersion(stringBuilder, dependency, ruleNames, ruleVersions, num2, "parent");
						num = Math.Max(num, val);
					}
				}
			}
			if ((dependents & Dependents.Children) != Dependents.None)
			{
				BitSet bitSet3 = ruleRelations.children[dependency.Item1.Rule];
				for (int num3 = bitSet3.NextSetBit(0); num3 >= 0; num3 = bitSet3.NextSetBit(num3 + 1))
				{
					if (num3 >= 0 && num3 < ruleVersions.Length && !bitSet.Get(num3))
					{
						bitSet.Set(num3);
						int val2 = CheckDependencyVersion(stringBuilder, dependency, ruleNames, ruleVersions, num3, "child");
						num = Math.Max(num, val2);
					}
				}
			}
			if ((dependents & Dependents.Ancestors) != Dependents.None)
			{
				BitSet ancestors = ruleRelations.GetAncestors(dependency.Item1.Rule);
				for (int num4 = ancestors.NextSetBit(0); num4 >= 0; num4 = ancestors.NextSetBit(num4 + 1))
				{
					if (num4 >= 0 && num4 < ruleVersions.Length && !bitSet.Get(num4))
					{
						bitSet.Set(num4);
						int val3 = CheckDependencyVersion(stringBuilder, dependency, ruleNames, ruleVersions, num4, "ancestor");
						num = Math.Max(num, val3);
					}
				}
			}
			if ((dependents & Dependents.Descendants) != Dependents.None)
			{
				BitSet descendants = ruleRelations.GetDescendants(dependency.Item1.Rule);
				for (int num5 = descendants.NextSetBit(0); num5 >= 0; num5 = descendants.NextSetBit(num5 + 1))
				{
					if (num5 >= 0 && num5 < ruleVersions.Length && !bitSet.Get(num5))
					{
						bitSet.Set(num5);
						int val4 = CheckDependencyVersion(stringBuilder, dependency, ruleNames, ruleVersions, num5, "descendant");
						num = Math.Max(num, val4);
					}
				}
			}
			int version = dependency.Item1.Version;
			if (version > num)
			{
				string value2 = $"Rule dependency version mismatch: {ruleNames[dependency.Item1.Rule]} has maximum dependency version {num} (expected {version}) in {dependency.Item1.Recognizer.ToString()}";
				stringBuilder.AppendLine(dependency.Item2.ToString());
				stringBuilder.AppendLine(value2);
			}
		}
		if (stringBuilder.Length > 0)
		{
			throw new InvalidOperationException(stringBuilder.ToString());
		}
	}

	private static void ReportUnimplementedDependents(StringBuilder errors, Tuple<RuleDependencyAttribute, ICustomAttributeProvider> dependency, Dependents dependents)
	{
		Dependents dependents2 = dependents;
		dependents2 &= ~ImplementedDependents;
		if (dependents2 != Dependents.None)
		{
			string value = $"Cannot validate the following dependents of rule {dependency.Item1.Rule}: {dependents2}";
			errors.AppendLine(value);
		}
	}

	private static int CheckDependencyVersion(StringBuilder errors, Tuple<RuleDependencyAttribute, ICustomAttributeProvider> dependency, string[] ruleNames, int[] ruleVersions, int relatedRule, string relation)
	{
		string text = ruleNames[dependency.Item1.Rule];
		string text2;
		if (relation == null)
		{
			text2 = text;
		}
		else
		{
			string arg = ruleNames[relatedRule];
			text2 = $"rule {arg} ({relation} of {text})";
		}
		int version = dependency.Item1.Version;
		int num = ruleVersions[relatedRule];
		if (num > version)
		{
			string value = $"Rule dependency version mismatch: {text2} has version {num} (expected <= {version}) in {dependency.Item1.Recognizer.ToString()}";
			errors.AppendLine(dependency.Item2.ToString());
			errors.AppendLine(value);
		}
		return num;
	}

	private static int[] GetRuleVersions(Type recognizerClass, string[] ruleNames)
	{
		int[] array = new int[ruleNames.Length];
		FieldInfo[] fields = recognizerClass.GetFields();
		FieldInfo[] array2 = fields;
		foreach (FieldInfo fieldInfo in array2)
		{
			bool isStatic = fieldInfo.IsStatic;
			bool flag = fieldInfo.FieldType == typeof(int);
			if (!(isStatic && flag) || !fieldInfo.Name.StartsWith("RULE_"))
			{
				continue;
			}
			try
			{
				string text = fieldInfo.Name.Substring("RULE_".Length);
				if (text.Length == 0 || !char.IsLower(text[0]))
				{
					continue;
				}
				int num = (int)fieldInfo.GetValue(null);
				if (num >= 0 && num < array.Length)
				{
					MethodInfo ruleMethod = GetRuleMethod(recognizerClass, text);
					if (!(ruleMethod == null))
					{
						int num2 = ((RuleVersionAttribute)Attribute.GetCustomAttribute(ruleMethod, typeof(RuleVersionAttribute)))?.Version ?? 0;
						array[num] = num2;
					}
				}
			}
			catch (ArgumentException)
			{
				throw;
			}
			catch (MemberAccessException)
			{
				throw;
			}
		}
		return array;
	}

	private static MethodInfo GetRuleMethod(Type recognizerClass, string name)
	{
		MethodInfo[] methods = recognizerClass.GetMethods();
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			if (methodInfo.Name.Equals(name) && Attribute.IsDefined(methodInfo, typeof(RuleVersionAttribute)))
			{
				return methodInfo;
			}
		}
		return null;
	}

	private static string[] GetRuleNames(Type recognizerClass)
	{
		FieldInfo field = recognizerClass.GetField("ruleNames");
		return (string[])field.GetValue(null);
	}

	public static IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> GetDependencies(Type clazz)
	{
		IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> result = new List<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>();
		GetElementDependencies(AsCustomAttributeProvider(clazz), result);
		ConstructorInfo[] constructors = clazz.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			GetElementDependencies(AsCustomAttributeProvider(constructorInfo), result);
			ParameterInfo[] parameters = constructorInfo.GetParameters();
			foreach (ParameterInfo obj in parameters)
			{
				GetElementDependencies(AsCustomAttributeProvider(obj), result);
			}
		}
		FieldInfo[] fields = clazz.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo obj2 in fields)
		{
			GetElementDependencies(AsCustomAttributeProvider(obj2), result);
		}
		MethodInfo[] methods = clazz.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			GetElementDependencies(AsCustomAttributeProvider(methodInfo), result);
			if (methodInfo.ReturnParameter != null)
			{
				GetElementDependencies(AsCustomAttributeProvider(methodInfo.ReturnParameter), result);
			}
			ParameterInfo[] parameters2 = methodInfo.GetParameters();
			foreach (ParameterInfo obj3 in parameters2)
			{
				GetElementDependencies(AsCustomAttributeProvider(obj3), result);
			}
		}
		return result;
	}

	private static void GetElementDependencies(ICustomAttributeProvider annotatedElement, IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> result)
	{
		object[] customAttributes = annotatedElement.GetCustomAttributes(typeof(RuleDependencyAttribute), inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			RuleDependencyAttribute item = (RuleDependencyAttribute)customAttributes[i];
			result.Add(Tuple.Create(item, annotatedElement));
		}
	}

	private static RuleRelations ExtractRuleRelations(Type recognizer)
	{
		string serializedATN = GetSerializedATN(recognizer);
		if (serializedATN == null)
		{
			return null;
		}
		ATN aTN = new ATNDeserializer().Deserialize(serializedATN.ToCharArray());
		RuleRelations ruleRelations = new RuleRelations(aTN.ruleToStartState.Length);
		foreach (ATNState state in aTN.states)
		{
			if (!state.epsilonOnlyTransitions)
			{
				continue;
			}
			foreach (Transition transition in state.transitions)
			{
				if (transition.TransitionType == TransitionType.Rule)
				{
					RuleTransition ruleTransition = (RuleTransition)transition;
					ruleRelations.AddRuleInvocation(state.ruleIndex, ruleTransition.target.ruleIndex);
				}
			}
		}
		return ruleRelations;
	}

	private static string GetSerializedATN(Type recognizerClass)
	{
		FieldInfo field = recognizerClass.GetField("_serializedATN", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null)
		{
			return (string)field.GetValue(null);
		}
		if (recognizerClass.BaseType != null)
		{
			return GetSerializedATN(recognizerClass.BaseType);
		}
		return null;
	}

	private RuleDependencyChecker()
	{
	}

	protected static ICustomAttributeProvider AsCustomAttributeProvider(ICustomAttributeProvider obj)
	{
		return obj;
	}
}
