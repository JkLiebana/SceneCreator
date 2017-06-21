//C# Example

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet.Core;
using NeuronDotNet.Core.Initializers;


class Trainer : EditorWindow
{

    [MenuItem("Scene Creator/Trainer")]
    static void Init()
    {
        Trainer window = (Trainer)EditorWindow.GetWindow(typeof(Trainer));
        window.maxSize = new Vector2(620,620); 
        window.Show();
    }

    int selected = 0;
    public Texture2D[] pictures;
    private int neurons = 400, pictureWidth = 20, pictureHeight = 20;
    private const int outputNum = 4;
      
    private int epochs = 3000, hidden1Neurons = 21, hidden2Neurons = 20;
    private double learningRate = 0.05d;
     
    void OnGUI()
    {
        GUILayout.Label("Create a network", EditorStyles.largeLabel);

        GUILayout.Label("IMPORTANT! Only one network can be created at the same time", EditorStyles.boldLabel);

        neurons = EditorGUILayout.IntField("Neurons:", neurons);
        hidden1Neurons = EditorGUILayout.IntField("Hidden Neurons 1:", hidden1Neurons);
        hidden2Neurons = EditorGUILayout.IntField("Hidden Neurons 2:", hidden2Neurons);


        learningRate = EditorGUILayout.DoubleField("learningRate:", learningRate);
        epochs = EditorGUILayout.IntField("epochs:", epochs);



        Rect r_1 = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r_1, GUIContent.none))
            CreateNewNetwork();

        GUIStyle s = new GUIStyle();
        s.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Create Network", s);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

         
        GUILayout.Label("IMPORTANT! Size must be 20x20 px", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("pictures");

        EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
        so.ApplyModifiedProperties(); // Remember to apply modified properties

        string[] options = new string[]
        {
            "Triangle", "Square", "Circle", "Cross"
        };

        GUILayout.Label("Choose what are you adding", EditorStyles.helpBox);
        selected = EditorGUILayout.Popup("Label", selected, options);

        EditorGUILayout.Space();
        EditorGUILayout.Space();


        Rect r = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r, GUIContent.none))
            AddPicture();

        s.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Add", s);
        EditorGUILayout.EndHorizontal();

        Rect r3 = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r3, GUIContent.none))
            SaveTrainingSet();

        s.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Save", s);
        EditorGUILayout.EndHorizontal();

        Rect r4 = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r4, GUIContent.none))
            ReadTrainingSet();

        s.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Read", s);
        EditorGUILayout.EndHorizontal();


        Rect r5 = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r5, GUIContent.none))
            CreateTrainingSet();

        s.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Train network", s);
        EditorGUILayout.EndHorizontal();

        Rect r6 = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r6, GUIContent.none))
            CheckPerformance();

        s.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Check Performance", s);
        EditorGUILayout.EndHorizontal();
    }


    void AddPicture()
    {
        if(pictures.Length == 0)
        {
            Debug.LogError("A picture is needed!");
            return;
        }

        for(int i = 0; i < pictures.Length; i++)
        {
            if (pictures[i].width > pictureWidth || pictures[i].height > pictureHeight)
            {
                Debug.LogError("The size must be 20x20!");
                return;
            }
            NetworkManager.Instance.AddCase(pictures[i], selected);
        }
    }
    string s = "";
    void ShowTrainingSet()
    {

        foreach(double[] par in NetworkManager.Instance.trainingSetInputs)
        {
            s = "";
            //Now you can access the key and value both separately from this attachStat as:
            for(int i = 0; i<par.Length; i++)
            {
                s += par[i].ToString();
                if (i % pictureWidth == 0)
                    s += "\n";

            }
            Debug.Log(s);
        }
    }

    void SaveTrainingSet()
    {
        NetworkManager.Instance.SaveTrainingSet();

    }

    void ReadTrainingSet()
    {
        NetworkManager.Instance.ReadTrainingSet();

    }

    void CreateTrainingSet()
    {

        if(NetworkManager.Instance._neuralNetwork == null)
        {
            Debug.Log("You need to create a network first!");
            return;
        }

        if(NetworkManager.Instance.trainingSetInputs == null || NetworkManager.Instance.trainingSetInputs.Count == 0)
        {
            Debug.Log("You need to add training cases first!");
            return;
        }

        TrainingSet trainingSet = new TrainingSet(NetworkManager.Instance.neuronCount, outputNum);
        List<double[]> tempInputs = NetworkManager.Instance.trainingSetInputs;
        List<double> tempOutput = NetworkManager.Instance.trainingSetOutputs;

        for(int i = 0; i < tempInputs.Count; i++)
        {
            if(tempOutput[i] == 0)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { 1, -1, -1, -1 }));
            else if(tempOutput[i] == 1)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { -1, 1, -1, -1 }));
            else if (tempOutput[i] == 2)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { -1, -1, 1, -1 }));
            else if (tempOutput[i] == 3)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { -1, -1, -1, 1 }));
           


        }
        Debug.Log("All training cases added succesfully");
        NetworkManager.Instance.setTrainingSet(trainingSet);
        NetworkManager.Instance.TrainNetwork(epochs);
    }

    void CreateNewNetwork(){ 

        Debug.Log("Creating new network...");

        LinearLayer inputLayer = new LinearLayer(neurons);
        SigmoidLayer hiddenLayer = new SigmoidLayer(hidden1Neurons);
        SigmoidLayer hiddenLayer2 = new SigmoidLayer(hidden2Neurons);

        LinearLayer outputLayer = new LinearLayer(outputNum);  


        BackpropagationConnector conn1 = new BackpropagationConnector(inputLayer, hiddenLayer);
        conn1.Initializer = new RandomFunction(0d, 0.00001d);
        BackpropagationConnector conn3 = new BackpropagationConnector(hiddenLayer, hiddenLayer2);
        conn3.Initializer = new RandomFunction(0d, 0.00001d);
        BackpropagationConnector conn2 = new BackpropagationConnector(hiddenLayer2, outputLayer);
        conn2.Initializer = new RandomFunction(0d, 0.00001d);
        
        conn1.Initialize();
        conn2.Initialize();
        conn3.Initialize();


        if (NetworkManager.Instance._neuralNetwork != null)
        {
            Debug.Log("A network already exists... new network will overwrite it");
        }
        Debug.Log("Created.");

        NetworkManager.Instance._neuralNetwork = new BackpropagationNetwork(inputLayer, outputLayer);
        NetworkManager.Instance._neuralNetwork.SetLearningRate(learningRate);
        NetworkManager.Instance.setNeuralNetwork(NetworkManager.Instance._neuralNetwork);
         
        NetworkManager.Instance.InitializeNetwork(neurons);

    }

    List<Texture2D[]> testGroup;
    void CheckPerformance()
    {
        int index = -1;


        testGroup = new List<Texture2D[]>();       
        testGroup.Add(Resources.LoadAll<Texture2D>("Sprites/TrainingSet/Prueba"));
        Debug.Log("TestGroup Size: " + testGroup[0].Length);
        perforCounter = 0;
        for (int i = 0; i < testGroup[0].Length; i++)
        {

            name = testGroup[0][i].name.Substring(0, 3);
            switch (name)
            {
                case "tri":
                    index = 0;
                    break;

                case "cua":
                    index = 1;
                    break;

                case "cir":
                    index = 2;
                    break;

                case "eqx":
                    index = 3;
                    break;

            }
            CheckImage(testGroup[0][i], index);
        }

        Debug.Log("Performance result: " + perforCounter);
    }

    private int perforCounter = 0;
    void CheckImage(Texture2D image, int index)
    {

        double[] pxs = new double[neurons];
        Color[] pixels = image.GetPixels();
        for (int i = 0; i < image.width * image.height; i++)
        {
            pxs[i] = pixels[i].grayscale;
        }

        double[] output = NetworkManager.Instance._neuralNetwork.Run(pxs);
        int maxVal = MaxValue(output);
        if (maxVal == index)
        {
            perforCounter++;
        }

    }

    public int MaxValue(double[] array){
        int k = 0;
        int temp = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > 0)
            {
                temp = i;
                k++;
            }
        }

        return temp;

    }
}