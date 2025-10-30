using KEventSys;
using KGameClient;
using Kusuri;
using Kusuri.GameUI;
using System;

namespace KModel
{
	public class UserModel : ModelBase, IMonoSingleton
	{
		private long _lastAliveTime;			
		private long _nextAliveTime;
		private long _reconnectTryTime;
		private int _aliveTestInterval = 5;
		private int _timeOutInterval = 20;
		private long _reconnectInterval = 15;
		private bool _testAliveFlag = false;

		public User CurUser { get; private set; }

		void IMonoSingleton.Update()
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
		void IMonoSingleton.Destroy()
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
	}
}
