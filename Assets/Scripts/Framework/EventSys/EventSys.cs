using Kusuri;
using System;
using System.Collections.Generic;

namespace KEventSys
{
	public delegate void EventFunc(params object[] args);
	

	/// <summary>
	/// 非线程安全的事件系统
	/// </summary>
	public class EventSys : Singleton<EventSys>, IMonoSingleton
	{
		// 字符串索引的事件
		private Dictionary<string, HashSet<int>> _strFuncDic = new();
		private Queue<(string key, int eid)> _strDelayAdd = new();
		private Dictionary<int, string> _strDelayRemove = new();
		
		// 事件回调
		private Dictionary<int, Delegate> _funcDic = new();
		// 自增事件id
		private int _eid = 0;

		void IMonoSingleton.Destroy()
		{
			// 清理索引字典
			foreach (var strSet in _strFuncDic.Values)
			{
				strSet.Clear();
			}
			_strFuncDic.Clear();
			_funcDic.Clear();
			// 重置事件id
			_eid = 0;
		}

		// 添加字符串索引的事件回调，回调参数为 Object[]
		public int AddListener(string key, EventFunc func)
		{
			if (func == null) throw new ArgumentNullException($"事件系统的回调不能为空 key = {key}");
			int eid = ++_eid;
			_funcDic[eid] = func;
			_strDelayAdd.Enqueue((key, eid));	// 加入延迟添加队列中
			return eid;
		}
		

		public void RemoveListener(string key, int id)
		{
			_strDelayRemove.Add(id, key);		// 加入延迟处理集中
			_funcDic.Remove(id);
		}

		public void Send(string key, params object[] args)
		{
			DelayOperation();
			if (_strFuncDic.TryGetValue(key, out var idSet))
			{
				foreach (var eid in idSet)
				{
					((EventFunc)_funcDic[eid])?.Invoke(args);
				}
			}
		}

		public void DelayOperation()
		{
			// 处理延迟添加
			while (_strDelayAdd.TryDequeue(out var item))
			{
				// 这个回调在延迟移除集合中
				if (_strDelayRemove.ContainsKey(item.eid))
				{
					_strDelayRemove.Remove(item.eid);
					continue;
				}

				// 添加到索引字典中
				if (_strFuncDic.TryGetValue(item.key, out var eidSet) == false)
				{
					_strFuncDic[item.key] = eidSet = new();
				}
				eidSet.Add(item.eid);
			}

			// 处理删除
			foreach (var item in _strDelayRemove)
			{
				if (_strFuncDic.TryGetValue(item.Value, out var eidSet))
				{
					eidSet.Remove(item.Key);
				}
			}
			_strDelayRemove.Clear();
		}
	}
}
