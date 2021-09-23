using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;

public class GetInferenceFromModel : MonoBehaviour
{

    public Texture2D texture;
    
    public NNModel modelAsset;

    private Model _runtimeModel;

    private IWorker _engine;

    /// <summary>
    /// A struct used for holding the results of our prediction in a way that's easy for us to view from the inspector.
    /// </summary>
    [Serializable]
    public struct Prediction
    {
        // The most likely value for this prediction
        public int predictedValue;
        // The list of likelihoods for all the possible classes
        public float[] predicted;

        public void SetPrediction(Tensor t)
        {
            // Extract the float value outputs into the predicted array.
            predicted = t.AsFloats();
            // The most likely one is the predicted value.
            predictedValue = Array.IndexOf(predicted, predicted.Max());
            Debug.Log($"Predicted {predictedValue}");
        }
    }

    public Prediction prediction;
    
    // Start is called before the first frame update
    void Start()
    {
        // Set up the runtime model and worker.
        _runtimeModel = ModelLoader.Load(modelAsset);
        _engine = WorkerFactory.CreateWorker(_runtimeModel, WorkerFactory.Device.GPU);
        // Instantiate our prediction struct.
        prediction = new Prediction();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // making a tensor out of a grayscale texture
            var channelCount = 1; //grayscale, 3 = color, 4 = color+alpha
            // Create a tensor for input from the texture.
            var inputX = new Tensor(texture, channelCount);

            // Peek at the output tensor without copying it.
            Tensor outputY = _engine.Execute(inputX).PeekOutput();
            // Set the values of our prediction struct using our output tensor.
            prediction.SetPrediction(outputY);
            
            // Dispose of the input tensor manually (not garbage-collected).
            inputX.Dispose();
        }
    }

    private void OnDestroy()
    {
        // Dispose of the engine manually (not garbage-collected).
        _engine?.Dispose();
    }
}
