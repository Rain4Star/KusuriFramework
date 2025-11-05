using System;
using UnityEngine;
using KUISys;
using KModel;

namespace Kusuri.GameUI
{
	[UIElem("SmallGame/SmallGameUI")]
	public class SmallGameUI : UIElem
	{
		public Transform topTF;
		private CommonTitle commonTitle;


		public ScrollVer smallGameSv;

		public void Start()
		{
			commonTitle = UIMgr.Ins.AddElem<CommonTitle>(topTF);
			commonTitle.SetTitle("Small Game UI");
			commonTitle.SetCloseFunc(() => 
			{
				UIMgr.Ins.CloseWindow<SmallGameUI>();
				UIMgr.Ins.OpenWindow<LogInUI>();
			});
			smallGameSv.Init((GameObject)UIMgr.LoadUIElemPrefab(typeof(SmallGameItem)),
				(int idx, UIElem elem) => 
				{
					((SmallGameItem)elem).Show(idx);
				}
			);
			smallGameSv.SetDataNum(ModelMgr.Ins.GetModel<SmallGameModel>().smallGameCfg.Count).Flush();
		}
	}
}
