using System;
using System.Collections;
using System.Collections.Generic;
using System.Xaml;
using MS.Internal.Xaml.Context;
using MS.Internal.Xaml.Parser;

namespace MS.Internal.Xaml;

internal class NodeStreamSorter : IEnumerator<XamlNode>, IDisposable, IEnumerator
{
	private class SeenCtorDirectiveFlags
	{
		public bool SeenInstancingProperty;

		public bool SeenOutOfOrderCtorDirective;
	}

	private struct ReorderInfo
	{
		public int Depth { get; set; }

		public int OriginalOrderIndex { get; set; }

		public XamlNodeType XamlNodeType { get; set; }
	}

	private XamlParserContext _context;

	private XamlXmlReaderSettings _settings;

	private IEnumerator<XamlNode> _source;

	private Queue<XamlNode> _buffer;

	private XamlNode _current;

	private ReorderInfo[] _sortingInfoArray;

	private XamlNode[] _originalNodesInOrder;

	private Dictionary<string, string> _xmlnsDictionary;

	private List<SeenCtorDirectiveFlags> _seenStack = new List<SeenCtorDirectiveFlags>();

	private int _startObjectDepth;

	private List<int> _moveList;

	private bool HaveSeenInstancingProperty
	{
		get
		{
			return _seenStack[_startObjectDepth].SeenInstancingProperty;
		}
		set
		{
			_seenStack[_startObjectDepth].SeenInstancingProperty = value;
		}
	}

	private bool HaveSeenOutOfOrderCtorDirective
	{
		get
		{
			return _seenStack[_startObjectDepth].SeenOutOfOrderCtorDirective;
		}
		set
		{
			_seenStack[_startObjectDepth].SeenOutOfOrderCtorDirective = value;
		}
	}

	public XamlNode Current => _current;

	object IEnumerator.Current => _current;

	private void InitializeObjectFrameStack()
	{
		if (_seenStack.Count == 0)
		{
			_seenStack.Add(new SeenCtorDirectiveFlags());
		}
		_seenStack[0].SeenInstancingProperty = false;
		_seenStack[0].SeenOutOfOrderCtorDirective = false;
	}

	private void StartObjectFrame()
	{
		_startObjectDepth++;
		if (_seenStack.Count <= _startObjectDepth)
		{
			_seenStack.Add(new SeenCtorDirectiveFlags());
		}
		_seenStack[_startObjectDepth].SeenInstancingProperty = false;
		_seenStack[_startObjectDepth].SeenOutOfOrderCtorDirective = false;
	}

	private void EndObjectFrame()
	{
		_startObjectDepth--;
	}

	public NodeStreamSorter(XamlParserContext context, XamlPullParser parser, XamlXmlReaderSettings settings, Dictionary<string, string> xmlnsDictionary)
	{
		_context = context;
		_settings = settings;
		_source = parser.Parse().GetEnumerator();
		_xmlnsDictionary = xmlnsDictionary;
		_buffer = new Queue<XamlNode>();
		_sortingInfoArray = null;
		StartNewNodeStreamWithSettingsPreamble();
		ReadAheadAndSortCtorProperties();
	}

	public bool MoveNext()
	{
		do
		{
			if (_buffer.Count > 0)
			{
				_current = _buffer.Dequeue();
				continue;
			}
			if (!_source.MoveNext())
			{
				return false;
			}
			_current = _source.Current;
			if (_current.NodeType == XamlNodeType.StartObject)
			{
				_buffer.Enqueue(_current);
				ReadAheadAndSortCtorProperties();
				_current = _buffer.Dequeue();
			}
		}
		while (_current.IsEndOfAttributes);
		return true;
	}

	public void Reset()
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	private void StartNewNodeStreamWithSettingsPreamble()
	{
		bool flag = false;
		while (!flag)
		{
			_source.MoveNext();
			XamlNode current = _source.Current;
			switch (current.NodeType)
			{
			case XamlNodeType.NamespaceDeclaration:
				_buffer.Enqueue(current);
				break;
			case XamlNodeType.StartObject:
				flag = true;
				EnqueueInitialExtraXmlNses();
				_buffer.Enqueue(current);
				EnqueueInitialXmlState();
				break;
			case XamlNodeType.None:
				if (current.IsLineInfo)
				{
					_buffer.Enqueue(current);
				}
				break;
			}
		}
	}

	private void EnqueueInitialExtraXmlNses()
	{
		if (_xmlnsDictionary == null)
		{
			return;
		}
		foreach (string key in _xmlnsDictionary.Keys)
		{
			if (_context.FindNamespaceByPrefixInParseStack(key) == null)
			{
				string ns = _xmlnsDictionary[key];
				XamlNode item = new XamlNode(XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration(ns, key));
				_buffer.Enqueue(item);
			}
		}
	}

	private void EnqueueInitialXmlState()
	{
		string text = _context.FindNamespaceByPrefix("xml");
		XamlSchemaContext schemaContext = _context.SchemaContext;
		if (_settings.XmlSpacePreserve)
		{
			EnqueueOneXmlDirectiveProperty(XamlLanguage.Space, "preserve");
		}
		if (!string.IsNullOrEmpty(_settings.XmlLang))
		{
			EnqueueOneXmlDirectiveProperty(XamlLanguage.Lang, _settings.XmlLang);
		}
		if (_settings.BaseUri != null)
		{
			EnqueueOneXmlDirectiveProperty(XamlLanguage.Base, _settings.BaseUri.ToString());
		}
	}

	private void EnqueueOneXmlDirectiveProperty(XamlMember xmlDirectiveProperty, string textValue)
	{
		XamlNode item = new XamlNode(XamlNodeType.StartMember, xmlDirectiveProperty);
		_buffer.Enqueue(item);
		XamlNode item2 = new XamlNode(XamlNodeType.Value, textValue);
		_buffer.Enqueue(item2);
		_buffer.Enqueue(new XamlNode(XamlNodeType.EndMember));
	}

	private void ReadAheadAndSortCtorProperties()
	{
		InitializeObjectFrameStack();
		_moveList = null;
		ReadAheadToEndObjectOrFirstPropertyElement();
		if (_moveList != null)
		{
			SortContentsOfReadAheadBuffer();
		}
	}

	private void ReadAheadToEndObjectOrFirstPropertyElement()
	{
		ReadAheadToEndOfAttributes();
		ReadAheadToFirstInstancingProperty();
	}

	private void ReadAheadToEndOfAttributes()
	{
		int num = 0;
		bool flag = false;
		do
		{
			if (!_source.MoveNext())
			{
				throw new InvalidOperationException("premature end of stream before EoA");
			}
			XamlNode current = _source.Current;
			switch (current.NodeType)
			{
			case XamlNodeType.StartObject:
				StartObjectFrame();
				break;
			case XamlNodeType.EndObject:
				EndObjectFrame();
				if (num == 0)
				{
					flag = true;
				}
				break;
			case XamlNodeType.None:
				if (current.IsEndOfAttributes && num == 0)
				{
					flag = true;
				}
				break;
			case XamlNodeType.StartMember:
				num++;
				if (!HaveSeenOutOfOrderCtorDirective)
				{
					CheckForOutOfOrderCtorDirectives(current);
				}
				break;
			case XamlNodeType.EndMember:
				num--;
				break;
			}
			_buffer.Enqueue(current);
		}
		while (!flag);
	}

	private void ReadAheadToFirstInstancingProperty()
	{
		int num = 0;
		bool flag = false;
		do
		{
			if (!_source.MoveNext())
			{
				throw new InvalidOperationException("premature end of stream after EoA");
			}
			XamlNode current = _source.Current;
			switch (current.NodeType)
			{
			case XamlNodeType.StartMember:
				num++;
				if (CheckForOutOfOrderCtorDirectives(current) && num == 1)
				{
					flag = true;
				}
				break;
			case XamlNodeType.EndMember:
				num--;
				break;
			case XamlNodeType.EndObject:
				if (num == 0)
				{
					flag = true;
				}
				break;
			}
			_buffer.Enqueue(current);
		}
		while (!flag);
	}

	private bool CheckForOutOfOrderCtorDirectives(XamlNode node)
	{
		XamlMember member = node.Member;
		bool result = false;
		if (IsCtorDirective(member))
		{
			if (HaveSeenInstancingProperty)
			{
				HaveSeenOutOfOrderCtorDirective = true;
				if (_moveList == null)
				{
					_moveList = new List<int>();
				}
				_moveList.Add(_buffer.Count);
			}
		}
		else if (!member.IsDirective || !(member == XamlLanguage.Key))
		{
			HaveSeenInstancingProperty = true;
			result = true;
		}
		return result;
	}

	private bool IsCtorDirective(XamlMember member)
	{
		if (!member.IsDirective)
		{
			return false;
		}
		if (member == XamlLanguage.Initialization || member == XamlLanguage.PositionalParameters || member == XamlLanguage.FactoryMethod || member == XamlLanguage.Arguments || member == XamlLanguage.TypeArguments || member == XamlLanguage.Base)
		{
			return true;
		}
		return false;
	}

	private bool IsInstancingMember(XamlMember member)
	{
		if (IsCtorDirective(member))
		{
			return false;
		}
		if (member.IsDirective && member == XamlLanguage.Key)
		{
			return false;
		}
		return true;
	}

	private void SortContentsOfReadAheadBuffer()
	{
		BuildSortingBuffer();
		MoveList_Process();
		ReloadSortedBuffer();
	}

	private void BuildSortingBuffer()
	{
		_originalNodesInOrder = _buffer.ToArray();
		_buffer.Clear();
		_sortingInfoArray = new ReorderInfo[_originalNodesInOrder.Length];
		int num = 0;
		ReorderInfo reorderInfo = default(ReorderInfo);
		for (int i = 0; i < _originalNodesInOrder.Length; i++)
		{
			reorderInfo.Depth = num;
			reorderInfo.OriginalOrderIndex = i;
			reorderInfo.XamlNodeType = _originalNodesInOrder[i].NodeType;
			switch (reorderInfo.XamlNodeType)
			{
			case XamlNodeType.StartObject:
			case XamlNodeType.GetObject:
				num = (reorderInfo.Depth = num + 1);
				break;
			case XamlNodeType.EndObject:
				reorderInfo.Depth = num--;
				break;
			}
			_sortingInfoArray[i] = reorderInfo;
		}
	}

	private void ReloadSortedBuffer()
	{
		for (int i = 0; i < _sortingInfoArray.Length; i++)
		{
			int originalOrderIndex = _sortingInfoArray[i].OriginalOrderIndex;
			_buffer.Enqueue(_originalNodesInOrder[originalOrderIndex]);
		}
		_sortingInfoArray = null;
	}

	private void MoveList_Process()
	{
		int deepestCtorIdx;
		int deepestDepth;
		while (MoveList_RemoveStartMemberIndexWithGreatestDepth(out deepestCtorIdx, out deepestDepth))
		{
			if (BackupTo(deepestCtorIdx, XamlNodeType.StartObject, deepestDepth, out var end) && AdvanceTo(end, XamlNodeType.StartMember, deepestDepth, out var end2))
			{
				SortMembers(end2);
			}
		}
	}

	private bool MoveList_RemoveStartMemberIndexWithGreatestDepth(out int deepestCtorIdx, out int deepestDepth)
	{
		deepestDepth = -1;
		deepestCtorIdx = -1;
		int index = -1;
		if (_moveList.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < _moveList.Count; i++)
		{
			int num = _moveList[i];
			if (_sortingInfoArray[num].Depth > deepestDepth)
			{
				deepestDepth = _sortingInfoArray[num].Depth;
				deepestCtorIdx = num;
				index = i;
			}
		}
		_moveList.RemoveAt(index);
		return true;
	}

	private void SortMembers(int start)
	{
		int depth = _sortingInfoArray[start].Depth;
		int num = start;
		int end;
		int end2;
		while (num < _sortingInfoArray.Length && _sortingInfoArray[num].XamlNodeType == XamlNodeType.StartMember && AdvanceToNextInstancingMember(num, depth, out end) && AdvanceToNextCtorDirective(end, depth, out end2))
		{
			int num2 = AdvanceOverNoninstancingDirectives(end2, depth);
			SwapRanges(end, end2, end2 + num2);
			num = end2 + num2;
		}
	}

	private bool AdvanceToNextInstancingMember(int current, int depth, out int end)
	{
		end = current;
		int originalOrderIndex = _sortingInfoArray[current].OriginalOrderIndex;
		XamlMember member = _originalNodesInOrder[originalOrderIndex].Member;
		while (!IsInstancingMember(member))
		{
			if (!AdvanceTo(current, XamlNodeType.StartMember, depth, out end))
			{
				return false;
			}
			current = end;
			originalOrderIndex = _sortingInfoArray[current].OriginalOrderIndex;
			member = _originalNodesInOrder[originalOrderIndex].Member;
		}
		return true;
	}

	private bool AdvanceToNextCtorDirective(int current, int depth, out int end)
	{
		end = current;
		int originalOrderIndex = _sortingInfoArray[current].OriginalOrderIndex;
		XamlMember member = _originalNodesInOrder[originalOrderIndex].Member;
		while (!IsCtorDirective(member))
		{
			if (!AdvanceTo(current, XamlNodeType.StartMember, depth, out end))
			{
				return false;
			}
			current = end;
			originalOrderIndex = _sortingInfoArray[current].OriginalOrderIndex;
			member = _originalNodesInOrder[originalOrderIndex].Member;
		}
		return true;
	}

	private int AdvanceOverNoninstancingDirectives(int start, int depth)
	{
		int num = start;
		int end = num;
		int originalOrderIndex = _sortingInfoArray[num].OriginalOrderIndex;
		XamlMember member = _originalNodesInOrder[originalOrderIndex].Member;
		while (!IsInstancingMember(member))
		{
			if (!AdvanceTo(num, XamlNodeType.StartMember, depth, out end) && AdvanceTo(num, XamlNodeType.EndObject, depth, out end))
			{
				return end - start;
			}
			num = end;
			originalOrderIndex = _sortingInfoArray[num].OriginalOrderIndex;
			member = _originalNodesInOrder[originalOrderIndex].Member;
		}
		return end - start;
	}

	private void SwapRanges(int beginning, int middle, int end)
	{
		int num = middle - beginning;
		int num2 = end - middle;
		ReorderInfo[] array = new ReorderInfo[num];
		Array.Copy(_sortingInfoArray, beginning, array, 0, num);
		Array.Copy(_sortingInfoArray, middle, _sortingInfoArray, beginning, num2);
		Array.Copy(array, 0, _sortingInfoArray, beginning + num2, num);
	}

	private bool AdvanceTo(int start, XamlNodeType nodeType, int searchDepth, out int end)
	{
		for (int i = start + 1; i < _sortingInfoArray.Length; i++)
		{
			XamlNodeType xamlNodeType = _sortingInfoArray[i].XamlNodeType;
			int depth = _sortingInfoArray[i].Depth;
			if (depth == searchDepth)
			{
				if (xamlNodeType == nodeType)
				{
					end = i;
					return true;
				}
			}
			else if (depth < searchDepth)
			{
				end = i;
				return false;
			}
		}
		end = _sortingInfoArray.Length;
		return false;
	}

	private bool BackupTo(int start, XamlNodeType nodeType, int searchDepth, out int end)
	{
		for (int num = start - 1; num >= 0; num--)
		{
			XamlNodeType xamlNodeType = _sortingInfoArray[num].XamlNodeType;
			int depth = _sortingInfoArray[num].Depth;
			if (depth == searchDepth)
			{
				if (xamlNodeType == nodeType)
				{
					end = num;
					return true;
				}
				if (depth < searchDepth)
				{
					end = num;
					return false;
				}
			}
		}
		end = 0;
		return false;
	}
}
