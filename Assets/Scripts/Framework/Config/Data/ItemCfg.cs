using Newtonsoft.Json;

namespace KConfig
{
	[CfgItem("ItemCfg", "id", false, new string[] {"name", "desc"})]
	public class ItemCfg
	{
		public int id;          // id
		public string name;     // 名字 lang
		public byte quality;    // 品质
		public int cat;         // 种类
		public int icon;		// 图标
		public int sellPrice;   // 售价
		public string desc;		// 描述 lang
	}
}

