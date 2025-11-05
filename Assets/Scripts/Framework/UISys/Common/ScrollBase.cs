using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KUISys
{
	[RequireComponent(typeof(ScrollRect))]
	public abstract class ScrollBase : MonoBehaviour
	{
		//==== 滚动列表数据项 ====
		protected GameObject item;      // 对应的预制体
		public Vector2 cellSize;        // item 的大小
		public Vector2 spacing;         // 间隔
		[Tooltip("x/z 水平方向\ny/w 竖直方向")]
		public Vector4 padding;
		protected int itemCnt;          // item 总数
		[SerializeField]
		protected List<Vector2> _itemPosList;       // item 的位置

		protected Dictionary<int, UIElem> _showDic;     // 正在使用的 item 
		protected Queue<UIElem> _itemPool;              // item 对象池
		protected List<int> _itemToHideList;            // 刷新时，需要隐藏的 item 的下标列表


		//==== 相关组件 ====
		protected ScrollRect _svRect;
		protected RectTransform _contentRect;
		protected RectTransform _viewRect;
		protected float _nextFreshTime;

		//==== 回调函数 ====
		protected Action<int, UIElem> _showFunc;
		protected Action<int> _sizeFunc;        // 使用不定高度时，计算高度的回调

		protected abstract void ResizeContent();

		protected abstract void MakeContent();

		protected abstract bool IsInViewPort(int idx, float left, float right, float top, float bot);

		public void Awake()
		{
			// 获取组件
			_svRect = GetComponent<ScrollRect>();
			_contentRect = _svRect.content;
			_viewRect = _svRect.viewport;

			_showDic = new();
			_itemPool = new();
			_itemToHideList = new();
			_itemPosList = new();
		}

		public void OnEnable()
		{
			_svRect.onValueChanged.AddListener(OnDragging);
		}

		public void OnDisable()
		{
			_svRect.onValueChanged.RemoveAllListeners();
		}

		public ScrollBase Init(GameObject prefab, Action<int, UIElem> showFunc, Action<int> sizeFunc = null)
		{
			_sizeFunc = sizeFunc;
			_showFunc = showFunc;

			item = prefab;
			var rectTF = item.GetComponent<RectTransform>();

			rectTF.pivot = rectTF.anchorMax = (rectTF.anchorMin = new Vector2(0, 1));
			
			rectTF.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
			rectTF.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);
			return this;
		}

		public ScrollBase SetItem(GameObject go)
		{
			item = go;
			return this;
		}

		public ScrollBase SetDataNum(int num, bool resize = true)
		{
			itemCnt = num;
			if (resize == false) return this;
			ResizeContent();
			return this;
		}

		public ScrollBase Flush()
		{
			MakeContent();
			return this;
		}

		protected UIElem ShowItem(int idx)
		{
			UIElem ui = null;
			// 已经在显示
			if (_showDic.ContainsKey(idx)) ui = _showDic[idx];
			else if (_itemPool.Count == 0)
			{
				// ui 池子为空，新建一个 item
				var go = Instantiate(item, _contentRect);
				go.name = item.name;
				ui = go.GetComponent<UIElem>();
				_showDic[idx] = ui;
				ui.GetComponent<RectTransform>().anchoredPosition = _itemPosList[idx];
			}
			else
			{
				// 从 ui 池子中取出来一个
				ui = _itemPool.Dequeue();
				_showDic[idx] = ui;
				ui.gameObject.SetActive(true);
				ui.GetComponent<RectTransform>().anchoredPosition = _itemPosList[idx];
			}
			ui.gameObject.name = $"{item.name}__{idx}";

			_showFunc?.Invoke(idx, ui);
			return ui;
		}

		public void HideItem(int idx)
		{
			if (_showDic.ContainsKey(idx) == false) return;
			UIElem go = _showDic[idx];
			_showDic.Remove(idx);
			go.gameObject.SetActive(false);
			_itemPool.Enqueue(go);
		}

		protected void OnDragging(Vector2 scrollPos)
		{
			if (Time.time > _nextFreshTime)
			{
				MakeContent();
				_nextFreshTime = Time.time + 0.03f;
			}
		}
	}
}
