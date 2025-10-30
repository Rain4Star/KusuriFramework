using KUISys;
using UnityEngine.UI;

namespace Kusuri.GameUI
{
	[UIElem("Test/FPSTestUI")]
	public class FPSTestUI : UIElem
	{
		public Text fpsTxt = null;
		public Text txt = null;
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
