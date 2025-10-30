using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace KGameClient
{
	public delegate void CmdFunc<T>(T arg);
	public delegate void CmdFunc();
	public abstract class CmdProcessor
	{
		protected Dictionary<byte, (Type type, Delegate func)> _funcDic = new();

		public CmdProcessor()
		{
			Init();
		}

		public abstract void Init();

		protected void AddProcFunc<T>(byte sub, CmdFunc<T> func)
		{
			_funcDic.Add(sub, (typeof(T), func));
		}
		protected void AddProcFunc(byte sub, CmdFunc func)
		{
			_funcDic.Add(sub, (null, func));
		}

		public (Delegate func, object data) GetProcessFunc(byte sub, byte[] rawData)
		{
			if (_funcDic.TryGetValue(sub, out var item))
			{
				if (rawData == null)
				{
					return (item.func, null);
				}
				else
				{
					string json = Encoding.UTF8.GetString(rawData);
					if (item.type != null) return (item.func, JsonConvert.DeserializeObject(json, item.type));
					else return (item.func, JObject.Parse(json));
				}
			}
			return (null, null);
		}

		public bool Process(byte small, byte[] data)
		{
			if (_funcDic.TryGetValue(small, out var item))
			{
				if (data == null)
				{
					item.func?.DynamicInvoke();
				}
				else
				{
					string json = Encoding.UTF8.GetString(data);
					if (item.type != null) item.func?.DynamicInvoke(JsonConvert.DeserializeObject(json, item.type));
					else item.func?.DynamicInvoke(JObject.Parse(json));
				}

			}
			return false;
		}
	}
}
