using KConfig;
using UnityEngine;

namespace Kusuri.GameUI
{
	public class UITools
	{
		public static CfgObj iconCfg;

		public static void Init()
		{
			iconCfg = ConfigMgr.Ins.GetCfgObj<IconCfg>();
		}

		public static Sprite LoadSprite(int iconId)
		{
			var cfg = iconCfg.GetItem<IconCfg>(iconId);
			if(cfg == null) return null;
			return AssetTools.LoadSprite(cfg.atlas, cfg.name);
		}

		public static Sprite LoadQuality(int quality)
		{
			if(quality == 0) return null;
			var cfg = iconCfg.GetItem<IconCfg>(quality);
			return AssetTools.LoadSprite(cfg.atlas, cfg.name);
		}

	}
}
