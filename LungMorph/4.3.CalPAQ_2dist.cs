using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace LungMorphApp
{
	class CalBlobAQ2 : CalParen, IDisposable
	{
		internal UnmanagedImage UnmanagedResult; public Bitmap BitmapResult { get { return UnmanagedResult?.ToManagedImage(false); } }
			
		[Description("Size cutoff between alveolar and saccular airspaces (µm2)")]
		public double Sac_Threshold { get; set; }		
		[Description("Size cutoff between alveolar and saccular airspaces (log10 µm2)")]
		public double Sac_Log_Threshold { get; set; }		
		[Description("Size cutoff between ductal/destructive and saccular airspaces (µm2)")]
		public double DucDes_Threshold { get; set; }		
		[Description("Size cutoff between ductal/destructive and saccular airspaces (log10 µm2)")]
		public double DucDes_Log_Threshold { get; set; }		
		[Description("Mean alveolar size (µm2)")]
		public double Alv_Size { get; set; }
		[Description("Mean alveolar size (log10 µm2)")]
		public double Alv_Log_Size { get; set; }		
		[Description("Mean saccular size (µm2)")]
		public double Sac_Size { get; set; }
		[Description("Mean saccular size (log10 µm2)")]
		public double Sac_Log_Size { get; set; }	
		[Description("Mean ductal/destructive size (µm2)")]
		public double DucDes_Size { get; set; }
		[Description("Mean ductal/destructive size (log10 µm2)")]
		public double DucDes_Log_Size { get; set; }

		[Description("Number of airspaces that are in the alveolar category (#)")]
		public int Alv_count { get; set; }
		[Description("Number of airspaces that are in the alveolar category (#/mm2)")]
		public double Alv_Count { get; set; }
		[Description("Number of airspaces that are in the saccular category (#)")]
		public int Sac_count { get; set; }
		[Description("Number of airspaces that are in the saccular category (#/mm2)")]
		public double Sac_Count { get; set; }
		[Description("Number of airspaces that are in the ductal/destructive category (#)")]
		public int DucDes_count { get; set; }
		[Description("Number of airspaces that are in the ductal/destructive category (#/mm2)")]
		public double DucDes_Count { get; set; }		

		[Description("Sum of alveolar area (µm2)")]
		public double Alv_SumArea { get; set; }
		[Description("Sum of saccular area (µm2)")]
		public double Sac_SumArea { get; set; }
		[Description("Sum of ductal/destructive area (µm2)")]
		public double DucDes_SumArea { get; set; }
		[Description("Sum of alveolar boundary (µm)")]
		public double Alv_SumBoundary { get; set; }
		[Description("Sum of saccular boundary (µm)")]
		public double Sac_SumBoundary { get; set; }
		[Description("Sum of ductal/destructive boundary (µm)")]
		public double DucDes_SumBoundary { get; set; }

		[Description("Alveolar boundary per paren area (1/µm)")]
		public double Alv_Boundary { get; set; }
		[Description("Saccular boundary per paren area (1/µm)")]
		public double Sac_Boundary { get; set; }
		[Description("Ductal/Destructive boundary per paren area (1/µm)")]
		public double DucDes_Boundary { get; set; }

		[Description("Alveolar Fraction (% in parenchymal area)")]
		public double Alv_Fraction { get; set; }
		[Description("Saccular Fraction (% in parenchymal area)")]
		public double Sac_Fraction { get; set; }
		[Description("Ductal/Destructive Fraction (% in parenchymal area)")]
		public double DucDes_Fraction { get; set; }

        [Description("Tissue Fraction (% in parenchymal area)")]
        public double Tis_Fraction { get; set; }
        [Description("Ductal/Destructive to Alveolar Ratio (DF/AF)")]
        public double DucDes_Alv_Ratio { get; set; }


        public static string getEntryNote { get { return "Entry Created at,"+DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local); } }
		public static string getDescription {
			get {
				return string.Join(",",
					nameof(FileName).Description(typeof(CalBlobAQ2)), nameof(Group).Description(typeof(CalBlobAQ2)), nameof(Individual).Description(typeof(CalBlobAQ2)), nameof(Grouping).Description(typeof(CalBlobAQ2)), nameof(Low_Threshold).Description(typeof(CalBlobAQ2)), nameof(Sac_Threshold).Description(typeof(CalBlobAQ2)), nameof(DucDes_Threshold).Description(typeof(CalBlobAQ2)), nameof(High_Threshold).Description(typeof(CalBlobAQ2)), nameof(Alv_count).Description(typeof(CalBlobAQ2)), nameof(Sac_count).Description(typeof(CalBlobAQ2)), nameof(DucDes_count).Description(typeof(CalBlobAQ2)), nameof(Alv_Count).Description(typeof(CalBlobAQ2)), nameof(Sac_Count).Description(typeof(CalBlobAQ2)), nameof(DucDes_Count).Description(typeof(CalBlobAQ2)), nameof(Total_SumArea).Description(typeof(CalBlobAQ2)), nameof(Lung_SumArea).Description(typeof(CalBlobAQ2)), nameof(Paren_SumArea).Description(typeof(CalBlobAQ2)), nameof(Tis_SumArea).Description(typeof(CalBlobAQ2)), nameof(Alv_Size).Description(typeof(CalBlobAQ2)), nameof(Sac_Size).Description(typeof(CalBlobAQ2)), nameof(DucDes_Size).Description(typeof(CalBlobAQ2)), nameof(Alv_SumArea).Description(typeof(CalBlobAQ2)), nameof(Sac_SumArea).Description(typeof(CalBlobAQ2)), nameof(DucDes_SumArea).Description(typeof(CalBlobAQ2)), nameof(Alv_SumBoundary).Description(typeof(CalBlobAQ2)), nameof(Sac_SumBoundary).Description(typeof(CalBlobAQ2)), nameof(DucDes_SumBoundary).Description(typeof(CalBlobAQ2)), nameof(Alv_Boundary).Description(typeof(CalBlobAQ2)), nameof(Sac_Boundary).Description(typeof(CalBlobAQ2)), nameof(DucDes_Boundary).Description(typeof(CalBlobAQ2)), nameof(Alv_Fraction).Description(typeof(CalBlobAQ2)), nameof(Sac_Fraction).Description(typeof(CalBlobAQ2)), nameof(DucDes_Fraction).Description(typeof(CalBlobAQ2)), nameof(Tis_Fraction).Description(typeof(CalBlobAQ2)), nameof(DucDes_Alv_Ratio).Description(typeof(CalBlobAQ2)));
			}
		}
		public static string getHeader {
			get {
				return string.Join(",",
					nameof(FileName), nameof(Group), nameof(Individual), nameof(Grouping), nameof(Low_Threshold), nameof(Sac_Threshold), nameof(DucDes_Threshold), nameof(High_Threshold), nameof(Alv_count), nameof(Sac_count), nameof(DucDes_count), nameof(Alv_Count), nameof(Sac_Count), nameof(DucDes_Count), nameof(Total_SumArea), nameof(Lung_SumArea), nameof(Paren_SumArea), nameof(Tis_SumArea), nameof(Alv_Size), nameof(Sac_Size), nameof(DucDes_Size), nameof(Alv_SumArea), nameof(Sac_SumArea), nameof(DucDes_SumArea), nameof(Alv_SumBoundary), nameof(Sac_SumBoundary), nameof(DucDes_SumBoundary), nameof(Alv_Boundary), nameof(Sac_Boundary), nameof(DucDes_Boundary), nameof(Alv_Fraction), nameof(Sac_Fraction), nameof(DucDes_Fraction), nameof(Tis_Fraction), nameof(DucDes_Alv_Ratio));

			}
		}
		public string getResult {
			get {
				return string.Join(",", $"\"{FileName}\"", $"\"{Group}\"", $"\"{Individual}\"", $"\"{Grouping}\"",
                    Low_Threshold, Sac_Threshold, DucDes_Threshold, High_Threshold, Alv_count, Sac_count, DucDes_count, Alv_Count, Sac_Count, DucDes_Count, Total_SumArea, Lung_SumArea, Paren_SumArea, Tis_SumArea, Alv_Size, Sac_Size, DucDes_Size, Alv_SumArea, Sac_SumArea, DucDes_SumArea, Alv_SumBoundary, Sac_SumBoundary, DucDes_SumBoundary, Alv_Boundary, Sac_Boundary, DucDes_Boundary, Alv_Fraction, Sac_Fraction, DucDes_Fraction, Tis_Fraction, DucDes_Alv_Ratio);
			}
		}

		public new void Dispose()
		{
			base.Dispose(); UnmanagedResult?.Dispose();
		}
		public CalBlobAQ2(UISettings ui, FileData f, CalPAC categ = null) : base(ui, f)
		{
			try {
				// low-alv-des-high cut
				if (categ!=null) { // use alv des cutoffs from categorize class
					Alv_Log_Size=categ.p1Mean;
					Sac_Log_Size=categ.p2Mean;
					DucDes_Log_Size=categ.p3Mean;
					Alv_Size=Math.Pow(10.0d, categ.p1Mean)-1;
					Sac_Size=Math.Pow(10.0d, categ.p2Mean)-1;
					DucDes_Size=Math.Pow(10.0d, categ.p3Mean)-1;
					Sac_Threshold=Math.Pow(10.0d, categ.c2_Sac_Log_Threshold)-1;
					DucDes_Threshold=Math.Pow(10.0d, categ.c2_DucDes_Log_Threshold)-1;
				} else { // alv des cutoffs from ui
					Sac_Threshold=Math.Pow(10.0d, ui.BlobAlv)-1;
					DucDes_Threshold=Math.Pow(10.0d, ui.BlobDes)-1;
				}
				Sac_Log_Threshold=Math.Log10(Sac_Threshold+1);
				DucDes_Log_Threshold=Math.Log10(DucDes_Threshold+1);

				AreaComponentsLabeling aclabel = new AreaComponentsLabeling() {
					Low=(int)Math.Round(Low_Threshold/ui.um2px2),
					High=(int)Math.Round(Math.Min(int.MaxValue, High_Threshold/ui.um2px2)),
					Alv=(int)Math.Round(Sac_Threshold/ui.um2px2),
					Des=(int)Math.Round(DucDes_Threshold/ui.um2px2) };
				Invert AFinvert = new Invert();
				UnmanagedResult=aclabel.Apply(AFinvert.Apply(UnmanagedBlackWhite));
				//Low_count = Low_sumArea = High_count = High_sumArea = 0;
				foreach (Blobx blob in aclabel.BlobCounter.blobs) {
					if (blob.Area<aclabel.Alv) { // alv
						Alv_count++; Alv_SumArea+=ui.um2px2*blob.Area; Alv_SumBoundary+=ui.umpx*blob.Perimeter;
					} else if (blob.Area<=aclabel.Des) { // sac
						Sac_count++; Sac_SumArea+=ui.um2px2*blob.Area; Sac_SumBoundary+=ui.umpx*blob.Perimeter;
					} else {  // duct/des
						DucDes_count++; DucDes_SumArea+=ui.um2px2*blob.Area; DucDes_SumBoundary+=ui.umpx*blob.Perimeter;
					}
				}

				Total_SumArea=ui.um2px2*(UnmanagedMarkup.Width*UnmanagedMarkup.Height); // total area
				Lung_SumArea=Total_SumArea-Low_SumArea-High_SumArea; // lung area
				Paren_SumArea=Lung_SumArea-NonParen_SumArea; // parenchymal area
				Tis_SumArea=Paren_SumArea-Alv_SumArea-Sac_SumArea-DucDes_SumArea; // septum tissue				
												
          	    Alv_Count=1000.0d*1000.0d*Alv_count/Paren_SumArea;	Sac_Count=1000.0d*1000.0d*Sac_count/Paren_SumArea;
				DucDes_Count=1000.0d*1000.0d*DucDes_count/Paren_SumArea;
				Alv_Boundary=1.0d*Alv_SumBoundary/Paren_SumArea; Sac_Boundary=1.0d*Sac_SumBoundary/Paren_SumArea;
				DucDes_Boundary=1.0d*DucDes_SumBoundary/Paren_SumArea;
				Alv_Fraction=1.0d*Alv_SumArea/Paren_SumArea; Sac_Fraction=1.0d*Sac_SumArea/Paren_SumArea;
				DucDes_Fraction=1.0d*DucDes_SumArea/Paren_SumArea; // is still better to divide by paren area
                Tis_Fraction = 1.0d - Alv_Fraction - Sac_Fraction - DucDes_Fraction;
                DucDes_Alv_Ratio = DucDes_Fraction / Alv_Fraction;

                StringBuilder header = new StringBuilder();
				header.Append($"{FileName}  ps: {ui.PixelScale:G2}/{ui.ResizeValue:G2} px/um");
				header.Append($"\nsac thres:{Sac_Log_Threshold:0.00} duc/des thres:{DucDes_Log_Threshold:0.00} log\x2081\x2080µm\xB2");
				header.Append($"\nlung: {Lung_SumArea:G2}µm\xB2, {(1.0d*Lung_SumArea/Total_SumArea):0%} image (blw: {Low_SumArea/Total_SumArea:0%} ovr: {1.0d*High_SumArea/Total_SumArea:0%})");
				header.Append($"\nparenchyma: {Paren_SumArea:G2}µm\xB2, {(Paren_SumArea/Lung_SumArea):0%} lung (exc: {1.0d*NonParen_SumArea/Total_SumArea:0%})");
				header.Append($"\nseptum: {Tis_SumArea:G2}µm\xB2, {Tis_SumArea/Paren_SumArea:0%} paren (airspace: {(Alv_SumArea+Sac_SumArea+DucDes_SumArea)/Total_SumArea:0%})");
				
				StringBuilder footer = new StringBuilder();
				if (Math.Abs(Alv_Size*Sac_Size*DucDes_Size)>0.01d) footer.Append($" AS:{Alv_Size:G2} SS:{Sac_Size:G2} DS:{DucDes_Size:G2} (Size, µm\xB2)");
				footer.Append($"\nAC:{Alv_Count:G2} SC:{Sac_Count:G2} DC:{DucDes_Count:G2} (Count\x2090, 1/mm\xB2)"
								+$"\nAB:{Alv_SumBoundary:G2} SB:{Sac_Boundary:G2} DB:{DucDes_Boundary:G2} (Boundary\x2090, 1/µm)"
								+$"\nAF:{Alv_Fraction:0.0%} SF:{Sac_Fraction:0.0%} DF:{DucDes_Fraction:0.0%} (Area\x2090 Fraction, %)"
                                +$"\nTF:{Tis_Fraction:0.0%} D2A:{DucDes_Alv_Ratio:F2} (Fraction, Ratio)");

				UnmanagedMarkup=UnmanagedImage.FromManagedImage(AddBlobText(UnmanagedMarkup.ToManagedImage(false), Color.Black, $"{header}", $"{footer}", null, null, // ,blobs, blobDes.ToArray()
						  (int)Math.Round(0.02d*UnmanagedMarkup.Width*Math.Sqrt(ui.ExportDetailRatio))));
				UnmanagedResult=UnmanagedImage.FromManagedImage(AddBlobText(UnmanagedResult.ToManagedImage(false), Color.PaleGreen, $"{header}", $"{footer}", null, null, // ,blobs, blobDes.ToArray()
						  (int)Math.Round(0.02d*UnmanagedMarkup.Width*Math.Sqrt(ui.ExportDetailRatio))));
			} catch { throw new Exception("Error Occured During Airspace Quantification"); }
		}
	}
}
