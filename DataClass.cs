using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Media;
using AForge.Imaging;
using OxyPlot;
using System.Collections.ObjectModel;

namespace LungMorphApp
{
	public class UISettings : INotifyPropertyChanged
	{
		bool _LoadStartup=false;
		public bool LoadStartup { get { return _LoadStartup; } set { _LoadStartup=value; OnPropertyChanged(); } }

		bool _Lock = false;
		public void Unlock() { _Lock=false; }
		public void LockUp() { _Lock=true; }

		string _WorkDirectory = Directory.GetCurrentDirectory();
		public string WorkDirectory {
			get {	return _WorkDirectory.TrimEnd('\\'); }
			set { if (!_Lock) { _WorkDirectory=value; OnPropertyChanged(); } }
		}
		string _ProjectName = $"{DateTime.Now:yyyy-MM-dd}";
		public string ProjectName {
			get { return _ProjectName; }
			set {
				if (!_Lock) {
					string[] sep = value.Split(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' });
					if (sep.Length==1) { _ProjectName=value; OnPropertyChanged(); } else { MessageBox.Show("Invalid ProjectName"); }
				}
			}
		}
		bool _ListTypeSwitch = false;
		public bool ListTypeSwitch {
			get { return _ListTypeSwitch; }
			set { if (!_Lock) { _ListTypeSwitch=value; OnPropertyChanged(); OnPropertyChanged(nameof(PerformConversionIsEnabled)); } }
		}
		string _ListType = "jpg";
		public string ListType {
			get { return _ListType; }
			set { if (!_Lock) { _ListType=value; OnPropertyChanged(); OnPropertyChanged(nameof(PerformConversionIsEnabled)); } }
		}

		bool _ListSizeSwitch = false;
		public bool ListSizeSwitch {
			get { return _ListSizeSwitch; }
			set { if (!_Lock) { if (_ListSizeSwitch!=value) { _ListSizeSwitch=value; OnPropertyChanged(); } } }
		}
		int _ListSizeWidth = 1392;
		public int ListSizeWidth {
			get { return _ListSizeWidth; }
			set { if (!_Lock) { if (_ListSizeWidth!=value) { _ListSizeWidth=value; OnPropertyChanged(); } } }
		}
		int _ListSizeHeight = 1040;
		public int ListSizeHeight {
			get { return _ListSizeHeight; }
			set { if (!_Lock) { if (_ListSizeHeight!=value) { _ListSizeHeight=value; OnPropertyChanged(); } } }
		}

		bool _ListSubFolderSwitch = false;
		public bool ListSubFolderSwitch {
			get { return _ListSubFolderSwitch; }
			set { if (!_Lock) { _ListSubFolderSwitch=value; OnPropertyChanged(); } }
		}
		int _ListSubFolder = 2;
		public int ListSubFolder {
			get { return _ListSubFolder; }
			set { if (!_Lock) { _ListSubFolder=value; OnPropertyChanged(); } }
		}

		float _PixelScale = 1.0f;
		public float PixelScale {
			get { return _PixelScale; }
			set { if (!_Lock) { _PixelScale=value; OnPropertyChanged(); } }
		}

		bool _ResizeRatioSwitch = true;
		public bool ResizeRatioSwitch {
			get { return _ResizeRatioSwitch; }
			set { if (!_Lock) { _ResizeRatioSwitch=value; OnPropertyChanged(); } }
		}
		float _ResizeRatio = 0.65f;
		public float ResizeRatio {
			get { return _ResizeRatio; }
			set { if (!_Lock) { _ResizeRatio=value; OnPropertyChanged(); } }
		}
		public float ResizeValue { get { return (_ResizeRatioSwitch) ? _ResizeRatio : 1.0f;} }

		bool _CropRatioSwitch = false;
		public bool CropRatioSwitch {
			get { return _CropRatioSwitch; }
			set { if (!_Lock) { _CropRatioSwitch=value; OnPropertyChanged(); } }
		}
		float _CropRatio = 0.5f;
		public float CropRatio {
			get { return _CropRatio; }
			set { if (!_Lock) { _CropRatio=value; OnPropertyChanged(); } }
		}
		public float CropValue { get { return (_CropRatioSwitch) ? _CropRatio : 1.0f;} }

		bool _BlackBalanceSwitch = false;
		public bool BlackBalanceSwitch {
			get { return _BlackBalanceSwitch; }
			set { if (!_Lock) { _BlackBalanceSwitch=value; OnPropertyChanged(); } }
		}
		double _BlackBalance = 0.1d;
		public double BlackBalance {
			get { return _BlackBalance; }
			set { if (!_Lock) { _BlackBalance=value; OnPropertyChanged(); } }
		}
		bool _WhiteBalanceSwitch = false;
		public bool WhiteBalanceSwitch {
			get { return _WhiteBalanceSwitch; }
			set { if (!_Lock) { _WhiteBalanceSwitch=value; OnPropertyChanged(); } }
		}
		double _WhiteBalance = 0.9d;
		public double WhiteBalance {
			get { return _WhiteBalance; }
			set { if (!_Lock) { _WhiteBalance=value; OnPropertyChanged(); } }
		}

		bool _ExcludeColorSwitch = true;
		public bool ExcludeColorSwitch {
			get { return _ExcludeColorSwitch; }
			set { if (!_Lock) { _ExcludeColorSwitch=value; OnPropertyChanged(); } }
		}
		string _ExcludeColorHex = "#FF00FF00"; // green
		public string ExcludeColorHex {
			get { return _ExcludeColorHex; }
			set { if (!_Lock) { _ExcludeColorHex=value; OnPropertyChanged(); } }
		}
		int _ExcludeColorRadius = 80;
		public int ExcludeColorRadius {
			get { return _ExcludeColorRadius; }
			set { if (!_Lock) { _ExcludeColorRadius=value; OnPropertyChanged(); } }
		}
        bool _RotateImageSwitch = false;
		public bool RotateImageSwitch {
			get { return _RotateImageSwitch; }
			set { if (!_Lock) { _RotateImageSwitch=value; OnPropertyChanged(); } }
		}
		int _RotateImage = 90;
		public int RotateImage {
			get { return _RotateImage; }
			set { if (!_Lock) { _RotateImage=value; OnPropertyChanged(); } }
		}
        public int RotateDegree {
            get { return (_RotateImageSwitch) ? _RotateImage : 0;}
            }
		bool _GaussianBlurSwitch = false;
		public bool GaussianBlurSwitch {
			get { return _GaussianBlurSwitch; }
			set { if (!_Lock) { _GaussianBlurSwitch=value; OnPropertyChanged(); } }
		}
		int _GaussianBlur = 1;
		public int GaussianBlur {
			get { return _GaussianBlur; }
			set { if (!_Lock) { _GaussianBlur=value; OnPropertyChanged(); } }
		}
		bool _FillHoleAirspaceSwitch = true;
		public bool FillHoleAirspaceSwitch {
			get { return _FillHoleAirspaceSwitch; }
			set { if (!_Lock) { _FillHoleAirspaceSwitch=value; OnPropertyChanged(); } }
		}
		int _FillHoleAirspace = 12;
		public int FillHoleAirspace {
			get { return _FillHoleAirspace; }
			set { if (!_Lock) { _FillHoleAirspace=value; OnPropertyChanged(); } }
		}
		bool _FillHoleTissueSwitch = true;
		public bool FillHoleTissueSwitch {
			get { return _FillHoleTissueSwitch; }
			set { if (!_Lock) { _FillHoleTissueSwitch=value; OnPropertyChanged(); } }
		}
		int _FillHoleTissue = 12;
		public int FillHoleTissue {
			get { return _FillHoleTissue; }
			set { if (!_Lock) { _FillHoleTissue=value; OnPropertyChanged(); } }
		}
		bool _MorphoDilateSwitch = true;
		public bool MorphoDilateSwitch {
			get { return _MorphoDilateSwitch; }
			set { if (!_Lock) { _MorphoDilateSwitch=value; OnPropertyChanged(); } }
		}
		int _MorphoDilate = 1;
		public int MorphoDilate {
			get { return _MorphoDilate; }
			set { if (!_Lock) { _MorphoDilate=value; OnPropertyChanged(); } }
		}
		bool _MorphoErodeSwitch = true;
		public bool MorphoErodeSwitch {
			get { return _MorphoErodeSwitch; }
			set { if (!_Lock) { _MorphoErodeSwitch=value; OnPropertyChanged(); } }
		}
		int _MorphoErode = 1;
		public int MorphoErode {
			get { return _MorphoErode; }
			set { if (!_Lock) { _MorphoErode=value; OnPropertyChanged(); } }
		}
		
		int _ThresholdIndex = 0;
		public int ThresholdIndex {
			get { return _ThresholdIndex; }
			set { if (!_Lock) { _ThresholdIndex=value; OnPropertyChanged(); } }
		}
		bool _ThreshGlobalIsAbsolute = true;
		public bool ThreshGlobalIsAbsolute {
			get { return _ThreshGlobalIsAbsolute; }
			set { if (!_Lock) { _ThreshGlobalIsAbsolute=value; OnPropertyChanged(); } }
		}
		int _ThreshGlobalAbsolute = 185;
		public int ThreshGlobalAbsolute {
			get { return _ThreshGlobalAbsolute; }
			set { if (!_Lock) { _ThreshGlobalAbsolute=value; OnPropertyChanged(); } }
		}
		int _ThreshGlobalRelative = 185;
		public int ThreshGlobalRelative {
			get { return _ThreshGlobalRelative; }
			set { if (!_Lock) { _ThreshGlobalRelative=value; OnPropertyChanged(); } }
		}
		float _ThreshLocalBrightnessDifference = 0.001f;
		public float ThreshLocalBrightnessDifference {
			get { return _ThreshLocalBrightnessDifference; }
			set { if (!_Lock) { _ThreshLocalBrightnessDifference=value; OnPropertyChanged(); } }
		}
		int _ThreshLocalWindowSize = 300;
		public int ThreshLocalWindowSize {
			get { return _ThreshLocalWindowSize; }
			set { if (!_Lock) { _ThreshLocalWindowSize=value; OnPropertyChanged(); } }
		}		
		bool _PerformAirspaceProfiling = true;
		public bool PerformAirspaceProfiling {
			get { return _PerformAirspaceProfiling; }
			set { if (!_Lock) { _PerformAirspaceProfiling=value; OnPropertyChanged(); } }
		}
		bool _PerformAirspaceQuantification;
		public bool PerformAirspaceQuantification {
			get { return _PerformAirspaceQuantification; }
			set { if (!_Lock) { _PerformAirspaceQuantification=value; OnPropertyChanged(); } }
		}
		
		bool _BlobMinSwitch = false;
		public bool BlobMinSwitch {
			get { return _BlobMinSwitch; }
			set { if (!_Lock) { _BlobMinSwitch=value; OnPropertyChanged(); } }
		}
		double _BlobMin = 1.5d; ///<summary>minimum log10(µm^2) for blobs</summary>
		public double BlobMin {
			get { return _BlobMin; }
			set { if (!_Lock) { _BlobMin=value; OnPropertyChanged(); } }
		}
		//[XmlIgnore]public double BlobMinDouble { get { return (_BlobMinSwitch) ? BlobMin : 0.0d; } }
		//[XmlIgnore]public int BlobMinInt { get { return (_BlobMinSwitch) ? (int)Math.Round(Math.Pow(10.0d, BlobMin)*PixelScale*PixelScale/ResizeRatio/ResizeRatio) : 0; } }
		bool _BlobMaxSwitch = false;
		public bool BlobMaxSwitch {
			get { return _BlobMaxSwitch; }
			set { if (!_Lock) { _BlobMaxSwitch=value; OnPropertyChanged(); } }
		}
		double _BlobMax = 5.0d; /// <summary>maximum log10(µm^2) for blobs</summary>
		public double BlobMax {
			get { return _BlobMax; }
			set { if (!_Lock) { _BlobMax=value; OnPropertyChanged(); } }
		}
		//[XmlIgnore]public double BlobMaxDouble { get { return (_BlobMaxSwitch) ? BlobMax : 9.3d; } }
		//[XmlIgnore]public int BlobMaxInt { get { return (_BlobMaxSwitch) ? (int)Math.Round(Math.Pow(10.0d, BlobMax)*PixelScale*PixelScale/ResizeRatio/ResizeRatio) : int.MaxValue; } }
		int _CutoffSelection = 0; /// <summary>Cutoff selection: 0-Auto 1-Custom</summary>
		public int CutoffSelection {
			get { return _CutoffSelection; }
			set { if (!_Lock) { _CutoffSelection=value; OnPropertyChanged(); } }
		}
		int _ProfilingIndex = 3; /// <summary>0-All 1-Group 2-Individual 3-Image</summary>
		public int ProfilingIndex {
			get { return _ProfilingIndex; }
			set { if (!_Lock) { _ProfilingIndex=value; OnPropertyChanged(); } }
		}
		double _BlobAlv = 3.0d; /// <summary>Custon: alveolus - sac cutoff</summary>
		public double BlobAlv {
			get { return _BlobAlv; }
			set { if (!_Lock) { _BlobAlv=value; OnPropertyChanged(); } }
		}
		//[XmlIgnore]public int BlobAlvInt { get { return (int)Math.Round(Math.Pow(10.0d, BlobAlv)*PixelScale*PixelScale/ResizeRatio/ResizeRatio); } }
		double _BlobDes = 4.0d; /// <summary>Custon: duct,destruction - sac cutoff</summary>
		public double BlobDes {
			get { return _BlobDes; }
			set { if (!_Lock) { _BlobDes=value; OnPropertyChanged(); } }
		}
		bool _Categ1Dist=false; // less preferable to profile with 1-distribution
		public bool Categ1Dist {
			get { return _Categ1Dist; }
			set { if (!_Lock) { _Categ1Dist=value; OnPropertyChanged(); } }
		}
		bool _Categ2Dist=true; // default to profile with 2-distributions
		public bool Categ2Dist {
			get { return _Categ2Dist; }
			set { if (!_Lock) { _Categ2Dist=value; OnPropertyChanged(); } }
		}
		//[XmlIgnore]public int BlobDesInt { get { return (int)Math.Round(Math.Pow(10.0d, BlobDes)*PixelScale*PixelScale/ResizeRatio/ResizeRatio); } }
		bool _ExportDetailSwitch = false;
		public bool ExportDetailSwitch {
			get { return _ExportDetailSwitch; }
			set { if (!_Lock) { _ExportDetailSwitch=value; OnPropertyChanged(); } }
		}
		[XmlIgnore]public int ExportDetailRatio { get { if (ExportDetailSwitch) return 1; return 2; } }
		bool _MultiThreadingSwitch;
		public bool MultiThreadingSwitch {
			get { return _MultiThreadingSwitch; }
			set { if (!_Lock) { _MultiThreadingSwitch=value; OnPropertyChanged(); } }
		}
		
		[XmlIgnore]public bool PerformConversionIsEnabled { get { return ((_ListType=="tif")); } }

		[XmlIgnore]public double um2px2 { get { return Math.Pow(umpx,2.0d); } }
		[XmlIgnore]public double umpx { get { return 1.0d/PixelScale/ResizeValue; } }

		[XmlIgnore]public System.Threading.CancellationTokenSource cts { get; set; }
		bool _IsBusy = false;
		[XmlIgnore]public bool IsBusy { get { return _IsBusy; } set { _IsBusy=value; OnPropertyChanged(); } }
		public void ShowBusySign(string msg = "Please wait...") { Status=msg; IsBusy=true; }
		public void StopBusySign(string msg = "Ready...") { IsBusy=false; Status=msg; }
		string _Status = "Ready...";
		[XmlIgnore]public string Status {
			get { return _Status; }
			set { _Status=value; OnPropertyChanged(); }
		}
		int _ProgressPercent = 0;
		[XmlIgnore]
		public int ProgressPercent {
			get { return _ProgressPercent; }
			set { _ProgressPercent=value; OnPropertyChanged(); OnPropertyChanged(nameof(Progress)); }
		}
		[XmlIgnore]public double Progress { get { return 0.01d*_ProgressPercent; } }

		ObservableCollection<FileData> _FileList = new ObservableCollection<FileData>();
		[XmlIgnore]public ObservableCollection<FileData> FileList { get { return _FileList; } set { if (!_Lock) { _FileList=value; OnPropertyChanged(); } } }


		ImageSource _ImageSource1,_ImageSource2,_ImageSource3,_ImageSource4,_ImageSource5,_ImageSource6;
		[XmlIgnore]public ImageSource ImageSource1 { get { return _ImageSource1; } set { _ImageSource1=value; OnPropertyChanged(); } }
		[XmlIgnore]public ImageSource ImageSource2 { get { return _ImageSource2; } set { _ImageSource2=value; OnPropertyChanged(); } }
		[XmlIgnore]public ImageSource ImageSource3 { get { return _ImageSource3; } set { _ImageSource3=value; OnPropertyChanged(); } }
		[XmlIgnore]public ImageSource ImageSource4 { get { return _ImageSource4; } set { _ImageSource4=value; OnPropertyChanged(); } }
		[XmlIgnore]public ImageSource ImageSource5 { get { return _ImageSource5; } set { _ImageSource5=value; OnPropertyChanged(); } }
		[XmlIgnore]public ImageSource ImageSource6 { get { return _ImageSource6; } set { _ImageSource6=value; OnPropertyChanged(); } }

		int _ScrollViewerIndex = 0;
		public int ScrollViewerIndex {
			get { return _ScrollViewerIndex; }
			set { _ScrollViewerIndex=value; OnPropertyChanged(); OnPropertyChanged(nameof(ImageSourceX)); }
		}[XmlIgnore]public ImageSource ImageSourceX {
			get {
				switch (_ScrollViewerIndex) {
					case 0: return null;
					case 1: return _ImageSource1;
					case 2: return _ImageSource2;
					case 3: return _ImageSource3;
					case 4: return _ImageSource4;
					case 5: return _ImageSource5;
					case 6: return _ImageSource6;
					default: return null;
				}
			}
		}

		public void UpdateImageSource(Bitmap bmp1 = null, Bitmap bmp2 = null, Bitmap bmp3 = null, Bitmap bmp4 = null, Bitmap bmp5 = null, Bitmap bmp6 = null, Bitmap bmp7 = null)
		{
			try {
				ImageSource1=bmp1?.Bitmap2BitmapImage();
				ImageSource2=bmp2?.Bitmap2BitmapImage();
				ImageSource3=bmp3?.Bitmap2BitmapImage();
				ImageSource4=bmp4?.Bitmap2BitmapImage();
				ImageSource5=bmp5?.Bitmap2BitmapImage();
				ImageSource6=bmp6?.Bitmap2BitmapImage();
				OnPropertyChanged(nameof(ImageSourceX));
			} catch (Exception ex) { throw new Exception("Error encounted while updating image display"+"\n"+ex.Message); }
		}

		PlotModel _Graph1, _Graph2, _Graph3;
		[XmlIgnore]public PlotModel Graph1 { get { return _Graph1; } set { _Graph1=value; OnPropertyChanged(); } }
		[XmlIgnore]public PlotModel Graph2 { get { return _Graph2; } set { _Graph2=value; OnPropertyChanged(); } }
		[XmlIgnore]public PlotModel Graph3 { get { return _Graph3; } set { _Graph3=value; OnPropertyChanged(); } }

		public void UpdateAll(UISettings nui)
		{
			foreach (PropertyInfo info in nui.GetType().GetProperties()) {
				//if (info.CanRead) object o = propertyInfo.GetValue(myObject, null);
				if (info.CanWrite) info.SetValue(this, info.GetValue(nui));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class FileData // to appear in the ListView
	{
		[Description("Image file name including the full path")] public string FileName { get; set; }
		[Description("Group identifier the 1st level of subfolder")] public string Group { get; set; }
		[Description("Individual identifier the 2nd level of subfolder")] public string Individual { get; set; }
		[Description("File name for export results replacing all slashes with underscores")] public string OutName {
			get {
				string temp = FileName.Replace("\\", "_");
				return Path.GetFileNameWithoutExtension(temp);
			}
		}
		[Description("Grouping strategy for airspace profiling (the airspaces within the same grouping identifier will be pooled together for profiling and categorization)")] public string Grouping { get; set; }
		[Description("Text color to be displayed on the list view")] public string DisplayColor { get; set; }
		[Description("Image file type, usually jpg or tif")] public string FileType { get; set; }
		[Description("Original image width")] public int Width { get; set; }
		[Description("Original image height")] public int Height { get; set; }

		public void ExtendFileData(FileData f)
		{
			this.FileName=f.FileName;
			this.Group=f.Group;
			this.Individual=f.Individual;
			this.Grouping=f.Grouping;
			this.DisplayColor=f.DisplayColor;
			this.FileType=f.FileType;
			this.Width=f.Width;
			this.Height=f.Height;		
		}
	}

	public class Progress
	{
		object _lock = new object();
		DateTime StartTime { get; set; }
		string Work { get; set; }
		public int CurrentCount { get; private set; } = 0;
		public int TotalCount { get; private set; } = 100;
		public Progress(string work = "Ready...", int total = 100)
		{
			Work=work;
			TotalCount=total;
			CurrentCount=0;
			StartTime=DateTime.Now;
		}

		public void Increment(UISettings ui, string targetfile = null, string entry = null)
		{
			lock (_lock) {
				if ((int)(100.0d*(CurrentCount+1)/TotalCount)>(int)(100.0d*CurrentCount/TotalCount)) {
					int perct = (int)Math.Round(100.0d*(CurrentCount+1)/TotalCount);
					double seconds = 1.0d*(DateTime.Now-StartTime).TotalSeconds/perct*(100.0d-perct);
					ui.Status=$"{Work} {perct}% {seconds/3600:00}:{(seconds/60)%60:00}:{seconds%60:00} left";
					ui.ProgressPercent=perct;
				}
				if (targetfile!=null) { using (var file = File.AppendText(targetfile)) file.Write("\n"+entry); }
				CurrentCount++;
			}
		}
	}
}


