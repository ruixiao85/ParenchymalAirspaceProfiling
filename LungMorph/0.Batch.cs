using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using AForge.Imaging;
using System.ComponentModel;

namespace LungMorphApp
{
	public static class Batch
	{
		public static async Task BatchAQ1(this UISettings ui, List<FileData> listfiles)
		{
			try {
				Directory.CreateDirectory($"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ1"); // quantify with 1-dist					
				using (var file = File.AppendText($"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ1.csv")) {
					file.Write("\n"+CalPAQ1.getEntryNote+"\n"+CalPAQ1.getDescription+"\n"+CalPAQ1.getHeader);
				}
				var progress = new Progress($"Analyzing {listfiles.Count} files ...", listfiles.Count);
				using (ui.cts=new CancellationTokenSource()) {
					if (!ui.MultiThreadingSwitch) { // false - single thread
						foreach (FileData imgfile in listfiles) {
							using (var img = await Task<CalPAQ1>.Factory.StartNew(() => { return new CalPAQ1(ui, imgfile); })) {
								ui.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
								ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ1\\{img.OutName}.png", ui.ExportDetailSwitch);
								progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ1.csv", img.getResult);
							}
							ui.cts.Token.ThrowIfCancellationRequested();
						}
					} else { // true - open multi thread pool
						await Task.Factory.StartNew(() => {
							Parallel.ForEach(listfiles, new ParallelOptions() {
								CancellationToken=ui.cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount
							}, (FileData imgfile) => {
								using (var img = new CalPAQ1(ui, imgfile)) {
									ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ1\\{img.OutName}.jpe", ui.ExportDetailSwitch);
									progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ1.csv", img.getResult);
								}
							});
						});
					}
				}
			} catch { throw new Exception("Error encountered during batch airspace quantification."); }
		}

		public static async Task BatchAQ2(this UISettings ui, List<FileData> listfiles)
		{
			try {
				Directory.CreateDirectory($"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ2"); // quantify with 2-dist								
				using (var file = File.AppendText($"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ2.csv")) {
					file.Write("\n"+CalBlobAQ2.getEntryNote+"\n"+CalBlobAQ2.getDescription+"\n"+CalBlobAQ2.getHeader);
				}
				var progress = new Progress($"Analyzing {listfiles.Count} files ...", listfiles.Count);
				using (ui.cts=new CancellationTokenSource()) {
					if (!ui.MultiThreadingSwitch) { // false - single thread
						foreach (FileData imgfile in listfiles) {
							using (var img = await Task<CalBlobAQ2>.Factory.StartNew(() => { return new CalBlobAQ2(ui, imgfile); })) {
								ui.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
								ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ2\\{img.OutName}.png", ui.ExportDetailSwitch);
								progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ2.csv", img.getResult);
							}
							ui.cts.Token.ThrowIfCancellationRequested();
						}
					} else { // true - open multi thread pool
						await Task.Factory.StartNew(() => {
							Parallel.ForEach(listfiles, new ParallelOptions() {
								CancellationToken=ui.cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount
							}, (FileData imgfile) => {
								using (var img = new CalBlobAQ2(ui, imgfile)) {
									ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ2\\{img.OutName}.jpe", ui.ExportDetailSwitch);
									progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ2.csv", img.getResult);
								}
							});
						});
					}
				}
			} catch { throw new Exception("Error encountered during batch airspace quantification."); }
		}

		public static async Task BatchAP1(this UISettings ui, List<FileData> listfiles)
		{
			try {
				Directory.CreateDirectory($"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAP"); // profile
				using (var file = File.AppendText($"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAP.csv")) {
					file.Write("\n"+CalPAC.getEntryNote+"\n"+CalPAC.getDescription+"\n"+CalPAC.getHeader);
				}
				Directory.CreateDirectory($"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ1"); // quantify with 1-dist					
				using (var file = File.AppendText($"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ1.csv")) {
					file.Write("\n"+CalPAQ1.getEntryNote+"\n"+CalPAQ1.getDescription+"\n"+CalPAQ1.getHeader);
				}

				switch (ui.ProfilingIndex) {
					case 0: foreach (FileData f in listfiles) f.Grouping="All"; break;
					case 1: foreach (FileData f in listfiles) f.Grouping=f.Group; break;
					case 2: foreach (FileData f in listfiles) f.Grouping=f.Group+"-"+f.Individual; break;
					case 3: foreach (FileData f in listfiles) f.Grouping=f.OutName; break;
				}
				var filegroups = listfiles.GroupBy(g => g.Grouping).Select(l => l.ToList()).ToList();
				var progress = new Progress($"Analyzing {listfiles.Count} files in {filegroups.Count} groups ...", listfiles.Count*2+filegroups.Count); // 2X the work + additional categorization
				using (ui.cts=new CancellationTokenSource()) {
					foreach (List<FileData> filegroup in filegroups) { // Process each group
						if (!ui.MultiThreadingSwitch) { // false - single thread
							var areaPool = new ConcurrentBag<double>();
							foreach (FileData imgfile in filegroup) {
								using (var img = await Task<CalPAP>.Factory.StartNew(() => { return new CalPAP(ui, imgfile, ref areaPool); })) {
									ui.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
									ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAP\\{img.OutName}.jpe", ui.ExportDetailSwitch);
									progress.Increment(ui); //incremenet without writing result
								}
								ui.cts.Token.ThrowIfCancellationRequested();
							}

							var categ = await Task<CalPAC>.Factory.StartNew(() => { return new CalPAC(ui, filegroup[0].Grouping, filegroup.Count, areaPool.ToList()); });
							ui.Graph1=categ.ToOxyPlot(1);
							ui.Graph1.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_1-Count.png");
							ui.Graph2=categ.ToOxyPlot(2);
							ui.Graph2.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_2-Area.png");
							ui.Graph3=categ.ToOxyPlot(3);
							ui.Graph3.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_3-Count&Area.png");
							progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAP.csv", categ.getResult);
							ui.cts.Token.ThrowIfCancellationRequested();

							foreach (FileData imgfile in filegroup) {
								using (var img = await Task<CalPAQ1>.Factory.StartNew(() => { return new CalPAQ1(ui, imgfile, categ); })) {
									ui.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
									ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ1\\{img.OutName}.png", ui.ExportDetailSwitch);
									progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ1.csv", img.getResult);
								}
								ui.cts.Token.ThrowIfCancellationRequested();
							}

						} else { // true - open multi thread pool
							var categ = await Task<CalPAC>.Factory.StartNew(() => {
								var areaPool = new ConcurrentBag<double>();
								Parallel.ForEach(filegroup, new ParallelOptions() {
									CancellationToken=ui.cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount
								}, (FileData imgfile) => {
									using (var img = new CalPAP(ui, imgfile, ref areaPool)) {
										ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAP\\{img.OutName}.jpe", ui.ExportDetailSwitch);
										progress.Increment(ui); //incremenet without writing result
									}
								});
								return new CalPAC(ui, filegroup[0].Grouping, filegroup.Count, areaPool.ToList());
							}); ui.cts.Token.ThrowIfCancellationRequested();

							ui.Graph1=categ.ToOxyPlot(1);
							ui.Graph1.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_1-Count.png");
							ui.Graph2=categ.ToOxyPlot(2);
							ui.Graph2.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_2-Area.png");
							ui.Graph3=categ.ToOxyPlot(3);
							ui.Graph3.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_3-Count&Area.png");
							progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAP.csv", categ.getResult);
							ui.cts.Token.ThrowIfCancellationRequested();

							await Task.Factory.StartNew(() => {
								Parallel.ForEach(filegroup, new ParallelOptions() {
									CancellationToken=ui.cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount
								}, (FileData imgfile) => {
									using (var img = new CalPAQ1(ui, imgfile, categ)) {
										ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ1\\{img.OutName}.jpe", ui.ExportDetailSwitch);
										progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ1.csv", img.getResult);
									}
								});
							});
							ui.cts.Token.ThrowIfCancellationRequested();
						}
					}
				}
			} catch { throw new Exception("Error encountered during batch airspace profiling."); }
		}

		public static async Task BatchAP2(this UISettings ui, List<FileData> listfiles)
		{
			try {
				Directory.CreateDirectory($"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAP"); // profile
				using (var file = File.AppendText($"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAP.csv")) {
					file.Write("\n"+CalPAC.getEntryNote+"\n"+CalPAC.getDescription+"\n"+CalPAC.getHeader);
				}
				Directory.CreateDirectory($"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ2"); // quantify with 2-dist								
				using (var file = File.AppendText($"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ2.csv")) {
					file.Write("\n"+CalBlobAQ2.getEntryNote+"\n"+CalBlobAQ2.getDescription+"\n"+CalBlobAQ2.getHeader);
				}

				switch (ui.ProfilingIndex) {
					case 0: foreach (FileData f in listfiles) f.Grouping="All"; break;
					case 1: foreach (FileData f in listfiles) f.Grouping=f.Group; break;
					case 2: foreach (FileData f in listfiles) f.Grouping=f.Group+"-"+f.Individual; break;
					case 3: foreach (FileData f in listfiles) f.Grouping=f.OutName; break;
				}
				var filegroups = listfiles.GroupBy(g => g.Grouping).Select(l => l.ToList()).ToList();
				var progress = new Progress($"Analyzing {listfiles.Count} files in {filegroups.Count} groups ...", listfiles.Count*2+filegroups.Count); // 2X the work + additional categorization
				using (ui.cts=new CancellationTokenSource()) {
					foreach (List<FileData> filegroup in filegroups) { // Process each group
						if (!ui.MultiThreadingSwitch) { // false - single thread
							var areaPool = new ConcurrentBag<double>();
							foreach (FileData imgfile in filegroup) {
								using (var img = await Task<CalPAP>.Factory.StartNew(() => { return new CalPAP(ui, imgfile, ref areaPool); })) {
									ui.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
									ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAP\\{img.OutName}.jpe", ui.ExportDetailSwitch);
									progress.Increment(ui); //incremenet without writing result
								}
								ui.cts.Token.ThrowIfCancellationRequested();
							}

							var categ = await Task<CalPAC>.Factory.StartNew(() => { return new CalPAC(ui, filegroup[0].Grouping, filegroup.Count, areaPool.ToList()); });
							ui.Graph1=categ.ToOxyPlot(1);
							ui.Graph1.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_1-Count.png");
							ui.Graph2=categ.ToOxyPlot(2);
							ui.Graph2.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_2-Area.png");
							ui.Graph3=categ.ToOxyPlot(3);
							ui.Graph3.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_3-Count&Area.png");
							progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAP.csv", categ.getResult);
							ui.cts.Token.ThrowIfCancellationRequested();

							foreach (FileData imgfile in filegroup) {
								using (var img = await Task<CalBlobAQ2>.Factory.StartNew(() => { return new CalBlobAQ2(ui, imgfile, categ); })) {
									ui.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
									ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ2\\{img.OutName}.png", ui.ExportDetailSwitch);
									progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ2.csv", img.getResult);
								}
								ui.cts.Token.ThrowIfCancellationRequested();
							}
						} else { // true - open multi thread pool
							var categ = await Task<CalPAC>.Factory.StartNew(() => {
								var areaPool = new ConcurrentBag<double>();
								Parallel.ForEach(filegroup, new ParallelOptions() {
									CancellationToken=ui.cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount
								}, (FileData imgfile) => {
									using (var img = new CalPAP(ui, imgfile, ref areaPool)) {
										ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAP\\{img.OutName}.jpe", ui.ExportDetailSwitch);
										progress.Increment(ui); //incremenet without writing result
									}
								});
								return new CalPAC(ui, filegroup[0].Grouping, filegroup.Count, areaPool.ToList());
							}); ui.cts.Token.ThrowIfCancellationRequested();

							ui.Graph1=categ.ToOxyPlot(1);
							ui.Graph1.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_1-Count.png");
							ui.Graph2=categ.ToOxyPlot(2);
							ui.Graph2.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_2-Area.png");
							ui.Graph3=categ.ToOxyPlot(3);
							ui.Graph3.ExportPng($"{ui.WorkDirectory}\\{ui.ProjectName}\\{filegroup[0].Grouping}_3-Count&Area.png");
							progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAP.csv", categ.getResult);
							ui.cts.Token.ThrowIfCancellationRequested();

							await Task.Factory.StartNew(() => {
								Parallel.ForEach(filegroup, new ParallelOptions() {
									CancellationToken=ui.cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount
								}, (FileData imgfile) => {
									using (var img = new CalBlobAQ2(ui, imgfile, categ)) {
										ui.Export2Bitmap(img.BitmapMarkup, img.BitmapResult, $"{ui.WorkDirectory}\\{ui.ProjectName}\\LMAQ2\\{img.OutName}.jpe",ui.ExportDetailSwitch);
										progress.Increment(ui, $"{ui.WorkDirectory}\\{ui.ProjectName}\\_{ui.ProjectName}_LMAQ2.csv", img.getResult);
									}
								});
							});
							ui.cts.Token.ThrowIfCancellationRequested();
						}
					}
				}
			} catch { throw new Exception("Error encountered during batch airspace profiling."); }
		}

		public static string Description(this string propertyname, Type type)
		{
			try {
				DescriptionAttribute description = (DescriptionAttribute)(type.GetProperty(propertyname).GetCustomAttributes(typeof(DescriptionAttribute), true)[0]);
				return description.Description;
			} catch { return ""; }
		}

		public static bool Export2Bitmap(this UISettings ui, Bitmap bmp1, Bitmap bmp2, string filename, Boolean separate)
		{
			try { // compare bmp1 bmp2 dimension may be neccessary
                if (separate) { //export in 2 files
                    int lastdot = filename.LastIndexOf('.');
                    string file2 = filename.Substring(0, lastdot) + "_bw" + filename.Substring(lastdot);
                    using (Bitmap finalBitmap = new Bitmap(bmp1.Width / ui.ExportDetailRatio, bmp1.Height / ui.ExportDetailRatio))
                    {
                        using (Graphics gm = Graphics.FromImage(finalBitmap))
                        {
                            gm.Clear(Color.Black); //set background color
                            gm.DrawImage(bmp1, new Rectangle(0, 0, bmp1.Width / ui.ExportDetailRatio, bmp1.Height / ui.ExportDetailRatio));                            
                        }
                        finalBitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg.getEncoder(), 85L.jpgEncoderCQ());
                    }
                    using (Bitmap finalBitmap = new Bitmap(bmp2.Width / ui.ExportDetailRatio, bmp2.Height / ui.ExportDetailRatio))
                    {
                        using (Graphics gm = Graphics.FromImage(finalBitmap))
                        {
                            gm.Clear(Color.Black); //set background color                            
                            gm.DrawImage(bmp2, new Rectangle(0, 0, bmp2.Width / ui.ExportDetailRatio, bmp2.Height / ui.ExportDetailRatio));
                        }
                        finalBitmap.Save(file2, System.Drawing.Imaging.ImageFormat.Jpeg.getEncoder(), 85L.jpgEncoderCQ());
                    }
                    return true;
                } else { //export in 1 file, horizontal merge
                    using (Bitmap finalBitmap = new Bitmap(bmp1.Width * 2 / ui.ExportDetailRatio, bmp1.Height / ui.ExportDetailRatio))
                    {
                        using (Graphics gm = Graphics.FromImage(finalBitmap))
                        {
                            gm.Clear(Color.Black); //set background color
                            gm.DrawImage(bmp1, new Rectangle(0, 0, bmp1.Width / ui.ExportDetailRatio, bmp1.Height / ui.ExportDetailRatio));
                            gm.DrawImage(bmp2, new Rectangle(bmp1.Width / ui.ExportDetailRatio, 0, bmp1.Width / ui.ExportDetailRatio, bmp1.Height / ui.ExportDetailRatio));
                        }
                        finalBitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg.getEncoder(), 85L.jpgEncoderCQ());
                        return true;
                    }
                }
            } catch { return false; }
		}
		/// <summary>
		/// Plot calculated distribution with OxyPlot (type 0-null 1-size 2-area 3-combined)
		/// </summary>
		/// <param name="categ">categorized information CalBlobAC</param>
		/// <param name="type">0-null 1-size 2-area 3-combined</param>
		/// <returns></returns>
		public static PlotModel ToOxyPlot(this CalPAC categ, int type)
		{
			try {
				var plot = new PlotModel();
				switch (type) {
					case 1: plot.Subtitle=$"Size Distribution\n{categ.Grouping} (N={categ.N})"; break;
					case 2: plot.Subtitle=$"Area Distribution\n{categ.Grouping} (N={categ.N})"; break;
					case 3: plot.Subtitle=$"Size & Area-Weighted Size Distribution\n{categ.Grouping} (N={categ.N})"; break;
					default: plot.Subtitle=""; break;
				}
				plot.Axes.Add(new LinearAxis { Title="Airspace log10 µm\xB2", Position=AxisPosition.Bottom, Minimum=0, Maximum=7, MajorStep=1, MinorStep=0.25, TickStyle=TickStyle.Inside });
				plot.Axes.Add(new LinearAxis { Title="Density", Position=AxisPosition.Left, Minimum=0, Maximum=1, MajorStep=0.2, MinorStep=0.05, TickStyle=TickStyle.Inside });
				var fillsize = OxyColor.FromArgb(130, 0, 0, 230);
				var fillarea = OxyColor.FromArgb(130, 225, 19, 2);
				var outline = OxyColor.FromArgb(160, 0, 0, 0);
				var draw1 = OxyColor.FromArgb(255, 79, 154, 6);
				var draw2 = OxyColor.FromArgb(255, 220, 158, 30);
				var draw3 = OxyColor.FromArgb(255, 204, 0, 0);
				double width = 0.05; double offset = 0.05;
				if (type==1||type==3) { // size
					var histogram = new RectangleBarSeries() { Title=$"Size Dist (LogLikelihood={categ.gmm1Fit:G2})", FillColor=fillsize, StrokeColor=outline };
					for (int i = 0; i<categ.DistCount.Count; i++) { histogram.Items.Add(new RectangleBarItem(categ.DistCount[i][0]-width-offset, 0, categ.DistCount[i][0]+width-offset, categ.DistCount[i][1])); }
					plot.Series.Add(histogram);
					plot.Series.Add(FormGaussian($"Population 1 (µ={categ.p1Mean:0.00}, σ={categ.p1Covariance:0.00}, λ={categ.p1Proportion:0.00})", draw1, 0, 10, categ.p1Mean, categ.p1Covariance, categ.p1Proportion));
					plot.Annotations.Add(new LineAnnotation() { Type=LineAnnotationType.Vertical, X=categ.c1_DucDes_CI95_1tail, Text=$"x={categ.c1_DucDes_CI95_1tail:0.0}", Color=fillsize, StrokeThickness=2 });
				}
				if (type==2||type==3) { // area
					var histogram = new RectangleBarSeries() { Title=$"Area-Weighted Size Dist (LogLikelihood={categ.gmm2Fit:G2})", FillColor=fillarea, StrokeColor=outline };
					for (int i = 0; i<categ.DistArea.Count; i++) { histogram.Items.Add(new RectangleBarItem(categ.DistArea[i][0]-width+offset, 0, categ.DistArea[i][0]+width+offset, categ.DistArea[i][1])); }
					plot.Series.Add(histogram);
					plot.Series.Add(FormGaussian($"Population 2 (µ={categ.p2Mean:0.00}, σ={categ.p2Covariance:0.00}, λ={categ.p2Proportion:0.00})", draw2, 0, 10, categ.p2Mean, categ.p2Covariance, categ.p2Proportion));
					plot.Series.Add(FormGaussian($"Population 3 (µ={categ.p3Mean:0.00}, σ={categ.p3Covariance:0.00}, λ={categ.p3Proportion:0.00})", draw3, 0, 10, categ.p3Mean, categ.p3Covariance, categ.p3Proportion));
					if (type==3) plot.Annotations.Add(new LineAnnotation() { Type=LineAnnotationType.Vertical, X=categ.c2_Sac_Log_Threshold, Text=$"x={categ.c2_Sac_Log_Threshold:0.0}", Color=OxyColor.FromArgb(130, 220, 158, 30), StrokeThickness=2 });
					plot.Annotations.Add(new LineAnnotation() { Type=LineAnnotationType.Vertical, X=categ.c2_DucDes_Log_Threshold, Text=$"x={categ.c2_DucDes_Log_Threshold:0.0}", Color=fillarea, StrokeThickness=2 });
				}
				return plot;
			} catch { throw new Exception("Error encountered generating histogram and distribution graph."); }
		}
		public static LineSeries FormGaussian(string title, OxyColor color, double x0, double x1, double mean, double variance, double proportion = 1, int n = 500)
		{
			var ls = new LineSeries { Title=title, Color=color };
			for (int i = 0; i<n; i++) {
				double x = x0+((x1-x0)*i/(n-1));
				double f = 1.0/Math.Sqrt(2*Math.PI*variance)*Math.Exp(-(x-mean)*(x-mean)/2/variance);
				ls.Points.Add(new DataPoint(x, f*proportion));
			}
			return ls;
		}
		
		public static void ExportPng(this PlotModel plot, string file)
		{
			try {
				OxyPlot.Wpf.PngExporter.Export(plot, file, 1024, 768, OxyColors.White, 150);
			} catch { throw new Exception("Error encountered exporting the distribution graph."); }
		}
		//public static double GaussDist(double x, double mu = 0, double sigma = 1)
		//{
		//	double p1 = -0.5d*Math.Pow(((x-mu)/sigma), 2);
		//	double p2 = (sigma*Math.Sqrt(2*Math.PI));
		//	return Math.Pow(Math.E, p1)/p2;
		//}
		public static unsafe bool IsBlack(this UnmanagedImage image)
		{
			//image.PixelFormat == PixelFormat.Format8bppIndexed
			int offset = image.Stride-image.Width;
			byte* p = (byte*)image.ImageData.ToPointer();
			for (int y = 0; y<image.Height; y++) {
				for (int x = 0; x<image.Width; x++, p++) {
					if (*p!=0) return false;
				}
				p+=offset;
			}
			return true;
		}
		public static unsafe int NonBlackArea(this UnmanagedImage image)
		{
			//image.PixelFormat == PixelFormat.Format8bppIndexed        
			int nonblackarea = 0;
			int offset = image.Stride-image.Width;
			byte* p = (byte*)image.ImageData.ToPointer();
			for (int y = 0; y<image.Height; y++) {
				for (int x = 0; x<image.Width; x++, p++) {
					if (*p!=0) nonblackarea++;
				}
				p+=offset;
			}
			return nonblackarea;
		}
		
		
	}

}
