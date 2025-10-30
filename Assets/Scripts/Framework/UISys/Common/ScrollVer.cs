using System;
using UnityEngine;

namespace KUISys
{
	public class ScrollVer : ScrollBase
	{
		private float gap = 0;

		public new void Awake()
		{
			base.Awake();
			gap = spacing.y + cellSize.y;
		}

		protected override void ResizeContent()
		{
			// 设置 content 的高度
			_contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemCnt * cellSize.y + (itemCnt - 1) * spacing.y - padding.y + padding.w);

			// 计算所用 item 的位置
			int oldCnt = _itemPosList.Count;
			float y = oldCnt * gap + padding.y;		// 起始位置
			
			while (oldCnt++ < itemCnt)
			{
				_itemPosList.Add(new Vector2(padding.x, y));
				y -= gap;
			}
		}

		protected override void MakeContent()
		{
			// 将视窗的 顶和底 坐标转化到 content 的坐标系中
			Vector2 top = new Vector2(_viewRect.rect.width/ 2, _viewRect.rect.height/ 2);
			Vector2 bot = new Vector2(top.x, -top.y);
			top = _contentRect.InverseTransformPoint(_viewRect.TransformPoint(top));
			bot = _contentRect.InverseTransformPoint(_viewRect.TransformPoint(bot));

			// 判断哪些 item 需要隐藏
			_itemToHideList.Clear();
			
			int lim = Math.Min(itemCnt, (int)((-bot.y + gap) / gap));
			int beg = Math.Max(0, (int)((-top.y - gap) / gap));
			foreach (var item in _showDic)
			{
				// 不在视窗内，把他加入隐藏列表中
				if (IsInViewPort(item.Key, 0, 0, top.y, bot.y) == false)
				{
					_itemToHideList.Add(item.Key);
				}
			}
			foreach (var idx in _itemToHideList) HideItem(idx);

			// 显示在视窗中的
			for (int i = beg; i < lim; i++)
			{
				if (_showDic.ContainsKey(i) == false && IsInViewPort(i, 0, 0, top.y, bot.y))
				{
					ShowItem(i);
				}
			}
		}

		protected override bool IsInViewPort(int idx, float left, float right, float top, float bot)
		{
			Vector2 pos = _itemPosList[idx];
			return pos.y - 5 > bot && pos.y - gap + 5 < top;
		}
	}
}
