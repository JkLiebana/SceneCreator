//C# Example

using UnityEngine;
using UnityEditor;
using System.Collections;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet.Core;

class Checker : EditorWindow
{

    private int neurons = 400;

    [MenuItem("Scene Creator/Checker")]
    static void Init()
    {
        Checker window = (Checker)EditorWindow.GetWindow(typeof(Checker));
        window.maxSize = new Vector2(620, 620);
        window.Show();
    }

    private Texture2D image;

    void OnGUI()
    {
        GUILayout.Label("Test singles images", EditorStyles.helpBox);
        image = (Texture2D)EditorGUILayout.ObjectField("Image", image, typeof(Texture2D), false);

        Rect r_1 = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r_1, GUIContent.none))
            TestImage();

        GUIStyle s = new GUIStyle();
        s.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Test", s);
        EditorGUILayout.EndHorizontal();


    }


    void TestImage()
    {

        double[] pxs = new double[neurons];
        Color[] pixels = image.GetPixels();
        for (int i = 0; i < image.width * image.height; i++)
        {
            pxs[i] = pixels[i].grayscale;
        }

        NetworkManager.Instance._neuralNetwork.StopLearning();
        double[] output = NetworkManager.Instance._neuralNetwork.Run(pxs);
        Debug.Log("Neural Network results: ");
        Debug.Log("Nueron 1: " + output[0] + "/ Neuron 2: " + output[1] + "/ Neuron 3: " + output[2] + "/ Neuron 4: " + output[3]);

        switch (MaxValue(output)) {

            case 0:
                Debug.Log("TRIANGLE");
                break;
            case 1:
                Debug.Log("SQUARE");
                break;
            case 2:
                Debug.Log("CIRCLE");
                break;
            case 3:
                Debug.Log("CROSS");
                break;
            default:
                Debug.Log("UNDEFINED");
                break;

        }
    }

    public int MaxValue(double[] array)
    {
        int k = 0;
        int temp = 0;
        for(int i = 0; i < array.Length; i++)
        {
            if(array[i] > 0)
            {
                temp = i;
                k++;
            }
        }

        return temp;

    }

}