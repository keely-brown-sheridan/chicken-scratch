// using System;
// using System.IO;
// using System.Runtime.InteropServices;
// using UnityEngine;

// public class GifRecorder : MonoBehaviour
// {


//     private void Start()
//     {
//         texture = new Texture2D(width, height, TextureFormat.RGB24, false);
//         pixels = new Color32[width * height];

        

//         byte[] colorMap = new byte[256 * 3];

//         for (int i = 0; i < 256; i++)
//         {
//             colorMap[i * 3] = (byte)i;
//             colorMap[i * 3 + 1] = (byte)i;
//             colorMap[i * 3 + 2] = (byte)i;
//         }

//         EGifPutScreenDesc(gif, width, height, 8, 0, 0, colorMap);
//     }

//     private void Update()
//     {
//         captureTimer += Time.deltaTime;

//         if (captureTimer >= 1f / fps)
//         {
//             captureTimer -= 1f / fps;

//             texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//             texture.GetPixels32();

//             byte[] line = new byte[width];

//             for (int i = 0; i < height; i++)
//             {
//                 for (int j = 0; j < width; j++)
//                 {
//                     line[j] = pixels[i * width + j].r;
//                 }

//                 EGifPutImageDesc(gif, 0, i, width, 1, 0, null);
//                 EGifPutLine(gif, line, width);
//             }

//             frameCount++;
//         }

//         if (frameCount >= fps * captureTime)
//         {
//             EGifCloseFile(gif);
//             enabled = false;
//         }
//     }
// }