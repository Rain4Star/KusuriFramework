using Kusuri;
using System;
using System.Text;
using UnityEngine;

namespace KUISys
{
	public class ScrollMulti : ScrollBase
	{
		private float _xGap = 0, _yGap = 0;           // 两个方向上的间隔
		[Tooltip("限制 行(ture)/列(false)")]
		public bool limitRowOrCol = false;
		[Tooltip("限制 行/列 数，结合 limitRowOrCol 使用")]
		public int limitCnt = 4;

		private int _rowCnt, _colCnt;

		public new void Awake()
		{
			base.Awake();
			_contentRect.pivot = _contentRect.anchorMin = _contentRect.anchorMax = new Vector2(0, 1);		// content 也坐上锚固
			_xGap = spacing.x + cellSize.x;
			_yGap = spacing.y + cellSize.y;
		}

		protected override bool IsInViewPort(int idx, float left, float right, float top, float bot)
		{
			Vector2 pos = _itemPosList[idx];
			return pos.x + _xGap + 5 > left && pos.x - 5 < right && pos.y - 5 > bot && pos.y - _yGap + 5 < top;
		}

		protected override void MakeContent()
		{
			// 计算四个边界
			Vector2 lt = new Vector2(-_viewRect.rect.width / 2, _viewRect.rect.height / 2);     // 左上
			Vector2 rb = -lt;
			lt = _contentRect.InverseTransformPoint(_viewRect.TransformPoint(lt));
			rb = _contentRect.InverseTransformPoint(_viewRect.TransformPoint(rb));

			// 判断哪些需要隐藏
			_itemToHideList.Clear();
			foreach (var item in _showDic)
			{
				if (IsInViewPort(item.Key, lt.x, rb.x, lt.y, rb.y) == false)
				{
					_itemToHideList.Add(item.Key);
				}
			}
			foreach (var idx in _itemToHideList) HideItem(idx);

			// 判断哪些需要显示
			int rowBeg = Math.Max(0, (int)((-lt.y - _yGap) / _yGap));
			int rowEnd = Math.Min(_rowCnt, (int)((-rb.y + _yGap) / _yGap));
			int colBeg = Math.Max(0, (int)((lt.x - _xGap) / _xGap));
			int colEnd = Math.Min(_colCnt, (int)((rb.x + _xGap) / _xGap));
			while (rowBeg < rowEnd)
			{
				int idx = rowBeg * _colCnt + colBeg;
				for (int i = colBeg; i < colEnd && idx < itemCnt; i++)
				{
					if (_showDic.ContainsKey(idx) == false && IsInViewPort(idx, lt.x, rb.x, lt.y, rb.y))
					{
						ShowItem(idx);
					}
					idx++;
				}
				rowBeg++;
			}
		}

		protected override void ResizeContent()
		{
			if (limitRowOrCol == true)
			{
				// 限制行的数量
				_colCnt = (itemCnt - 1 + limitCnt) / limitCnt;
				_rowCnt = limitCnt;
				if (itemCnt < _rowCnt) _rowCnt = itemCnt;
				// 限制行数情况下，重定义大小要清空所有正在显示的，
				// 因为遍历是按行遍历，限制行数后，索引会变
				_itemToHideList.Clear();
				_itemToHideList.AddRange(_showDic.Keys);
				foreach (int i in _itemToHideList)
				{
					HideItem(i);
				}
			}
			else
			{
				// 限制列的数量
				_rowCnt = (itemCnt - 1 + limitCnt) / limitCnt;
				_colCnt = limitCnt;
				if (itemCnt < _colCnt) _colCnt = itemCnt;
			}
			// 计算 content 的大小
			_contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rowCnt * _yGap - spacing.y - padding.y + padding.w);
			_contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _colCnt * _xGap - spacing.x + padding.x - padding.z);
			float x = padding.x, y = padding.y;
			_itemPosList.Clear();
			for (int i = 0; i < _rowCnt; i++)
			{
				for (int j = 0; j < _colCnt; j++)
				{
					_itemPosList.Add(new Vector2(x, y));
					x += _xGap;
				}
				x = padding.x;
				y -= _yGap;
			}
		}
	}
}
