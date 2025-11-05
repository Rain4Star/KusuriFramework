using System;
using System.Diagnostics;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;

using Debug = UnityEngine.Debug;

namespace Kusuri
{
	public static class Utils
	{
		public static bool netDebug = true;
		public static bool debug = true;
		public static bool eventDebug = true;

		/// <summary>
		/// 在 offset 位置，写入一个 int 到 buffer 中
		/// </summary>
		/// <param name="val">要写入的值</param>
		/// <param name="buffer">目标数组</param>
		/// <param name="offset">偏移量</param>
		public static void IntToByte(int val, byte[] buffer, int offset = 0)
		{
			for (int i = 0; i < 4; i++)
			{
				buffer[i + offset] = (byte)((val >> (i * 8)) & 0xff);
			}
		}

		public static void Error(string str, string info = null)
		{
			if (info == null) Debug.LogError(str);
			else Debug.LogError($"[{info}]: {str}");
		}
		public static void Error(object obj, string info = null)
		{
			if (obj == null) return;
			try
			{
				if (string.IsNullOrEmpty(info))
				{
					Debug.LogError(JsonConvert.SerializeObject(obj));
				}
				else
				{
					Debug.LogError($"[{info}]: {JsonConvert.SerializeObject(obj)}");
				}
			}
			catch (Exception ex) 
			{
				throw ex;
			}
			
		}


		[Conditional("__DEBUG__")]
		public static void Warn(string str, string info = null)
		{
			if (debug == false) return;
			if (info == null) Debug.LogWarning(str);
			else Debug.LogWarning($"[{info}]: {str}");
		}
		[Conditional("__DEBUG__")]
		public static void Warn(object obj, string info = null)
		{
			if (debug == false) return;
			try
			{
				if (string.IsNullOrEmpty(info))
				{
					Debug.LogWarning(JsonConvert.SerializeObject(obj));
				}
				else
				{
					Debug.LogWarning($"[{info}]: {JsonConvert.SerializeObject(obj)}");
				}
			}
			catch {}
		}

		[Conditional("__DEBUG__")]
		public static void Print(string str, string info = null)
		{
			if (debug == false) return;
			if (info == null) Debug.Log(str);
			else Debug.Log($"[{info}]: {str}");
		}
		[Conditional("__DEBUG__")]
		public static void Print(object obj, string info = null)
		{
			if (debug == false) return;
			try
			{
				if (string.IsNullOrEmpty(info))
				{
					Debug.Log(JsonConvert.SerializeObject(obj));
				}
				else
				{
					Debug.Log($"[{info}]: {JsonConvert.SerializeObject(obj)}");
				}
			}
			catch { }
		}


		[Conditional("__DEBUG__")]
		public static void PrintEvent(string key, int cbCnt, params object[] args)
		{
			if(eventDebug == false) return;
			try
			{
				StringBuilder sb = new();
				foreach (var arg in args)
				{
					sb.Append($"{arg}\n");
				}
				Debug.Log($"[Event]: key = <color=#ca0065>{key}</color>, call back count = <color=#ca0065>{cbCnt}</color>\n{sb.ToString()}");
			}
			catch { }
		}

		[Conditional("__NET_DEBUG__")]
		public static void PrintSend(byte tp, byte sub, string data, int len)
		{
			if (netDebug == false) return;
			Debug.Log($"<color=#00ca00><size=18><<== Send  {tp} - {sub}</size></color>    len = {len}{(string.IsNullOrEmpty(data) ? "" : $"\n{data} ")}");
		}
		[Conditional("__NET_DEBUG__")]
		public static void PrintRecv(byte tp, byte sub, byte[] data, int len)
		{
			if (netDebug == false) return;
			string json = JsonConvert.SerializeObject(data);
			Debug.Log($"<color=#caca00><size=18>==>> Recv  {tp} - {sub}</size></color>    len = {len}{(string.IsNullOrEmpty(json) ? "" : $"\n{json} ")}");
		}


	}
}


