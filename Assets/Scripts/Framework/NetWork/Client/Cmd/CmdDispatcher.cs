using Kusuri;
using System;
using System.Collections.Generic;


namespace KGameClient
{
	public class CmdDispatcher
	{
		private Dictionary<byte, CmdProcessor> _processorDic = new();
		private Queue<(Delegate func, object data)> _cmdQueue = new();
		private Object _lock = new();

		// 解析协议，并将其送入处理队列中，在 Update 中处理
		public void Dispatch(byte tp, byte sub, byte[] data)
		{
			if (_processorDic.TryGetValue(tp, out var proc))
			{
				var item = proc.GetProcessFunc(sub, data);			// 解耦网络接收与协议处理的时机
				if (item.func != null)
				{
					Utils.PrintRecv(tp, sub, data, data == null ? 0 : data.Length);
					lock (_lock)
					{
						// 将 cmd 加入队列中
						_cmdQueue.Enqueue(item);
					}
				}
				else
				{
					Utils.Warn($"没有处理 tp = {tp}, sub = {sub} 的函数");
				}
			}
			else
			{
				Utils.Warn($"没有处理 tp = {tp} 的 Processor");
			}
		}

		public void Update()
		{
			try
			{
				lock (_lock)
				{
					while (_cmdQueue.Count > 0)
					{
						var item = _cmdQueue.Dequeue();
						item.func?.DynamicInvoke(item.data);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{

			}
		}

		public void AddProcessor(byte tp, CmdProcessor proc)
		{
			_processorDic.Add(tp, proc);
		}

		public void Clear()
		{
			_processorDic.Clear();
			_cmdQueue.Clear();
		}
	}
}
