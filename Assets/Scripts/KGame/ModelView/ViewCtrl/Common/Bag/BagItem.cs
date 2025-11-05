using KUISys;
using UnityEngine;
using UnityEngine.UI;

namespace Kusuri.GameUI
{
	[UIElem("Common/Bag/BagItem")]
	public class BagItem : UIElem
	{
		public Button clickBtn;
		public Image iconImg, qualityImg;
		public GameObject selectGo;
		public Text numTxt;
	}
}
