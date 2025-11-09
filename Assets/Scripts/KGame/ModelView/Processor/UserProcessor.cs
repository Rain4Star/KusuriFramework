using KGameClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace KModel
{
	public class UserProcessor : CmdProcessor
	{
		public override void Init()
		{
			AddProcFunc(1, (CmdFunc<int>)SC_UserOpError);
			AddProcFunc(2, (CmdFunc<User>)SC_LogInUser);
			AddProcFunc(3, (CmdFunc<long>)SC_TestAlive);
			AddProcFunc(4, (CmdFunc<int[]>)SC_SetTestAliveParam);
			AddProcFunc(5, (CmdFunc<BagItem[]>)SC_SendBag);
			AddProcFunc(6, (CmdFunc<(BagItem.EUseType opType, (int id, int opCnt)[] items)>)SC_UpdateBagItems);
			AddProcFunc(7, (CmdFunc<(BagItem.EUseType opType, int id, int opCnt)>)SC_UpdateBagItem);

		}

		// 1-1 CS 创建用户
		public static void CS_CreateUser(string uname, string pwad)
		{
			GameClient.Ins.SendMesg(1, 1, JsonConvert.SerializeObject((uname, pwad)));
		}
		// 1-1 SC 用户操作错误
		private void SC_UserOpError(int err)
		{
			ModelMgr.Ins.GetModel<UserModel>().SC_UserOpError(err);
		}


		// 1-2 CS 用户登录
		public static void CS_LoginUser(string uname, string pwad)
		{
			GameClient.Ins.SendMesg(1, 2, JsonConvert.SerializeObject((uname, pwad)));
		}
		// 1-2 SC 服务器下发用户实例
		private void SC_LogInUser(User user)
		{
			ModelMgr.Ins.GetModel<UserModel>().SC_LogInUser(user);
		}


		// 1-3 SC 心跳检测
		private void SC_TestAlive(long tick)
		{
			ModelMgr.Ins.GetModel<UserModel>().SC_TestAlive(tick);
		}
		// 1-3 CS 心跳检测
		public static void CS_TestAlive()
		{
			GameClient.Ins.SendMesg(1, 3, null);
		}


		// 1-4 SC 同步心跳检测参数
		private void SC_SetTestAliveParam(int[] paramArr)
		{
			ModelMgr.Ins.GetModel<UserModel>().SC_SetTestAliveParam(paramArr);
		}
		// 1-4 CS 请求心跳检测参数
		public static void CS_GetTestAliveParam()
		{
			GameClient.Ins.SendMesg(1, 4, null);
		}


		// 1-5 SC 下发背包数据
		private void SC_SendBag(BagItem[] bag)
		{
			ModelMgr.Ins.GetModel<UserModel>().SC_SendBag(bag);
		}
		// 1-5 CS 客户端请求背包数据
		public static void CS_GetBag()
		{
			GameClient.Ins.SendMesg(1, 5, null);
		}


		// 1-6 SC 更新多个背包物品
		private void SC_UpdateBagItems((BagItem.EUseType opType, (int id, int opCnt)[] items) data)
		{
			ModelMgr.Ins.GetModel<UserModel>().SC_UpdateItems(data.opType, data.items);
		}
		// 1-6 CS 请求批处理多个背包物品
		public static void CS_UpdateBagItems(BagItem.EUseType opType, (int id, int opCnt)[] items)
		{
			GameClient.Ins.SendMesg(1, 6, JsonConvert.SerializeObject(items));
		}


		// 1-7 SC 更新一个背包物品
		private void SC_UpdateBagItem((BagItem.EUseType opType, int id, int opCnt) data)
		{
			ModelMgr.Ins.GetModel<UserModel>().SC_UpdateItem(data.opType, data.id, data.opCnt);
		}
		// 1-7 CS 请求更新一个背包物品
		public static void CS_UpdateBagItem((BagItem.EUseType opType, int id, int opCnt) item)
		{
			GameClient.Ins.SendMesg(1, 7, JsonConvert.SerializeObject(item));
		}
	}
}
