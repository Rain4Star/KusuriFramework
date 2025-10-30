using Newtonsoft.Json;
using KGameClient;

namespace KModel
{
	public class AssetProcessor : CmdProcessor
	{
		public override void Init()
		{

		}

		public static void CS_SendAsset(AssetFragment frag)
		{
			GameClient.Ins.SendMesg(255, 1, JsonConvert.SerializeObject(frag));
		}
	}
}
