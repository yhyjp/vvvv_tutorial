#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "LSystem", Category = "LSystem", Help = "Generate lines by L-System", Tags = "")]
	#endregion PluginInfo
	public class C1_0LSystemLSystemNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0, IsSingle = true)]
		public ISpread<String> FInput;
		[Input("Angle", DefaultValue = 1.0, IsSingle = true)]
		public ISpread<double> FInputAngle;
		[Input("Size", DefaultValue = 1.0, IsSingle = true)]
		public ISpread<double> FInputSize;
		[Input("DeltaAngle", DefaultValue = 1.0, IsSingle = true)]
		public ISpread<double> FInputDeltaAngle;
		[Input("DeltaSize", DefaultValue = 1.0, IsSingle = true)]
		public ISpread<double> FInputDeltaSize;
		[Input("Depth", DefaultValue = 1.0, IsSingle = true)]
		public ISpread<int> FInputDepth;
		[Input("Roughness", DefaultValue = 1)]
		public ISpread<int> FInputRoughness;
		[Input("VerticesMax", DefaultValue = 50000)]
		public ISpread<int> FInputVerticesMax;
		[Input("Enabled", DefaultValue = 1.0, IsSingle = true)]
		public ISpread<bool> FInputEnabled;
		

		[Output("FromX")]
		public ISpread<double> FOutputFromX;
		[Output("FromY")]
		public ISpread<double> FOutputFromY;
		[Output("ToX")]
		public ISpread<double> FOutputToX;
		[Output("ToY")]
		public ISpread<double> FOutputToY;

		[Import()]
		public ILogger FLogger;
		
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInputEnabled[0] == false) return;
			String [] exprList = FInput[0].Split('\n');
			int verticesMax = FInputVerticesMax[0];
			int roughness = Math.Max(FInputRoughness[0], 1);
			String lString = getLString(exprList, FInputDepth[0], verticesMax);
			int nPoints = Math.Min(verticesMax, countChar(lString, 'F') / roughness);
			FOutputFromX.SliceCount = nPoints;
			FOutputFromY.SliceCount = nPoints;
			FOutputToX.SliceCount = nPoints;
			FOutputToY.SliceCount = nPoints;
			draw(lString, FInputAngle[0], FInputSize[0], roughness);
		}
		
		class State
		{
			public double currentAngle_;
			public double angle_, size_;
			public double x_, y_;
			public State() {}
			public State(double currentAngle, double angle, double size, double x, double y)
			{
				currentAngle_ = currentAngle;
				angle_ = angle;
				size_ = size;
				x_ = x;
				y_ = y;
			}
			
			public State copy()
			{
				return new State(currentAngle_, angle_, size_, x_, y_);
			}
		}
		
		void draw(String s, double initAngle, double initSize, int roughness)
		{
			double deltaAngle = FInputDeltaAngle[0];
			double deltaSize = FInputDeltaSize[0];
			Stack<State> stStack = new Stack<State>();
			State st = new State(90, initAngle, initSize, 0, 0);
			int p = 0;
			int pc = 0;
			double prevX = st.x_;
			double prevY = st.y_;
			double degToRad = Math.PI / 180.0;
			for (int i=0; i < s.Length; ++i)
			{
				char c = s[i];
				if (c == '+') st.currentAngle_ += st.angle_;
				else if (c == '-') st.currentAngle_ -= st.angle_;
				else if (c == '>') st.size_ *= (1 + deltaSize);
				else if (c == '<') st.size_ *= (1 - deltaSize);
				else if (c == ')') st.angle_ *= (1 + deltaAngle);
				else if (c == '(') st.angle_ *= (1 - deltaAngle);
				else if (c == '[') stStack.Push(st.copy());
				else if (c == ']') st = stStack.Pop();
				else if (c == '!') st.angle_ = -st.angle_;
				else if (c == '|') st.angle_ += 180;
				else if (c == 'F')
				{
					double rad = st.currentAngle_ * degToRad;
					double dx = Math.Cos(rad) * st.size_;
					double dy = Math.Sin(rad) * st.size_;
					double toX = st.x_ + dx;
					double toY = st.y_ + dy;
					
					if (pc % roughness == 0) {
						FOutputFromX[p] = -prevX;
						FOutputFromY[p] = prevY;
						FOutputToX[p] = -toX;
						FOutputToY[p] = toY;
						prevX = toX;
						prevY = toY;
						++p;
					}
					++pc;
					
					st.x_ = toX;
					st.y_ = toY;
				}
			}
		}
		
		private int countChar(String s, char c)
		{
			int res = 0;
			foreach (char t in s) if (t==c) ++res;
			return res;
		}
		
		private String getLString(String[] exprList, int depth, int verticesMax)
		{
			List<Term> terms = new List<Term>();
			Dictionary<char, Term> termDict = new Dictionary<char, Term>();
		
			for (int i=0; i < exprList.Length; ++i)
			{
				String expr = exprList[i].Replace(" ", "");
				if (expr == "") continue;
				Term t = new Term(expr);
				terms.Add(t);
				termDict[t.Name] = t;
			}
			
			Term result = terms[0];
			
			StringBuilder res = new StringBuilder();
			result.travase(res, termDict, 0, depth, 0, verticesMax); 
			return res.ToString();
		}
		
		private class Term
		{
			private char name_;
			private String value_;
			
			public char Name { get { return name_; } }
			public String Value { get { return value_; } }
			
			public Term(String expr)
			{
				analyze(expr);
			}
			
			public Term(char name, String val)
			{
				name_ = name;
				value_ = val;
			}
			
			public Term(Term src)
			{
				name_ = src.Name;
				value_ = src.Value;
			}
			
			private void analyze(String expr)
			{
				String t = expr.Replace(" ", "");
				String [] nameAndValue = expr.Split(':');
				name_ = nameAndValue[0][0];
				value_ = nameAndValue[1];
			}
			
			public override String ToString()
			{
				return name_ + " = " + value_;
			}
			
			public void replace(Term term)
			{
				value_ = value_.Replace(term.Name.ToString(), term.Value);
			}
			
			public bool isSystemChar(char c)
			{
				return (c=='+' || c == '-' || c == '>' || c == '<' ||
						c=='(' || c == ')' || c == '[' || c == ']' ||
						c=='!' || c == '|' || c == 'F');
			}
			
			public void travase(StringBuilder res,
								 Dictionary<char, Term> dict,
								 int d, int depth,
								 int cf, int fMax)
			{
				if (d == depth) return;
				if (cf >= fMax) return;
				for (int i=0; i < value_.Length; ++i)
				{
					char c = value_[i];
					if (isSystemChar(c)) {
						res.Append(c);
						if (c == 'F') ++cf;
						if (cf >= fMax) return;
					}
					else if (dict.ContainsKey(c))
					{
						dict[c].travase(res, dict, d+1, depth, cf, fMax);
					}
				}
			}
		}
	}
}
