using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;


namespace LungMorphApp
{
	class PreProcess : FileData, IDisposable
	{
		public Bitmap BitmapOriginal { get; set; }
		internal UnmanagedImage UnmanagedMarkup; public Bitmap BitmapMarkup { get { return UnmanagedMarkup?.ToManagedImage(false); } }
		internal UnmanagedImage UnmanagedExclude; public Bitmap BitmapExclude { get { return UnmanagedExclude?.ToManagedImage(false); } }
		internal UnmanagedImage UnmanagedGray; public Bitmap BitmapGray { get { return UnmanagedGray?.ToManagedImage(false); } }
		
		
		public void Dispose()
		{
			BitmapOriginal?.Dispose(); UnmanagedMarkup?.Dispose(); UnmanagedExclude?.Dispose(); UnmanagedGray?.Dispose(); 
		}
		
		public PreProcess(UISettings ui, FileData file)
		{
			try {
				ExtendFileData(file);
				BitmapOriginal=(ui.WorkDirectory+"\\"+FileName).FileTo24bbpRgb(ResizeRatio:ui.ResizeValue, FrameCrop:ui.CropValue, ImageZoom:1, RotateDegree:ui.RotateDegree);
				UnmanagedMarkup=UnmanagedImage.FromManagedImage(BitmapOriginal);
				ImageStatistics stats = null;
				Threshold AFbinary = new Threshold(1);
				Grayscale AFgray = new Grayscale(0.1, 0.7, 0.2);
				if (ui.ExcludeColorSwitch && ui.ExcludeColorRadius>0) {
					System.Windows.Media.Color excolor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ui.ExcludeColorHex);
					EuclideanColorFiltering AFexcolor = new EuclideanColorFiltering(new RGB(excolor.R, excolor.G, excolor.B), (short)ui.ExcludeColorRadius);
					UnmanagedExclude=AFbinary.Apply(AFgray.Apply(AFexcolor.Apply(UnmanagedMarkup)));
				} else UnmanagedExclude=UnmanagedImage.Create(UnmanagedMarkup.Width, UnmanagedMarkup.Height, PixelFormat.Format8bppIndexed);
				if (ui.WhiteBalanceSwitch||ui.BlackBalanceSwitch) { // need to apply auto white/black balance
					Invert AFinvert = new Invert();
					stats = new ImageStatistics(UnmanagedMarkup, AFinvert.Apply(UnmanagedExclude));
					int lowend=(ui.BlackBalanceSwitch) ? (int)Math.Round(0.333d*(stats.RedWithoutBlack.Center2QuantileValue(ui.BlackBalance)+stats.GreenWithoutBlack.Center2QuantileValue(ui.BlackBalance)+stats.BlueWithoutBlack.Center2QuantileValue(ui.BlackBalance))):0;
					LevelsLinear levelsLinear = new LevelsLinear {
						InRed=new IntRange(lowend, (ui.WhiteBalanceSwitch) ? stats.RedWithoutBlack.Center2QuantileValue(ui.WhiteBalance) : 255),
						InGreen=new IntRange(lowend, (ui.WhiteBalanceSwitch) ? stats.GreenWithoutBlack.Center2QuantileValue(ui.WhiteBalance) : 255),
						InBlue=new IntRange(lowend, (ui.WhiteBalanceSwitch) ? stats.BlueWithoutBlack.Center2QuantileValue(ui.WhiteBalance) : 255),
					};
					//LevelsLinear levelsLinear = new LevelsLinear {
					//	InRed=new IntRange((ui.BlackBalanceSwitch)?stats.RedWithoutBlack.Center2QuantileValue(ui.BlackBalance):0, (ui.WhiteBalanceSwitch)?stats.RedWithoutBlack.Center2QuantileValue(ui.WhiteBalance):255),
					//	InGreen=new IntRange((ui.BlackBalanceSwitch)?stats.GreenWithoutBlack.Center2QuantileValue(ui.BlackBalance):0, (ui.WhiteBalanceSwitch)?stats.GreenWithoutBlack.Center2QuantileValue(ui.WhiteBalance):255),
					//	InBlue=new IntRange((ui.BlackBalanceSwitch)?stats.BlueWithoutBlack.Center2QuantileValue(ui.BlackBalance):0, (ui.WhiteBalanceSwitch)?stats.BlueWithoutBlack.Center2QuantileValue(ui.WhiteBalance):255),
					//};
					levelsLinear.ApplyInPlace(UnmanagedMarkup);
				}
				if (ui.GaussianBlurSwitch && ui.GaussianBlur!=0) { // Gaussian Blur and Darken
					GaussianBlur AFgblur = new GaussianBlur(11.0, Math.Max(ui.GaussianBlur,0)*2+1); // Gaussian Blur sigma = 8.0 kernel size = 7
					Intersect AFintersect = new Intersect(AFgblur.Apply(UnmanagedMarkup));
					UnmanagedMarkup=AFintersect.Apply(UnmanagedMarkup);
				}
				UnmanagedGray=AFgray.Apply(UnmanagedMarkup); // directly turn into gray
			} catch { throw new Exception("Error Occured During PreProcessing"); }
		}
	}

}
