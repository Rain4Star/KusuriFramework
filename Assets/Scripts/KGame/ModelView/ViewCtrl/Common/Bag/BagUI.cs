using KConfig;
using KEventSys;
using KModel;
using KUISys;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KBagItem = KModel.BagItem;

namespace Kusuri.GameUI
{
	[UIElem("Common/Bag/BagUI", 2)]
	public class BagUI : UIElem
	{
		// 组件
		public ScrollMulti bagSv;
		public RectTransform titleRoot;

		public Text catTxt, qualityTxt, priceTxt;
		public Text nameTxt, descTxt;
		public Button useBtn, sellBtn, dropBtn, addBtn;

		// 数据和配置
		private List<KBagItem> _bag;
		private int _selectIdx;

		// 事件 id
		private int _eIdBag;

		public void Awake()
		{
			var title = UIMgr.Ins.AddElem<CommonTitle>(titleRoot);
			title.SetCloseFunc(() => UIMgr.Ins.CloseWindow<BagUI>());
			title.SetTitle("背包[TO_CONFIG]");

			_bag = ModelMgr.Ins.GetModel<UserModel>().Bag;
			bagSv.Init
			(
				UIMgr.LoadUIElemPrefab(typeof(BagItem)) as GameObject,
				(int idx, UIElem elem) =>
				{
					var bagItem = (BagItem)elem;
					// 更新视图
					bagItem.UpdateItem
					(
						_bag[idx],			// 背包物品
						_bag[idx].cfg,		// 背包物品对应的配置
						idx					// 点击回调时的参数
					);
					// 设置点击回调
					bagItem.SetClickFunc(OnItemClick);
				}
			);
		}

		public void Start()
		{
			bagSv.SetDataNum(_bag.Count).Flush();
		}

		private void OnEnable()
		{
			Clear();
			_eIdBag = EventSys.Ins.AddListener("UPDATE_BAG_ALL", (_) => 
			{
				_bag = ModelMgr.Ins.GetModel<UserModel>().Bag;
				Utils.Print(_bag.Count);
				bagSv.SetDataNum(_bag.Count).Flush();
			});

			useBtn.onClick.AddListener(UseClick);
			sellBtn.onClick.AddListener(SellClick);
			dropBtn.onClick.AddListener(DropClick);
			addBtn.onClick.AddListener(AddClick);
		}

		private void OnDisable()
		{
			EventSys.Ins.RemoveListener("UPDATE_BAG_ALL", _eIdBag);
			useBtn.onClick.RemoveAllListeners();
			sellBtn.onClick.RemoveAllListeners();
			dropBtn.onClick.RemoveAllListeners();
			addBtn.onClick.RemoveAllListeners();
		}


		// 背包 sv 中物品的点击回调函数
		private void OnItemClick(int idx)
		{
			if (idx < 0)
			{
				Clear();
				return;
			}
			ItemCfg cfg = _bag[idx].cfg;
			if (cfg == null) return;
			_selectIdx = idx;
			catTxt.text = cfg.cat.ToString();
			qualityTxt.text = cfg.quality.ToString();
			priceTxt.text = cfg.sellPrice.ToString();
			nameTxt.text = cfg.name;
			descTxt.text = cfg.desc;
		}

		public override void Clear()
		{
			_selectIdx = -1;
			catTxt.text = null;
			qualityTxt.text = null;
			priceTxt.text = null;
			nameTxt.text = null;
			descTxt.text = null;
		}

		private void UseClick()
		{
			if (_selectIdx == -1) return;
			UserProcessor.CS_UpdateBagItem((KBagItem.EUseType.Use, _bag[_selectIdx].Id, 1));
		}

		private void SellClick()
		{
			if (_selectIdx == -1) return;
			UserProcessor.CS_UpdateBagItem((KBagItem.EUseType.Sell, _bag[_selectIdx].Id, 1));
		}

		private void DropClick()
		{
			if (_selectIdx == -1) return;
			UserProcessor.CS_UpdateBagItem((KBagItem.EUseType.Drop, _bag[_selectIdx].Id, 1));
		}

		private void AddClick()
		{
			if (_selectIdx == -1) return;
			UserProcessor.CS_UpdateBagItem((KBagItem.EUseType.Add, _bag[_selectIdx].Id, 5));
		}
	}
}
