using System;
using Android.Widget;
using Android.Graphics;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Java.Lang;
using Android.Graphics.Drawables;
using CircleImageView;

namespace Rocket.Droid
{
	/// <summary>
	/// Ported from https://github.com/hdodenhof/CircleImageView
	/// </summary>
	/// <remarks>
	/// https://github.com/hdodenhof/CircleImageView/blob/master/circleimageview/src/main/java/de/hdodenhof/circleimageview/CircleImageView.java
	/// </remarks>
	public class CircleImageView : ImageView 
	{
		private static Android.Widget.ImageView.ScaleType SCALE_TYPE = Android.Widget.ImageView.ScaleType.CenterCrop;

		private readonly Bitmap.Config BITMAP_CONFIG = Bitmap.Config.Argb8888;

		private const int COLORDRAWABLE_DIMENSION = 2;
		private const int DEFAULT_BORDER_WIDTH = 0;
		private static int DEFAULT_BORDER_COLOR = Color.Black;

		private RectF mDrawableRect;
		private RectF mBorderRect;

		private Matrix mShaderMatrix;
		private Paint mBitmapPaint;
		private Paint mBorderPaint;

		private Color mBorderColor;
		private int _borderWidth = DEFAULT_BORDER_WIDTH;

		private Bitmap mBitmap;
		private BitmapShader mBitmapShader;
		private int mBitmapWidth;
		private int mBitmapHeight;

		private float mDrawableRadius;
		private float mBorderRadius;

		private ColorFilter mColorFilter;

		private bool mReady;
		private bool mSetupPending;

		public CircleImageView (IntPtr javaRef, Android.Runtime.JniHandleOwnership transfer) : base (javaRef, transfer) {
		}

		public CircleImageView (Context context) : base(context) {
			mBorderColor = Color.Black;

			Init ();
		}

		public CircleImageView(Context context, IAttributeSet attrs) : this (context, attrs, 0) {
		}

		public CircleImageView(Context context, IAttributeSet attrs, int defStyle) : base (context, attrs, defStyle) {
			TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.CircleImageView, defStyle, 0);

			_borderWidth = a.GetDimensionPixelSize (Resource.Styleable.CircleImageView_border_width, DEFAULT_BORDER_WIDTH);
			mBorderColor = a.GetColor (Resource.Styleable.CircleImageView_border_color, DEFAULT_BORDER_COLOR);

			a.Recycle ();

			Init();
		}

		private void Init() {
			this.SetScaleType (SCALE_TYPE);
			mReady = true;

			mDrawableRect = new RectF ();
			mBorderRect = new RectF ();

			mShaderMatrix = new Matrix ();
			mBitmapPaint = new Paint ();
			mBorderPaint = new Paint ();

			if (mSetupPending) {
				setup();
				mSetupPending = false;
			}
		}

		public override ScaleType GetScaleType ()
		{
			return SCALE_TYPE;
		}

		public override void SetScaleType (Android.Widget.ImageView.ScaleType scaleType)
		{
			if (scaleType != SCALE_TYPE) {
				throw new IllegalArgumentException(string.Format ("ScaleType %s not supported.", scaleType));
			}
		}

		public override void SetAdjustViewBounds (bool adjustViewBounds)
		{
			if (adjustViewBounds) {
				throw new IllegalArgumentException("adjustViewBounds not supported.");
			}
		}

		protected override void OnDraw (Canvas canvas) {
			if (this.Drawable == null) {
				return;
			}

			canvas.DrawCircle (this.Width / 2, this.Height / 2, mDrawableRadius, mBitmapPaint);
			if (_borderWidth != 0) {
				canvas.DrawCircle (this.Width / 2, this.Height / 2, mBorderRadius, mBorderPaint);
			}
		}

		protected override void OnSizeChanged (int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged (w, h, oldw, oldh);
			setup();
		}

		public int GetBorderColor() {
			return mBorderColor;
		}

		public void SetBorderColor(int borderColor) {
			if (borderColor == mBorderColor) {
				return;
			}

			mBorderColor = new Color (borderColor);
			mBorderPaint.Color = mBorderColor;
			Invalidate();
		}

		public override void SetImageBitmap (Bitmap bm)
		{
			base.SetImageBitmap (bm);
			mBitmap = bm;
			setup();
		}

		public override void SetImageDrawable (Drawable drawable)
		{
			base.SetImageDrawable (drawable);
			mBitmap = getBitmapFromDrawable(drawable);
			setup();
		}

		public override void SetImageResource (int resId)
		{
			base.SetImageResource (resId);
			mBitmap = getBitmapFromDrawable(this.Drawable);
			setup();
		}

		public override void SetImageURI (Android.Net.Uri uri)
		{
			base.SetImageURI (uri);
			mBitmap = getBitmapFromDrawable(this.Drawable);
			setup();
		}

		public override void SetColorFilter (ColorFilter cf)
		{
			base.SetColorFilter (cf);
			if (cf == mColorFilter) {
				return;
			}

			mColorFilter = cf;
			mBitmapPaint.SetColorFilter(mColorFilter);
			Invalidate();
		}

		private Bitmap getBitmapFromDrawable(Drawable drawable) {
			if (drawable == null) {
				return null;
			}

			if (drawable is BitmapDrawable) {
				return ((BitmapDrawable) drawable).Bitmap;
			}

			try {
				Bitmap bitmap;

				if (drawable is ColorDrawable) {
					bitmap = Bitmap.CreateBitmap (COLORDRAWABLE_DIMENSION, COLORDRAWABLE_DIMENSION, BITMAP_CONFIG);
				} else {
					bitmap = Bitmap.CreateBitmap (drawable.IntrinsicWidth, drawable.IntrinsicHeight, BITMAP_CONFIG);
				}

				Canvas canvas = new Canvas(bitmap);
				drawable.SetBounds (0, 0, canvas.Width, canvas.Height);
				drawable.Draw (canvas);
				return bitmap;
			} catch { //catch (OutOfMemoryException e) {
				return null;
			}			
		}

		private void setup() {
			if (!mReady) {
				mSetupPending = true;
				return;
			}

			if (mBitmap == null) {
				return;
			}

			mBitmapShader = new BitmapShader(mBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);

			mBitmapPaint.AntiAlias = true;
			mBitmapPaint.SetShader(mBitmapShader);

			mBorderPaint.SetStyle (Paint.Style.Stroke);
			mBorderPaint.AntiAlias = true;
			mBorderPaint.Color = mBorderColor;
			mBorderPaint.StrokeWidth = _borderWidth;

			mBitmapHeight = mBitmap.Height;
			mBitmapWidth = mBitmap.Width;

			mBorderRect.Set (0, 0, this.Width, this.Height);

			mBorderRadius = Java.Lang.Math.Min((mBorderRect.Height () - _borderWidth) / 2, (mBorderRect.Width () - _borderWidth) / 2);

			mDrawableRect.Set (_borderWidth, _borderWidth, mBorderRect.Width () - _borderWidth, mBorderRect.Height () - _borderWidth);
			mDrawableRadius = Java.Lang.Math.Min (mDrawableRect.Height () / 2, mDrawableRect.Width () / 2);

			updateShaderMatrix();
			Invalidate();
		}

		private void updateShaderMatrix() {
			float scale;
			float dx = 0;
			float dy = 0;

			mShaderMatrix.Set (null);

			if (mBitmapWidth * mDrawableRect.Height () > mDrawableRect.Width () * mBitmapHeight) {
				scale = mDrawableRect.Height () / (float) mBitmapHeight;
				dx = (mDrawableRect.Width () - mBitmapWidth * scale) * 0.5f;
			} else {
				scale = mDrawableRect.Width () / (float) mBitmapWidth;
				dy = (mDrawableRect.Height () - mBitmapHeight * scale) * 0.5f;
			}

			mShaderMatrix.SetScale (scale, scale);
			mShaderMatrix.PostTranslate ((int) (dx + 0.5f) + _borderWidth, (int) (dy + 0.5f) + _borderWidth);

			mBitmapShader.SetLocalMatrix (mShaderMatrix);
		}

	}
}

