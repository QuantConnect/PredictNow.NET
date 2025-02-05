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
using QuantConnect.PredictNowNET.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace QuantConnect.PredictNowNET;

/// <summary>
/// REST Client for PredictNow CAI and CPO 
/// </summary>
public class PredictNowClient : IDisposable
{
    private readonly HttpClient _cpoClient = GetClient(Config.Get("predict-now-cpo-url"));
    private readonly HttpClient _caiClient = GetClient(Config.Get("predict-now-cai-url"));
    private readonly string _userId = string.Empty;
    private readonly string _userName = string.Empty;
    private string _lastErrorMessage;

    /// <summary>
    /// Last error message
    /// </summary>
    public string LastErrorMessage
    {
        get => _lastErrorMessage;
        set 
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _lastErrorMessage = value;
            }  
        }
    }

    /// <summary>
    /// Creates a new instance of REST Client for PredictNow CAI and CPO for a given user 
    /// </summary>
    /// <param name="userId">User identification</param>
    /// <param name="userName">User Name</param>
    /// <returns></returns>
    public PredictNowClient(string userId, string? userName = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            Console.WriteLine($"PredictNowClient: {nameof(userId)} cannot be null, empty, or consists only of white-space characters.");
            return;
        }

        if (!Config.GetBool("predict-now-verify-users", true))
        {
            _userId = userId;
            _userName = userName ?? _userName;
            return;
        }

        var usersUrl = Config.Get("predict-now-users-url");
        if (string.IsNullOrWhiteSpace(usersUrl))
        {
            Console.WriteLine($"Invalid {nameof(usersUrl)} cannot be null, empty, or consists only of white-space characters. Please contact support@quantconnect.");
            return;
        }

        var result = Encoding.UTF8.GetString(new HttpClient().GetByteArrayAsync(new Uri(usersUrl)).Result);
        if (string.IsNullOrWhiteSpace(result))
        {
            Console.WriteLine($"PredictNowClient: {nameof(result)} cannot be null, empty, or consists only of white-space characters. Please contact support@quantconnect.");
            return;
        }

        if (result.Contains(userId))
        {
            _userId = userId;
            _userName = userName ?? _userName;
        }

        AccessDenied();
    }

    private bool AccessDenied()
    {
        if (!string.IsNullOrWhiteSpace(_userId))
        {
            return false;
        }
        Console.WriteLine("Sorry you are not authorized to access this service. The PredictNow CAI-CPO optimization service starts at $1,000 per month. Please contact sales@predictnow.ai to register for access.");
        return true;
    }

    /// <summary>
    /// Checks whether we can connect to the endpoint
    /// </summary>
    public bool Connected
    {
        get
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/");
            var success = TryRequest<Dictionary<string, string>>(_cpoClient, request, out _, out var errorMessage);
            LastErrorMessage = errorMessage;
            return success;
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

        return TryRequest<JobCreationResult>(_cpoClient, request, out var result, out var errorMessage) ? result : JobCreationResult.Null(errorMessage);
    }

    /// <summary>
    /// Get the job for a given id from in-sample and out-of-sample backtests and live prediction
    /// </summary>
    /// <param name="jobId">The id of the job.</param>
    /// <returns>The Job object with current information</returns>
    public Job GetJobForId(string jobId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"get-cpo-job-status/{jobId}");
        return TryRequest<Job>(_cpoClient, request, out var result, out var errorMessage) ? result : Job.Null(errorMessage);
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

        using var request = new HttpRequestMessage(HttpMethod.Post, "get-backtest-performance")
        {
            Content = new StringContent(JsonConvert.SerializeObject(backtestParameters), Encoding.UTF8, "application/json")
        };

        return TryRequest<Performance>(_cpoClient, request, out var result, out var errorMessage) ? result : Performance.Null(errorMessage);
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

        using var request = new HttpRequestMessage(HttpMethod.Post, "get-backtest-weights")
        {
            Content = new StringContent(JsonConvert.SerializeObject(backtestParameters), Encoding.UTF8, "application/json")
        };

        if (TryRequest<Dictionary<DateTime, Dictionary<string, double>>>(_cpoClient, request, out var result, out var errorMessage))
        {
            return result;
        }
        LastErrorMessage = errorMessage;
        return [];
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

        using var request = new HttpRequestMessage(HttpMethod.Post, "get-live-prediction-weights")
        {
            Content = new StringContent(JsonConvert.SerializeObject(livePredictionParameters), Encoding.UTF8, "application/json")
        };

        if (TryRequest<Dictionary<DateTime, Dictionary<string, double>>>(_cpoClient, request, out var result, out var errorMessage))
        {
            return result;
        }
        LastErrorMessage = errorMessage;
        return [];
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
    /// Create the model
    /// </summary>
    /// <param name="name">The name of the model</param>
    /// <param name="parameters">Model parameters</param>
    /// <returns>The response to this request</returns>
    public ModelResponse CreateModel(string name, ModelParameters parameters)
    {
        // TODO: understand the hyp_dict parameter
        var value = new { params_ = parameters, model_name = name, username = _userName, hyp_dict = new Dictionary<string, string>() };
        var requestParameters = JsonConvert.SerializeObject(value).Replace("params_", "params");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/models")
        {
            Content = new StringContent(requestParameters, Encoding.UTF8, "application/json")
        };

        TryRequest<ModelResponse>(_caiClient, request, out var result, out var errorMessage);
        result.Message += errorMessage;
        return result;
    }

    /// <summary>
    /// Train the model
    /// </summary>
    /// <param name="modelName">The name of the model</param>
    /// <param name="filename">The path to the file with the data to train the model with</param>
    /// <param name="label">The label in the data</param>
    /// <returns>The response to this request</returns>
    public TrainModelResponse Train(string modelName, string filename, string label)
    {
        var fileInfo = new FileInfo(filename);
        if (!fileInfo.Exists)
        {
            return new TrainModelResponse { Message = $"{fileInfo.FullName} does not exist" };
        }

        using var stream = File.OpenRead(filename);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/trainings")
        {
            Content = new MultipartFormDataContent
            {
                { new StringContent(_userName), "username" },
                { new StringContent(_userId), "email" },
                { new StringContent(modelName), "model_name" },
                { new StringContent(Guid.NewGuid().ToString()), "train_id" },
                { new StringContent(label), "label" },
                { new StreamContent(stream), fileInfo.Name, fileInfo.Name }
            }
        };

        TryRequest<TrainModelResponse>(_caiClient, request, out var result, out var errorMessage);
        result.Message += errorMessage;
        return result;
    }

    /// <summary>
    /// Get the training results
    /// </summary>
    /// <param name="modelName">The name of the model</param>
    /// <param name="trainId">ID of the trainint job</param>
    /// <returns>The prediction result</returns>
    public TrainingResult GetTrainingResult(string modelName, string? trainId = null)
    {
        if (!string.IsNullOrWhiteSpace(trainId))
        {
            var trainingStatus = GetTrainingStatus(trainId);
            if (!trainingStatus.Completed)
            {
                return new TrainingResult(trainingStatus);
            }
        }
        
        using var request = new HttpRequestMessage(HttpMethod.Post, "/get_result")
        {
            Content = new MultipartFormDataContent
            {
                { new StringContent(_userName), "username" },
                { new StringContent(modelName), "model_name" },
            }
        };

        return TryRequest<TrainingResult>(_caiClient, request, out var result, out var errorMessage) ? result : TrainingResult.Null(errorMessage);
    }

    /// <summary>
    /// Get the training status
    /// </summary>
    /// <param name="trainId">ID of the trainint job</param>
    /// <returns>The prediction status</returns>
    public TrainingStatus GetTrainingStatus(string trainId)
    {
        var requestUri = $"/get_status?username={_userName}&train_id={trainId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return TryRequest<TrainingStatus>(_caiClient, request, out var result, out var errorMessage) ? result : TrainingStatus.Null(errorMessage);
    }

    /// <summary>
    /// Predict with the trained model
    /// </summary>
    /// <param name="modelName">Name of the model</param>
    /// <param name="filename">Path to file with the input for the prediction</param>
    /// <param name="exploratoryDataAnalysis">True if the predition should use exploratory data analysis</param>
    /// <param name="probabilityCalibration">True if the model should refine the probability</param>
    /// <returns>The prediction result</returns>
    public PredictResult Predict(string modelName, string filename, bool exploratoryDataAnalysis = false, bool probabilityCalibration = false)
    {
        var fileInfo = new FileInfo(filename);
        if (!fileInfo.Exists)
        {
            return PredictResult.Null($"{fileInfo.FullName} does not exist");
        }

        using var stream = File.OpenRead(filename);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/predictions")
        {                            
            Content = new MultipartFormDataContent
            {
                { new StringContent(_userName), "username" },
                { new StringContent(modelName), "model_name" },
                { new StringContent(exploratoryDataAnalysis ? "yes" : "no"), "eda" },
                { new StringContent(probabilityCalibration ? "yes" : "no"), "prob_calib" },
                { new StreamContent(stream), fileInfo.Name, fileInfo.Name }
            }
        };

        return TryRequest<PredictResult>(_caiClient, request, out var result, out var errorMessage) ? result : PredictResult.Null(errorMessage);
    }

    /// <summary>
    /// Release unmanaged resource
    /// </summary>
    public void Dispose() 
    {
        _caiClient.Dispose();
        _cpoClient.Dispose();
        GC.SuppressFinalize(this);
    }

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

        if (TryRequest<Dictionary<string, string>>(_cpoClient, request, out var result, out var errorMessage))
        {
            if (result != null && result.TryGetValue("message", out var message))
            {
                return message;
            }
        }

        return errorMessage;
    }

    /// <summary>
    /// List all files of a given type: returns, contraits or features
    /// </summary>
    /// <param name="type">Type of file content: returns, contraits or features</param>
    /// <returns>Array of string with the files names</returns>
    private string[] ListFiles(string type)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"list-{type}-files/{_userId}");

        TryRequest<Dictionary<string, List<string>>>(_cpoClient, request, out var result, out var errorMessage);
        LastErrorMessage = errorMessage;
        return result.Values == null ? [] : result.Values.SelectMany(x => x).ToArray();
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

        return TryRequest<JobCreationResult>(_cpoClient, request, out var result, out var errorMessage) ? result : JobCreationResult.Null(errorMessage);
    }

    /// <summary>
    /// Create a HTTP Client for a given URL
    /// </summary>
    /// <param name="url">URL of an empty</param>
    /// <returns>HTTP Client associated with URL</returns>
    private static HttpClient GetClient(string url)
    {
        var client = new HttpClient { BaseAddress = new Uri(url) };
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    /// <summary>
    /// Try to fulfill a HTTP Request and deserialize its result
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize</typeparam>
    /// <param name="client">HTTP client</param>
    /// <param name="request">HTTP request</param>
    /// <param name="result">Deserialized object</param>
    /// <returns>True if the result is a non-null deserialized object</returns>
    private bool TryRequest<T>(HttpClient client, HttpRequestMessage request, out T result, out string errorMessage)
    {
        result = default;
        errorMessage = string.Empty;
        if (AccessDenied()) return false;

        if (request.RequestUri == null)
        {
            errorMessage = $"RequestUri cannot be null";
            return false;
        }

        var responseContent = string.Empty;
        var errorCode = HttpStatusCode.OK;

        try
        {
            var response = client.Send(request);
            errorCode = response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                errorMessage = $"{request.RequestUri.LocalPath}: Status: {errorCode}, Response content: {response.Content.ReadAsStringAsync().Result}";
                return false;
            }
            responseContent = response.Content.ReadAsStringAsync().Result;
            result = JsonConvert.DeserializeObject<T>(responseContent);  
        }
        catch (Exception e)
        {
            errorMessage = $"{request.RequestUri.LocalPath}: Error: {e.Message}, Status: {errorCode}, Response content: {responseContent}";
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