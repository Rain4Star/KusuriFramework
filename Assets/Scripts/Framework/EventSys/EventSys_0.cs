//using Kusuri;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;
//using UnityEngine.Profiling;

//namespace KEventSys
//{
	
	




//	public class EventSys_0 : Singleton<EventSys_0>, IMonoUpdate
//	{
//		// 参数基于 string 的回调字典
//		private Dictionary<string, HashSet<int>> _paramFuncDic = new();
//		private Dictionary<int, Delegate> _funcDic = new();
//		// 主线程事件队列
//		private Queue<(string key, object[] args)> _mainQueue = new();
//		// 延迟添加，一来防止多线程在运行时修改，而来防止递归注册事件
//		private ConcurrentQueue<(string key, int id, Delegate func)> _delayAddQueue = new();
//		private ConcurrentDictionary<int, string> _delayRemoveDic = new();
//		private int _funcId = 0;

//		private ReaderWriterLockSlim _lock = new();

//		void IMonoUpdate.Update()
//		{
//			_lock.EnterReadLock();
//			try
//			{
//				int cnt = 0;
//				while (_mainQueue.TryDequeue(out (string key, object[] args) item) && cnt++ < 32)   // 队列不为空，且限制一帧最多处理 32 条事件
//				{
//					Utils.Print($"{item.key}    {JsonConvert.SerializeObject(item.args)}", "发送事件");
//					if (_paramFuncDic.TryGetValue(item.key, out var set))
//					{
//						foreach (var funcId in set)
//						{
//							((EventFunc)_funcDic[funcId])?.Invoke(item.args);
//						}
//					}
//				}
//			}
//			catch (Exception ex)
//			{
//				throw ex;
//			}
//			finally { _lock.ExitReadLock(); }
//		}



//		void IMonoUpdate.Destroy()
//		{
//			_lock.EnterWriteLock();
//			try
//			{
//				foreach (var kvp in _paramFuncDic)
//				{
//					kvp.Value.Clear();
//				}
//				_paramFuncDic.Clear();
//				_delayAddQueue.Clear();
//				_delayRemoveDic.Clear();
//				_funcDic.Clear();
//			}
//			catch (Exception ex)
//			{
//				throw ex;
//			}
//			finally
//			{
//				_lock.ExitWriteLock();
//				//_lock.Dispose();
//			}
//		}
//		#region 事件处理
//		/// <summary>
//		/// 添加监听
//		/// </summary>
//		/// <param name="key">事件标识: 若使用泛型 Send()，该参数必须为 typeof(T).Name，Where T : EventParam </param>
//		/// <param name="func">事件回调</param>
//		/// <returns>事件回调自增 id</returns>
//		/// <exception cref="ArgumentNullException"></exception>
//		public int AddListener(string key, EventFunc func)
//		{
//			if (func == null) throw new ArgumentNullException($"回调为空:{nameof(func)}");

//			int res = Interlocked.Increment(ref _funcId);
//			_delayAddQueue.Enqueue((key, res, func));
//			return res;
//		}
//		//public int AddListener<T>(EventFuncGeneric<T> func) where T : EventParam
//		//{
//		//	if (func == null) throw new ArgumentNullException($"回调为空:{nameof(func)}");
//		//	int res = Interlocked.Increment(ref _funcId);
//		//	_delayAddQueue.Enqueue((typeof(T).Name, res, func));
//		//	return res;
//		//}


//		/// <summary>
//		/// 移除监听回调
//		/// </summary>
//		/// <param name="key">事件标识: 若使用泛型 Send()，该参数必须为 typeof(T).Name，Where T : EventParam </param>
//		/// <param name="id">事件回调 id，id 为 0 则移除该事件的所有回调，其他则移除指定 id 的回调</param>
//		public void RemoveListener(string key, int id)
//		{
//			_delayRemoveDic.TryAdd(id, key);
//		}

//		public void SendMain(string key, params object[] args)
//		{
//			Profiler.BeginSample("SEND_MAIN");
//			ProcessDelayOperation();
//			_mainQueue.Enqueue((key, args));
//			Profiler.EndSample();
//		}

//		/// <summary>
//		/// 泛型方法 推送事件
//		/// </summary>
//		/// <typeparam name="T"></typeparam>
//		/// <param name="param">事件回调参数</param>
//		//public void Send<T>(T param) where T : EventParam
//		//{
//		//	ProcessDelayOperation();

//		//	string key = typeof(T).Name;
//		//	if (!_paramFuncDic.ContainsKey(key)) return;

//		//	try
//		//	{
//		//		_lock.EnterReadLock();
//		//		foreach (var funcId in _paramFuncDic[key])
//		//		{
//		//			((EventFuncGeneric<T>)_funcDic[funcId])?.Invoke(param);
//		//		}
//		//	}
//		//	catch (Exception ex)
//		//	{
//		//		Debug.LogException(ex);
//		//	}
//		//	finally
//		//	{
//		//		_lock.ExitReadLock();
//		//	}
//		//}

//		/// <summary>
//		/// 可变参数 推送事件
//		/// </summary>
//		/// <param name="key">事件标识</param>
//		/// <param name="args">回调参数</param>
//		public void Send(string key, params object[] args)
//		{
//			Profiler.BeginSample("SEND");
//			ProcessDelayOperation();
//			_lock.EnterReadLock();
//			try
//			{
//				if (_paramFuncDic.TryGetValue(key, out var set))
//				{
//					foreach (var funcId in set)
//					{
//						((EventFunc)_funcDic[funcId])?.Invoke(args);
//					}
//				}
//			}
//			catch (Exception ex)
//			{
//				Debug.LogException(ex);
//			}
//			finally
//			{
//				_lock.ExitReadLock();
//			}
//			Profiler.EndSample();
//		}

//		/// <summary>
//		/// 处理延迟添加和删除操作
//		/// </summary>
//		private void ProcessDelayOperation()
//		{
//			_lock.EnterWriteLock();
//			try
//			{
//				// 处理添加
//				while (_delayAddQueue.TryDequeue(out var item))
//				{
//					// 该事件回调在移除列表中，不添加它
//					if (_delayRemoveDic.ContainsKey(item.id))
//					{
//						_delayRemoveDic.TryRemove(item.id, out _);
//						continue;
//					}
//					if (_paramFuncDic.TryGetValue(item.key, out var set) == false)
//					{
//						set = new();
//						_paramFuncDic[item.key] = set;
//					}
//					set.Add(item.id);
//					_funcDic.Add(item.id, item.func);
//				}

//				// 处理删除
//				foreach (var item in _delayRemoveDic)
//				{
//					// 处理移除所有监听
//					if (item.Key == 0)
//					{
//						foreach (int id in _paramFuncDic[item.Value])
//						{
//							_funcDic.Remove(id);
//						}
//						_paramFuncDic.Remove(item.Value);
//						continue;
//					}
//					// 处理移除单个监听
//					if (_paramFuncDic.ContainsKey(item.Value))
//					{
//						_paramFuncDic[item.Value].Remove(item.Key);
//					}
//					_funcDic.Remove(item.Key);
//				}
//				_delayRemoveDic.Clear();
//			}
//			catch (Exception ex)
//			{
//				Debug.LogException(ex);
//			}
//			finally
//			{
//				_lock.ExitWriteLock();
//			}
//		}
//		#endregion
//	}
//}