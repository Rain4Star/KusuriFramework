using System;

namespace Kusuri
{
	public class KTimer : MonoSingleton<KTimer>
	{
		private Action[] _timeFunc;
		private readonly int[] _intervals = { 50, 200, 1000 };
		private long[] _nextTirggerTime;
		private volatile bool _isRunning;

		protected override void Init()
		{
			_timeFunc = new Action[3];
			_nextTirggerTime = new long[3];
		}

		public void Update()
		{
			if (_isRunning) return;
			_isRunning = true;

			try
			{
				long now = DateTime.UtcNow.Ticks;
				long nowMs = now / TimeSpan.TicksPerMillisecond;

				for (int i = 0; i < _timeFunc.Length; i++)
				{
					if (nowMs > _nextTirggerTime[i])
					{
						_timeFunc[i]?.Invoke();
						_nextTirggerTime[i] = nowMs + _intervals[i];
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				_isRunning = false;
			}
		}

		/// <summary>
		/// 注册定时任务
		/// </summary>
		/// <param name="lv"> 定时器触发级别：0->50ms		1->100ms	2->1000ms </param>
		/// <param name="func"></param>
		/// <returns></returns>
		public Action Regist(int lv, Action func)
		{
			_timeFunc[lv] += func;
			return func;
		}

		public void Unregist(int lv, Action func)
		{
			_timeFunc[lv] -= func;
		}
	}
}
