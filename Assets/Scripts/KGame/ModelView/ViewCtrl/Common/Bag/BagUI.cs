using KConfig;
using KEventSys;
using KModel;
using KUISys;
using System.Collections.Generic;
using UnityEngine;
using KBagItem = KModel.BagItem;

namespace Kusuri.GameUI
{
	[UIElem("Common/Bag/BagUI", 2)]
	public class BagUI : UIElem
	{
		public ScrollMulti bagSv;
		public RectTransform titleRoot;

		private CfgObj _itemCfg;
		private List<KBagItem> _bag;

		// 事件 id
		private int _eIdBag;

		public void Awake()
		{
			var title = UIMgr.Ins.AddElem<CommonTitle>(titleRoot);
			title.SetCloseFunc(() => UIMgr.Ins.CloseWindow<BagUI>());
			title.SetTitle("背包[TO_CONFIG]");

			_itemCfg = ConfigMgr.Ins.GetCfgObj<ItemCfg>();

			_bag = ModelMgr.Ins.GetModel<UserModel>().Bag;
			bagSv.Init(
				UIMgr.LoadUIElemPrefab(typeof(BagItem)) as GameObject,
				(int idx, UIElem elem) =>
				{
					((BagItem)elem).UpdateItem(_bag[idx], _itemCfg.GetItem<ItemCfg>(_bag[idx].Id));
				}
			);
		}

		public void Start()
		{
			bagSv.SetDataNum(_bag.Count).Flush();
		}

		private void OnEnable()
		{
			_eIdBag = EventSys.Ins.AddListener("UPDATE_BAG_ALL", (_) => 
			{
				bagSv.SetDataNum(_bag.Count).Flush();
			});
		}

		private void OnDisable()
		{
			EventSys.Ins.RemoveListener("UPDATE_BAG_ALL", _eIdBag);
		}
	}
}
