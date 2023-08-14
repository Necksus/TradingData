using CefSharp;
using IPTMGrabber.Utils;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System.Data;

namespace IPTMGrabber.InvestorWebsite
{
    internal class EarningPredictionModel
    {
        private readonly string _dataRoot;
        private PredictionEngine<EventInfoInfo, EventInfoInfoPrediction> _predictionEngine;

        // Définition de la classe de données
        public class EventInfoInfo
        {
            [LoadColumn(0)]
            public string Description { get; set; }

            [LoadColumn(1)]
            [ColumnName("Label")]
            public int EarningRelated { get; set; }
        }

        // Définition de la classe de prédiction
        public class EventInfoInfoPrediction
        {
            [ColumnName("PredictedLabel")]
            public int Category;
        }
        public void TrainModel()
        {
            var mlContext = new MLContext();

            // Load data, then split between training and test data
            var data = mlContext.Data.LoadFromEnumerable(GetTrainingData());
            var dataSplit = mlContext.Data.TrainTestSplit(data);

            // Create learning pipeline
            var pipeline = mlContext.Transforms.Text
                .FeaturizeText(inputColumnName: "Description", outputColumnName: "Description")
                .Append(mlContext.Transforms.Concatenate("Features", "Description"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: "Label"))
//                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())// .SdcaNonCalibrated())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));


            // Train the model
            var model = pipeline.Fit(dataSplit.TrainSet);

            // Evaluate the model
            var predictions = model.Transform(dataSplit.TestSet);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions);
            Console.WriteLine($"Metrics - Log-loss: {metrics.LogLoss}");

            // Save the model
            mlContext.Model.Save(model, dataSplit.TrainSet.Schema, TrainingModel);
        }

        public int PredictEarning(string text)
        {
            if (_predictionEngine == null)
            {
                var mlContext = new MLContext();

                // Charger le modèle à partir du fichier
                var loadedModel = mlContext.Model.Load(TrainingModel, out var modelSchema);

                // Créer le moteur de prédiction
                _predictionEngine = mlContext.Model.CreatePredictionEngine<EventInfoInfo, EventInfoInfoPrediction>(loadedModel);
            }

            var prediction = _predictionEngine.Predict(new EventInfoInfo {Description = text});

            return prediction.Category;
        }

        public string TrainingModel 
            => Path.Combine(_dataRoot, "NewsEvents", "EarningPrediction.zip");

        public EarningPredictionModel(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        private IEnumerable<EventInfoInfo> GetTrainingData()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(_dataRoot, "NewsEvents", "News"), "*"))
            {
                foreach (var eventInfo in Enumerators.EnumerateFromCsv<EventInfo>(file))
                {
                    yield return new EventInfoInfo
                    {
                        Description = eventInfo.Description,
                        EarningRelated = eventInfo.EarningRelated!.Value
                    };
                }
            }
        }
    }
}
