using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LungMorphApp
{
	/// <summary>
	/// Interaction logic for MagnifyImageViewer.xaml
	/// </summary>
	public partial class MagnifyImageViewer : UserControl
	{
		public MagnifyImageViewer()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(MagnifyImageViewer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnImageSourceChanged)));
		public ImageSource ImageSource {
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}
		private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var myUserControl = (MagnifyImageViewer)sender;
			if (myUserControl!=null) {
				myUserControl.Image.Source=(ImageSource)e.NewValue;
			}
		}

		void ToggleZoom(object sender, MouseButtonEventArgs e)
		{
			if (magnifier.Visibility==Visibility.Hidden) {
				magnifier.Visibility=Visibility.Visible; magnifier.Radius=80;	magnifier.ZoomFactor=0.28d;
			} else if (magnifier.Visibility==Visibility.Visible && magnifier.ZoomFactor==0.28d) {
				magnifier.Visibility=Visibility.Visible; magnifier.Radius=160;	magnifier.ZoomFactor=0.12d;
			} else magnifier.Visibility=Visibility.Hidden;
		}

		void EnableZoom(object sender, MouseEventArgs e)
		{
			magnifier.Visibility=Visibility.Visible;
		}

		void DisableZoom(object sender, MouseEventArgs e)
		{
			magnifier.Visibility=Visibility.Hidden;
		}
	}
}
