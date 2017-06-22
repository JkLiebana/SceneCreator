using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet.Core;
using System.Text;
using System.IO;
using System;

public class NetworkManager : Singleton<NetworkManager> {

    public BackpropagationNetwork _neuralNetwork;
    public List<double[]> trainingSetInputs;
    public List<double> trainingSetOutputs;
    public TrainingSet _trainingSet;
    private int neurons = 400;

    public int neuronCount;

    public void InitializeNetwork(int n)
    {
        neuronCount = n;
        _neuralNetwork.Initialize();
        
    }


    public void setNeuralNetwork(BackpropagationNetwork network)
    {
        this._neuralNetwork = network;
    }

    public BackpropagationNetwork getNeuralNetwork()
    {
        return this._neuralNetwork;
    }

    public void setTrainingSet(TrainingSet trainingSet)
    {
        this._trainingSet = trainingSet;
    }

    public TrainingSet getTrainingSet()
    {
        return this._trainingSet;
    }


    public void SaveNetwork()
    {

    }

    Dictionary<double[], int> temp;
    double[] inputs;
    bool added = false;
    public void AddCase(Texture2D picture, int selected) {

        if (temp == null) {
            temp = new Dictionary<double[], int>();
        }
        if (trainingSetInputs == null)
        {
            trainingSetInputs = new List<double[]>();
            trainingSetOutputs = new List<double>();
        }

        inputs = new double[picture.width * picture.height];

        added = false;
        Color[] pixels = picture.GetPixels();
        for (int i = 0; i < picture.width * picture.height ; i++) {
            
            inputs[i] = pixels[i].grayscale;

        }

        foreach(double[] x in trainingSetInputs)
        {
            if(AreEquals(x, inputs))
            {
                added = true;
            }
        }

        if (!added)
        {
            trainingSetInputs.Add(inputs);
            trainingSetOutputs.Add(selected);
            Debug.Log("---" + picture.name + " added.");
        }else {
            Debug.Log("---" + picture.name + " was already added.");
        }
    }


    List<string> rowDataTemp = new List<string>();
    private List<string[]> rowData = new List<string[]>();
    private StringBuilder sb;
    private StreamWriter outStream;
    public void SaveTrainingSet()
    {
        Debug.Log("Saving...");
        rowData.Clear();
        // You can add up the values in as many cells as you want.
        for (int i = 0; i < trainingSetInputs.Count; i++)
        {
            rowDataTemp.Clear();
           

            rowDataTemp.Add(trainingSetOutputs[i].ToString());
            for(int j = 0; j < trainingSetInputs[i].Length; j++)
            {
                if(trainingSetInputs[i][j] == 0)
                {
                    rowDataTemp.Add(j.ToString());
                }
            }
            rowData.Add(rowDataTemp.ToArray());
        }

        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        sb = new StringBuilder();

        for (int index = 0; index < length; index++)
        {
            sb.AppendLine(string.Join(delimiter, output[index]));
        }


        string filePath = getPath("trainer");

        outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();

        Debug.Log("Saving complete!");


    }


    string fileData;
    string[] lines, lineData;
    double[] tempFromTxt;
    int c;

    public void ReadTrainingSet()
    {
        Debug.Log("Reading...");
        if (trainingSetInputs == null)
            trainingSetInputs = new List<double[]>();
        if (trainingSetOutputs == null)
            trainingSetOutputs = new List<double>();

        trainingSetInputs.Clear();
        trainingSetOutputs.Clear(); 

        fileData = System.IO.File.ReadAllText(getPath("trainer"));
        lines  = fileData.Split("\n"[0]);

        for(int i = 0; i < lines.Length; i++)
        {
            tempFromTxt = new double[neurons];
            lineData = (lines[i].Trim()).Split(","[0]);
            if (lineData[0] == "")
                continue;

            int.TryParse(lineData[0], out c);
            
            trainingSetOutputs.Add(c);
            
            for(int j = 0; j < neurons; j++)
            {
                tempFromTxt[j] = 1;
            }
            for(int j = 0; j < lineData.Length; j++)
            {
                int.TryParse(lineData[j], out c);
                tempFromTxt[c] = 0;
            }
            trainingSetInputs.Add(tempFromTxt);

         
        }
        Debug.Log("Ready.");

    }

    private string getPath(string file)
    {
        switch (file)
        {
            case "trainer":
                return Application.dataPath + "/Data/" + "Trainer_Saved_data.txt";
            default:
                return "";
        }
    }


    bool areEquals;
    bool AreEquals(double[] x, double[] y)
    {

        areEquals = true;        
        for (int i = 0; i < y.Length; i++)
        {
            if(x[i] != y[i])
            {
                return false;
            }
     
        }
        return true;

    }

    public void TrainNetwork(int epochs)
    {
        Debug.Log("previous MSE: " + NetworkManager.Instance._neuralNetwork.MeanSquaredError);

        NetworkManager.Instance._neuralNetwork.Learn(this._trainingSet, epochs);
         
        Debug.Log("actual MSE: " + NetworkManager.Instance._neuralNetwork.MeanSquaredError);

    }
}
