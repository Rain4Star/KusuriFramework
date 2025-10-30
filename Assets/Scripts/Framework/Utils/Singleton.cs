using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kusuri
{
	public interface IMonoSingleton
	{
		public virtual void Start() { }
		public virtual void OnEnable() { }
		public virtual void OnDisable() { }
		public virtual void Update() { }
		public virtual void Destroy() { }
	}

	public class Singleton<T> where T : Singleton<T>, new()
	{
		private static T _ins;
		/// <summary>
		/// 单例
		/// </summary>
		public static T Ins => _ins;

		static Singleton()
		{
			_ins = new T();
			Utils.Print($"<color=#ca0000><size=18>Create Singleton {typeof(T).Name}</size></color>");
			if (_ins is IMonoSingleton)
			{
				MonoSingletonMgr.Add(_ins as IMonoSingleton);
			}
			_ins.Init();
		}

		protected Singleton() { }

		protected virtual void Init() { }
	}

	public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>, new()
	{
		private static T _ins;
		public static T Ins
		{
			get
			{
				if (_ins == null)
				{
					_ins = GameObject.FindObjectOfType<T>() as T;
					if (_ins == null)
					{
						GameObject go = new GameObject($"Ins_{typeof(T).Name}");
						_ins = go.AddComponent<T>();
						_ins.Init();
						GameObject.DontDestroyOnLoad(go);
					}
				}
				return _ins;
			}
		}

		protected MonoSingleton() { }

		protected virtual void Init() { }

		public virtual void Clear() { }
	}


	/// <summary>
	/// 普通 Singleton 单例生命周期管理器
	/// </summary>
	public class MonoSingletonMgr
	{
		public static bool isInGame = false;

		private static List<IMonoSingleton> _monoInsList = new();

		public static void Add(IMonoSingleton ins)
		{
			_monoInsList.Add(ins);
		}

		public static void Update()
		{
			int cnt = _monoInsList.Count;
			
			for (int i = 0; i < cnt; i++)
			{
				
				_monoInsList[i].Update();
			}
		}

		public static void OnDestroy()
		{
			foreach (var ins in _monoInsList)
			{
				ins.Destroy();
			}
		}
	}
}
