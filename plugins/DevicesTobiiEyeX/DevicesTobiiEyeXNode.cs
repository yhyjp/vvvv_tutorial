#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using EyeXFramework;
using Tobii.EyeX.Framework;

#endregion usings
namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "TobiiEyeX", Category = "Devices", Help = "Basic template with one value in/out", Tags = "")]
    #endregion PluginInfo
    public class DevicesTobiiEyeXNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input", DefaultValue = 1.0)]
        public ISpread<double> FInput;

        [Output("Output")]
        public ISpread<double> FOutput;

        [Import()]
        public ILogger FLogger;

        bool setuped_ = false;
        EyeXHost host_;
    	GazePointDataStream stream_;

        #endregion fields & pins

    	double gazeX_, gazeY_;

        private void setup()
        {
            host_ = new EyeXHost();
            {
                stream_ = host_.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);
                {
                    host_.Start();

                    stream_.Next += (s, e) => {
						gazeX_ = e.X;
                    	gazeY_ = e.Y;
                    };
                }
            }
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (setuped_ == false)
            {
                setuped_ = true;
                setup();
            }
        	
            FOutput.SliceCount = 2;

        	FOutput[0] = gazeX_;
        	FOutput[1] = gazeY_;
        	
            //FLogger.Log(LogType.Debug, "hi tty!");
        }
    }
}
