using KUISys;
using KModel;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Kusuri.GameUI
{
	[UIElem("SmallGame/SmallGameItem")]
	public class SmallGameItem : UIElem
	{
		public Text nameTxt, descTxt;
		public Image iconImg;

		public Button btn;
		private UnityAction _btnClickFunc;

		public void Show(int idx)
		{
			var cfg = ModelMgr.Ins.GetModel<SmallGameModel>().smallGameCfg[idx];

			nameTxt.text = cfg.name;
			descTxt.text = cfg.desc;
			iconImg.color = cfg.iconClr;
			btn.onClick.RemoveAllListeners();
			if(_btnClickFunc != null) btn.onClick.AddListener(_btnClickFunc);
		}

		private void OnEnable()
		{
			// 在此处添加事件监听
			if (_btnClickFunc != null) btn.onClick.AddListener(_btnClickFunc);
		}

		private void OnDisable()
		{
			// 在此处移除事件监听
			btn.onClick.RemoveAllListeners();
		}
	}
}
