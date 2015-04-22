#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "CS_Test", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueCS_TestNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("MouseXY", DefaultValue = 1.0)]
		public ISpread<Vector2D> FMouseXY;
		[Input("Click", DefaultValue = 0)]
		public ISpread<bool> FClick;
		[Input("QuadLocation", DefaultValue = 1.0)]
		public ISpread<Vector2D> FLocation;
		[Input("QuadSize", DefaultValue = 1.0)]
		public ISpread<Vector2D> FSize;
		[Input("Init", DefaultValue = 0)]
		public ISpread<bool> FInit;

		[Output("On")]
		public ISpread<int> FOn;

		[Import()]
		public ILogger FLogger;
		
		List<int> state;
		
		#endregion fields & pins

		bool isOn(Vector2D mouse, Vector2D location, Vector2D size)
		{
			bool isOnX = (location.x - size.x/2 < mouse.x && mouse.x < location.x + size.x/2);
			bool isOnY = (location.y - size.y/2 < mouse.y && mouse.y < location.y + size.y/2);
			return (isOnX && isOnY);
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			int n = FLocation.SliceCount;
			if (state == null || n != state.Count || FInit[0]) {
				state = new List<int>();
				for (int i=0; i < n; ++i) state.Add(0);
			}
			
			FOn.SliceCount = n;
			for (int i=0; i < n; ++i)
			{
				if (isOn(FMouseXY[0], FLocation[i], FSize[i]))
				{
					if (FClick[0])
					{
						++state[i];
						if (state[i] >= 3) state[i] = 0;
					}
				}
				
				FOn[i] = state[i];
			}
		}
	}
}
