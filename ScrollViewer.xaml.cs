using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LungMorphApp
{
	/// <summary>
	/// Interaction logic for ScrollViewer.xaml
	/// </summary>
	public partial class ScrollViewer : UserControl
	{

		System.Windows.Point? lastCenterPositionOnTarget; // Scroll Viewer
		System.Windows.Point? lastMousePositionOnTarget;
		System.Windows.Point? lastDragPoint;

		public ScrollViewer()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(ScrollViewer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnImageSourceChanged)));
		public ImageSource ImageSource {
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}
		private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var myUserControl = (ScrollViewer)sender;
			if (myUserControl!=null) {
				myUserControl.Image.Source=(ImageSource)e.NewValue;
			}
		}

		void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (lastDragPoint.HasValue) {
				System.Windows.Point posNow = e.GetPosition(scrollViewer);
				double dX = posNow.X-lastDragPoint.Value.X;
				double dY = posNow.Y-lastDragPoint.Value.Y;
				lastDragPoint=posNow;
				scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset-dX);
				scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset-dY);
			}
			//if (bmp != null && canvImage.Source != null)
			//{ 
			//    Point p = Mouse.GetPosition(myCanvas);
			//    label.Content = p.X.ToString("0") + "," + p.Y.ToString("0") + " | " + (p.X * DpiRatio).ToString("0") + "," + (p.Y * DpiRatio).ToString("0");
			//    vID.Content=nID.ToString(); vXC.Content=(p.X).ToString("0"); vYC.Content=(p.Y).ToString("0");
			//    vX.Content=(p.X * DpiRatio).ToString("0"); vY.Content=(p.Y * DpiRatio).ToString("0");
			//    AFilter.Crop magcrop = new AFilter.Crop( new Rectangle(int.Parse(vX.Content.ToString()),int.Parse(vY.Content.ToString()),
			//        int.Parse(t1.Text), int.Parse(t2.Text)));
			//    //AFilter.ResizeBicubic magresize = new AFilter.ResizeBicubic(int.Parse(f1.Text),int.Parse(f2.Text));
			//    Bitmap newImage = magcrop.Apply(bmp);
			//    MagView.Image=newImage;
			//}
		}
		void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var mousePos = e.GetPosition(scrollViewer);
			if (mousePos.X<=scrollViewer.ViewportWidth&&mousePos.Y<
				 scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
			{
				scrollViewer.Cursor=Cursors.SizeAll;
				lastDragPoint=mousePos;
				Mouse.Capture(scrollViewer);
			}
		}
		void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			scrollViewer.Cursor=Cursors.Arrow;
			scrollViewer.ReleaseMouseCapture();
			lastDragPoint=null;
		}
		void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			lastMousePositionOnTarget=Mouse.GetPosition(myCanvas);
			if (e.Delta>0) { slider.Value+=0.5; }
			if (e.Delta<0) { slider.Value-=0.5; }
			e.Handled=true;
		}
		void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			scaleTransform.ScaleX=Math.Pow(2, e.NewValue);
			scaleTransform.ScaleY=Math.Pow(2, e.NewValue);
			var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth/2, scrollViewer.ViewportHeight/2);
			lastCenterPositionOnTarget=scrollViewer.TranslatePoint(centerOfViewport, myCanvas);
		}
		void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.ExtentHeightChange!=0.0d||e.ExtentWidthChange!=0.0d) {
				System.Windows.Point? targetBefore = null;
				System.Windows.Point? targetNow = null;
				if (!lastMousePositionOnTarget.HasValue) {
					if (lastCenterPositionOnTarget.HasValue) {
						var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth/2, scrollViewer.ViewportHeight/2);
						System.Windows.Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, myCanvas);
						targetBefore=lastCenterPositionOnTarget;
						targetNow=centerOfTargetNow;
					}
				} else {
					targetBefore=lastMousePositionOnTarget;
					targetNow=Mouse.GetPosition(myCanvas);
					lastMousePositionOnTarget=null;
				}
				if (targetBefore.HasValue) {
					double dXInTargetPixels = targetNow.Value.X-targetBefore.Value.X;
					double dYInTargetPixels = targetNow.Value.Y-targetBefore.Value.Y;
					double multiplicatorX = e.ExtentWidth/myCanvas.ActualWidth;
					double multiplicatorY = e.ExtentHeight/myCanvas.ActualHeight;
					double newOffsetX = scrollViewer.HorizontalOffset-dXInTargetPixels*multiplicatorX;
					double newOffsetY = scrollViewer.VerticalOffset-dYInTargetPixels*multiplicatorY;
					if (double.IsNaN(newOffsetX)||double.IsNaN(newOffsetY)) { return; }
					scrollViewer.ScrollToHorizontalOffset(newOffsetX);
					scrollViewer.ScrollToVerticalOffset(newOffsetY);
				}
			}
		}


	}

	public class SliderToZoomConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Math.Pow(2.0d, double.Parse(value.ToString()));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Math.Log(2.0d, double.Parse(value.ToString()));
		}
	}
}
