#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "MoveWindow", Category = "Window", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	
	public class WindowMoveWindowNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Handle", DefaultValue = 0)]
		public ISpread<int> FHandle;
		[Input("Location", DefaultValue = 0)]
		public ISpread<Vector2D> FLocation;
		[Input("Size", DefaultValue = 0)]
		public ISpread<Vector2D> FSize;
		
		[Output("Pos")]
        public ISpread<Vector2D> FOut;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		[DllImport("User32.dll")]
        static extern int MoveWindow(
            IntPtr hWnd,
            int x,
            int y,
            int nWidth,
            int nHeight,
            int bRepaint            
            );


		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			IntPtr handle = (IntPtr)FHandle[0];
			Vector2D p = FLocation[0];
			Vector2D s = FSize[0];
			MoveWindow(handle, (int)p.x, (int)p.y, (int)s.x, (int)s.y, 0);
			
			FOut.SliceCount = 1;
			FOut[0] = p;
		}
	}
}
