using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace LungMorphApp
{
	public partial class UserWindow : Window
	{
		public UISettings UI { get; set; }
		public string CurrentVersion {
			get { return System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed
                  ? System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
                  : System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}
        string UserName="Unamed"; string ExpirationDate="00-00"; bool EnableBatch=false;
		public UserWindow(string username, string expdate, bool allowbatch)
		{
			InitializeComponent(); // Initiate View
			TaskbarItemInfo.ProgressState=System.Windows.Shell.TaskbarItemProgressState.Normal;
            
            EnableBatch=allowbatch; UserName=username; ExpirationDate=expdate;
			btn_StartBatch.IsEnabled=EnableBatch;

			this.Title=this.Title+" - "+UserName+" - "+ExpirationDate;
			UI=new UISettings();
			this.DataContext=UI;
			Menu_Generate(null, null);
		}
		async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (File.Exists("LastSaved.xsetting")) await UI.LoadCrypt();
			if (UI.LoadStartup) {
				if (File.Exists("LastSaved.xlayout")) await Dock.LoadCrypt(UI);
				List_Scan(null, null);
			}
		}
		
		void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if ((Keyboard.Modifiers&ModifierKeys.Control)==ModifierKeys.Control)
			{
				if (Keyboard.IsKeyDown(Key.T)) btn_ThresholdTest_Click(null, null);
				if (Keyboard.IsKeyDown(Key.S)) btn_SingleTest_Click(null, null);
				if (Keyboard.IsKeyDown(Key.B)&&(EnableBatch=true)) btn_StartBatch_Click(null, null);
				if (Keyboard.IsKeyDown(Key.OemTilde)) { cmb_SV.SelectedIndex=0; }
				if (Keyboard.IsKeyDown(Key.D1)) { cmb_SV.SelectedIndex=1; }
				if (Keyboard.IsKeyDown(Key.D2)) { cmb_SV.SelectedIndex=2; }
				if (Keyboard.IsKeyDown(Key.D3)) { cmb_SV.SelectedIndex=3; }
				if (Keyboard.IsKeyDown(Key.D4)) { cmb_SV.SelectedIndex=4; }
				if (Keyboard.IsKeyDown(Key.D5)) { cmb_SV.SelectedIndex=5; }
				if (Keyboard.IsKeyDown(Key.D6)) { cmb_SV.SelectedIndex=6; }
				if (Keyboard.IsKeyDown(Key.D7)) { cmb_SV.SelectedIndex=7; }
			}
		}

		void Menu_Generate(object sender, RoutedEventArgs e)
		{
			Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\preset\\");
			Menu_Setting_Generate(); Menu_Layout_Generate();
		}
		void Menu_Setting_Generate()
		{
			try {
				Uset.Items.Clear();
				MenuItem miSave = new MenuItem { Header="Save Settings...", Icon=new Image { Source=new BitmapImage(new Uri("Resources\\Save.png", UriKind.Relative)) } };
				miSave.Click+=Menu_Setting_Save;
				Uset.Items.Add(miSave);
				MenuItem miLoad = new MenuItem { Header="Load Settings...", Icon=new Image { Source=new BitmapImage(new Uri("Resources\\Load.png", UriKind.Relative)) } };
				miLoad.Click+=Menu_Setting_Load;
				Uset.Items.Add(miLoad);
				string[] settingfiles = Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\preset\\", "*.xsetting", SearchOption.TopDirectoryOnly);
				if (settingfiles.Length>0) {
					Uset.Items.Add(new Separator());
					int n=0;	foreach (string sfile in settingfiles) {
						MenuItem miTemp = new MenuItem() { Header=Path.GetFileName(sfile), Icon=new Image { Source=new BitmapImage(new Uri("Resources\\Load.png", UriKind.Relative))} };
						miTemp.Click+=Menu_Setting_LoadFile;
						Uset.Items.Add(miTemp);
						n++; if (n>20) break;
					}
				}
			} catch { MessageBox.Show("Failed to generate menu items for settings!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
		}
		void Menu_Layout_Generate()
		{
			try {
				Ulyt.Items.Clear();
				MenuItem miSave = new MenuItem { Header="Save Layout...", Icon=new Image { Source=new BitmapImage(new Uri("Resources\\Save.png", UriKind.Relative))} };
				miSave.Click+=Menu_Layout_Save;
				Ulyt.Items.Add(miSave);
				MenuItem miLoad = new MenuItem { Header="Load Layout...", Icon=new Image { Source=new BitmapImage(new Uri("Resources\\Load.png", UriKind.Relative))} };
				miLoad.Click+=Menu_Layout_Load;
				Ulyt.Items.Add(miLoad);
				string[] settingfiles = Directory.GetFiles(Directory.GetCurrentDirectory()+"\\preset\\", "*.xlayout", SearchOption.TopDirectoryOnly);
				if (settingfiles.Length>0) {
					Ulyt.Items.Add(new Separator());
					int n = 0; foreach (string sfile in settingfiles) {
						MenuItem miTemp = new MenuItem() { Header=Path.GetFileName(sfile), Icon=new Image { Source=new BitmapImage(new Uri("Resources\\Load.png", UriKind.Relative))} };
						miTemp.Click+=Menu_Layout_LoadFile;
						Ulyt.Items.Add(miTemp);
						n++; if (n>=20) break;
					}
				}
			} catch { MessageBox.Show("Failed to generate menu items for layout!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
		}
		async void Menu_File_QuickLoad_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists("LastSaved.xlayout")) await Dock.LoadCrypt(UI);
			if (File.Exists("LastSaved.xsetting")) await UI.LoadCrypt();
			List_Scan(null, null);
		}		
		async void Menu_File_QuickSave_Click(object sender, RoutedEventArgs e)
		{
			await Dock.SaveCrypt(UI);
			await UI.SaveCrypt();
		}
		async void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
		{
			MenuItem mnu = (MenuItem)e.OriginalSource;
			if (mnu.Header.ToString()=="Exit") {
				this.Close();
			} else if (mnu.Header.ToString().Contains("Exit")) {
				await UI.SaveCrypt(); await Dock.SaveCrypt(UI); this.Close();
			}
		}
		void Menu_Theme_Change(object sender, RoutedEventArgs e)
		{
			try {
				Utma.Icon=null; Utmg.Icon=null; Utmm.Icon=null; Utmv.Icon=null;
				MenuItem mnu = (MenuItem)e.OriginalSource;
				mnu.Icon=new Image { Source=new BitmapImage(new Uri("Resources\\Check.png", UriKind.Relative)) };
				switch (mnu.Header.ToString()) {
					case "Aero": Dock.Theme=new Xceed.Wpf.AvalonDock.Themes.AeroTheme(); break;
					case "Generic": Dock.Theme=new Xceed.Wpf.AvalonDock.Themes.GenericTheme(); break;
					case "Metro": Dock.Theme=new Xceed.Wpf.AvalonDock.Themes.MetroTheme(); break;
					case "VS2010": Dock.Theme=new Xceed.Wpf.AvalonDock.Themes.VS2010Theme(); break;
				}
			} catch {
				MessageBox.Show("Failed to load the selected theme!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		void Menu_Help_About_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show($"Lung Morphometry Parenchymal Airspace Profiling v{CurrentVersion}\n"+
				"is a Windows Presentation Foundation Application\n"+
				"Built with AvalonDock, AForge/Accord.NET...\n"+
				"This software is under Attribution-NonCommercial-NoDerivs CC BY-NC-ND License\n"+
				"Created by Rui Xiao\n", "About the software", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		void Menu_Help_Update_Click(object sender, RoutedEventArgs e)
		{					
			var NumOnlyVersion=new String(CurrentVersion.Where(Char.IsDigit).ToArray());
			System.Diagnostics.Process.Start("http://www.ruixiao85.net/version/"+NumOnlyVersion);
		}
		
		void OpenHomePage(object sender, MouseButtonEventArgs e)
		{			
			System.Diagnostics.Process.Start("http://www.ruixiao85.net");
		}
		async void Menu_Setting_Save(object sender, RoutedEventArgs e)
		{
			Directory.CreateDirectory("preset");
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.InitialDirectory=Directory.GetCurrentDirectory()+"\\preset\\";
			dlg.DefaultExt=".xsetting"; dlg.Filter="XML Files (*.xsetting)|*.xsetting";
			if (dlg.ShowDialog()==true) { await UI.SaveCrypt(dlg.FileName); }
		}
		async void Menu_Setting_Load(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();			
			dlg.InitialDirectory=Directory.GetCurrentDirectory()+"\\preset\\";
			dlg.DefaultExt=".xsetting"; dlg.Filter="XML Files (*.xsetting)|*.xsetting";
			if (dlg.ShowDialog()==true) { await UI.LoadCrypt(dlg.FileName); }
			List_Scan(null, null);
		}

		async void Menu_Setting_LoadFile(object sender, RoutedEventArgs e)
		{ 
			MenuItem mnu = (MenuItem)e.OriginalSource;
			await UI.LoadCrypt(Directory.GetCurrentDirectory()+"\\preset\\"+mnu.Header.ToString());
			List_Scan(null, null);
		}
		async void Menu_Layout_Save(object sender, RoutedEventArgs e)
		{
			Directory.CreateDirectory("preset");
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();			
			dlg.InitialDirectory=Directory.GetCurrentDirectory()+"\\preset\\";
			dlg.DefaultExt=".xlayout"; dlg.Filter="XML Files (*.xlayout)|*.xlayout";
			if (dlg.ShowDialog()==true) { await Dock.SaveCrypt(UI, dlg.FileName); }
		}
		async void Menu_Layout_Load(object sender, RoutedEventArgs e)
		{
			var dlg = new Microsoft.Win32.OpenFileDialog();				
			dlg.InitialDirectory=Directory.GetCurrentDirectory()+"\\preset\\";
			dlg.DefaultExt=".xlayout"; dlg.Filter="XML Files (*.xlayout)|*.xlayout";
			if (dlg.ShowDialog()==true) { await Dock.LoadCrypt(UI, dlg.FileName); }
		}
		async void Menu_Layout_LoadFile(object sender, RoutedEventArgs e)
		{
			MenuItem mnu = (MenuItem)e.OriginalSource;
			await Dock.LoadCrypt(UI, Directory.GetCurrentDirectory()+"\\preset\\"+mnu.Header.ToString());
		}

		//void FileExplorer2WorkDirectory(object sender, MouseButtonEventArgs e) { UI.WorkDirectory=myFileExplorer.SelectedPath; List_Scan(null, null); }
		void Urt_pDrop(object sender, DragEventArgs e) { e.Handled=true; }
		void Urt_Drop(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
			UI.WorkDirectory=Urt.Text=Path.GetDirectoryName(files[0]);
		}
		void btn_WorkDirectory_Click(object sender, RoutedEventArgs e)
		{
			var fbd = new WinForms.FolderBrowserDialog();
			fbd.RootFolder=Environment.SpecialFolder.Desktop;
			if (fbd.ShowDialog()==WinForms.DialogResult.OK) { UI.WorkDirectory=Urt.Text=fbd.SelectedPath; }
			List_Scan(null, null);
		}
		//void btn_Background_Click(object sender, RoutedEventArgs e)
		//{
		//	WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog();
		//	ofd.Title="Select an image file as background.";
		//	ofd.Filter="Image Files (*.jpg; *.tif)|*.JPG;*.TIF;*.TIFF|All Files (*.*)|*.*";
		//	if (ofd.ShowDialog()==WinForms.DialogResult.OK) {
		//		txt_BG.Text=ofd.FileName;
		//	}
		//}
		void sw_APC_Changed(object sender, SelectionChangedEventArgs e)
		{
			if (sw_APC.SelectedIndex==0) {
				Analysis_LMAP.IsEnabled=true; Analysis_LMAQ.IsEnabled=false; Analysis_LMAP.IsChecked=true; // Full Set
			} else if (sw_APC.SelectedIndex==1) {
				Analysis_LMAP.IsEnabled=false; Analysis_LMAQ.IsEnabled=true; Analysis_LMAQ.IsChecked=true; // Last Part
			}
		}
		void FileTypeSpin(object sender, Xceed.Wpf.Toolkit.SpinEventArgs e)
		{
			if (UI.ListType.Substring(0, 1)=="j") UI.ListType="tif"; else UI.ListType="jpg";
		}

		async void List_Scan(object sender, RoutedEventArgs e)
		{
			try {
				UI.FileList.Clear();
				lst_Scan.IsEnabled=false; lst_Stop.IsEnabled=true;
				string[] listfiles = await Task<string[]>.Factory.StartNew(() => { return UI.WorkDirectory.getFiles("*.jpg|*.jpeg|*.tif*", SearchOption.AllDirectories); });
				Stb_Middle.Text=$"Filtering [{listfiles.Length}] files...";
				var progress = new Progress($"Scanning {listfiles.Length} files ...", listfiles.Length);
				using (UI.cts=new CancellationTokenSource()) {
					foreach (string file in listfiles) {
						var thisfile = await Task<FileData>.Factory.StartNew((f) => {
							var _bitmapFrame = BitmapFrame.Create(new Uri((string)f), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
							string[] folders = ((string)f).Substring(UI.WorkDirectory.Length+1).Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
							string _group = (folders.Length<1) ? "" : folders[0];
							string _individual = (folders.Length<2) ? "" : folders[1];
							string _filetype = Path.GetExtension((string)f).Substring(1);
							if (((!UI.ListTypeSwitch)||(_filetype==UI.ListType))&&
									  ((!UI.ListSizeSwitch)||(_bitmapFrame.PixelWidth==UI.ListSizeWidth&&_bitmapFrame.PixelHeight==UI.ListSizeHeight))&&
									  ((!UI.ListSubFolderSwitch)||(folders.Length>=UI.ListSubFolder)))
								return new FileData { FileName=((string)f).Substring(UI.WorkDirectory.Length+1),	DisplayColor="Green", Group=_group, Individual=_individual,	FileType=_filetype, Width=_bitmapFrame.PixelWidth,	Height=_bitmapFrame.PixelHeight };
							else return null;
						}, file);
						if (thisfile!=null) UI.FileList.Add(thisfile);
						progress.Increment(UI);
						UI.cts.Token.ThrowIfCancellationRequested();
					}
				}
				Stb_Middle.Text=(UI.FileList.Count==0) ? "No Match Found." : $"[{UI.FileList.Count}] Files";
			} catch {
				MessageBox.Show("Invalid Target Folder or Cancelled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			} finally {
				UI.Status="Ready..."; UI.ProgressPercent=0;
				lst_Scan.IsEnabled=true; lst_Stop.IsEnabled=false;
			}
		}
		void List_Stop(object sender, RoutedEventArgs e)
		{
			try {
				UI.Status="Canceled..."; UI.ProgressPercent=0; Thread.Sleep(200);
				lst_Scan.IsEnabled=true; lst_Stop.IsEnabled=false;
				UI.cts.Cancel();
			} catch { }
		}
		void List_Remove(object sender, RoutedEventArgs e)
		{
			if (ListFiles.SelectedItems.Count>0) {
				if (MessageBox.Show("Do you want to remove the "+ListFiles.SelectedItems.Count.ToString()+" file(s) that you selected?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)==MessageBoxResult.Yes) {
					var FileListTemp = new ObservableCollection<FileData>() ;
					foreach (FileData file in ListFiles.SelectedItems) { FileListTemp.Add(file); } // use temp list to save remove list
					foreach (FileData file in FileListTemp) { UI.FileList.Remove(file); } // remove
					FileListTemp.Clear(); // dispose temp list					
				}
			}
		}
		void List_Clear(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Do you want to cancel all processes and clear the list?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)==MessageBoxResult.Yes) {
try {
					UI.cts.Cancel();
					UI.ProgressPercent=0; UI.Status="Ready...";
					Analysis_MultiThread.IsEnabled=true; Analysis_ExportDetail.IsEnabled=true;
					UI.FileList.Clear();
				} catch { MessageBox.Show("Error clearing the list.", "Error"); }
			}
		}

		void List_PopExplorer(object sender, MouseButtonEventArgs e)
		{
			ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", UI.WorkDirectory);
			Process.Start(pfi);
		}

		FileData SelectedFile {	get {
			var selFiles = from FileData f in (ListFiles.SelectedItems) select f;
			foreach (FileData file in selFiles) return file; return null;
		}}
		List<FileData> SelectedFiles { get {
			var FileListTemp = new List<FileData>();
			var selFiles = from FileData f in (ListFiles.SelectedItems) orderby f.FileName select f;
			foreach (FileData file in selFiles) FileListTemp.Add(file);
			return FileListTemp;
		}}


		async void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try {
				this.Cursor=Cursors.AppStarting; UI.LockUp();
				if (ListFiles.SelectedItems.Count==1) {
					using (var img = await Task<PreProcess>.Factory.StartNew((f) => { return new PreProcess(UI, (FileData)f); }, SelectedFile)) {
						//using (var img = new PreProcess(UI, SelectedFile)) {
						UI.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude);
						StringBuilder stb_right=new StringBuilder();
						stb_right.Append($"{img.BitmapOriginal.PixelFormat} ");
						if (img.Width*img.Height>10000000) stb_right.Append($"[resize ratio < 1] ");
							else stb_right.Append($"[resize ratio = 1] ");
						Stb_Right.Text=stb_right.ToString();
					}
				} else if (ListFiles.SelectedItems.Count>1) {
					Stb_Right.Text=$"{ListFiles.SelectedItems.Count} files selected.";
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Error");
			} finally {
				this.Cursor=Cursors.Arrow; UI.Unlock();
			}
		}
		async void btn_ThresholdTest_Click(object sender, RoutedEventArgs e)
		{
			try {
				this.Cursor=Cursors.AppStarting; UI.LockUp();
				if (ListFiles.SelectedItems.Count==1) {
					using (var img = await Task<Binarize>.Factory.StartNew((f) => { return new Binarize(UI, (FileData)f); }, SelectedFile)) {
						UI.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude,
								  img.BitmapGray, img.BitmapBlackWhite);
						Stb_Right.Text=$"{img.BitmapOriginal.PixelFormat}";
					}
				} else { throw new Exception("Please select one file from the list."); }
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Error");
			} finally {
				this.Cursor=Cursors.Arrow; UI.Unlock();
			}
		}
		async void btn_SingleTest_Click(object sender, RoutedEventArgs e)
		{
			try {
				this.Cursor=Cursors.AppStarting; UI.LockUp();
				if (ListFiles.SelectedItems.Count==1) {
				    if (Analysis_LMAP.IsChecked==true) {
						var areaPool = new ConcurrentBag<double>();
						using (var img = await Task<CalPAP>.Factory.StartNew((f) => { return new CalPAP(UI, (FileData)f, ref areaPool); }, SelectedFile)) {
							UI.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
							if (areaPool.Count<10) throw new Exception("Not enough blobs for profiling.");
							var categ = await Task<CalPAC>.Factory.StartNew(() => { return new CalPAC(UI, img.FileName, 1, areaPool.ToList()); });
							areaPool=null;
							UI.Graph1=categ.ToOxyPlot(1); UI.Graph2=categ.ToOxyPlot(2);
							UI.Graph3=categ.ToOxyPlot(3);
							if (UI.Categ1Dist) { // 1-dist
								if (MessageBox.Show($"Airspace profiling identified one population: {categ.p1Mean:G2} (µm^2).\nClick Yes if you like to use the calculated cutoff (one-tail CI95): {categ.c1_DucDes_CI95_1tail:G2} for quantification.", $"Proceed to quantification?", MessageBoxButton.YesNo, MessageBoxImage.Question)==MessageBoxResult.Yes) {
									using (var img1 = await Task<CalPAQ1>.Factory.StartNew((f) => { return new CalPAQ1(UI, (FileData)f, categ); }, SelectedFile)) {
										UI.UpdateImageSource(img1.BitmapOriginal, img1.BitmapMarkup, img1.BitmapExclude, img1.BitmapGray, img1.BitmapBlackWhite, img1.BitmapResult);
									}
								}
							} else {
								if (MessageBox.Show($"Airspace profiling identified three populations: {categ.p1Mean:G2}, {categ.p2Mean:G2} and {categ.p3Mean:G2} (µm^2).\nClick Yes if you like to use the calculated cutoffs: {categ.c2_Sac_Log_Threshold:G2}, {categ.c2_DucDes_Log_Threshold:G2} (log10 µm^2) for quantification?", $"Proceed to quantification?", MessageBoxButton.YesNo, MessageBoxImage.Question)==MessageBoxResult.Yes) {
									using (var img2 = await Task<CalBlobAQ2>.Factory.StartNew((f) => { return new CalBlobAQ2(UI, (FileData)f, categ); }, SelectedFile)) {
										UI.UpdateImageSource(img2.BitmapOriginal, img2.BitmapMarkup, img2.BitmapExclude, img2.BitmapGray, img2.BitmapBlackWhite, img2.BitmapResult);
									}
								}
							}
						}
					} else if (Analysis_LMAQ.IsChecked==true) {
						if (UI.Categ1Dist) {
							using (var img = await Task<CalPAQ1>.Factory.StartNew((f) => { return new CalPAQ1(UI, (FileData)f); }, SelectedFile)) {
								UI.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
							}
						} else {
							using (var img = await Task<CalBlobAQ2>.Factory.StartNew((f) => { return new CalBlobAQ2(UI, (FileData)f); }, SelectedFile)) {
								UI.UpdateImageSource(img.BitmapOriginal, img.BitmapMarkup, img.BitmapExclude, img.BitmapGray, img.BitmapBlackWhite, img.BitmapResult);
							}
						}
					}
				} else { throw new Exception("Please select one file from the list."); }
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Error");
			} finally {
				this.Cursor=Cursors.Arrow; UI.Unlock();
			}
		}
		async void btn_StartBatch_Click(object sender, EventArgs e)
		{
			try {
				this.Cursor=Cursors.Wait; UI.LockUp();
				btn_StartBatch.IsEnabled=false; btn_StopBatch.IsEnabled=true;
				Analysis_MultiThread.IsEnabled=false; Analysis_ExportDetail.IsEnabled=false;
				if (EnableBatch==false) throw new Exception("Batch processing is not enabled for this copy.");
				if (ListFiles.SelectedItems.Count==0) throw new Exception("Please select at least one file from the list.");
				if ((!Directory.Exists($"{UI.WorkDirectory}\\{UI.ProjectName}"))||(MessageBox.Show($"A folder with the project name: {UI.ProjectName} already exists."+"\nOverwrite?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)==MessageBoxResult.Yes)) {
					Directory.CreateDirectory($"{UI.WorkDirectory}\\{UI.ProjectName}");
					using (UI.cts=new CancellationTokenSource()) {
						//Email(SelectedFiles);
					    if (Analysis_LMAP.IsChecked==true) { // LM - AP
							await UI.SaveCrypt($"{UI.WorkDirectory}\\{UI.ProjectName}\\_{UI.ProjectName}_LMAP.xsetting");	
							if (UI.Categ1Dist) {	await UI.BatchAP1(SelectedFiles); } else {	await UI.BatchAP2(SelectedFiles); }
						} else if (Analysis_LMAQ.IsChecked==true) { // LM - AQ
							await UI.SaveCrypt($"{UI.WorkDirectory}\\{UI.ProjectName}\\_{UI.ProjectName}LMAQ.xsetting");							
							if (UI.Categ1Dist) {	await UI.BatchAQ1(SelectedFiles); } else {	await UI.BatchAQ2(SelectedFiles); }
						}
					}
					MessageBox.Show($"The results can be found at:\n{UI.WorkDirectory}\\{UI.ProjectName}", "Completed", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Error");
			} finally {
				UI.Status="Ready..."; UI.ProgressPercent=0;
				btn_StartBatch.IsEnabled=true; btn_StopBatch.IsEnabled=false;
				Analysis_MultiThread.IsEnabled=true; Analysis_ExportDetail.IsEnabled=true;
				this.Cursor=Cursors.Arrow; UI.Unlock();
			}
		}
		void btn_StopBatch_Click(object sender, EventArgs e)
		{
			try {
				UI.Status="Canceled...";
				UI.cts.Cancel();
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Error");
			} finally {
				UI.ProgressPercent=0;  Thread.Sleep(200);
				btn_StartBatch.IsEnabled=true; btn_StopBatch.IsEnabled=false;
				this.Cursor=Cursors.Arrow; UI.Unlock();
			}
		}


	}
}

