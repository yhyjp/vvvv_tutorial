#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SendTexture", Category = "Raw", Help = "Basic raw template which copies up to count bytes from the input to the output", Tags = "")]
	#endregion PluginInfo
	public class RawSendTextureNode : IPluginEvaluate, IStartable
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Stream> FStreamIn;

		[Output("Output")]
		public ISpread<int> running;

		[Import()]
        public ILogger FLogger;
		
		byte[] FBuffer = new byte[1024*1024];
		
		int size = 0;
		bool sending = false;
		Socket s;
		Thread trd = null;
		
		#endregion fields & pins
		
		private void recvTask()
		{
			while(true){
				if(sending && s != null){
					s.Send(FBuffer, 0, size, SocketFlags.None);
					sending = false;
					FLogger.Log(LogType.Debug, size+"da:");
				}
				Thread.Sleep(5);
			}
		}
		
		//called when all inputs and outputs defined above are assigned from the host
		public void Start()
		{
			if (s==null)
			{
				TcpClient tcpClient = new TcpClient();
				tcpClient.Connect("localhost", 4444);
				s = tcpClient.Client;
			}
			if (trd != null) trd.Abort();
			trd = new Thread(new ThreadStart(recvTask));
			trd.IsBackground = true;
			trd.Start();
			FLogger.Log(LogType.Debug,"Start");
		}
		
		public void Shutdown()
		{
			trd.Abort();
			s.Close();
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (sending) return;
			if (s == null) {
				Start();
				return;
			}
			for (int i = 0; i < spreadMax; i++) {
				var inputStream = FStreamIn[i];
				size = (int)inputStream.Length;
				inputStream.Read(FBuffer, 0, size);
				sending = true;
			}
		}
	}
}
