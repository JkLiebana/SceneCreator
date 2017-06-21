using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Recogniser :  EditorWindow {

    [MenuItem("Scene Creator/Recogniser")]
    static void Init()
    {
        Recogniser window = (Recogniser)EditorWindow.GetWindow(typeof(Recogniser));
        window.Show();
    }

    private int cellSize = 6, cellCount = 180;
    private Texture2D image;
    public GameObject square, triangle, circumference, cross;
    void OnGUI()
    {
        GUILayout.Label("Here you can start the recognition phase", EditorStyles.helpBox);
        GUILayout.Label("At first, choose the differents prefabs", EditorStyles.helpBox);

        square = (GameObject)EditorGUILayout.ObjectField("As square: ", square, typeof(GameObject), false);
        triangle = (GameObject)EditorGUILayout.ObjectField("As triangle: ", triangle, typeof(GameObject), false);
        circumference = (GameObject)EditorGUILayout.ObjectField("As circumference: ", circumference, typeof(GameObject), false);
        cross = (GameObject)EditorGUILayout.ObjectField("As cross: ", cross, typeof(GameObject), false);



        if (GUILayout.Button("Set prefabs"))
            SetPrefabs();

        if (showRecognition)
        {

            cellSize = EditorGUILayout.IntField("Neurons:", cellSize);
            cellCount = EditorGUILayout.IntField("Hidden Neurons 1:", cellCount);

            image = (Texture2D)EditorGUILayout.ObjectField("Image to recognise", image, typeof(Texture2D), false);

            Rect recognitionButton = EditorGUILayout.BeginHorizontal("Button");
            if (GUI.Button(recognitionButton, GUIContent.none))
                RecognisePicture();

            GUIStyle s = new GUIStyle();
            s.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Recognise", s);
            EditorGUILayout.EndHorizontal();

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty texturesObtained = so.FindProperty("texturesRescaled");

            EditorGUILayout.PropertyField(texturesObtained, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties
        }
        if (showCreate)
            if (GUILayout.Button("Create scene"))
                CreateScene();

    }

    private bool showCreate = false, showRecognition = false;
    public FigureDetection figureDetection;
    public List<Texture2D> texturesFromRecognition, texturesRescaled;
    public List<float[]> imagesPositions;

    void RecognisePicture()
    {
        figureDetection = new FigureDetection();
        texturesFromRecognition = figureDetection.StartDetection(image, cellCount, cellSize);
        imagesPositions = figureDetection.GetReturnPositions();

        ResizePictures();
        showCreate = true;
    }

    void SetPrefabs()
    {
        Debug.Log("Prefabs setted");
        showRecognition = true;
    }

    void CreateScene()
    {
        _base = GameObject.Find("Base");
        for(int i = 0; i < texturesRescaled.Count; i++)
        {
            ClassifyTexture(texturesRescaled[i], i);
        }


    }


    public int widthTo = 20, heightTo = 20;

    void ResizePictures()
    {
        
        Texture2D txt, picture;
        texturesRescaled = new List<Texture2D>();
        for (int i = 0; i < texturesFromRecognition.Count; i++)
        {

            picture = texturesFromRecognition[i];


            txt = new Texture2D(picture.width, picture.height, TextureFormat.ARGB32, false);
            txt.SetPixels(picture.GetPixels());
            TextureScale.Bilinear(txt, widthTo, heightTo);

            texturesRescaled.Add(txt);

        }


    }

    private int neurons = 400;
    private GameObject temp;
    public GameObject _base;
    void ClassifyTexture(Texture2D _image, int count)
    {

        double[] pxs = new double[neurons];
        Color[] pixels = _image.GetPixels();
        for (int i = 0; i < _image.width * _image.height; i++)
        {
            pxs[i] = pixels[i].grayscale;
        }

        NetworkManager.Instance._neuralNetwork.StopLearning();
        double[] output = NetworkManager.Instance._neuralNetwork.Run(pxs);
        
        switch (MaxValue(output))
        {

            case 0:
                temp = PrefabUtility.InstantiatePrefab(triangle) as GameObject;
                break;
            case 1:
                temp = PrefabUtility.InstantiatePrefab(square) as GameObject;

                break;
            case 2:
                temp = PrefabUtility.InstantiatePrefab(circumference) as GameObject;

                break;
            case 3:
                temp = PrefabUtility.InstantiatePrefab(cross) as GameObject;

                break;

            default:
                break;

        }

        temp.transform.parent = _base.transform;
        temp.transform.localPosition = new Vector3(imagesPositions[count][0]/100, 0, imagesPositions[count][1]/100);


    }

    public int MaxValue(double[] array)
    {
        int temp = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > 0)
            {
                temp = i;
            }
        }
        if (temp < 0)
            return -1;
        return temp;

    }


}
