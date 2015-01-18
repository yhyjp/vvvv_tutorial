#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "FFDamageMove", Category = "String", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class StringFFDamageMoveNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Text", DefaultString = "text")]
		public ISpread<String> FInputText;
		[Input("PosXY", DefaultValue = 0)]
		public ISpread<double> FInputPosXY;
		[Input("Time", DefaultValue = 0)]
		public ISpread<double> FInputTime;
		[Input("Width", DefaultValue = 1.0)]
		public ISpread<double> FInputWidth;

		[Output("Char")]
		public ISpread<String> FOutputChar;
		[Output("PosXY")]
		public ISpread<double> FOutputXY;
		[Output("LocalTime")]
		public ISpread<double> FOutputLocalTime;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutputChar.SliceCount = 0;
			FOutputXY.SliceCount = 0;
			FOutputLocalTime.SliceCount = 0;
			
			SpreadMax = Math.Max(Math.Max(FInputText.SliceCount, FInputTime.SliceCount), FInputWidth.SliceCount);
			
			for (int i=0; i < SpreadMax; ++i)
			{
				String text = FInputText[i%FInputText.SliceCount];
				double time = FInputTime[i%FInputTime.SliceCount];
				double width = FInputWidth[i%FInputWidth.SliceCount];
				double x = FInputPosXY[(i*2+0)%FInputPosXY.SliceCount];
				double y = FInputPosXY[(i*2+1)%FInputPosXY.SliceCount];
				f(text, time, width, x, y);
			}
			
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
		
		private void f(String s, double t, double width, double x, double y)
		{
			for (int i=0; i < s.Length; ++i)
			{
				double tt = (t + (s.Length-i-1)*0.1)*2;
				double lt = ((tt > 1) ? 1 : tt);
				double dy = Math.Sin(lt * Math.PI)/5;
				FOutputChar.Add(s[i].ToString());
				FOutputLocalTime.Add(t);
				FOutputXY.Add(x + width*i); //x
				FOutputXY.Add(y + dy); //y
			}
		}
	}
}
