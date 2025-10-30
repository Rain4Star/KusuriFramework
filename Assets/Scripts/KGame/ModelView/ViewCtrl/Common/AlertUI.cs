using KUISys;
using UnityEngine.UI;

namespace Kusuri.GameUI
{
	[UIElem("Common/AlertUI")]
	public class AlertUI : UIElem
	{
		public Text titleTxt, descTxt;
		public Button maskBtn, leftBtn, rightBtn;



		private void OnEnable()
		{
			// 在此处添加事件监听
		}

		private void OnDisable()
		{
			// 在此处移除事件监听
		}
	}
}
