using Accord.MachineLearning;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace LungMorphApp
{
	public class CalPAC
	{
		[Description("Grouping identifier")] public string Grouping { get; set; }
		[Description("Numbers of images included in this profiling and categorization process (#)")] public int N { get; set; }

		[Description("Log-likelihood ratio of Gaussian Mixture Model #1")] public double gmm1Fit { get; set; }
		[Description("Mean of Population #1: Alveolar (log10 µm2)")] public double p1Mean { get; set; }
		[Description("Covariance of Population #1: Alveolar (log10 µm2)")] public double p1Covariance { get; set; }
		[Description("Proportion of Population #1: Alveolar (log10 µm2)")] public double p1Proportion { get; set; }

		[Description("Log-likelihood ratio of Gaussian Mixture Model #2")] public double gmm2Fit { get; set; }
		[Description("Mean of Population #2: Saccular (log10 µm2)")] public double p2Mean { get; set; }
		[Description("Covariance of Population #2: Saccular (log10 µm2)")] public double p2Covariance { get; set; }
		[Description("Proportion of Population #2: Saccular (log10 µm2)")] public double p2Proportion { get; set; }
		[Description("Mean of Population #3: Ductal/Destructive (log10 µm2)")] public double p3Mean { get; set; }
		[Description("Covariance of Population #3: Ductal/Destructive (log10 µm2)")] public double p3Covariance { get; set; }
		[Description("Proportion of Population #3: Ductal/Destructive (log10 µm2)")] public double p3Proportion { get; set; }

		/// 90CI=1.645d 95CI=1.96d
		[Description("Threshold calculated with size distribution using 95% confidence Interval (1-tail) from Population #1")] public double c1_DucDes_CI95_1tail { get { return p1Mean+1.645d*Math.Sqrt(p1Covariance); } }
		//[Description("Confidence Interval of 95% from Population #1: Alveolar (log10 µm2)")] public double c1_Alv_CI95 { get { return p1Mean+1.96d*Math.Sqrt(p1Covariance); } }

		[Description("Threshold calculated with both size and area-weighted distributions that separates single alveolar and saccular airspaces (log10 µm2)")] public double c2_Sac_Log_Threshold { get { return (p1Mean*Math.Sqrt(p2Covariance)+p2Mean*Math.Sqrt(p1Covariance))/(Math.Sqrt(p1Covariance)+Math.Sqrt(p2Covariance)); } }
		[Description("Threshold calculated with both size and area-weighted distributions that separates ductal/destructive and saccular airspaces (log10 µm2)")] public double c2_DucDes_Log_Threshold { get { return (p3Mean*Math.Sqrt(p2Covariance)+p2Mean*Math.Sqrt(p3Covariance))/(Math.Sqrt(p3Covariance)+Math.Sqrt(p2Covariance)); } }



		public List<double[]> DistCount { get; set; }
		public List<double[]> DistArea { get; set; }

		public static string getEntryNote { get { return "Entry Created at,"+DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local); } }
		public static string getDescription {
			get {
				return string.Join(",",
					nameof(Grouping).Description(typeof(CalPAC)), nameof(N).Description(typeof(CalPAC)), nameof(p1Mean).Description(typeof(CalPAC)), nameof(p1Covariance).Description(typeof(CalPAC)), nameof(p1Proportion).Description(typeof(CalPAC)), nameof(p2Mean).Description(typeof(CalPAC)), nameof(p2Covariance).Description(typeof(CalPAC)), nameof(p2Proportion).Description(typeof(CalPAC)), nameof(p3Mean).Description(typeof(CalPAC)), nameof(p3Covariance).Description(typeof(CalPAC)), nameof(p3Proportion).Description(typeof(CalPAC)), nameof(c1_DucDes_CI95_1tail).Description(typeof(CalPAC)), nameof(c2_Sac_Log_Threshold).Description(typeof(CalPAC)), nameof(c2_DucDes_Log_Threshold).Description(typeof(CalPAC)));
			}
		}
		public static string getHeader {
			get {
				return string.Join(",",
					nameof(Grouping), nameof(N), nameof(p1Mean), nameof(p1Covariance), nameof(p1Proportion), nameof(p2Mean), nameof(p2Covariance), nameof(p2Proportion), nameof(p3Mean), nameof(p3Covariance), nameof(p3Proportion), nameof(c1_DucDes_CI95_1tail), nameof(c2_Sac_Log_Threshold), nameof(c2_DucDes_Log_Threshold));
			}
		}
		public string getResult {
			get {
				return string.Join(",", $"\"{Grouping}\"",
                    N, p1Mean, p1Covariance, p1Proportion, p2Mean, p2Covariance, p2Proportion, p3Mean, p3Covariance, p3Proportion, c1_DucDes_CI95_1tail, c2_Sac_Log_Threshold, c2_DucDes_Log_Threshold);
			}
		}

		public CalPAC(UISettings ui, string grouping, int n, List<double> AreaPool)
		{
			try {
				//Stopwatch sw = new Stopwatch();
				//sw.Start();

				Grouping=grouping; N=n;
				AreaPool.Sort(); // for weight sampling
				//double px2um2=ui.PixelScale*ui.PixelScale/ui.ResizeRatio/ui.ResizeRatio;
				var CountDis = new List<double[]>();
				double AreaSum = 0.0d;
				for (int i = 0; i<AreaPool.Count; i++) {
					double[] data = new double[1];
					data[0]=Math.Log10(AreaPool[i]+1); //data[1] = (double)(mFull[i]/100.0d);
					CountDis.Add(data);
					AreaSum+=AreaPool[i];
				}

				///1-Dist fitted with 1-Population
				GaussianMixtureModel gmm1 = new GaussianMixtureModel(1);
				gmm1Fit=gmm1.Compute(CountDis.ToArray());
				p1Mean=gmm1.Gaussians[0].Mean[0];
				p1Covariance=gmm1.Gaussians[0].Covariance[0, 0];
				p1Proportion=gmm1.Gaussians[0].Proportion;
				///1-Dist fitted with 2-Populations / take the larger proportion
				//GaussianMixtureModel gmm1=new GaussianMixtureModel(2);
				//gmm1Fit=gmm1.Compute(countDis.ToArray());
				//int m=(gmm1.Gaussians[0].Proportion>gmm1.Gaussians[1].Proportion) ? 0 : 1;
				//p1Mean=gmm1.Gaussians[m].Mean[0];
				//p1Covariance=gmm1.Gaussians[m].Covariance[0, 0];
				//p1Proportion=gmm1.Gaussians[m].Proportion;

				///List<double[]> areaDis = WeightExpansion(AreaPool, divider=10.0d);
				//var AreaDis = new List<double[]>();
				//double divider = 10.0d;
				//for (int i = 0; i<AreaPool.Count; i++) {
				//	double[] data = new double[1];
				//	data[0]=Math.Log10(AreaPool[i]+1);
				//	for (int r = 0; r<Math.Round(AreaPool[i]/divider); r++) { AreaDis.Add(data); }
				//}

				///List<double[]> areaDis = WeightSampling(AreaPool, AreaSum, Math.Max((int)Math.Round(AreaSum/5000.0d), 1)); // sample every divided by 5000 evenly or every one if not too large				
				var AreaDis = new List<double[]>();
				int askip = (int)Math.Round(Math.Max(AreaSum/5000.0d, 1));
				for (int sn = 0; sn<(int)Math.Floor(AreaSum); sn+=askip) { // sample number			
					double s = 0.0d;
					for (int bi = 0; bi<AreaPool.Count; bi++) {
						s+=AreaPool[bi];
						if (s>sn) {
							double[] data = new double[1];
							data[0]=Math.Log10(AreaPool[bi]+1);
							AreaDis.Add(data); break;
						}
					}
				}

				///2-Dist fitted with 2-Populations / 
				GaussianMixtureModel gmm2=new GaussianMixtureModel(2);
				gmm2Fit=gmm2.Compute(AreaDis.ToArray());
				int a = (gmm2.Gaussians[0].Mean[0]<gmm2.Gaussians[1].Mean[0]) ? 0 : 1;
				p2Mean=gmm2.Gaussians[a].Mean[0];
				p2Covariance=gmm2.Gaussians[a].Covariance[0, 0];
				p2Proportion=gmm2.Gaussians[a].Proportion;
				p3Mean=gmm2.Gaussians[1-a].Mean[0];
				p3Covariance=gmm2.Gaussians[1-a].Covariance[0, 0];
				p3Proportion=gmm2.Gaussians[1-a].Proportion;
				
				DistCount=Bucketize(CountDis);
				DistArea=Bucketize(AreaDis);

				//sw.Stop();
				//Debug.Write(sw.Elapsed);
			} catch { throw new Exception("Error Occured During Airspace Categorization"); }
		}
							
		public static List<double[]> Bucketize(List<double[]> source, int dimension = 0, double min = 0, double max = 10, double step = 0.2)
		{
			List<double> bx = new List<double>(); List<double> by = new List<double>(); int tot = 0;
			for (double i = min; i<=max; i+=step) { bx.Add(i); by.Add(0); }
			double[] bxa = bx.ToArray(); double[] bya = by.ToArray();
			foreach (double[] value in source) {
				for (int i = 0; i<bxa.Length; i++) {
					if ((value[dimension]-bxa[i])<=(step/2)) { bya[i]++; tot++; break; }
				}
			}
			//if (tot < source.Count) { MessageBox.Show("Some points are not included.","Warning",MessageBoxButton.OK,MessageBoxImage.Warning); }
			List<double[]> buckets = new List<double[]>();
			for (int i = 0; i<bxa.Length; i++) { buckets.Add(new double[] { bxa[i], bya[i]/tot/step }); }
			return buckets;
		}
	}
}
