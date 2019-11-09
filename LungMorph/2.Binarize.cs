
using System;
using System.Drawing;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace LungMorphApp
{
	class Binarize : PreProcess, IDisposable
	{
		internal UnmanagedImage UnmanagedBlackWhite; public Bitmap BitmapBlackWhite { get { return UnmanagedBlackWhite?.ToManagedImage(false); } }
				
		[Description("Sum of Excluded Area by Color (µm2)")] public double NonParen_SumArea { get; set; }
		[Description("Sum of excluded small area (µm2)")] public double Low_SumArea { get; set; }
		[Description("Sum of excluded large area (µm2)")] public double High_SumArea { get; set; }
		[Description("Size smaller than this will be excluded from the lung (log10 µm2)")] public double Low_Threshold { get; set; }
		[Description("Size larger than this will be excluded from the lung (log10 µm2)")] public double High_Threshold { get; set; }

		public new void Dispose()
		{
			base.Dispose();
			UnmanagedBlackWhite?.Dispose();
		}
		public Binarize(UISettings ui, FileData file) : base(ui, file)
		{
			try {
				Invert AFinvert = new Invert();
				switch (ui.ThresholdIndex) { // threshold method selection "Global mean" / "Local adaptive"
					case 0: // Global
						if (ui.ThreshGlobalIsAbsolute) { // use absolute
							Threshold AFglobalbinary = new Threshold(ui.ThreshGlobalAbsolute);
							UnmanagedBlackWhite=AFglobalbinary.Apply(UnmanagedGray);
						} else { // use relative							
							ImageStatistics stats= new ImageStatistics(UnmanagedGray, AFinvert.Apply(UnmanagedExclude));	
							Threshold AFglobalbinary = new Threshold(stats.Gray.Center2QuantileValue(1.0d*ui.ThreshGlobalRelative/255.0d));
							UnmanagedBlackWhite=AFglobalbinary.Apply(UnmanagedGray);
						}
						break;
					case 1: // Local
						BradleyLocalThresholdingX AFlocalbinary = new BradleyLocalThresholdingX() {
							PixelBrightnessDifferenceLimit=ui.ThreshLocalBrightnessDifference,
							WindowSize=ui.ThreshLocalWindowSize, UpperLimit=250 };
						UnmanagedBlackWhite=AFlocalbinary.Apply(UnmanagedGray);
						break;
				}
				if (ui.FillHoleAirspaceSwitch && ui.FillHoleAirspace!=0) { // fill holes of airspaces
					FillHoles AFfillinair = new FillHoles() { CoupledSizeFiltering=true, MaxHoleHeight=ui.FillHoleAirspace, MaxHoleWidth=ui.FillHoleAirspace };
					//FillHolesArea AFfillinair=new FillHolesArea() { MaxHoleArea=ui.FillHoleAirspace };
					AFfillinair.ApplyInPlace(UnmanagedBlackWhite);
				}
				UnmanagedBlackWhite=AFinvert.Apply(UnmanagedBlackWhite);
				if (ui.FillHoleTissueSwitch && ui.FillHoleTissue!=0) { // fill holes of tissue
					FillHoles AFfillintissue = new FillHoles() { CoupledSizeFiltering=true, MaxHoleHeight=ui.FillHoleTissue, MaxHoleWidth=ui.FillHoleTissue };					
					//FillHolesArea AFfillintissue=new FillHolesArea() { MaxHoleArea=ui.FillHoleTissue };
					AFfillintissue.ApplyInPlace(UnmanagedBlackWhite);
				}
				if (ui.MorphoDilateSwitch&&ui.MorphoDilate!=0) { // Morphological Dilate
					int n = (Math.Max(ui.MorphoDilate, 0)*2+1); short[,] morphmatrix = new short[n, n];
					for (int i = 0; i<n; i++) { for (int j = 0; j<n; j++) { morphmatrix[i, j]=1; } }
					Dilatation AFdilate = new Dilatation(morphmatrix);
					AFdilate.ApplyInPlace(UnmanagedBlackWhite);
				}
				if (ui.MorphoErodeSwitch&&ui.MorphoErode!=0) { // Morphological Erode

					int n = (Math.Max(ui.MorphoErode, 0)*2+1); short[,] morphmatrix = new short[n, n];
					for (int i = 0; i<n; i++) { for (int j = 0; j<n; j++) { morphmatrix[i, j]=1; } }
					Erosion AFerode = new Erosion(morphmatrix);
					AFerode.ApplyInPlace(UnmanagedBlackWhite);
				}
				if (ui.ExcludeColorSwitch) {
					NonParen_SumArea=ui.um2px2*UnmanagedExclude.NonBlackArea();
				}
				if (ui.BlobMinSwitch) {
					Low_Threshold=Math.Pow(10.0d, ui.BlobMin)-1;	
				} else {
					Low_Threshold=0.0d;
				}
				if (ui.BlobMaxSwitch) {
					High_Threshold=Math.Pow(10.0d, ui.BlobMax)-1;	
				} else {
					High_Threshold=int.MaxValue;
				}
				
				if (ui.BlobMinSwitch||ui.BlobMaxSwitch) {
					Merge AFmerge1=new Merge(UnmanagedExclude);					
					ExcludeSize AFexcsize = new ExcludeSize() { Low=(int)Math.Round(Low_Threshold/ui.um2px2), High=(int)Math.Round(Math.Min(int.MaxValue, High_Threshold/ui.um2px2)) };
					Merge AFmerge2 = new Merge(AFexcsize.Apply(AFinvert.Apply(AFmerge1.Apply(UnmanagedBlackWhite))));
					AFmerge2.ApplyInPlace(UnmanagedExclude);
					Low_SumArea=ui.um2px2*AFexcsize.LowCount;
					High_SumArea=ui.um2px2*AFexcsize.HighCount;
				}				
			} catch { throw new Exception("Error Occured During Binarization"); }
		}
		

	}
}
