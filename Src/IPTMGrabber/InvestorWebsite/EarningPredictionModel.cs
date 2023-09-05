using IPTMGrabber.Utils;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace IPTMGrabber.InvestorWebsite
{
    public class EarningPredictionModel
    {
        private const string PredictionModelName = "EarningPrediction.zip";
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
        public void TrainModel(string dataroot, string modelFolder)
        {
            var mlContext = new MLContext();

            // Load data, then split between training and test data
            var data = mlContext.Data.LoadFromEnumerable(GetTrainingData(dataroot));
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
            mlContext.Model.Save(model, dataSplit.TrainSet.Schema, Path.Combine(modelFolder, PredictionModelName));
        }

        public int PredictEarning(string text)
        {
            if (_predictionEngine == null)
            {
                var mlContext = new MLContext();

                // Charger le modèle à partir du fichier
                var loadedModel = mlContext.Model.Load(GetType().Assembly.GetManifestResourceStream($"IPTMGrabber.MachineLearning.{PredictionModelName}"), out var modelSchema);

                // Créer le moteur de prédiction
                _predictionEngine = mlContext.Model.CreatePredictionEngine<EventInfoInfo, EventInfoInfoPrediction>(loadedModel);
            }

            var prediction = _predictionEngine.Predict(new EventInfoInfo {Description = text});

            return prediction.Category;
        }



        private IEnumerable<EventInfoInfo> GetTrainingData(string dataroot)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(dataroot, "NewsEvents", "News"), "*"))
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
