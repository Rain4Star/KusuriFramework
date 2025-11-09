using KConfig;
using KEventSys;
using KGameClient;
using System;
using System.Collections.Generic;

namespace KModel
{
	public class UserModel : ModelBase
	{
		private long _lastAliveTime;			
		private long _nextAliveTime;
		private long _reconnectTryTime;
		private int _aliveTestInterval = 5;
		private int _timeOutInterval = 20;
		private long _reconnectInterval = 15;
		private bool _testAliveFlag = false;

		private CfgObj _itemCfg;
		public User CurUser { get; private set; }

		public List<BagItem> Bag 
		{
			get 
			{
				if (_bagDicChanged) SortBag();
				return _sortBag;
			}
		}
		private List<BagItem> _sortBag = new();
		private bool _bagDicChanged = false;
		public Dictionary<int, BagItem> BagDic { get; private set; } = new();

		public override void Init(ModelMgr ins)
		{
			ins.onUpdate += Update;
			_itemCfg = ConfigMgr.Ins.GetCfgObj<ItemCfg>();
		}

		public void Update()
		{
			if (_testAliveFlag == true)
			{
				long curTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

				if (_lastAliveTime + _timeOutInterval < curTime && _reconnectTryTime < curTime)   // 连接超时
				{
					_reconnectTryTime = curTime + _reconnectInterval;        // 5秒重连一次
					GameClient.Ins.Reconnect();
				}
				if (_nextAliveTime < curTime)
				{
					UserProcessor.CS_TestAlive();
					_nextAliveTime = curTime + _aliveTestInterval;   // 三秒后再发
				}
			}
		}
		public void Destroy()
		{
			_testAliveFlag = false;
			if (CurUser != null) LogOutUser(CurUser);
		}

		// 开启心跳检测任务
		public void CS_TestAlive()
		{
			UserProcessor.CS_GetTestAliveParam();
			_lastAliveTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
			_reconnectTryTime = _lastAliveTime + _reconnectInterval;
			_nextAliveTime = _lastAliveTime;
		}
		// SC 1-3 心跳检测
		public void SC_TestAlive(long tick)
		{
			long delay = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond - tick;
			//EventSys.Ins.SendMain("SC_MAIN_TEST_ALIVE", delay);
			_lastAliveTime = tick;
		}
		// SC 1-4 心跳检测参数
		public void SC_SetTestAliveParam(int[] paramArr)
		{
			_aliveTestInterval = paramArr[0];
			_timeOutInterval = paramArr[1];
			_reconnectInterval = paramArr[2];
			_testAliveFlag = true;
		}


		#region 用户数据模块
		// SC 1-2 用户登录
		public void SC_LogInUser(User user)
		{
			if (CurUser != null) LogOutUser(CurUser);
			CurUser = user;
			EventSys.Ins.Send("USER_LOGIN");
		}

		// SC 1-1 通用操作错误
		public void SC_UserOpError(int errCode)
		{
			
		}


		// 退出登录
		public void LogOutUser(User user)
		{

		}
		#endregion 用户数据模块


		#region 背包模块
		private void SortBag()
		{
			_sortBag.Clear();
			foreach (BagItem item in BagDic.Values)
			{
				_sortBag.Add(item);
			}
			_sortBag.Sort((a, b) => a.Id.CompareTo(b.Id));
			_bagDicChanged = false;
		}


		// SC 1-5 下发用户背包数据
		public void SC_SendBag(BagItem[] bag)
		{
			BagDic.Clear();
			if(bag != null)
			{
				BagDic.EnsureCapacity(bag.Length);
				foreach (BagItem item in bag)
				{
					if (item.cfg == null) item.cfg = _itemCfg.GetItem<ItemCfg>(item.Id);
					BagDic.Add(item.Id, item);
				}
			}
			_bagDicChanged = true;
			EventSys.Ins.Send("UPDATE_BAG_ALL");
		}


		private void InnerUpdateItem(BagItem.EUseType opType, int id, int opCnt)
		{
			if (opCnt < 0) return;      // opCnt 为符数时，是一些出错情况，并不是某种操作的反操作
			if (BagDic.TryGetValue(id, out var item) == false)
			{
				item = new BagItem(id, 0);
				item.cfg = _itemCfg.GetItem<ItemCfg>(item.Id);
				BagDic.Add(id, item);
				_bagDicChanged = true;
			}
			item.Update(opType, opCnt);
			// 更新后数量为 0，移除这个物品
			if (item.Count == 0)
			{
				BagDic.Remove(id);
				_bagDicChanged = true;
			}
		}

		// SC 1-6 下发更新
		public void SC_UpdateItems(BagItem.EUseType opType, (int id, int opCnt)[] items)
		{
			foreach (var item in items)
			{
				InnerUpdateItem(opType, item.id, item.opCnt);
			}
			EventSys.Ins.Send("UPDATE_BAG_ALL");
		}

		// SC 1-7 下发更新单个物品
		public void SC_UpdateItem(BagItem.EUseType opType, int id, int opCnt)
		{
			InnerUpdateItem(opType, id, opCnt);
			EventSys.Ins.Send("UPDATE_BAG_ITEM", id);
		}


		#endregion 背包模块
	}
}
