using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class OCRResult
{
    public string text;
    public float confidence;
    public float modelTime;
}

public class TrOCRManager : MonoBehaviour
{
    public GameObject bedroomCanvas;
    public GameObject raycastCanvas;
    public GameObject hapticCanvas;
    private GameObject resultCanvas;
    private UnityEngine.UI.RawImage rawImage;
    private TMP_Text resultText;
    public Camera bedroomCamera;
    public Camera raycastCamera;
    public Camera hapticCamera;
    public ServerManager serverManager;
    private Camera cam;
    public int imageWidth = 512;
    public int imageHeight = 512;
    public WritingManager writingManager;
    private int currentBoard;
    private string savePath;
    private int saveCounter = 0;
    private int loadCounter = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentBoard = writingManager.GetCurrentBoard();
        if (currentBoard == 0)
        {
            cam = bedroomCamera;
            resultCanvas = bedroomCanvas;
            rawImage = resultCanvas.GetComponentInChildren<RawImage>();
            resultText = resultCanvas.GetComponentInChildren<TMP_Text>();
        }
        else if (currentBoard == 1)
        {
            cam = raycastCamera;
            resultCanvas = raycastCanvas;
            rawImage = resultCanvas.GetComponentInChildren<RawImage>();
            resultText = resultCanvas.GetComponentInChildren<TMP_Text>();
        }
        else if (currentBoard == 2)
        {
            cam = hapticCamera;
            resultCanvas = hapticCanvas;
            rawImage = resultCanvas.GetComponentInChildren<RawImage>();
            resultText = resultCanvas.GetComponentInChildren<TMP_Text>();
        }

    }

    public void takeScreenshot()
    {
        FileManager.Instance.StopTimer();
        FileManager.Instance.CreateTaskFolder();
        resultText.color = Color.white;
        resultText.text = "Waiting to scan";
        RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
        cam.targetTexture = rt;
        cam.Render();
        Texture2D image = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        image.ReadPixels(new UnityEngine.Rect(0, 0, imageWidth, imageHeight), 0, 0);
        image.Apply(false);
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] byteArray = image.EncodeToPNG();
        var dirPath = UnityEngine.Application.streamingAssetsPath + "/PreScan";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        savePath = dirPath + "/Alphabet" + saveCounter + ".png";
        saveCounter++;
        //Save image with FileManager
        FileManager.Instance.SaveImage(byteArray);
        System.IO.File.WriteAllBytes(savePath, byteArray);
        UnityEngine.Debug.Log("Screenshot Path: " + savePath);
        Texture oldTex = rawImage.texture;
        rawImage.texture = image;
        if (oldTex != null)
        {
            Destroy(oldTex);
        }
    }

    public async void scanScreenshot()
    {
        try
        {
            resultText.color = Color.white;
            resultText.text = "Scanning...";
            loadCounter = saveCounter - 1;
            UnityEngine.Debug.Log("scanScreenshot() called");
            string basePath = Application.streamingAssetsPath;

            string imagePath = savePath;
            string scriptPath = Path.Combine(Application.streamingAssetsPath, "script.py");
            OCRResult ocrResult;
            string jsonResult;

            byte[] image = File.ReadAllBytes(imagePath);

            jsonResult = await serverManager.PostImage(image);
            ocrResult = JsonUtility.FromJson<OCRResult>(jsonResult);
            //CreateJsonFile call
            FileManager.Instance.CreateJsonFile(ocrResult.text, ocrResult.confidence);
            UnityEngine.Debug.Log(ocrResult.text);
            UnityEngine.Debug.Log(ocrResult.confidence);
            editCanvas(ocrResult);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.Log("Error in scanScreenshot: " + ex.Message);
            resultText.color = Color.red;
            resultText.text = "Scan failed!";
        }

        //Old scanScreenshot before TrOCR implementation
        //if (model == null)
        //{
        //    UnityEngine.Debug.LogError("model is NULL! Make sure it is assigned in the Inspector.");
        //    return;
        //}

        //if (rawImage == null)
        //{
        //    UnityEngine.Debug.LogError("RawImage component is missing! Make sure this GameObject has a RawImage.");
        //    return;
        //}


        //UnityEngine.Texture2D texture;
        ////if (savePath.IsUnityNull()) return;
        //string loadPath = savePath;
        //if (!System.IO.File.Exists(loadPath))
        //{
        //    UnityEngine.Debug.Log("File Path doesn't exist: " + loadPath);
        //    return;
        //}
        //byte[] fileData = System.IO.File.ReadAllBytes(loadPath);
        //texture = new UnityEngine.Texture2D(2, 2);
        //texture.LoadImage(fileData);
        //texture.Apply();
        //UnityEngine.Debug.Log($"Loaded texture size: {texture.width}x{texture.height}");
        //if (texture.width == 2 && texture.height == 2)
        //{
        //    UnityEngine.Debug.LogError("Image failed to load or is corrupt!");
        //}
        //// some constants for drawing
        //const int textPadding = 2;
        //const HersheyFonts textFontFace = HersheyFonts.HersheyPlain;
        //double textFontScale = System.Math.Max(texture.width, texture.height) / 512.0;
        //Scalar boxColor = Scalar.DeepPink;
        //Scalar textColor = Scalar.White;

        //// load alphabet
        //AlphabetOCR alphabet = new AlphabetOCR(model.bytes);

        //// scan image
        //var image = OpenCvSharp.Unity.TextureToMat(texture);
        //UnityEngine.Debug.Log($"Converted Mat size: {image.Width}x{image.Height}");
        //IList<AlphabetOCR.RecognizedLetter> letters = alphabet.ProcessImage(image);
        //foreach (var letter in letters)
        //{
        //    int line;
        //    var bounds = Cv2.BoundingRect(letter.Rect);

        //    // text box.
        //    var textData = string.Format("{0}: {1}%", letter.Data, System.Math.Round(letter.Confidence * 100));
        //    var textSize = Cv2.GetTextSize(textData, textFontFace, textFontScale, 1, out line);
        //    var textBox = new OpenCvSharp.Rect(
        //        bounds.X + (bounds.Width - textSize.Width) / 2 - textPadding,
        //        bounds.Bottom,
        //        textSize.Width + textPadding * 2,
        //        textSize.Height + textPadding * 2
        //    );

        //    // draw shape
        //    image.Rectangle(bounds, boxColor, 2);
        //    image.Rectangle(textBox, boxColor, -1);
        //    image.PutText(textData, textBox.TopLeft + new Point(textPadding, textPadding + textSize.Height), textFontFace, textFontScale, textColor, (int)(textFontScale + 0.5));
        //}

        //// result
        //texture = OpenCvSharp.Unity.MatToTexture(image);
        //byte[] byteArray = texture.EncodeToPNG();
        //System.IO.File.WriteAllBytes(UnityEngine.Application.dataPath + "/StreamingAssets/Results/result" + loadCounter + ".png", byteArray);



        //// output
        ////rawImage = resultCanvas.GetComponent<RawImage>();
        //rawImage.texture = texture;
        //rawImage.enabled = false;
        //rawImage.enabled = true;

        //var transform = resultCanvas.GetComponent<UnityEngine.RectTransform>();
        //transform.sizeDelta = new UnityEngine.Vector2(1024, 1024);
    }

    public void editCanvas(OCRResult result)
    {
        if (result != null)
        {
            float conf = result.confidence;
            if (conf >= 0.8f)
            {
                resultText.color = Color.green;
            }
            else if (conf < 0.8f && conf >= 0.6)
            {
                resultText.color = Color.yellow;
            }
            else
            {
                resultText.color = Color.red;
            }
            resultText.text = result.text;
        }
        else
        {
            resultText.color = Color.white;
            resultText.text = "Error :(";
        }
    }
}
