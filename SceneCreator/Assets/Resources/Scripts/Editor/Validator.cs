using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet.Core;
using NeuronDotNet.Core.Initializers;

public class Validator : EditorWindow {

    private const int outputNum = 4;
    private int neurons = 400;
    private double learningRate = 0.1d;
    private int epochs = 3000, hidden1Neurons = 50, hidden2Neurons = 50;


    [MenuItem("Scene Creator/Validator")]
    static void Init()
    {
        Validator window = (Validator)EditorWindow.GetWindow(typeof(Validator));
        window.maxSize = new Vector2(620, 620);
        window.Show();
    }

    void OnGUI(){
        GUILayout.Label("Cross Validation is done here", EditorStyles.helpBox);

        neurons = EditorGUILayout.IntField("Neurons:", neurons);
        hidden1Neurons = EditorGUILayout.IntField("Hidden 1 Neurons:", hidden1Neurons);
        hidden2Neurons = EditorGUILayout.IntField("Hidden 2 Neurons:", hidden2Neurons);
        learningRate = EditorGUILayout.DoubleField("Learning Rate:", learningRate);
        epochs = EditorGUILayout.IntField("Epochs:", epochs);


        if (GUILayout.Button("Start"))
            ValidateProcess();
    }

     
    List<double> performanceResults;

    void ValidateProcess(){

        performanceResults = new List<double>();

        for(int i = 0; i < 10; i++){
            CreateNewNetwork();
            CreateTrainingGroup(i);
            CreateTestGroup(i);
            CreateTrainingSet();
            TestPerformance();


            performanceResults.Add(perforCounter);
            Debug.Log("Performance of case " + i + ": " + perforCounter);
            Debug.Log("-------------------------");

            perforCounter = 0;

        }

        double total = (performanceResults[0] + performanceResults[1] + performanceResults[2] + performanceResults[3] + performanceResults[4] + performanceResults[5] + performanceResults[6] + performanceResults[7] + performanceResults[8] + performanceResults[9]) / 10;
        Debug.Log("Final performance: " + total);
    }


    private List<double[]> trainingSetInputs;
    private List<double> trainingSetOutputs;
    void CreateTrainingGroup(int i)
    {

        List<Texture2D[]> tempToAdd = new List<Texture2D[]>();
         
        for (int j = 0; j < 10; j++) {

            if (j == i)
                continue;

            tempToAdd.Add(Resources.LoadAll<Texture2D>("Sprites/TrainingSet/Validacion/G" + (j + 1)));

        }

        string name = "";

        trainingSetInputs = new List<double[]>();
        trainingSetOutputs = new List<double>();

        for (int x = 0; x < tempToAdd.Count; x++){
            for(int y = 0; y < tempToAdd[x].Length; y++){

                name = tempToAdd[x][y].name.Substring(0, 3);

                switch (name){

                    case "tri":
                        AddCase(tempToAdd[x][y], 0);
                        break;

                    case "cua":
                        AddCase(tempToAdd[x][y], 1);
                        break;

                    case "cir":
                        AddCase(tempToAdd[x][y], 2);
                        break;

                    case "eqx":
                        AddCase(tempToAdd[x][y], 3);
                        break;
                }
            }
        }
    }

    List<Texture2D[]> testGroup;
    void CreateTestGroup(int i){

        testGroup = new List<Texture2D[]>();

        for (int j = 0; j < 10; j++){

            if (j == i){
                testGroup.Add(Resources.LoadAll<Texture2D>("Sprites/TrainingSet/Validacion/G" + (j + 1)));
                break;
            }
        }
    }


    Dictionary<double[], int> temp;
    double[] inputs;
    public void AddCase(Texture2D picture, int selected)
    {

        if (temp == null)
        {
            temp = new Dictionary<double[], int>();
        }
        

        inputs = new double[picture.width * picture.height];

        Color[] pixels = picture.GetPixels();
        for (int i = 0; i < picture.width * picture.height; i++)
        {

            inputs[i] = pixels[i].grayscale;

        }
             
        trainingSetInputs.Add(inputs);
        trainingSetOutputs.Add(selected);
        
    }

    BackpropagationNetwork neuralNetwork;
    void CreateNewNetwork()
    {

        LinearLayer inputLayer = new LinearLayer(neurons);
        SigmoidLayer hiddenLayer = new SigmoidLayer(hidden1Neurons);
        SigmoidLayer hiddenLayer2 = new SigmoidLayer(hidden2Neurons);


        LinearLayer outputLayer = new LinearLayer(outputNum);


        BackpropagationConnector conn1 = new BackpropagationConnector(inputLayer, hiddenLayer);
        conn1.Initializer = new RandomFunction(0d, 0.001d);
        BackpropagationConnector conn3 = new BackpropagationConnector(hiddenLayer, hiddenLayer2);
        conn3.Initializer = new RandomFunction(0d, 0.001d);
        BackpropagationConnector conn2 = new BackpropagationConnector(hiddenLayer2, outputLayer);
        conn2.Initializer = new RandomFunction(0d, 0.001d);

        conn1.Initialize();
        conn2.Initialize();
        conn3.Initialize();


        neuralNetwork = new BackpropagationNetwork(inputLayer, outputLayer);
        neuralNetwork.SetLearningRate(learningRate);

        neuralNetwork.Initialize();
    }

    private TrainingSet trainingSet;
    void CreateTrainingSet()
    {
        if (trainingSetInputs == null || trainingSetInputs.Count == 0)
        {
            Debug.Log("You need to add training cases first!");
            return;
        }

        trainingSet = new TrainingSet(neurons, outputNum);
        List<double[]> tempInputs = trainingSetInputs;
        List<double> tempOutput = trainingSetOutputs;

        for (int i = 0; i < tempInputs.Count; i++)
        {
            if (tempOutput[i] == 0)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { 1, -1, -1, -1 }));
            else if (tempOutput[i] == 1)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { -1, 1, -1, -1 }));
            else if (tempOutput[i] == 2)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { -1, -1, 1, -1 }));
            else if (tempOutput[i] == 3)
                trainingSet.Add(new TrainingSample(tempInputs[i], new double[outputNum] { -1, -1, -1, 1 }));



        }

        neuralNetwork.Learn(this.trainingSet, epochs);
    }


    void TestPerformance()
    {
        int index = -1;

        for(int i = 0; i < testGroup[0].Length; i++)
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
    }


    private int perforCounter = 0;    
    void CheckImage(Texture2D image, int index){

        double[] pxs = new double[neurons];
        Color[] pixels = image.GetPixels();
        for (int i = 0; i < image.width * image.height; i++)
        {
            pxs[i] = pixels[i].grayscale;
        }
        
        double[] output = neuralNetwork.Run(pxs);
        int maxVal = MaxValue(output);
        if(maxVal == index)
        {
            perforCounter++;
        }

    }

    public int MaxValue(double[] array)
    {
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
