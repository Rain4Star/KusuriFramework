using Kusuri;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KUISys
{
	public class UIMgr : MonoSingleton<UIMgr>
	{
		public static string uiAss = "Kusuri.KModeView";
		public static string uiRelativePath = Path.Combine("Resources", "Prefab", "UI");			// ui 预设相对目录
		public static string uiPath = Path.Combine(Application.dataPath, uiRelativePath);			// ui 预设目录
		public static string scPath = Path.Combine(Application.dataPath, "Scripts", "KGame", "ModelView", "ViewCtrl");      // ui 脚本目录

		private Dictionary<Type, (HashSet<GameObject> use, Queue<UIElem> notUse)> _elemPool = new();
		private Dictionary<Type, UIElem> _windowPool = new();
		
		public Transform _poolRootTF;
		//[SerializeField]
		public RectTransform[] _uiLayerTF;

		public void Awake()
		{
			DontDestroyOnLoad(this);
			InvokeRepeating("RemoveNotUse", 360.0f, 360.0f);			// 每 6 分钟清理一次 池子 和 Resources 中未使用的资源
		}

		public void OnDestroy()
		{
			
		}

		// 方便热更测试用的，热更要清理池子中的资源
		private void OnDisable()
		{
			foreach (var queue in _elemPool.Values)
			{
				// 清理池子中的资源
				while (queue.notUse.Count > 0)
				{
					Destroy(queue.notUse.Dequeue().gameObject);
				}
				queue.notUse.Clear();
				foreach (var item in queue.use)
				{
					Destroy(item.gameObject);
				}
				queue.use.Clear();
			}
			foreach (var elem in _windowPool.Values)
			{
				Destroy(elem);
			}
			_windowPool.Clear();
		}

		// 清理池子中的资源
		private void RemoveNotUse()
		{
			foreach (var item in _elemPool)
			{
				UIElemAttribute uiElemAtt = (UIElemAttribute)Attribute.GetCustomAttribute(item.Key, typeof(UIElemAttribute));
				if (uiElemAtt?.autoRemove == false) continue;
				foreach (var notUse in item.Value.notUse)
				{
					Destroy(notUse.gameObject);
				}
				item.Value.notUse.Clear();
			}
			foreach (var elem in _windowPool.Values)
			{
				UIElemAttribute uiElemAtt = (UIElemAttribute)Attribute.GetCustomAttribute(elem.GetType(), typeof(UIElemAttribute));
				if (uiElemAtt?.autoRemove == false) continue;
				Destroy(elem);
			}
			_windowPool.Clear();
			Resources.UnloadUnusedAssets();
		}

		// 获取 ui 场景相对 assets 的路径
		public static string GetUIScenePath(string resPath)
		{
			return Path.Combine("Scene", "UIScene", $"{resPath}.unity");
		}
		// 获取 ui 场景相对 assets 的路径
		public static string GetUIScenePath(Type type)
		{
			UIElemAttribute att = Attribute.GetCustomAttribute(type, typeof(UIElemAttribute)) as UIElemAttribute;
			if (att == null)
			{
				Utils.Error($"类型 {type.Name} 没有添加 UIElemAttribute 注解");
				return null;
			}
			return GetUIScenePath(att.resPath);
		}

		public void SetLayer(GameObject uiGo, int layer)
		{
			if (layer >= 0 && layer < _uiLayerTF.Length)
			{
				uiGo.transform.SetParent(_uiLayerTF[layer], false);
			}
		}


		// 加载 UIElem 预设
		public static UnityEngine.Object LoadUIElemPrefab(Type type)
		{
			UIElemAttribute att = (UIElemAttribute)Attribute.GetCustomAttribute(type, typeof(UIElemAttribute));
			string path = Path.Combine("Prefab", "UI", att.resPath);
			return Resources.Load(path);
		}

		// 异步加载 UIElem 预设
		public static ResourceRequest LoadUIElemPrefabAsync(Type type)
		{
			UIElemAttribute att = (UIElemAttribute)Attribute.GetCustomAttribute(type, typeof(UIElemAttribute));
			string path = Path.Combine("Prefab", "UI", att.resPath);
			// TO_DO：处理 uiElem 的依赖，此处暂时没添加热更新机制，暂且先不处理
			return Resources.LoadAsync(path);
		}

		/// <summary>
		/// 打开界面，并加载依赖
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onOpenFunc">界面加载完成后的回调</param>
		/// <param name="layer">指定界面层级，-1 按 UIElem 注解指定的设置</param>
		public void OpenWindow<T>(Action<T> onOpenFunc = null, int layer = -1) where T : UIElem
		{
			Type type = typeof(T);
			if (_windowPool.TryGetValue(type, out var wnd) == false)
			{
				var prefab = LoadUIElemPrefabAsync(type);
				prefab.completed += (_) =>
				{
					// 实例化预制体
					var go = Instantiate(prefab.asset) as GameObject;
					go.name = type.Name;
					// 获取 UIElem 组件
					T elem = go.GetComponent<T>();
					_windowPool[type] = elem;
					// 设置父节点
					if (layer == -1) SetLayer(go, elem.Layer());
					else SetLayer(go, layer);

					go.SetActive(true);
					// 执行打开界面回调
					onOpenFunc?.Invoke(elem);
				};
			}
			else if (wnd.gameObject.activeInHierarchy == false)
			{
				// 设置父节点
				if (layer == -1) SetLayer(wnd.gameObject, wnd.Layer());
				else SetLayer(wnd.gameObject, layer);
				wnd.gameObject.SetActive(true);
				// 执行打开界面回调
				onOpenFunc?.Invoke(wnd as T);
			}
		}
		// 关闭一个 ui 界面
		public void CloseWindow<T>()
		{
			Type type = typeof(T);
			if (_windowPool.TryGetValue(type, out var wnd))
			{
				//wnd.transform.parent = _poolRootTF;
				wnd.transform.SetParent(_poolRootTF, false);
				wnd.gameObject.SetActive(false);
			}
		}

		// 从 ui 池子中借一个 UIElem
		public T RentElem<T>(Transform parent) where T : UIElem
		{
			Type type = typeof(T);
			if (_elemPool.TryGetValue(type, out var queue) == false)
			{
				queue.use = new();
				queue.notUse = new();
				_elemPool.Add(type, queue);
			}
			// 池子中没有没有使用的了
			if (queue.notUse.Count == 0)
			{
				var prefab = LoadUIElemPrefab(type);
				var go = Instantiate(prefab, parent) as GameObject;
				T elem = go.GetComponent<T>();
				queue.use.Add(go);
				elem.gameObject.SetActive(true);
				return elem;
			}
			// 池子中有没使用的，直接拿一个
			else
			{
				UIElem elem = queue.notUse.Dequeue();
				queue.use.Add(elem.gameObject);
				elem.transform.SetParent(parent, false);
				elem.gameObject.SetActive(true);
				return elem as T;
			}

		}

		// 将 UIElem 还回 ui 池子
		public void ReturnElem(UIElem elem)
		{
			Type type = elem.GetType();		// 获取运行时类型
			if (_elemPool.TryGetValue(type, out var queue) == false) return;
			queue.use.Remove(elem.gameObject);
			queue.notUse.Enqueue(elem);
		}

		// 直接添加一个 UIElem
		public T AddElem<T>(Transform parent) where T : UIElem
		{
			Type type = typeof(T);
			var prefab = LoadUIElemPrefab(type);
			var go = Instantiate(prefab, parent) as GameObject;
			return go.GetComponent<T>();
		}
	}
}