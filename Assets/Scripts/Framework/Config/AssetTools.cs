using Kusuri;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace KConfig
{
	public class AssetTools
	{


		public static Sprite LoadSprite(string atlas, string name)
		{
			string path = Path.Combine(ConfigMgr.atlaPath, atlas, name);
			return Resources.Load<Sprite>(path);
		}

		public static void LoadSpriteAsync(string atlas, string name)
		{
			
		}
	}
}
