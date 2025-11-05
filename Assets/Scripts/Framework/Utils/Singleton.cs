using System;
using UnityEngine;

namespace Kusuri
{
	//public class Singleton<T> where T : Singleton<T>, new()
	//{
	//	private static T _ins;
	//	/// <summary>
	//	/// 单例
	//	/// </summary>
	//	public static T Ins => _ins;

	//	static Singleton()
	//	{
	//		_ins = new T();
	//		Utils.Print($"<color=#ca0000><size=18>Create Singleton {typeof(T).Name}</size></color>");
	//		_ins.Init();
	//	}

	//	protected Singleton() { }

	//	protected virtual void Init() { }
	//}

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
						Utils.Print($"Create <color=#00ca00>{go.name}</color>");
						_ins = go.AddComponent<T>();
					}
					_ins.Init();
					GameObject.DontDestroyOnLoad(_ins.gameObject);
				}
				return _ins;
			}
		}

		protected MonoSingleton() { }

		protected virtual void Init() { }

		public virtual void OnApplicationQuit()
		{
			Utils.Print($"Delete <color=#ca0000>Ins_{typeof(T).Name}</color>");
			_ins = null;
		}
	}
}
