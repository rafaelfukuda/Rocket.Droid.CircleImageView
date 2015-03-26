using System;
using Android.App;
using Android.Runtime;

namespace Sample
{
	[Application]
	public class SampleApplication : Application
	{
		public SampleApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}

		public override void OnCreate ()
		{
			base.OnCreate ();
		}
	}
}

