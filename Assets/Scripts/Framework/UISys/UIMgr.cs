using KEventSys;
using Kusuri;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace KUISys
{
	public class UIMgr : MonoSingleton<UIMgr>
	{
		private Dictionary<Type, (HashSet<UIElem> use, Queue<UIElem> notUse)> _elemPool = new();
		private Dictionary<Type, (UIElem ui, UIElemAttribute att, int layer)> _wndDic = new();
		
		public Transform _poolRootTF;
		public RectTransform[] _uiLayerTF;
		private List<HashSet<Type>> _uiStk;       // ui 堆栈管理
		private int _uiStkTop = 0;

		private List<Text> _multiLanguageText;

		public void Awake()
		{
			DontDestroyOnLoad(this);
			InvokeRepeating("RemoveNotUse", 360.0f, 360.0f);            // 每 6 分钟清理一次 池子 和 Resources 中未使用的资源
			_uiStk = new();
			_multiLanguageText = new();
		}

		public override void OnApplicationQuit()
		{
			base.OnApplicationQuit();
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
		}

		// 清理池子中的资源
		private void RemoveNotUse()
		{
			foreach (var item in _elemPool)
			{
				foreach (var notUse in item.Value.notUse)
				{
					Destroy(notUse.gameObject);
				}
				item.Value.notUse.Clear();
			}
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


		
		private void InitMultiLanguage(UIElem elem)
		{
			_multiLanguageText.Clear();
			elem.GetComponentsInChildren<Text>(true, _multiLanguageText);
			foreach (var txt in _multiLanguageText)
			{
				string lang = txt.text;
				if (string.IsNullOrEmpty(lang)) continue;
			}
		}

		#region UI 管理模块 TO_OPTIMIZE:这几个函数耦合度有点高，可维护性有待改善
		/// <summary>
		/// 打开界面，并加载依赖
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onOpenFunc">界面加载完成后的回调</param>
		/// <param name="layer">指定界面层级，-1 按 UIElem 注解指定的设置</param>
		public void OpenWindow<T>(int layer = -1) where T : UIElem => OpenWindow(typeof(T), layer);	
		public void OpenWindow(Type type, int layer = -1)
		{
			InnerOpenWindow(type, true, layer);
		}
		private void InnerOpenWindow(Type type, bool processStack = false, int layer = -1)
		{
			if (_wndDic.TryGetValue(type, out var item) == false)
			{
				UIElemAttribute att = (UIElemAttribute)Attribute.GetCustomAttribute(type, typeof(UIElemAttribute));
				if (layer == -1) layer = att.layer;

				var prefab = LoadUIElemPrefabAsync(type);
				prefab.completed += (_) =>
				{
					// 实例化预制体
					var go = Instantiate(prefab.asset) as GameObject;
					go.name = type.Name;
					// 获取 UIElem 组件
					UIElem elem = go.GetComponent<UIElem>();
					_wndDic[type] = (elem, att, layer);
					// 设置父节点
					SetLayer(go, layer);
					go.SetActive(true);
					EventSys.Ins.Send("UI_OPEN_WINDOW", elem);
					if (processStack) ProcessOpenStack(type);
				};
			}
			else if (item.ui.gameObject.activeInHierarchy == false)
			{
				if (layer == -1) layer = item.layer;
				// 设置父节点
				SetLayer(item.ui.gameObject, layer);
				item.ui.gameObject.SetActive(true);
				EventSys.Ins.Send("UI_OPEN_WINDOW", item.ui);
				if (processStack) ProcessOpenStack(type);
			}
		}

		// 关闭一个 ui 界面
		public void CloseWindow<T>() where T : UIElem => CloseWindow(typeof(T));
		public void CloseWindow(Type type)
		{
			ProcessCloseStack(type);
			InnerCloseWindow(type);
		}
		private void InnerCloseWindow(Type type)
		{
			if (_wndDic.TryGetValue(type, out var item))
			{
				Destroy(item.ui.gameObject);
				_wndDic.Remove(type);
			}
		}

		// 隐藏一个界面
		private void HideWindow(Type type)
		{
			if (_wndDic.TryGetValue(type, out var item))
			{
				item.ui.gameObject.SetActive(false);
			}
		}

		// 打开界面时，维护UI栈
		private void ProcessOpenStack(Type wType)
		{
			if (_wndDic.TryGetValue(wType, out var item) == false)
			{
				Utils.Print($"<color=#ca00ca>{wType.Name}</color>没有创建");
				return;
			}
			int openType = item.att.openType;
			switch (openType)
			{
				case 1:     // 隐藏栈顶
				case 2:     // 关闭栈顶
				{ 
					if(_uiStkTop > 0)
					{
						// 处理栈顶
						HashSet<Type> peek = _uiStk[_uiStkTop - 1];
						if(openType == 1) foreach (Type t in peek) HideWindow(t);
						else foreach (Type t in peek) InnerCloseWindow(t);
					}
					// 入栈，并处理 栈哈希表 的 复用
					if(_uiStkTop < _uiStk.Count) _uiStk[_uiStkTop].Add(wType);
					else
					{
						HashSet<Type> set = new();
						set.Add(wType);
						_uiStk.Add(set);
					}
					_uiStkTop++;
					break;
				}
				case 3:     // 堆式打开，与栈顶合批处理
				{
					if(_uiStkTop > 0)
					{
						// 加入栈顶哈希表中
						HashSet<Type> peek = _uiStk[_uiStkTop - 1];
						peek.Add(wType);
					}
					else
					{ 
						// 空栈，不需要处理栈
					}
					break;
				}
			}
		}
		// 关闭界面时，维护UI栈
		private void ProcessCloseStack(Type wType)
		{
			if (_wndDic.TryGetValue(wType, out var item) == false)
			{
				Utils.Print($"<color=#ca0065>{wType.Name}</color>没有创建");
				return;
			}
			if (item.ui.gameObject.activeInHierarchy == false)
			{
				return;
			}
			int openType = item.att.openType;
			switch (openType)
			{
				case 1:     // 显示次栈顶
				case 2:
				{
					if (_uiStkTop > 0) 
					{
						// 出栈
						HashSet<Type> peek = _uiStk[--_uiStkTop];
						// 关闭同一批的所有界面
						if (peek.Contains(wType))
						{
							foreach (Type t in peek) InnerCloseWindow(t);
							peek.Clear();
						}
						// 处理次栈顶
						if (_uiStkTop > 0)
						{
							var peek2 = _uiStk[_uiStkTop - 1];
							foreach (Type t in peek2) InnerOpenWindow(t, false);
						}
					}
					else
					{
						// 栈式管理，但关闭时没有栈顶，出错了
						Utils.Error($"处理 栈式隐藏 界面 <color=#ca0000>{wType.Name}</color>时发生了错误：栈中没有元素");
					}
					break;
				}
				case 3:     // 不处理栈顶，因为它会在1、2中被操作
				{	
					if (_uiStkTop > 0)
					{
						// 从栈顶哈希表中移除
						HashSet<Type> peek = _uiStk[_uiStkTop - 1];
						peek.Remove(wType);
					}
					// 它在空栈时创建的，和栈同一批处理，直接关闭即可
					InnerCloseWindow(wType);
					break;
				}
			}
		}
		#endregion


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
				queue.use.Add(elem);
				elem.gameObject.SetActive(true);
				return elem;
			}
			// 池子中有没使用的，直接拿一个
			else
			{
				UIElem elem = queue.notUse.Dequeue();
				queue.use.Add(elem);
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
			queue.use.Remove(elem);
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