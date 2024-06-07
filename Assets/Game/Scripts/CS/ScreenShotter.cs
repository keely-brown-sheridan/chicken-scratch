using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ChickenScratch
{
    public class ScreenShotter : MonoBehaviour
    {
        private Texture2D destinationTexture;
        private bool isPerformingScreenGrab;

        [SerializeField]
        private Camera screenshottingCamera;

        private string currentScreenshotFileName = "";
        private string currentScreenshotFolderName = "";


        void Start()
        {
            // Create a new Texture2D with the width and height of the screen, and cache it for reuse
            destinationTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            // Add the onPostRender callback
            Camera.onPostRender += OnPostRenderCallback;
        }

        void OnPostRenderCallback(Camera cam)
        {
            if (isPerformingScreenGrab)
            {
                // Check whether the Camera that has just finished rendering is the one you want to take a screen grab from
                if (cam == Camera.main)
                {
                    // Define the parameters for the ReadPixels operation
                    Rect regionToReadFrom = new Rect(0, 0, Screen.width, Screen.height);
                    int xPosToWriteTo = 0;
                    int yPosToWriteTo = 0;
                    bool updateMipMapsAutomatically = false;

                    // Copy the pixels from the Camera's render target to the texture
                    destinationTexture.ReadPixels(regionToReadFrom, xPosToWriteTo, yPosToWriteTo, updateMipMapsAutomatically);

                    //Save the image to the screenshots folder
                    byte[] destinationTextureData = destinationTexture.EncodeToPNG();
                    string savingFolder = Application.persistentDataPath + "\\" + currentScreenshotFolderName;
                    string savingFileName = savingFolder + "\\" + currentScreenshotFileName;

                    if (!Directory.Exists(savingFolder))
                    {
                        Directory.CreateDirectory(savingFolder);
                    }
                    File.WriteAllBytes(savingFileName, destinationTextureData);

                    // Reset the isPerformingScreenGrab state
                    isPerformingScreenGrab = false;
                }
            }
        }

        public void TakeScreenshot(string inFolderName, string inFileName)
        {
            currentScreenshotFolderName = inFolderName;
            currentScreenshotFileName = inFileName;
            string savingFolder = Application.persistentDataPath + "\\" + currentScreenshotFolderName;
            string savingFileName = savingFolder + "\\" + currentScreenshotFileName;

            if (!Directory.Exists(savingFolder))
            {
                Directory.CreateDirectory(savingFolder);
            }
            ScreenCapture.CaptureScreenshot(savingFileName);
            //isPerformingScreenGrab = true;
        }

        public string MergeImages(string inMergingFolderName, string inSaveFileName, string inSaveFolder)
        {
            List<Texture2D> loadedTextures = new List<Texture2D>();
            string[] filesInFolder = Directory.GetFiles(Application.persistentDataPath + "\\" + inMergingFolderName);

            int totalPixelWidth = 0;
            int totalPixelHeight = 0;
            foreach (string fileInFolder in filesInFolder)
            {

                byte[] byteArray = File.ReadAllBytes(fileInFolder);
                Texture2D sampleTexture = new Texture2D(2, 2);
                bool isLoaded = sampleTexture.LoadImage(byteArray);
                if (isLoaded)
                {
                    loadedTextures.Add(sampleTexture);
                    totalPixelWidth += sampleTexture.width;
                    if (totalPixelHeight < sampleTexture.height)
                    {
                        totalPixelHeight = sampleTexture.height;
                    }
                }
            }
            Texture2D finalTexture = new Texture2D(totalPixelWidth, totalPixelHeight);
            int currentTotalWidth = 0;

            foreach (Texture2D loadedTexture in loadedTextures)
            {
                Color[] loadedPixels = loadedTexture.GetPixels();
                for (int x = 0; x < loadedTexture.width; x++)
                {
                    for (int y = 0; y < loadedTexture.height; y++)
                    {
                        Color currentPixelColour = loadedPixels[x + loadedTexture.width * y];
                        finalTexture.SetPixel(x + currentTotalWidth, y, currentPixelColour);
                    }
                }
                currentTotalWidth += loadedTexture.width;
            }

            //Save compiled image
            byte[] bytes = finalTexture.EncodeToPNG();
            if (!Directory.Exists(Application.persistentDataPath + "\\" + inSaveFolder))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "\\" + inSaveFolder);
            }
            string outFilePath = Application.persistentDataPath + "\\" + inSaveFolder + "\\" + inSaveFileName;
            if (File.Exists(outFilePath))
            {
                File.Delete(outFilePath);
            }
            File.WriteAllBytes(outFilePath, bytes);
            for (int i = filesInFolder.Length - 1; i >= 0; i--)
            {
                //File.Delete(filesInFolder[i]);
            }

            return Application.persistentDataPath + "\\" + inSaveFolder;
        }

        public bool saveImage(string tempFolder, string savingFolder, string fileName)
        {
            string fullTempFolder = Application.persistentDataPath + "\\" + tempFolder;
            string fullSaveFolder = Application.persistentDataPath + "\\" + savingFolder;
            //Debug.LogError("Saving chain to - " + fullSaveFolder);
            if (File.Exists(fullTempFolder + "\\" + fileName))
            {
                if (File.Exists(fullSaveFolder + "\\" + fileName))
                {
                    try
                    {
                        File.Delete(fullSaveFolder + "\\" + fileName);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to save gif file, could not delete existing file[" + fullSaveFolder + "\\" + fileName + "]. Error: " + e.Message);
                        return false;
                    }
                }

                if (!Directory.Exists(fullSaveFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(fullSaveFolder);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to save gif file, could not create saving folder[" + fullSaveFolder + "]. Error: " + e.Message);
                        return false;
                    }
                }

                try
                {
                    File.Copy(fullTempFolder + "\\" + fileName, fullSaveFolder + "\\" + fileName);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to save gif file, could not copy file[" + fullTempFolder + "\\" + fileName + "]. Error: " + e.Message);
                    return false;
                }

                return true;
            }
            return false;
        }

        public void clearOldTempData()
        {
            string tempFolder = Application.persistentDataPath + "\\Screenshots\\temp";
            string[] tempDirectories;
            try
            {
                tempDirectories = Directory.GetDirectories(tempFolder);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to get temp directories[" + tempFolder + "] - " + e.Message);
                return;
            }

            foreach (string tempDirectory in tempDirectories)
            {
                try
                {
                    Directory.Delete(tempDirectory, true);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to delete tempDirectory[" + tempDirectory + "] in temp folder - " + e.Message);
                }
            }
        }

        // Remove the onPostRender callback
        void OnDestroy()
        {
            Camera.onPostRender -= OnPostRenderCallback;
        }
    }
}