using Newtonsoft.Json;
using System;
using System.Text;
using KGameClient;

namespace KModel
{
	public class AssetModel : ModelBase
	{
		private int _fragLen = 500;     // 片段大小不能太小，因为第一个包传的是 AssetData 作为数据，此时片段过小会出错
										// 255-1
		public void CS_SendAsset(string data, byte type, byte finTp = 0, byte finSub = 0)
		{
			byte[] dt = Encoding.UTF8.GetBytes(data);
			CS_SendAsset(dt, type, finTp, finSub);
		}

		public void CS_SendAsset(byte[] data, byte type, byte finTp = 0, byte finSub = 0)
		{
			int len = data.Length;
			AssetFragment frag;
			int lim = (len + _fragLen - 1) / _fragLen;        // 计算需要分多少个包
			int offset = 0;
			// 传一个 AssetData 包，告诉 接收方 整个数据有多少个片段
			frag.order = -1;
			AssetData assetData = new AssetData()
			{
				type = type,
				len = len,
				fragCount = lim,
				fragSize = _fragLen,
				finTp = finTp,
				finSub = finSub
			};
			frag.data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(assetData));
			AssetProcessor.CS_SendAsset(frag);
			int bufferlen = Math.Min(_fragLen, len);
			byte[] buffer = frag.data = new byte[bufferlen];

			// 发送数据
			for (int i = 0; i < lim; i++)
			{
				frag.order = i;
				int j = 0;
				while (j < bufferlen && offset < len) buffer[j++] = data[offset++];
				while (j < bufferlen) buffer[j++] = 0;
				AssetProcessor.CS_SendAsset(frag);
			}
		}

	}
}
