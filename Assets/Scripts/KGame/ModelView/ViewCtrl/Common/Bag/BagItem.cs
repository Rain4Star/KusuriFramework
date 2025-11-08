using KConfig;
using KUISys;
using System;
using UnityEngine;
using UnityEngine.UI;
using KBagItem = KModel.BagItem;


namespace Kusuri.GameUI
{
	[UIElem("Common/Bag/BagItem")]
	public class BagItem : UIElem
	{
		public Button clickBtn;
		public Image iconImg, qualityImg;
		public GameObject selectGo;
		public Text numTxt;

		private int _idx;
		private Action<int> _clickFunc;

		public void Awake()
		{
			Clear();
		}

		public void OnEnable()
		{
			clickBtn.onClick.AddListener(ClickFunc);
		}
		public void OnDisable()
		{
			clickBtn.onClick.RemoveAllListeners();
		}

		// 按钮的点击回调
		private void ClickFunc()
		{
			_clickFunc?.Invoke(_idx);
		}

		// 设置按钮视图
		public void UpdateItem(KBagItem item, ItemCfg itemCfg, int idx)
		{
			if (item == null)
			{
				// 没有对应的背包物品
				Clear();
				return;
			}
			numTxt.text = item.Count.ToString();
			qualityImg.sprite = UITools.LoadQuality(itemCfg.quality);
			iconImg.sprite = UITools.LoadSprite(itemCfg.icon);
			_idx = idx;
		}

		// 设置按钮点击回调，回调参数： UpdateItem 中传入的 idx
		public void SetClickFunc(Action<int> func)
		{
			_clickFunc = func;
		}

		// 清空视图
		public override void Clear()
		{
			clickBtn.onClick.RemoveAllListeners();
			iconImg.sprite = null;
			qualityImg.sprite = null;
			selectGo.SetActive(false);
			numTxt.text = string.Empty;
			_idx = 0;
			SetClickFunc(null);
		}
	}
}
