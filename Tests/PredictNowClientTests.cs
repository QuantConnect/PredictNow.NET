/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using NUnit.Framework;
using QuantConnect.Configuration;
using QuantConnect.PredictNowNET.Models;

namespace QuantConnect.PredictNowNET.Test;

public class PredictNowClientTests
{
    private PredictNowClient _client;
    private PortfolioParameters _portfolioParameters;
    private string _trainId = "009a02eb-7199-464a-888d-4c0245785d79"; 

    [SetUp]
    public void Setup()
    {
        Config.Set("predict-now-cai-url", Environment.GetEnvironmentVariable("PREDICTNOW-CAI-URL"));
        Config.Set("predict-now-cpo-url", Environment.GetEnvironmentVariable("PREDICTNOW-CPO-URL"));
        Config.Set("predict-now-users-url", Environment.GetEnvironmentVariable("PREDICTNOW-USERS-URL"));
        var email = Environment.GetEnvironmentVariable("PREDICTNOW-USER-EMAIL");
        var userName = Environment.GetEnvironmentVariable("PREDICTNOW-USER-NAME");
        _client = new PredictNowClient(email, userName);
        _portfolioParameters = new PortfolioParameters("Demo_Project_20231211", "ETF_return.csv", "ETF_constrain.csv", 1.0, "month", 1, "first", 3, "sharpe");
    }

    [Test, Order(1)]
    public void ConnectSuccessfully() => Assert.That(_client.Connected, Is.True);

    [Test, Order(2)]
    public void UploadReturnsFileSuccessfully()
    {
        var filename = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ETF_return.csv");
        var files = _client.UploadReturnsFile(filename);
        Assert.That(files, Is.Not.Null);
        Console.WriteLine($"UploadReturnsFileSuccessfully: {files}");
    }

    [Test, Order(2)]
    public void UploadConstraintFileSuccessfully()
    {
        var filename = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ETF_constrain.csv");
        var files = _client.UploadConstraintFile(filename);
        Assert.That(files, Is.Not.Null);
        Console.WriteLine($"UploadConstraintFileSuccessfully: {files}");
    }

    [Test, Order(2)]
    public void UploadFeaturesFileSuccessfully()
    {
        var filename = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Random_Feature.csv");
        var files = _client.UploadFeaturesFile(filename);
        Assert.That(files, Is.Not.Null);
        Console.WriteLine($"UploadFeaturesFileSuccessfully: {files}");
    }

    [Test, Order(3)]
    public void ListReturnsFilesSuccessfully()
    {
        var files = _client.ListReturnsFiles();
        Assert.That(files, Is.Not.Null);
        Assert.That(files, Does.Contain("ETF_return.csv"));
        Assert.That(files, Does.Not.Contain("ETF_constrain.csv"));
        Assert.That(files, Does.Not.Contain("Random_Feature.csv"));
        Console.WriteLine($"ListReturnsFilesSuccessfully: {string.Join(",", files)}");
    }

    [Test, Order(3)]
    public void ListConstraintFilesSuccessfully()
    {
        var files = _client.ListConstraintFiles();
        Assert.That(files, Is.Not.Null);
        Assert.That(files, Does.Contain("ETF_constrain.csv"));
        Assert.That(files, Does.Not.Contain("ETF_return.csv"));
        Assert.That(files, Does.Not.Contain("Random_Feature.csv"));
        Console.WriteLine($"ListReturnsFilesSuccessfully: {string.Join(",", files)}");
    }

    [Test, Order(3)]
    public void ListFeaturesFilesSuccessfully()
    {
        var files = _client.ListFeaturesFiles();
        Assert.That(files, Is.Not.Null);
        Assert.That(files, Does.Contain("Random_Feature.csv"));
        Assert.That(files, Does.Not.Contain("ETF_return.csv"));
        Assert.That(files, Does.Not.Contain("ETF_constrain.csv"));
        Console.WriteLine($"ListReturnsFilesSuccessfully: {string.Join(",", files)}");
    }

    [Test, Order(4)]
    public void RunInSampleBacktestSuccessfully()
    {
        var jobCreationResult = _client.RunInSampleBacktest(_portfolioParameters, new DateTime(2019, 01, 01), new DateTime(2019, 12, 31), 0.3, "debug");
        Assert.That(jobCreationResult.Message, Is.EqualTo("job submitted for cpo in-sample backtesting."));
        Assert.That(jobCreationResult.Id.Length, Is.GreaterThan(0));
        Console.WriteLine($"RunInSampleBacktestSuccessfully: {jobCreationResult.Id}");
        GetJobForId(jobCreationResult.Id);
    }

    [Test, Order(5)]
    public void RunOutOfSampleBacktestSuccessfully()
    {
        var jobCreationResult = _client.RunOutOfSampleBacktest(_portfolioParameters, new DateTime(2019, 01, 01), new DateTime(2019, 12, 31), "debug");
        Assert.That(jobCreationResult.Message, Is.EqualTo("job submitted for cpo back-testing."));
        Assert.That(jobCreationResult.Id.Length, Is.GreaterThan(0));
        Console.WriteLine($"RunOutOfSampleBacktestSuccessfully: {jobCreationResult.Id}");
        GetJobForId(jobCreationResult.Id);
    }

    [Test, Order(6)]
    public void GetBacktestWeightsSuccessfully()
    {
        var weightsByDate = _client.GetBacktestWeights(_portfolioParameters, new DateTime(2019, 01, 01), new DateTime(2019, 12, 31), "debug");
        Assert.That(weightsByDate, Is.Not.Empty);
    }

    [Test, Order(6)]
    public void GetBacktestPerformanceSuccessfully()
    {
        var performance = _client.GetBacktestPerformance(_portfolioParameters, new DateTime(2019, 01, 01), new DateTime(2019, 12, 31), debug:"debug");
        Assert.That(performance, Is.Not.Null);
    }

    [Test, Order(7)]
    public void RunLivePreditionSuccessfully()
    {
        var jobCreationResult = _client.RunLivePrediction(_portfolioParameters, new DateTime(2022, 07, 01), new DateTime(2022, 08, 01), debug: "debug");
        Assert.That(jobCreationResult.Message, Is.EqualTo("job submitted for cpo live prediction."));
        Assert.That(jobCreationResult.Id.Length, Is.GreaterThan(0));
        Console.WriteLine($"RunLivePreditionSuccessfully: {jobCreationResult.Id}");
        GetJobForId(jobCreationResult.Id);
    }

    [Test, Order(8)]
    public void GetLivePreditionWeightsSuccessfully()
    {
        var weightsByDate = _client.GetLivePredictionWeights(_portfolioParameters, new DateTime(2022, 07, 01), debug: "debug");
        Assert.That(weightsByDate, Is.Not.Empty);
    }


    [Test, Order(9)]
    public void CreateModelSuccessfully()
    {
        var modelParameters = new ModelParameters(Mode.Train, ModelType.Classification, FeatureSelection.Shap, Analysis.Small, Boost.Gbdt, 42.0, false, false, false, "no");
        var result = _client.CreateModel("fx-time-of-day", modelParameters);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
    }

    [Test, Order(10)]
    public void TrainModelSuccessfully()
    {
        var filename = Path.Combine(Directory.GetCurrentDirectory(), "Data", "fx-time-of-day.parquet");
        var response = _client.Train("fx-time-of-day", filename, "strategy_ret");
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Success, Is.True);
        _trainId = response.TrainId;
        var result = _client.GetTrainingResult("fx-time-of-day", response.TrainId);
        Assert.That(result.Completed, Is.False);
        Console.WriteLine(result);
        Console.WriteLine(_trainId);
    }

    [Test, Order(11)]
    public void GetTrainingResultSuccessfully()
    {
        var result = _client.GetTrainingResult("fx-time-of-day", _trainId);
        Assert.That(result.Completed, Is.False);
    }

    [Test, Order(12)]
    public void PredictSuccessfully()
    {
        var filename = Path.Combine(Directory.GetCurrentDirectory(), "Data", "fx-time-of-day.parquet");
        var result = _client.Predict("fx-time-of-day", filename);
        Assert.That(result.Filename.EndsWith(".parquet"), Is.True);
    }

    private void GetJobForId(string jobId)
    {
        var maxRetries = 5;
        var job = Job.Null(string.Empty);
        while(true)
        {
            if ((maxRetries -= 1) < 0) break;
            job = _client.GetJobForId(jobId);
            if (job.Status == "SUCCESS") break;
            Thread.Sleep(60000);
        }

        Assert.That(job, Is.Not.EqualTo(Job.Null(string.Empty)));
        Console.WriteLine($"GetJobForId: {jobId} : {job.Status}");
    }
}