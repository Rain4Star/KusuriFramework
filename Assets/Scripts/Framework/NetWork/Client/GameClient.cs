using Kusuri;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KGameClient
{
	public class GameClient : MonoSingleton<GameClient>, IDisposable
	{
		public static string ip = "127.0.0.1";
		public static int port = 12230;

		protected bool _isRun = false;

		private TcpClient _tcp;
		private NetworkStream _stream;
		private CancellationTokenSource _cts;
		public CancellationToken CancelToke => _cts.Token;
		private ConcurrentQueue<(byte tp, byte sub, string json)> _mesgQueue = new();

		private CmdDispatcher _dispatcher = new();

		public void Update() 
		{
			_dispatcher.Update();
		}
		public override void OnApplicationQuit()
		{
			base.OnApplicationQuit();
			Stop();
		}


		void IDisposable.Dispose()
		{
			_isRun = false;
			_cts?.Cancel();
			_tcp?.Close();
			_stream?.Dispose();
			_tcp?.Dispose();
			_cts?.Dispose();
			_mesgQueue.Clear();
			_dispatcher.Clear();
		}

		/// <summary>
		/// 启动 TCP 客户端
		/// </summary>
		public async Task StartClient(Delegate startFunc = null)
		{
			_cts = new();
			_tcp = new();
			_mesgQueue.Clear();
			int retry = 0;
			int time = 1_000;
			while (retry < 5)
			{
				try
				{
					await _tcp.ConnectAsync(ip, port);
					_stream = _tcp.GetStream();

					ThreadPool.QueueUserWorkItem(_ => { RevcFunc(); });
					ThreadPool.QueueUserWorkItem(_ => { SendFunc(); });

					_isRun = true;
					startFunc?.DynamicInvoke();
					Utils.Print($"已连接至{_tcp.Client.RemoteEndPoint}", "网络连接");
					break;
				}
				catch (SocketException ex)
				{
					time <<= 1;         // 重连等待时间，指数增长
					Utils.Error(ex.Message, "网络连接失败");
					await Task.Delay(time, _cts.Token);
				}
				catch (Exception ex)
				{
					Utils.Error(ex.Message, "网络连接错误");
					break;
				}
			}
		}

		public async Task Reconnect()
		{
			Utils.Warn("尝试重连");
			Stop();
			_cts = new();
			_tcp = new();
			_mesgQueue.Clear();
			try
			{
				await _tcp.ConnectAsync(ip, port);
				_stream = _tcp.GetStream();

				ThreadPool.QueueUserWorkItem(_ => { RevcFunc(); });
				ThreadPool.QueueUserWorkItem(_ => { SendFunc(); });

				_isRun = true;
				Utils.Print($"已连接至{_tcp.Client.RemoteEndPoint}", "网络连接");
			}
			catch (SocketException ex)
			{
				Utils.Error(ex.Message, "网络连接失败");
			}
			catch (Exception ex)
			{
				Utils.Error(ex.Message, "网络连接错误");
			}
		}

		/// <summary>
		/// 停止 TCP 客户端
		/// </summary>
		public void Stop()
		{
			Utils.Error("客户端Stop");
			_cts?.Cancel();
			_isRun = false;
			//_dispatcher.Clear();
			_stream?.Close();
			_tcp?.Close();
			_tcp?.Dispose();
			_tcp = null;

			_mesgQueue.Clear();
		}


		/// <summary>
		/// 发送协议
		/// </summary>
		/// <param name="tp">大协议号</param>
		/// <param name="sub">子协议号</param>
		/// <param name="json">数据的json</param>
		public void SendMesg(byte tp, byte sub, string json)
		{
			if (_isRun == false) return;
			_mesgQueue.Enqueue((tp, sub, json));
		}

		/// <summary>
		/// 添加协议处理类
		/// </summary>
		/// <param name="tp">大协议号</param>
		/// <param name="proc">处理类</param>
		public void AddProcessor(byte tp, CmdProcessor proc)
		{
			_dispatcher.AddProcessor(tp, proc);
		}

		public void InitProcessor((byte tp, CmdProcessor proc)[] arr)
		{
			foreach (var item in arr)
			{
				_dispatcher.AddProcessor(item.tp, item.proc);
			}
		}

		/// <summary>
		/// 接收线程
		/// </summary>
		private async Task RevcFunc()
		{
			byte[] prefix = new byte[6];
			try
			{
				while (_tcp?.Connected == true && _cts.IsCancellationRequested == false)
				{
					// 异步读取前缀，没有数据时会让出线程
					int len = await _stream.ReadAsync(prefix, 0, prefix.Length, _cts.Token);
					if (len == 0) break;
					byte tp = prefix[4], sub = prefix[5];

					// 读取数据
					int packLen = BitConverter.ToInt32(prefix, 0);
					byte[] buffer = null;
					if (packLen != 0)
					{
						buffer = ArrayPool<byte>.Shared.Rent(packLen);
						len = await ReadData(buffer, packLen);
						if (len == 0)
						{
							ArrayPool<byte>.Shared.Return(buffer);
							break;
						}
					}
					try
					{
						_dispatcher.Dispatch(tp, sub, buffer);
					}
					catch (Exception ex)
					{
						throw ex;
					}
					finally
					{
						if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
					}
				}
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut || ex.SocketErrorCode == SocketError.ConnectionReset)
			{
				Utils.Error(ex.Message, "网络 Send");
			}
			catch (IOException ex)
			{
				Utils.Error(ex.Message, "网络 Send");
			}
			catch (Exception ex)
			{
				Stop();
				throw ex;
			}
		}

		/// <summary>
		/// 异步读取网络流数据
		/// </summary>
		private async Task<int> ReadData(byte[] buffer, int len)
		{
			int cnt = 0;
			while (cnt < len)
			{
				int temp = await _stream.ReadAsync(buffer, cnt, len - cnt);
				if (temp == 0) return 0;
				cnt += len;
			}
			// rent 到的数组长度一般为 2 的幂，此时缓存可能污染现在的数据
			for (int i = len; i < buffer.Length; i++) buffer[i] = 0;
			return cnt;
		}

		/// <summary>
		/// 发送线程
		/// </summary>
		private void SendFunc()
		{
			byte[] prefix = new byte[6];
			try
			{
				while (_cts.IsCancellationRequested == false && _tcp.Connected == true)
				{
					if (_mesgQueue.TryDequeue(out var item))
					{
						byte[] data = string.IsNullOrEmpty(item.json) ? null : Encoding.UTF8.GetBytes(item.json);
						// if (item.json.Length > 65535) AssetModel.Ins.CS_SendAsset(item.json, 1, item.tp, item.sub);	// 超长文本 用 Asset协议 包装
						int packLen = data == null ? 0 : data.Length;
						
						Utils.IntToByte(packLen, prefix, 0);
						prefix[4] = item.tp;
						prefix[5] = item.sub;
						_stream?.WriteAsync(prefix, 0, 6, _cts.Token);
						// Thread.Sleep(Utils.random.Next(0,2) == 0 ? 100:1000);		// 测试高延迟，先发一个前缀过去
						// 此处单发送线程 + 消息队列，无需担心 stream 的同步问题
						if (packLen > 0) _stream?.WriteAsync(data, 0, packLen, _cts.Token);
						_stream?.Flush();
						Utils.PrintSend(item.tp, item.sub, item.json, packLen);
					}
					else
					{
						Thread.Sleep(20);
					}
				}
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut || ex.SocketErrorCode == SocketError.ConnectionReset)
			{
				Utils.Error(ex.Message, "网络 Send");
			}
			catch (IOException ex)
			{
				Utils.Error(ex.Message, "网络 Send");
			}
			catch (Exception ex)
			{
				Stop();
				throw ex;
			}
		}
	}
}