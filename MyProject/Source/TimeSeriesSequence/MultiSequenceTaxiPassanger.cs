﻿using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Network;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static TimeSeriesSequence.Entity.HelperClasses;

namespace TimeSeriesSequence
{
    public class MultiSequenceTaxiPassanger
    {

        string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"DataSet/");

        /// <summary>
        /// Prediction of taxi passangers based on data set
        /// </summary>
        public void RunPassangerTimeSeriesSequenceExperiment()
        {
            int inputBits = 88;
            int maxCycles = 50;
            int numColumns = 1024;

            //Read the taxi data set and write into new processed csv with reuired column
            var taxiData = HelperMethods.ProcessExistingDatafromCSVfile(path);

            //Encode the processed taxi passanger data
            List<Dictionary<string, int[]>> trainTaxiData = HelperMethods.EncodePassengerData(taxiData);

            EncoderBase encoder = HelperMethods.FetchDateTimeEncoder();

            Console.WriteLine("Starting to learn Taxi Passanger data");
            var learningExpResult = RunExperiment4(inputBits, maxCycles, numColumns, encoder, trainTaxiData);
            //var learningExpResult1 = RunExperiment1(inputBits, numColumns, encoder, trainTaxiData);
            CortexLayer<object, object> trainedCortexLayer = learningExpResult.Keys.ElementAt(0);
            HtmClassifier<string, ComputeCycle>  trainedClassifier = learningExpResult.Values.ElementAt(0);
            Console.WriteLine("Complete Learning");

            //Predict Data with User input
            Console.WriteLine("PLEASE ENTER DATE and TIME FOR PREDICTING TAXI PASSANGER:      *note format->dd/mm/yyyy hh:00");
            var userInputDateTime = Console.ReadLine();

            while (!userInputDateTime.Equals("e") && userInputDateTime != "E")
            {
                RunPassangerPrediction(userInputDateTime, trainedCortexLayer, trainedClassifier);

                Console.WriteLine("PLEASE ENTER DATE and TIME FOR PREDICTING TAXI PASSANGER:      *note format->dd/mm/yyyy hh:00");
                userInputDateTime = Console.ReadLine();
            }
        }

        /// <summary>
        /// Predict the no of passanger based on user Input Date time
        /// </summary>
        /// <param name="userInputDateTime"></param>
        /// <param name="trainedPassangerCortexLayer"></param>
        /// <param name="trainedPassangerClassifier"></param>
        private void RunPassangerPrediction(string? userInputDateTime, CortexLayer<object, object> trainedPassangerCortexLayer, HtmClassifier<string, ComputeCycle> trainedPassangerClassifier)
        {
            if (userInputDateTime != null)
            {
                DateTime userInput = DateTime.Parse(userInputDateTime);

                var sdr = HelperMethods.GetSDRofDateTime(userInput);
                var userLayerOutput = trainedPassangerCortexLayer.Compute(sdr, false) as ComputeCycle;
                var predictedValuesForUserInput = trainedPassangerClassifier.GetPredictedInputValues(userLayerOutput.PredictiveCells.ToArray(), 5);
                foreach (var predictedVal in predictedValuesForUserInput)
                {
                    Console.WriteLine("PASSANGER SIMILARITY " + predictedVal.Similarity + " PREDICTED PASSANGER NO :" + predictedVal.PredictedInput);
                }
            }

            else
                Console.WriteLine("Enter valid Date Time for Passanger Predicting");
        }

        //private HtmPredictionEngine RunExperiment1(int inputBits, int numColumns, EncoderBase encoder, Dictionary<string, List<double>> sequences)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    int maxMatchCnt = 0;

        //    var mem = new Connections(cfg);

        //    bool isInStableState = false;

        //    HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

        //    var numUniqueInputs = GetNumberOfInputs(sequences);

        //    CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

        //    TemporalMemory tm = new TemporalMemory();

        //    // For more information see following paper: https://www.scitepress.org/Papers/2021/103142/103142.pdf
        //    HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, numUniqueInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
        //    {
        //        if (isStable)
        //            // Event should be fired when entering the stable state.
        //            Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
        //        else
        //            // Ideal SP should never enter unstable state after stable state.
        //            Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

        //        // We are not learning in instable state.
        //        isInStableState = isStable;

        //        // Clear active and predictive cells.
        //        //tm.Reset(mem);
        //    }, numOfCyclesToWaitOnChange: 50);


        //    SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
        //    sp.Init(mem);
        //    tm.Init(mem);

        //    // Please note that we do not add here TM in the layer.
        //    // This is omitted for practical reasons, because we first eneter the newborn-stage of the algorithm
        //    // In this stage we want that SP get boosted and see all elements before we start learning with TM.
        //    // All would also work fine with TM in layer, but it would work much slower.
        //    // So, to improve the speed of experiment, we first ommit the TM and then after the newborn-stage we add it to the layer.
        //    layer1.HtmModules.Add("encoder", encoder);
        //    layer1.HtmModules.Add("sp", sp);

        //    //double[] inputs = inputValues.ToArray();
        //    int[] prevActiveCols = new int[0];

        //    int cycle = 0;
        //    int matches = 0;

        //    var lastPredictedValues = new List<string>(new string[] { "0" });

        //    int maxCycles = 50;

        //    //
        //    // Training SP to get stable. New-born stage.
        //    //

        //    for (int i = 0; i < maxCycles && isInStableState == false; i++)
        //    {
        //        matches = 0;

        //        cycle++;

        //        Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

        //        foreach (var inputs in sequences)
        //        {
        //            foreach (var input in inputs.Value)
        //            {
        //                Debug.WriteLine($" -- {inputs.Key} - {input} --");

        //                var lyrOut = layer1.Compute(input, true);

        //                if (isInStableState)
        //                    break;
        //            }

        //            if (isInStableState)
        //                break;
        //        }
        //    }

        //    // Clear all learned patterns in the classifier.
        //    cls.ClearState();

        //    // We activate here the Temporal Memory algorithm.
        //    layer1.HtmModules.Add("tm", tm);

        //    //
        //    // Loop over all sequences.
        //    foreach (var sequenceKeyPair in sequences)
        //    {
        //        Debug.WriteLine($"-------------- Sequences {sequenceKeyPair.Key} ---------------");

        //        int maxPrevInputs = sequenceKeyPair.Value.Count - 1;

        //        List<string> previousInputs = new List<string>();

        //        previousInputs.Add("-1.0");

        //        //
        //        // Now training with SP+TM. SP is pretrained on the given input pattern set.
        //        for (int i = 0; i < maxCycles; i++)
        //        {
        //            matches = 0;

        //            cycle++;

        //            Debug.WriteLine("");

        //            Debug.WriteLine($"-------------- Cycle {cycle} ---------------");
        //            Debug.WriteLine("");

        //            foreach (var input in sequenceKeyPair.Value)
        //            {
        //                Debug.WriteLine($"-------------- {input} ---------------");

        //                var lyrOut = layer1.Compute(input, true) as ComputeCycle;

        //                var activeColumns = layer1.GetResult("sp") as int[];

        //                previousInputs.Add(input.ToString());
        //                if (previousInputs.Count > (maxPrevInputs + 1))
        //                    previousInputs.RemoveAt(0);

        //                // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
        //                // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
        //                // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
        //                // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
        //                // memorized, it will match as the first one.
        //                if (previousInputs.Count < maxPrevInputs)
        //                    continue;

        //                string key = GetKey(previousInputs, input, sequenceKeyPair.Key);

        //                List<Cell> actCells;

        //                if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
        //                {
        //                    actCells = lyrOut.ActiveCells;
        //                }
        //                else
        //                {
        //                    actCells = lyrOut.WinnerCells;
        //                }

        //                cls.Learn(key, actCells.ToArray());

        //                Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
        //                Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

        //                //
        //                // If the list of predicted values from the previous step contains the currently presenting value,
        //                // we have a match.
        //                if (lastPredictedValues.Contains(key))
        //                {
        //                    matches++;
        //                    Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValues.FirstOrDefault(key)}.");
        //                }
        //                else
        //                    Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted values: {String.Join(',', lastPredictedValues)}");

        //                if (lyrOut.PredictiveCells.Count > 0)
        //                {
        //                    //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
        //                    var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

        //                    foreach (var item in predictedInputValues)
        //                    {
        //                        Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {item.PredictedInput} - {item.Similarity}");
        //                    }

        //                    lastPredictedValues = predictedInputValues.Select(v => v.PredictedInput).ToList();
        //                }
        //                else
        //                {
        //                    Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
        //                    lastPredictedValues = new List<string>();
        //                }
        //            }

        //            // The first element (a single element) in the sequence cannot be predicted
        //            double maxPossibleAccuraccy = (double)((double)sequenceKeyPair.Value.Count - 1) / (double)sequenceKeyPair.Value.Count * 100.0;

        //            double accuracy = (double)matches / (double)sequenceKeyPair.Value.Count * 100.0;

        //            Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {sequenceKeyPair.Value.Count}\t {accuracy}%");

        //            if (accuracy >= maxPossibleAccuraccy)
        //            {
        //                maxMatchCnt++;
        //                Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");

        //                //
        //                // Experiment is completed if we are 30 cycles long at the 100% accuracy.
        //                if (maxMatchCnt >= 30)
        //                {
        //                    sw.Stop();
        //                    Debug.WriteLine($"Sequence learned. The algorithm is in the stable state after 30 repeats with with accuracy {accuracy} of maximum possible {maxMatchCnt}. Elapsed sequence {sequenceKeyPair.Key} learning time: {sw.Elapsed}.");
        //                    break;
        //                }
        //            }
        //            else if (maxMatchCnt > 0)
        //            {
        //                Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with accuracy {accuracy}. This indicates instable state. Learning will be continued.");
        //                maxMatchCnt = 0;
        //            }

        //            // This resets the learned state, so the first element starts allways from the beginning.
        //            tm.Reset(mem);
        //        }
        //    }

        //    Debug.WriteLine("------------ END ------------");

        //    return new HtmPredictionEngine { Layer = layer1, Classifier = cls, Connections = mem };
        //}

        private static Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>> RunExperiment4(int inputBits, int maxCycles, int numColumns, EncoderBase encoder, List<Dictionary<string, int[]>> sequences)
        {
            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();
            Stopwatch sw = new Stopwatch();

            sequences = sequences.Take(1000).ToList();

            sw.Start();

            int maxMatchCnt = 0;

            var htmConfig = HelperMethods.FetchHTMConfig(inputBits, numColumns);

            var mem = new Connections(htmConfig);

            bool isInStableState = false;

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numUniqueInputs = sequences.Count;

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            TemporalMemory tm = new TemporalMemory();

            // For more information see following paper: https://www.scitepress.org/Papers/2021/103142/103142.pdf
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, numUniqueInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                isInStableState = isStable;

                // Clear active and predictive cells.
                //tm.Reset(mem);
            }, numOfCyclesToWaitOnChange: 50);


            SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
            sp.Init(mem);
            tm.Init(mem);

            // Please note that we do not add here TM in the layer.
            // This is omitted for practical reasons, because we first eneter the newborn-stage of the algorithm
            // In this stage we want that SP get boosted and see all elements before we start learning with TM.
            // All would also work fine with TM in layer, but it would work much slower.
            // So, to improve the speed of experiment, we first ommit the TM and then after the newborn-stage we add it to the layer.
            //layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            //double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];
            bool learn = true;
            int cycle = 0;
            int matches = 0;

            var lastPredictedValues = new List<string>(new string[] { "0" });

            // Training SP to get stable. New-born stage.

            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var sequence in sequences)
                {
                    foreach (var element in sequence)
                    {
                        var observationClass = element.Key; // OBSERVATION LABEL || SEQUENCE LABEL
                        var elementSDR = element.Value; // ALL ELEMENT IN ONE SEQUENCE

                        Console.WriteLine($"-------------- {observationClass} ---------------");

                        var lyrOut = layer1.Compute(elementSDR, true);     /* CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = TRUE */
                        //var lyrOut = layer1.Compute(elementSDR, learn);    /* CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = if TRUE */

                        if (isInStableState)
                            break;
                    }

                    if (isInStableState)
                        break;
                }
            }

            // Clear all learned patterns in the classifier.
            cls.ClearState();

            // ADDING TEMPORAL MEMEORY to CORTEX LAYER
            layer1.HtmModules.Add("tm", tm);

            string lastPredictedValue = "-1";
            List<string> lastPredictedValueList = new List<string>();
            double lastCycleAccuracy = 0;
            double accuracy = 0;

            List<List<string>> possibleSequence = new List<List<string>>();
            // TRAINING SP+TM TOGETHER
            foreach (var sequence in sequences)  // SEQUENCE LOOP
            {
                int SequencesMatchCount = 0; // NUMBER OF MATCHES
                var tempLOGFILE = new Dictionary<int, string>();
                var tempLOGGRAPH = new Dictionary<int, double>();
                double SaturatedAccuracyCount = 0;


                for (int i = 0; i < maxCycles; i++) // MAXCYCLE LOOP 
                {
                    //var ElementWisePrediction = new List<List<HtmClassifier<string, ComputeCycle>.ClassifierResult>>();
                    List<string> ElementWiseClasses = new List<string>();

                    // ELEMENT IN SEQUENCE MATCHES COUNT
                    int ElementMatches = 0;

                    foreach (var Elements in sequence) // SEQUENCE DICTIONARY LOOP
                    {

                        // OBSERVATION LABEl
                        var observationLabel = Elements.Key;
                        // ELEMENT SDR LIST FOR A SINGLE SEQUENCE
                        var ElementSdr = Elements.Value;

                        List<Cell> actCells = new List<Cell>();
                        var lyrOut = new ComputeCycle();

                        lyrOut = layer1.Compute(ElementSdr, learn) as ComputeCycle;
                        Debug.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));

                        // Active Cells
                        actCells = (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count) ? lyrOut.ActiveCells : lyrOut.WinnerCells;

                        cls.Learn(observationLabel, actCells.ToArray());


                        // CLASS VOTING IS USED FOR SEQUENCE CLASSIFICATION EXPERIMENT i.e CANCER SEQUENCE CLASSIFICATION EXPERIMENT
                       
                            if (lastPredictedValueList.Contains(observationLabel))
                            {
                                ElementMatches++;
                                lastPredictedValueList.Clear();
                                Debug.WriteLine($"Match. Actual value: {observationLabel} - Predicted value: {lastPredictedValue}");
                            }
                            else
                            {
                                Debug.WriteLine($"Mismatch! Actual value: {observationLabel} - Predicted values: {lastPredictedValue}");
                            }
                        

                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        if (learn == false)
                            Debug.WriteLine($"Inference mode");


                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            Debug.WriteLine($"Current Input: {observationLabel}");
                            Debug.WriteLine("The predictions with similarity greater than 50% are");

                            foreach (var t in predictedInputValue)
                            {


                                if (t.Similarity >= (double)50.00)
                                {
                                    Debug.WriteLine($"Predicted Input: {string.Join(", ", t.PredictedInput)},\tSimilarity Percentage: {string.Join(", ", t.Similarity)}, \tNumber of Same Bits: {string.Join(", ", t.NumOfSameBits)}");
                                }

                            }

                        }

                    }

                    accuracy = ((double)ElementMatches / (sequence.Count)) * 100;
                    Debug.WriteLine($"Cycle : {i} \t Accuracy:{accuracy}");
                    tempLOGGRAPH.Add(i, accuracy);
                    if (accuracy == 100)
                    {
                        SequencesMatchCount++;
                        if (SequencesMatchCount >= 30)
                        {
                            tempLOGFILE.Add(i, $"Cycle : {i} \t  Accuracy:{accuracy} \t Number of times repeated {SequencesMatchCount}");
                            break;
                        }
                        tempLOGFILE.Add(i, $"Cycle : {i} \t  Accuracy:{accuracy} \t Number of times repeated {SequencesMatchCount}");

                    }

                    else if (lastCycleAccuracy == accuracy && accuracy != 0)
                    {
                        SaturatedAccuracyCount++;
                        if (SaturatedAccuracyCount >= 20 && lastCycleAccuracy > 70)
                        {
                            Debug.WriteLine($"NO FURTHER ACCURACY CAN BE ACHIEVED");
                            Debug.WriteLine($"Saturated Accuracy : {lastCycleAccuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                            tempLOGFILE.Add(i, $"Cycle: { i} \t Accuracy:{accuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                            break;
                        }
                        else
                        {
                            tempLOGFILE.Add(i, $"Cycle: { i} \t Saturated Accuracy : {lastCycleAccuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                        }
                    }
                    else
                    {
                        SaturatedAccuracyCount = 0;
                        SequencesMatchCount = 0;
                        lastCycleAccuracy = accuracy;
                        tempLOGFILE.Add(i, $"cycle : {i} \t Accuracy :{accuracy} \t ");
                    }
                    lastPredictedValueList.Clear();
                }
                tm.Reset(mem);
                learn = true;
                OUTPUT_LOG_LIST.Add(tempLOGFILE);
            }

            sw.Stop();

            //****************DISPLAY STATUS OF EXPERIMENT
            Debug.WriteLine("-------------------TRAINING END------------------------");
            Console.WriteLine("-----------------TRAINING END------------------------");
            Debug.WriteLine("-------------------WRTING TRAINING OUTPUT LOGS---------------------");
            Console.WriteLine("-------------------WRTING TRAINING OUTPUT LOGS------------------------");
            //*****************


            DateTime now = DateTime.Now;
            string filename = now.ToString("g"); //

            filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"TrainingLogs\");

            //filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            // string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"TrainingLogs/{filename}");

            using (StreamWriter swOutput = File.CreateText(path + filename))
            {
                swOutput.WriteLine($"{filename}");
                foreach (var SequencelogCycle in OUTPUT_LOG_LIST)
                {
                    swOutput.WriteLine("******Sequence Starting*****");
                    foreach (var cycleOutPutLog in SequencelogCycle)
                    {
                        swOutput.WriteLine(cycleOutPutLog.Value, true);
                    }
                    swOutput.WriteLine("****Sequence Ending*****");

                }
            }

            Debug.WriteLine("-------------------TRAINING LOGS HAS BEEN CREATED---------------------");
            Console.WriteLine("-------------------TRAINING LOGS HAS BEEN CREATED------------------------");

            var returnDictionary = new Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>>();
            returnDictionary.Add(layer1, cls);
            return returnDictionary;

            // return new HtmPredictionEngine { Layer = layer1, Classifier = cls, Connections = mem };

        }

        private static Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>> RunExperiment(int inputBits, int maxCycles, int numColumns, EncoderBase encoder, List<Dictionary<string, int[]>> trainTaxiData)
        {
            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();
            Stopwatch sw = new Stopwatch();

            trainTaxiData = trainTaxiData.Take(1000).ToList();

            sw.Start();

            int maxMatchCnt = 0;

            var htmConfig = HelperMethods.FetchHTMConfig(inputBits, numColumns);

            var mem = new Connections(htmConfig);

            bool isInStableState = false;

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numUniqueInputs = trainTaxiData.Count;

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            TemporalMemory tm = new TemporalMemory();

            // For more information see following paper: https://www.scitepress.org/Papers/2021/103142/103142.pdf
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, numUniqueInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                isInStableState = isStable;

                // Clear active and predictive cells.
                //tm.Reset(mem);
            }, numOfCyclesToWaitOnChange: 50);


            SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
            sp.Init(mem);
            tm.Init(mem);

            // Please note that we do not add here TM in the layer.
            // This is omitted for practical reasons, because we first eneter the newborn-stage of the algorithm
            // In this stage we want that SP get boosted and see all elements before we start learning with TM.
            // All would also work fine with TM in layer, but it would work much slower.
            // So, to improve the speed of experiment, we first ommit the TM and then after the newborn-stage we add it to the layer.
            //layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            //double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];
            bool learn = true;
            int cycle = 0;
            int matches = 0;

            var lastPredictedValues = new List<string>(new string[] { "0" });

            // Training SP to get stable. New-born stage.

            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var sequence in trainTaxiData)
                {
                    foreach (var element in sequence)
                    {
                        var observationClass = element.Key; // OBSERVATION LABEL || SEQUENCE LABEL
                        var elementSDR = element.Value; // ALL ELEMENT IN ONE SEQUENCE

                        Console.WriteLine($"-------------- {observationClass} ---------------");

                        var lyrOut = layer1.Compute(elementSDR, true);     /* CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = TRUE */
                        //var lyrOut = layer1.Compute(elementSDR, learn);    /* CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = if TRUE */

                        if (isInStableState)
                            break;
                    }

                    if (isInStableState)
                        break;
                }
            }

            // Clear all learned patterns in the classifier.
            cls.ClearState();

            // We activate here the Temporal Memory algorithm.
            layer1.HtmModules.Add("tm", tm);

            string lastPredictedValue = "-1";
            List<string> lastPredictedValueList = new List<string>();
            double lastCycleAccuracy = 0;
            double accuracy = 0;

            List<List<string>> possibleSequence = new List<List<string>>();

            //
            // Loop over all sequences.
            foreach (var sequence in trainTaxiData)
            {
                int SequencesMatchCount = 0; // NUMBER OF MATCHES
                var tempLOGFILE = new Dictionary<int, string>();
                var tempLOGGRAPH = new Dictionary<int, double>();
                double SaturatedAccuracyCount = 0;

                for (int i = 0; i < maxCycles; i++)
                {
                    List<string> ElementWiseClasses = new List<string>();

                    int elementMatches = 0;

                    foreach (var Elements in sequence)
                    {
                        var observationLabel = Elements.Key;

                        var lyrOut = new ComputeCycle();

                        lyrOut = layer1.Compute(Elements.Value, learn) as ComputeCycle;
                        Debug.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));

                        List<Cell> actCells = (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count) ? lyrOut.ActiveCells : lyrOut.WinnerCells;

                        cls.Learn(observationLabel, actCells.ToArray());

                        if (lastPredictedValues.Contains(observationLabel))
                        {
                            elementMatches++;
                            Debug.WriteLine($"Match. Actual value: {observationLabel} - Predicted value: {lastPredictedValues.FirstOrDefault(observationLabel)}.");
                        }
                        else
                            Debug.WriteLine($"Missmatch! Actual value: {observationLabel} - Predicted values: {String.Join(',', lastPredictedValues)}");


                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        if (learn == false)
                            Debug.WriteLine($"Inference mode");

                        //if (lyrOut.PredictiveCells.Count > 0)
                        //{
                        //    var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                        //    Debug.WriteLine($"Current Input: {observationLabel}");
                        //    Debug.WriteLine("The predictions with similarity greater than 50% are");

                        //    foreach (var t in predictedInputValue)
                        //    {

                        //        if (t.Similarity >= (double)50.00)
                        //        {
                        //            Debug.WriteLine($"Predicted Input: {string.Join(", ", t.PredictedInput)},\tSimilarity Percentage: {string.Join(", ", t.Similarity)}, \tNumber of Same Bits: {string.Join(", ", t.NumOfSameBits)}");
                        //        }
                        //    }

                        //    lastPredictedValue = predictedInputValue.First().PredictedInput;

                        //}
                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                            var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var item in predictedInputValues)
                            {
                                Debug.WriteLine($"Current Input: {Elements} \t| Predicted Input: {item.PredictedInput} - {item.Similarity}");
                            }

                            lastPredictedValues = predictedInputValues.Select(v => v.PredictedInput).ToList();
                        }
                        else
                        {
                            Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                            lastPredictedValues = new List<string>();
                        }
                    }

                    accuracy = ((double)elementMatches / (sequence.Count)) * 100;
                    Debug.WriteLine($"Cycle : {i} \t Accuracy:{accuracy}");
                    tempLOGGRAPH.Add(i, accuracy);
                    if (accuracy >= 100)
                    {
                        SequencesMatchCount++;
                        if (SequencesMatchCount >= 30)
                        {
                            tempLOGFILE.Add(i, $"Cycle : {i} \t  Accuracy:{accuracy} \t Number of times repeated {SequencesMatchCount}");
                            break;
                        }
                        tempLOGFILE.Add(i, $"Cycle : {i} \t  Accuracy:{accuracy} \t Number of times repeated {SequencesMatchCount}");

                    }
                    else if (lastCycleAccuracy == accuracy && accuracy != 0)
                    {
                        SaturatedAccuracyCount++;
                        if (SaturatedAccuracyCount >= 20 && lastCycleAccuracy > 70)
                        {
                            Debug.WriteLine($"NO FURTHER ACCURACY CAN BE ACHIEVED");
                            Debug.WriteLine($"Saturated Accuracy : {lastCycleAccuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                            tempLOGFILE.Add(i, $"Cycle: { i} \t Accuracy:{accuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                            break;
                        }
                        else
                        {
                            tempLOGFILE.Add(i, $"Cycle: { i} \t Saturated Accuracy : {lastCycleAccuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                        }
                    }
                    else
                    {
                        SaturatedAccuracyCount = 0;
                        SequencesMatchCount = 0;
                        lastCycleAccuracy = accuracy;
                        tempLOGFILE.Add(i, $"cycle : {i} \t Accuracy :{accuracy} \t ");
                    }
                    lastPredictedValueList.Clear();

                }

                tm.Reset(mem);
                learn = true;
                OUTPUT_LOG_LIST.Add(tempLOGFILE);

            }


            sw.Stop();

            //****************DISPLAY STATUS OF EXPERIMENT
            Debug.WriteLine("-------------------TRAINING END------------------------");
            Console.WriteLine("-----------------TRAINING END------------------------");
            Debug.WriteLine("-------------------WRTING TRAINING OUTPUT LOGS---------------------");
            Console.WriteLine("-------------------WRTING TRAINING OUTPUT LOGS------------------------");
            //*****************

            DateTime now = DateTime.Now;
            string filename = now.ToString("g"); //

            filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"TrainingLogs\");

            //filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            // string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"TrainingLogs/{filename}");

            using (StreamWriter swOutput = File.CreateText(path + filename))
            {
                swOutput.WriteLine($"{filename}");
                foreach (var SequencelogCycle in OUTPUT_LOG_LIST)
                {
                    swOutput.WriteLine("******Sequence Starting*****");
                    foreach (var cycleOutPutLog in SequencelogCycle)
                    {
                        swOutput.WriteLine(cycleOutPutLog.Value, true);
                    }
                    swOutput.WriteLine("****Sequence Ending*****");

                }
            }

            Debug.WriteLine("-------------------TRAINING LOGS HAS BEEN CREATED---------------------");
            Console.WriteLine("-------------------TRAINING LOGS HAS BEEN CREATED------------------------");

            var returnDictionary = new Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>>();
            returnDictionary.Add(layer1, cls);

            return returnDictionary;

            // return new HtmPredictionEngine { Layer = layer1, Classifier = cls, Connections = mem };

        }


        public class HtmPredictionEngine
        {
            public void Reset()
            {
                var tm = this.Layer.HtmModules.FirstOrDefault(m => m.Value is TemporalMemory);
                ((TemporalMemory)tm.Value).Reset(this.Connections);
            }
            public List<ClassifierResult<string>> Predict(double input)
            {
                var lyrOut = this.Layer.Compute(input, false) as ComputeCycle;

                List<ClassifierResult<string>> predictedInputValues = this.Classifier.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                return predictedInputValues;
            }

            public Connections Connections { get; set; }

            public CortexLayer<object, object> Layer { get; set; }

            public HtmClassifier<string, ComputeCycle> Classifier { get; set; }
        }
        /// <summary>
        /// Read the datas from taxi data set and process it
        /// </summary>

        /// <summary>
        /// Gets the number of all unique inputs.
        /// </summary>
        /// <param name="sequences">Alle sequences.</param>
        /// <returns></returns>
        private static int GetNumberOfInputs(Dictionary<string, List<double>> sequences)
        {
            int num = 0;

            foreach (var inputs in sequences)
            {
                //num += inputs.Value.Distinct().Count();
                num += inputs.Value.Count;
            }

            return num;
        }

        /// <summary>
        /// Constracts the unique key of the element of an sequece. This key is used as input for HtmClassifier.
        /// It makes sure that alle elements that belong to the same sequence are prefixed with the sequence.
        /// The prediction code can then extract the sequence prefix to the predicted element.
        /// </summary>
        /// <param name="prevInputs"></param>
        /// <param name="input"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static string GetKey(List<string> prevInputs, double input, string sequence)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += (prevInputs[i]);
            }

            return $"{sequence}_{key}";
        }
    }
}