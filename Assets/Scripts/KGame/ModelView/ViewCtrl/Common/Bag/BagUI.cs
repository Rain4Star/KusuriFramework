using KUISys;
using UnityEngine;

namespace Kusuri.GameUI
{
	[UIElem("Common/Bag/BagUI")]
	public class BagUI : UIElem
	{
		public ScrollMulti bagSv;

		public RectTransform titleRoot;

		public void Awake()
		{
			var title = UIMgr.Ins.AddElem<CommonTitle>(titleRoot);
			title.SetCloseFunc(() => UIMgr.Ins.CloseWindow<BagUI>());
			title.SetTitle("背包[TO_CONFIG]");

			bagSv.Init(
				UIMgr.LoadUIElemPrefab(typeof(BagItem)) as GameObject,
				(int idx, UIElem elem) =>
				{

				}
			);
		}
	}
}
