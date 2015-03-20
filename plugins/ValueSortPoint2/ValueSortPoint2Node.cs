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
	[PluginInfo(Name = "SortPoint2", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueSortPoint2Node : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<Vector3D> FInput;
		[Input("Dist1", DefaultValue = 1.0)]
		public ISpread<double> FDist1;
		[Input("Dist2", DefaultValue = 1.0)]
		public ISpread<double> FDist2;
		[Input("Dist3", DefaultValue = 1.0)]
		public ISpread<double> FDist3;

		[Output("Output")]
		public ISpread<Vector3D> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		void swap(ref int a, ref int b)
		{
			int t = a;
			a = b;
			b = t;
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			int ct = FInput.SliceCount/3;
			FOutput.SliceCount = ct*4;

			int [] table = new int[]{0,1,0,2,1,2};
			int p = 0;
			for (int i = 0; i < ct; i++)
			{
				int [] idx = new int[]{ 0, 1, 2 };
				if (FDist1[i] > FDist2[i]) swap(ref idx[0], ref idx[1]);
				if (FDist1[i] > FDist3[i]) swap(ref idx[0], ref idx[2]);
				if (FDist2[i] > FDist3[i]) swap(ref idx[1], ref idx[2]);
				
				FOutput[p++] = FInput[i*3+table[idx[0]*2+0]];
				FOutput[p++] = FInput[i*3+table[idx[0]*2+1]];
				FOutput[p++] = FInput[i*3+table[idx[1]*2+0]];
				FOutput[p++] = FInput[i*3+table[idx[1]*2+1]];
			}
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
