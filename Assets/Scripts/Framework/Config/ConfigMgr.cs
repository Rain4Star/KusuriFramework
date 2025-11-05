using KConfig;
using Kusuri;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class ConfigMgr : MonoSingleton<ConfigMgr>
{
	internal class ConfigDic
	{
		public float lastCheck;
		public object dic;
		public uint visit;
	}

	public static readonly string _cfgPath = Path.Combine(Application.dataPath, "Scripts", "JsonData", "Config");
	public static string uiAss = "Kusuri.KModeView";
	public static string uiRelativePath = Path.Combine("Resources", "Prefab", "UI");            // ui 预设相对目录
	public static string uiPath = Path.Combine(Application.dataPath, uiRelativePath);           // ui 预设目录
	public static string scPath = Path.Combine(Application.dataPath, "Scripts", "KGame", "ModelView", "ViewCtrl");      // ui 脚本目录

	private Dictionary<string, ConfigDic> _cfgDic = new();

	private Queue<(string, ConfigDic)>[] _checkQueue;
	private uint _checkIdx = 0;

	protected override void Init()
	{
		_checkQueue = new Queue<(string, ConfigDic)>[3];
		for (int i = 0; i < 3; i++) _checkQueue[i] = new();
	}

	public void Awake()
	{
		InvokeRepeating("CheckNotUse", 600, 600);		// 10 分钟检测一波
	}

	private void CheckNotUse()
	{
		for (int i = 0; i <= _checkIdx; i++)
		{
			var queue = _checkQueue[i];
			int cnt = queue.Count;
			int lim = (32 << i);
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

	public T GetCfgByMainKey<T>(int id)
	{
		Type type = typeof(T);
		if (_cfgDic.TryGetValue(type.Name, out ConfigDic obj) == false)
		{
			if (TryLoadCfg<T>(type) == true) obj = _cfgDic[type.Name];
		}
		obj.visit++;
		if (obj.dic is Dictionary<int, T> dic)
		{
			if (dic.ContainsKey(id)) return dic[id];
		}
		return default;
	}
	//public T GetCfgByMainKey<T>(string mainKey)
	//{
	//	Type type = typeof(T);
	//	if (_cfgDic.TryGetValue(type.Name, out object obj))
	//	{
	//		if (obj is Dictionary<string, T> dic)
	//		{
	//			if (dic.ContainsKey(mainKey)) return dic[mainKey];
	//		}
	//	}
	//	else
	//	{
	//	}
	//	return default;
	//}

	private bool TryLoadCfg<T>(Type type)
	{
		if (_cfgDic.ContainsKey(type.Name)) return false;
		CfgItemAttribute att = (CfgItemAttribute)Attribute.GetCustomAttribute(type, typeof(CfgItemAttribute));
		if (att == null) return false;
		string path = Path.Combine(_cfgPath, att.resPath);
		if (File.Exists(path) == false) return false;
		try
		{
			var cfg = JsonConvert.DeserializeObject<T[]>(File.ReadAllText(path));
			// 反射获取字段
			FieldInfo field = type.GetField(att.mainKey, BindingFlags.Public | BindingFlags.Instance);
			Dictionary<int, T> dic = new(cfg.Length);
			foreach (var elem in cfg)
			{
				dic[(int)field.GetValue(elem)] = elem;
			}
			ConfigDic cfgDic = new();
			cfgDic.lastCheck = Time.time;
			cfgDic.dic = dic;
			_cfgDic[type.Name] = cfgDic;
			// 参与代际回收
			if (att.notClear == false) _checkQueue[0].Enqueue((type.Name, cfgDic));
			return true;
		}
		catch
		{
			return false;
		}
	}


	public Dictionary<int, T> GetCfgDic<T>()
	{
		Type type = typeof(T);
		if (_cfgDic.TryGetValue(type.Name, out var cfgDic) == false)
		{
			if (TryLoadCfg<T>(type) == true)
			{
				cfgDic = _cfgDic[type.Name];
			}
		}
		cfgDic.visit += 8;		// TO_OPTIMIZE 获取表，这里不知道如何处理访问计数，暂且直接加一个大的值
		if (cfgDic.dic is Dictionary<int, T> dic) return dic;
		else return null;
	}
}
