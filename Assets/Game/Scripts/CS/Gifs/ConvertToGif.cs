using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using System.Threading;

namespace uGIF
{
	public class ConvertToGif : MonoBehaviour
	{
		public float frameRate = 15;
		public bool capture;
		public int downscale = 1;
		public float captureTime = 10;
		public bool useBilinearScaling = true;

		public int numberOfFramesPerImage = 60;
		[System.NonSerialized]
		public byte[] bytes = null;

		private string savingPath = "";

		void Start ()
		{
			period = 1f / frameRate;
			colorBuffer = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
			startTime = Time.time;
		}

		public void SaveGIF (List<Texture2D> inImageData, string inSavingPath)
		{
			savingPath = inSavingPath;
			foreach(Texture2D image in inImageData)
			{
				for(int i = 0; i < numberOfFramesPerImage; i++)
				{
					frames.Add (new Image (image));
				}
				
			}
			
			bytes = null;
			var thread = new Thread (_Encode);
			thread.Start ();
			StartCoroutine(WaitForBytes());
		}

		IEnumerator WaitForBytes() {
			while(bytes == null) yield return null;
			System.IO.File.WriteAllBytes (savingPath, bytes);
			bytes = null;
		}

		public void _Encode ()
		{
			capture = false;

			var ge = new GIFEncoder ();
			ge.useGlobalColorTable = true;
			ge.repeat = 0;
			ge.FPS = frameRate;
			ge.transparent = new Color32 (255, 0, 255, 255);
			ge.dispose = 1;

			var stream = new MemoryStream ();
			ge.Start (stream);
			foreach (var f in frames) {
				if (downscale != 1) {
					if(useBilinearScaling) {
						f.ResizeBilinear(f.width/downscale, f.height/downscale);
					} else {
						f.Resize (downscale);
					}
				}
				f.Flip ();
				ge.AddFrame (f);
			}
			ge.Finish ();
			bytes = stream.GetBuffer ();
			stream.Close ();
		}

		List<Image> frames = new List<Image> ();
		Texture2D colorBuffer;
		float period;
		float T = 0;
		float startTime = 0;
        
	}
}