using KUISys;
using UnityEngine.UI;
using KEventSys;
using KModel;

namespace Kusuri.GameUI
{
	[UIElem("LogIn/LogInUI", 2)]
	public class LogInUI : UIElem
	{
		public Text titleTxt;

		public Button logBtn;
		public Button regBtn;

		public InputField unameInpt;
		public InputField pwadInpt;

		// 事件id
		private int _logEid;

		private void OnEnable()
		{
			// 在此处添加事件监听
			logBtn.onClick.AddListener(LogClick);
			regBtn.onClick.AddListener(RegClick);
			_logEid = EventSys.Ins.AddListener("USER_LOGIN", (objArr) => 
			{
				UIMgr.Ins.OpenWindow<BagUI>();
			});

		}

		private void OnDisable()
		{
			// 在此处移除事件监听
			logBtn.onClick.RemoveAllListeners();
			regBtn.onClick.RemoveAllListeners();
			EventSys.Ins.RemoveListener("USER_LOGIN", _logEid);
		}

		private void LogClick()
		{
			UserProcessor.CS_LoginUser(unameInpt.text, pwadInpt.text);
		}

		private void RegClick()
		{
			UserProcessor.CS_CreateUser(unameInpt.text, pwadInpt.text);
		}
	}
}
