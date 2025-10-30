using KUISys;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Kusuri.GameUI
{
	[UIElem("Common/CommonTitle")]
	public class CommonTitle : UIElem
	{
		public Text titleTxt;
		public Button closeBtn;

		// 依据配置表初始化
		public void InitByCfg(int uiCfgId)
		{

		}

		// 设置标题
		public void SetTitle(string title)
		{
			titleTxt.text = title;
		}

		// 设置放回按钮回调
		public void SetCloseFunc(UnityAction closeFunc)
		{
			closeBtn.onClick.RemoveAllListeners();
			closeBtn.onClick.AddListener(closeFunc);
		}

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
