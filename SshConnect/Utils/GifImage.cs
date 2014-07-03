using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace SshConnect
{
	public class GifImage : Image
	{
		public double fps = 20;

		public int FrameIndex
		{
			get { return (int)GetValue(FrameIndexProperty); }
			set { SetValue(FrameIndexProperty, value); }
		}

		public static readonly DependencyProperty FrameIndexProperty =
			DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

		public String Uri
		{
			get { return (String)GetValue(UriProperty); }
			set { SetValue(UriProperty, value); }
		}
		public static readonly DependencyProperty UriProperty =
			DependencyProperty.Register("Uri", typeof(String), typeof(GifImage), new UIPropertyMetadata(""));

		static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
		{
			GifImage ob = obj as GifImage;
			ob.Source = ob.gf.Frames[(int)ev.NewValue];
			ob.InvalidateVisual();
		}
		GifBitmapDecoder gf;
		Int32Animation anim;

		protected override void OnInitialized(EventArgs e)
		{
			gf = new GifBitmapDecoder(new Uri(Uri, UriKind.RelativeOrAbsolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
			anim = new Int32Animation(0, gf.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, gf.Frames.Count / (int)fps,
				(int)((gf.Frames.Count / (double)fps - gf.Frames.Count / (int)fps) * 1000))));
			anim.RepeatBehavior = RepeatBehavior.Forever;
			Source = gf.Frames[0];
		}

		bool animationIsWorking = false;
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			if (!animationIsWorking)
			{
				BeginAnimation(FrameIndexProperty, anim);
				animationIsWorking = true;
			}
		}
	}
}
