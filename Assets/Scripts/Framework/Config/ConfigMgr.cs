using Kusuri;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace KConfig
{
	public class ConfigMgr : MonoSingleton<ConfigMgr>
	{
		public static string atlaPath = Path.Combine("ArtSrc", "Atlas");

		public static string cfgPath = Path.Combine(Application.dataPath, "Scripts", "JsonData", "Config");
		public static string uiAss = "Kusuri.KModeView";
		public static string uiRelativePath = Path.Combine("Resources", "Prefab", "UI");            // ui 预设相对目录
		public static string uiPath = Path.Combine(Application.dataPath, uiRelativePath);           // ui 预设目录
		public static string scPath = Path.Combine(Application.dataPath, "Scripts", "KGame", "ModelView", "ViewCtrl");      // ui 脚本目录

		private Dictionary<string, CfgObj> _cfgDic = new();

		private Queue<(string, CfgObj)>[] _checkQueue;
		private uint _checkIdx = 0;

		protected override void Init()
		{
			_checkQueue = new Queue<(string, CfgObj)>[3];
			for (int i = 0; i < 3; i++) _checkQueue[i] = new();
		}

		public void Awake()
		{
			InvokeRepeating("CheckNotUse", 600, 600);       // 10 分钟检测一波
		}

		// T 带有 CfgItem 注解时，可以调用
		public CfgObj GetCfgObj<T>()
		{
			Type type = typeof(T);
			if (_cfgDic.TryGetValue(type.Name, out var obj)) return obj;
			if (_cfgDic.ContainsKey(type.Name)) return _cfgDic[type.Name];
			CfgItemAttribute att = (CfgItemAttribute)Attribute.GetCustomAttribute(type, typeof(CfgItemAttribute));
			if (att == null) return null;
			return LoadCfg<T>(type.Name, att.mainKey, att.resPath, att.notClear);
		}
		// T 没有 CfgItem 注解时，可以调用
		public CfgObj GetCfgObj<T>(string path, string mainKey = null, bool notClear = false)
		{
			if (_cfgDic.TryGetValue(path, out var obj)) return obj;
			return LoadCfg<T>(path, mainKey, path, notClear);
		}


		private void CheckNotUse()
		{
			for (int i = 0; i <= _checkIdx; i++)
			{
				var queue = _checkQueue[i];
				int cnt = queue.Count;
				int lim = 32 << i;
				float time = Time.time;
				while (cnt > 0)
				{
					cnt--;
					var (key, cfgDic) = queue.Dequeue();
					// 访问数够多或者是近期建立的
					if (cfgDic.visit > lim || time - cfgDic.lastCheck < 480.0f)
					{
						cfgDic.lastCheck = time;
						cfgDic.visit = 0;
						queue.Enqueue((key, cfgDic));
						continue;
					}
					// 从字典中移除
					_cfgDic.Remove(key);
					Utils.Print($"Rmove config {key}");
				}
			}
			_checkIdx++;
			if (_checkIdx == _checkQueue.Length) _checkIdx = 0;
		}
		private CfgObj LoadCfg<T>(string key, string mainKey, string path, bool notClear)
		{
			path = Path.Combine(cfgPath, path);
			if (File.Exists(path) == false)
			{
				Utils.Error($"不存在配置文件: {path}");
				return null;
			}
			try
			{
				string json = File.ReadAllText(path);
				CfgObj obj = new();
				obj.lastCheck = Time.time;
				_cfgDic[key] = obj;
				if (string.IsNullOrEmpty(mainKey))
				{
					// 加载为 list
					var list = JsonConvert.DeserializeObject<List<T>>(json);
					obj.data = list;
					obj.type = CfgObj.ECFG_TYPE.List;
					obj.count = list.Count;
				}
				else
				{
					// 加载为 int -> T 的字典
					T[] cfg = JsonConvert.DeserializeObject<T[]>(json);
					FieldInfo field = typeof(T).GetField(mainKey);
					Type fType = field.FieldType;

					if (fType == typeof(string))
					{
						Dictionary<string, T> dic = new(cfg.Length);
						foreach (var elem in cfg) dic[(string)field.GetValue(elem)] = elem;
						obj.data = dic;
						obj.type = CfgObj.ECFG_TYPE.StringDic;
					}
					else if (fType == typeof(int))
					{
						Dictionary<int, T> dic = new(cfg.Length);
						foreach (var elem in cfg) dic[(int)field.GetValue(elem)] = elem;
						obj.data = dic;
						obj.type = CfgObj.ECFG_TYPE.IntDic;
					}
					obj.count = cfg.Length;
				}
				if (notClear == false) _checkQueue[0].Enqueue((key, obj));
				return obj;
			}
			catch
			{
				return null;
			}
		}
	}
}