using KConfig;
using KUISys;
using UnityEngine;
using UnityEngine.UI;
using KBagItem = KModel.BagItem;


namespace Kusuri.GameUI
{
	[UIElem("Common/Bag/BagItem")]
	public class BagItem : UIElem
	{
		public Button clickBtn;
		public Image iconImg, qualityImg;
		public GameObject selectGo;
		public Text numTxt;

		public void Awake()
		{
			SetNone();
		}

		public void UpdateItem(KBagItem item, ItemCfg itemCfg)
		{
			if (item == null)
			{
				SetNone();
				return;
			}
			numTxt.text = item.Count.ToString();
			qualityImg.sprite = UITools.LoadQuality(itemCfg.quality);
			iconImg.sprite = UITools.LoadSprite(itemCfg.icon);
		}

		public void SetNone()
		{
			clickBtn.onClick.RemoveAllListeners();
			iconImg.sprite = null;
			qualityImg.sprite = null;
			selectGo.SetActive(false);
			numTxt.text = string.Empty;
		}
	}
}
