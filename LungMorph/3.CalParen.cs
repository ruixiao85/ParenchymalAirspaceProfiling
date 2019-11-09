using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Accord.MachineLearning;
using AForge.Imaging.Filters;
using AForge.Imaging;
using System.ComponentModel;

namespace LungMorphApp
{
	abstract class CalParen : Binarize, IDisposable
	{
						
		[Description("Sum of total area (µm2)")] public double Total_SumArea { get; set; }
		[Description("Sum of lung area (µm2)")] public double Lung_SumArea { get; set; }
		[Description("Sum of parenchyma area (µm2)")] public double Paren_SumArea { get; set; }
		[Description("Sum of septal tissue area (µm2)")] public double Tis_SumArea { get; set; }

		public new void Dispose() { base.Dispose(); }
		public CalParen(UISettings ui, FileData file) : base(ui, file)
		{
			Merge AFmerge = new Merge(UnmanagedExclude);
			AFmerge.ApplyInPlace(UnmanagedBlackWhite);		          
		}
					
		public static Bitmap AddBlobText(Bitmap bm, Color col, string header = "", string footer = "", Blob[] blobs = null, int[] blobDes = null, int fontsize = 36)
		{

			Graphics g = Graphics.FromImage(bm);
			GraphicsPath gp = new GraphicsPath(); // as outline (background)
			Pen p = new Pen(ColorTranslator.FromHtml("#bbbbbb"), 3); // outline pen color and width
			g.SmoothingMode=SmoothingMode.AntiAlias;
			g.InterpolationMode=InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode=PixelOffsetMode.HighQuality;
			StringFormat stringTopLeft = new StringFormat();
			StringFormat stringCenter = new StringFormat(); stringCenter.Alignment=StringAlignment.Center; stringCenter.LineAlignment=StringAlignment.Center;
			StringFormat stringBottomRight = new StringFormat(); stringBottomRight.Alignment=StringAlignment.Far; stringBottomRight.LineAlignment=StringAlignment.Far;
			Font f = new Font("Arial", fontsize, FontStyle.Regular, GraphicsUnit.Pixel);
			if (blobs!=null) {
				for (int l = 0; l<blobDes.Length; l++) {
					gp.AddString($"#{blobDes[l]}\n{Math.Log10(blobs[blobDes[l]].Area+1):0.0}",
						 f.FontFamily, (int)f.Style, f.Size, new PointF(blobs[blobDes[l]].CenterOfGravity.X, blobs[blobDes[l]].CenterOfGravity.Y), stringCenter);
				}
			}
			gp.AddString(header, f.FontFamily, (int)f.Style, f.Size, new PointF(0, 0), stringTopLeft);
			gp.AddString(footer, f.FontFamily, (int)f.Style, f.Size, new PointF(bm.Width, bm.Height), stringBottomRight);

			g.DrawPath(p, gp);
			g.FillPath(new SolidBrush(col), gp);
			gp.Dispose(); f.Dispose(); p.Dispose();
			g.Flush();
			return bm;
		}

	}
}
