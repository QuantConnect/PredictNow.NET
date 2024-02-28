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

using Newtonsoft.Json;
using Python.Runtime;
using QuantConnect.Configuration;
using QuantConnect.Logging;
using QuantConnect.PredictNowNET.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace QuantConnect.PredictNowNET;

/// <summary>
/// REST Client for PredictNow CPO 
/// </summary>
public class PredictNowClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _userId;

    /// <summary>
    /// Creates a new instance of the REST Client for PredictNow CPO 
    /// </summary>
    /// <param name="baseUrl">The base URL to PredictNow REST endpoints</param>
    /// <param name="userId">User identification</param>
    protected PredictNowClient(string baseUrl, string userId)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentNullException($"PredictNowClient: {nameof(baseUrl)} cannot be null, empty, or consists only of white-space characters.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentNullException($"PredictNowClient: {nameof(userId)} cannot be null, empty, or consists only of white-space characters.");
        }

        _userId = userId;
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 
    }

    /// <summary>
    /// Creates a new instance of REST Client for PredictNow CPO for a given user 
    /// </summary>
    /// <param name="userId">User identification</param>
    /// <returns></returns>
    public PredictNowClient(string userId) : this(Config.Get("predict-now-url"), userId) { }

    /// <summary>
    /// Checks whether we can connect to the endpoint
    /// </summary>
    public bool Connected
    {
        get
        {
            using var request = new HttpRequestMessage();
            return TryRequest<Dictionary<string, string>>(request, out _);
        }
    }

    /// <summary>
    /// List all files with return information
    /// </summary>
    /// <returns>Array of string with the files names</returns>
    public string[] ListReturnsFiles() => ListFiles("return");

    /// <summary>
    /// List all files with constraint information
    /// </summary>
    /// <returns>Array of string with the files names</returns>
    public string[] ListConstraintFiles() => ListFiles("constraint");

    /// <summary>
    /// List all files with feature information
    /// </summary>
    /// <returns>Array of string with the files names</returns>
    public string[] ListFeaturesFiles() => ListFiles("feature");

    /// <summary>
    /// Uploads one Returns file
    /// </summary>
    /// <param name="filename">Absolute file path</param>
    /// <returns>String with information about the file</returns>
    public string UploadReturnsFile(string filename) => UploadFile(filename, "Returns");

    /// <summary>
    /// Uploads one Constraint file
    /// <param name="filename">Absolute file path</param>
    /// </summary>
    /// <returns>String with information about the file</returns>
    public string UploadConstraintFile(string filename) => UploadFile(filename, "Constraint");

    /// <summary>
    /// Uploads one Constraint file
    /// </summary>
    /// <param name="filename">Absolute file path</param>
    /// <returns>String with information about the file</returns>
    public string UploadFeaturesFile(string filename) => UploadFile(filename, "features");

    /// <summary>
    /// Creates a job to run a in-sample backtest
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="trainingStartDate">Start date of the first rebalancing period to be included in the experiment.</param>
    /// <param name="trainingEndDate">The experiment terminates when the start of the period exceed the training end date.</param>
    /// <param name="samplingProportion">Float between 0 and 1, the fraction of base strategies to be kept. This parameter is usually set to 0.3 or 0.4.</param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Tuple of string. The first contains the message on the job submission, and the second the job id.</returns>
    public JobCreationResult RunInSampleBacktest(PortfolioParameters portfolioParameters, DateTime trainingStartDate, DateTime trainingEndDate, double samplingProportion, string? debug = null)
    {
        return RunBacktest("run-insample-backtest", portfolioParameters, trainingStartDate, trainingEndDate, samplingProportion, debug);
    }

    /// <summary>
    /// Creates a job to run a out-of-sample backtest
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="trainingStartDate">Start date of the first rebalancing period to be included in the experiment.</param>
    /// <param name="trainingEndDate">The experiment terminates when the start of the period exceed the training end date.</param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Tuple of string. The first contains the message on the job submission, and the second the job id.</returns>
    public JobCreationResult RunOutOfSampleBacktest(PortfolioParameters portfolioParameters, DateTime trainingStartDate, DateTime trainingEndDate, string? debug = null)
    {
        return RunBacktest("run-oos-backtest", portfolioParameters, trainingStartDate, trainingEndDate, null, debug);
    }

    /// <summary>
    /// Creates a job to run a live prediction
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="rebalanceDate">The target rebalance date.</param>
    /// <param name="nextRebalanceDate">The next rebalance date after current target date.</param>
    /// <param name="marketDays">The number of market days in the incoming rebalancing period. </param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Tuple of string. The first contains the message on the job submission, and the second the job id.</returns>
    public JobCreationResult RunLivePrediction(PortfolioParameters portfolioParameters, DateTime rebalanceDate, DateTime nextRebalanceDate, int? marketDays = null, string? debug = null)
    {
        portfolioParameters.SetUserId(_userId);
        var livePredictionParameters = new LivePredictionParameters(portfolioParameters, rebalanceDate, nextRebalanceDate, marketDays, debug);

        using var request = new HttpRequestMessage(HttpMethod.Post, "run-live-prediction")
        {
            Content = new StringContent(JsonConvert.SerializeObject(livePredictionParameters), Encoding.UTF8, "application/json")
        };

        return TryRequest<JobCreationResult>(request, out var result) ? result : JobCreationResult.Null;
    }

    /// <summary>
    /// Get the job for a given id from in-sample and out-of-sample backtests and live prediction
    /// </summary>
    /// <param name="jobId">The id of the job.</param>
    /// <returns>The Job object with current information</returns>
    public Job GetJobForId(string jobId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"get-cpo-job-status/{jobId}");
        return TryRequest<Job>(request, out var result) ? result : Job.Null;
    }

    /// <summary>
    /// Get the backtest performance
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="trainingStartDate">Start date of the first rebalancing period to be included in the experiment.</param>
    /// <param name="trainingEndDate">The experiment terminates when the start of the period exceed the training end date.</param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Performance object with metrics of the backtest</returns>
    public Performance GetBacktestPerformance(PortfolioParameters portfolioParameters, DateTime trainingStartDate, DateTime trainingEndDate, string? debug = null)
    {
        portfolioParameters.SetUserId(_userId);
        var backtestParameters = new BacktestParameters(portfolioParameters, trainingStartDate, trainingEndDate, null, debug);

        using var request = new HttpRequestMessage(HttpMethod.Get, "get-backtest-performance")
        {
            Content = new StringContent(JsonConvert.SerializeObject(backtestParameters), Encoding.UTF8, "application/json")
        };

        return TryRequest<Performance>(request, out var result) ? result : Performance.Null;
    }

    /// <summary>
    /// Get the backtest weights
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="trainingStartDate">Start date of the first rebalancing period to be included in the experiment.</param>
    /// <param name="trainingEndDate">The experiment terminates when the start of the period exceed the training end date.</param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Dictionary of weights ordered keyed by date</returns>
    public Dictionary<DateTime, Dictionary<string, double>> GetBacktestWeights(PortfolioParameters portfolioParameters, DateTime trainingStartDate, DateTime trainingEndDate, string? debug = null)
    {
        portfolioParameters.SetUserId(_userId);
        var backtestParameters = new BacktestParameters(portfolioParameters, trainingStartDate, trainingEndDate, null, debug);

        using var request = new HttpRequestMessage(HttpMethod.Get, "get-backtest-weights")
        {
            Content = new StringContent(JsonConvert.SerializeObject(backtestParameters), Encoding.UTF8, "application/json")
        };

        return TryRequest<Dictionary<DateTime, Dictionary<string, double>>>(request, out var result)
            ? result
            : new Dictionary<DateTime, Dictionary<string, double>>();
    }

    /// <summary>
    /// Get the backtest weights
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="trainingStartDate">Start date of the first rebalancing period to be included in the experiment.</param>
    /// <param name="trainingEndDate">The experiment terminates when the start of the period exceed the training end date.</param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Dictionary of weights ordered keyed by date</returns>
    public PyDict GetBacktestWeights(PyObject portfolioParameters, DateTime trainingStartDate, DateTime trainingEndDate, string? debug = null)
    {
        var weights = GetBacktestWeights(portfolioParameters.As<PortfolioParameters>(), trainingStartDate, trainingEndDate, debug);
        return ConvertCSharpDictionaryToPythonDict(weights);
    }

    /// <summary>
    /// Get the live prediction weights
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="rebalanceDate">The target rebalance date.</param>
    /// <param name="marketDays">The number of market days in the incoming rebalancing period. </param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Dictionary of weights ordered keyed by date</returns>
    public Dictionary<DateTime, Dictionary<string, double>> GetLivePredictionWeights(PortfolioParameters portfolioParameters, DateTime rebalanceDate, int? marketDays = null, string? debug = null)
    {
        portfolioParameters.SetUserId(_userId);
        var livePredictionParameters = new LivePredictionParameters(portfolioParameters, rebalanceDate, null, marketDays, debug);

        using var request = new HttpRequestMessage(HttpMethod.Get, "get-live-prediction-weights")
        {
            Content = new StringContent(JsonConvert.SerializeObject(livePredictionParameters), Encoding.UTF8, "application/json")
        };

        return TryRequest<Dictionary<DateTime, Dictionary<string, double>>>(request, out var result)
            ? result
            : new Dictionary<DateTime, Dictionary<string, double>>();
    }

    /// <summary>
    /// Get the live prediction weights
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="rebalanceDate">The target rebalance date.</param>
    /// <param name="marketDays">The number of market days in the incoming rebalancing period. </param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Dictionary of weights ordered keyed by date</returns>
    public PyDict GetLivePredictionWeights(PyObject portfolioParameters, DateTime rebalanceDate, int? marketDays = null, string? debug = null)
    {
        var weights = GetLivePredictionWeights(portfolioParameters.As<PortfolioParameters>(), rebalanceDate, marketDays, debug);
        return ConvertCSharpDictionaryToPythonDict(weights);
    }

    /// <summary>
    /// Release unmanaged resource
    /// </summary>
    public void Dispose() => _client.Dispose();

    /// <summary>
    /// Uploads a given file to its type: returns, contraits or features
    /// </summary>
    /// <param name="filename">Absolute file path</param>
    /// <param name="type">Type of file content: returns, contraits or features</param>
    /// <returns>String with information about the file</returns>
    private string UploadFile(string filename, string type)
    {
        var fileInfo = new FileInfo(filename);
        if (!fileInfo.Exists)
        {
            return $"{fileInfo.FullName} does not exist";
        }

        using var stream = File.OpenRead(filename);

        using var request = new HttpRequestMessage(HttpMethod.Post, "upload-data")
        {
            Content = new MultipartFormDataContent
            {
                { new StreamContent(stream), "file", fileInfo.Name },
                { new StringContent(_userId), "email" },
                { new StringContent(type), "type" },
            }
        };

        if (TryRequest<Dictionary<string, string>>(request, out var result))
        {
            if (result != null && result.TryGetValue("message", out var message))
            {
                return message;
            }
        }

        return $"The content of the response is invalid: {request.RequestUri?.AbsoluteUri}";
    }

    /// <summary>
    /// List all files of a given type: returns, contraits or features
    /// </summary>
    /// <param name="type">Type of file content: returns, contraits or features</param>
    /// <returns>Array of string with the files names</returns>
    private string[] ListFiles(string type)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"list-{type}-files/{_userId}");

        TryRequest<Dictionary<string, List<string>>>(request, out var result);

        return result.Values == null
            ? Array.Empty<string>()
            : result.Values.SelectMany(x => x).ToArray();     
    }

    /// <summary>
    /// Creates a job to run a backtest
    /// </summary>
    /// <param name="resource">Name of the endpoint resource</param>
    /// <param name="portfolioParameters">Portfolio parameters</param>
    /// <param name="trainingStartDate">Start date of the first rebalancing period to be included in the experiment.</param>
    /// <param name="trainingEndDate">The experiment terminates when the start of the period exceed the training end date.</param>
    /// <param name="samplingProportion">Float between 0 and 1, the fraction of base strategies to be kept. This parameter is usually set to 0.3 or 0.4.</param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    /// <returns>Tuple of string. The first contains the message on the job submission, and the second the job id.</returns>
    private JobCreationResult RunBacktest(string resource, PortfolioParameters portfolioParameters, DateTime trainingStartDate, DateTime trainingEndDate, double? samplingProportion = null, string? debug = null)
    {
        portfolioParameters.SetUserId(_userId);
        var backtestParameters = new BacktestParameters(portfolioParameters, trainingStartDate, trainingEndDate, samplingProportion, debug);

        using var request = new HttpRequestMessage(HttpMethod.Post, resource)
        {
            Content = new StringContent(JsonConvert.SerializeObject(backtestParameters), Encoding.UTF8, "application/json")
        };

        return TryRequest<JobCreationResult>(request, out var result) ? result : JobCreationResult.Null;
    }

    /// <summary>
    /// Try to fulfill a HTTP Request and deserialize its result
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize</typeparam>
    /// <param name="request">HTTP request</param>
    /// <param name="result">Deserialized object</param>
    /// <returns>True if the result is a non-null deserialized object</returns>
    private bool TryRequest<T>(HttpRequestMessage request, out T result)
    {
        result = default;
        var responseContent = string.Empty;
        var errorCode = HttpStatusCode.OK;

        try
        {
            var response = _client.Send(request);
            errorCode = response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"TryRequest({request.RequestUri.LocalPath}): Status: {errorCode}, Response content: {response.Content.ReadAsStringAsync().Result}");
                return false;
            }
            responseContent = response.Content.ReadAsStringAsync().Result;
            result = JsonConvert.DeserializeObject<T>(responseContent);
        }
        catch (Exception e)
        {
            Log.Error($"TryRequest({request.RequestUri.LocalPath}): Error: {e.Message}, Status: {errorCode}, Response content: {responseContent}");
        }
        return result != null;
    }

    /// <summary>
    /// Converts C# Dictionary to PyDict
    /// </summary>
    /// <param name="keyValuePairs">C# Dictionary</param>
    /// <returns>Python Dictionary</returns>
    private static PyDict ConvertCSharpDictionaryToPythonDict(Dictionary<DateTime, Dictionary<string, double>> keyValuePairs)
    {
        using (Py.GIL())
        {
            PyDict result = new();
            foreach (var kvp in keyValuePairs)
            {
                PyDict innerDict = new();
                foreach (var pair in kvp.Value)
                {
                    innerDict.SetItem(pair.Key, pair.Value.ToPython());
                }
                result.SetItem(kvp.Key.ToPython(), innerDict);
            }
            return result;
        }
    }
}