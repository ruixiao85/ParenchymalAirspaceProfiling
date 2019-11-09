using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace LungMorphApp
{
	/// <summary>
	/// Interaction logic for FileExplorer.xaml
	/// </summary>
	public partial class FileExplorer : UserControl
	{
		object dummyNode = null; // Tree View File Explorer
		string _SelectedPath = "";
		public string SelectedPath { get { return _SelectedPath; } }

		public FileExplorer()
		{
			InitializeComponent();
		}

		void Initialization(object sender, RoutedEventArgs e)
		{
			foreach (string s in Directory.GetLogicalDrives()) { // Tree View
				TreeViewItem item = new TreeViewItem();
				item.Header=s;
				item.Tag=s;
				item.FontWeight=FontWeights.Normal;
				item.Items.Add(dummyNode);
				item.Expanded+=new RoutedEventHandler(Folder_Expanded);
				foldersItem.Items.Add(item);
			}
		}
		void Folder_Expanded(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = (TreeViewItem)sender;
			if (item.Items.Count==1&&item.Items[0]==dummyNode) {
				item.Items.Clear();
				try {
					foreach (string s in Directory.GetDirectories(item.Tag.ToString())) {
						TreeViewItem subitem = new TreeViewItem();
						subitem.Header=s.Substring(s.LastIndexOf("\\")+1);
						subitem.Tag=s;
						subitem.FontWeight=FontWeights.Normal;
						subitem.Items.Add(dummyNode);
						subitem.Expanded+=new RoutedEventHandler(Folder_Expanded);
						item.Items.Add(subitem);
					}
				} catch { throw new Exception("Can not expand the selected folder."); }
			}
		}
		void FoldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeView tree = (TreeView)sender;
			TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);
			if (temp==null) return;
			_SelectedPath="";
			string temp1 = ""; string temp2 = "";
			while (true) {
				temp1=temp.Header.ToString();
				if (temp1.Contains(@"\")) temp2="";
				_SelectedPath=temp1+temp2+_SelectedPath;
				if (temp.Parent.GetType().Equals(typeof(TreeView))) break;
				temp=((TreeViewItem)temp.Parent);
				temp2=@"\";
			}
		}

	}

	[ValueConversion(typeof(string), typeof(bool))]
	public class HeaderToImageConverter : IValueConverter
	{
		public static HeaderToImageConverter Instance = new HeaderToImageConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((value as string).Contains(@"\")) {
				Uri uri = new Uri("pack://application:,,,/Resources/diskdrive.png");
				BitmapImage source = new BitmapImage(uri);
				return source;
			} else {
				Uri uri = new Uri("pack://application:,,,/Resources/folder.png");
				BitmapImage source = new BitmapImage(uri);
				return source;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Cannot convert back");
		}
	}
}
