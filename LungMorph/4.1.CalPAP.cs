using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace LungMorphApp
{
	class CalPAP : CalParen, IDisposable
	{
		internal UnmanagedImage UnmanagedResult; public Bitmap BitmapResult { get { return UnmanagedResult?.ToManagedImage(false); } }

		[Description("Number of airspaces that are in the normal range accepted for profiling and categorization (#)")]
		public int Normal_count { get; set; }
		[Description("Number of airspaces that are in the normal range accepted for profiling and categorization (#/mm2 of paren)")]
		public double Normal_Count { get; set; }
				
		[Description("Sum of accepted normal area (µm2)")]
		public double Normal_SumArea { get; set; }

		public static string getEntryNote { get { return "Entry Created at,"+DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local); } }
		
		public static string getDescription {
			get {
				return string.Join(",",
					nameof(FileName).Description(typeof(CalPAP)), nameof(Group).Description(typeof(CalPAP)),nameof(Individual).Description(typeof(CalPAP)), nameof(Grouping).Description(typeof(CalPAP)), nameof(Normal_count).Description(typeof(CalPAP)),  nameof(Low_Threshold).Description(typeof(CalPAP)), nameof(High_Threshold).Description(typeof(CalPAP)), nameof(Normal_Count).Description(typeof(CalPAP)), nameof(Normal_SumArea).Description(typeof(CalPAP)), nameof(Total_SumArea).Description(typeof(CalPAP)), nameof(Lung_SumArea).Description(typeof(CalPAP)), nameof(Paren_SumArea).Description(typeof(CalPAP)),  nameof(Tis_SumArea).Description(typeof(CalPAP)) );
			}
		}
        public static string getHeader {
			get {
				return string.Join(",",
					nameof(FileName), nameof(Group),nameof(Individual), nameof(Grouping), nameof(Normal_count), nameof(Low_Threshold), nameof(High_Threshold), nameof(Normal_Count), nameof(Normal_SumArea), nameof(Total_SumArea), nameof(Lung_SumArea), nameof(Paren_SumArea), nameof(Tis_SumArea));
			}
		}
		public string getResult {
			get {
				return string.Join(",", $"\"{FileName}\"", $"\"{Group}\"", $"\"{Individual}\"", $"\"{Grouping}\"",
                    Normal_count, Low_Threshold, High_Threshold, Normal_Count, Normal_SumArea, Total_SumArea, Lung_SumArea, Paren_SumArea, Tis_SumArea );
			}
		}

		public new void Dispose()
		{
			base.Dispose(); UnmanagedResult?.Dispose();
		}
		public CalPAP(UISettings ui, FileData file, ref ConcurrentBag<double> AreaBag) : base(ui, file)
		{
			try {				
				MulticolorComponentsLabeling mclabel = new MulticolorComponentsLabeling() {
					Low=(int)Math.Round(Low_Threshold/ui.um2px2),
					High=(int)Math.Round(Math.Min(int.MaxValue, High_Threshold/ui.um2px2))
				};

				Invert AFinvert = new Invert();
				UnmanagedResult=mclabel.Apply(AFinvert.Apply(UnmanagedBlackWhite));
				foreach (Blobx blob in mclabel.BlobCounter.blobs) {
					//if (blob.Area<mclabel.Low) {
					//	Low_count++; Low_sumArea+=blob.Area;
					//} else if (blob.Area<=mclabel.High) {
						Normal_count++; Normal_SumArea+=ui.um2px2*blob.Area; AreaBag.Add(ui.um2px2*blob.Area);
					//} else {
					//	High_count++; High_sumArea+=blob.Area;
					//}
				}

				Total_SumArea=ui.um2px2*(UnmanagedMarkup.Width*UnmanagedMarkup.Height); // total area
				Lung_SumArea=Total_SumArea-Low_SumArea-High_SumArea; // lung area
				Paren_SumArea=Lung_SumArea-NonParen_SumArea; // parenchymal area
				Tis_SumArea=Paren_SumArea-Normal_SumArea; // septum tissue				
				Normal_Count=1000.0d*1000.0d*Normal_count/Paren_SumArea; // counts per reference area

				StringBuilder header = new StringBuilder();
				header.Append($"{FileName}  ps: {ui.PixelScale:G2}/{ui.ResizeValue:G2} px/um"
					 +$"\nlung: {Lung_SumArea:G2}µm\xB2, {Lung_SumArea/Total_SumArea:0%} image (blw: {Low_SumArea/Total_SumArea:0%} ovr: {High_SumArea/Total_SumArea:0%})"
					 +$"\nparenchyma: {Paren_SumArea:G2}µm\xB2, {Paren_SumArea/Lung_SumArea:0%} lung (exc: {NonParen_SumArea/Total_SumArea:0%})"
					 +$"\nseptum: {Tis_SumArea:G2}µm\xB2, {Tis_SumArea/Paren_SumArea:0%} paren (airspace: {Normal_SumArea/Total_SumArea:0%})");
            
				StringBuilder footer = new StringBuilder();
				footer.Append($"Total #: {mclabel.BlobCounter.blobs.Count}");

				UnmanagedMarkup=UnmanagedImage.FromManagedImage(AddBlobText(UnmanagedMarkup.ToManagedImage(false), Color.Black, $"{header}", $"{footer}", null, null, (int)Math.Round(0.02d*UnmanagedMarkup.Width*Math.Sqrt(ui.ExportDetailRatio))));
				UnmanagedResult=UnmanagedImage.FromManagedImage(AddBlobText(UnmanagedResult.ToManagedImage(false), Color.PaleGreen, $"{header}", $"{footer}", null, null, (int)Math.Round(0.02d*UnmanagedMarkup.Width*Math.Sqrt(ui.ExportDetailRatio))));
			} catch { throw new Exception("Error Occured During Airspace Profiling"); }
		}

	}
}
