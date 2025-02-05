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

namespace QuantConnect.PredictNowNET.Models;

/// <summary>
/// Model Training Result
/// </summary>
public class TrainingResult
{
    private static readonly string _emptyJson = "{}";
    private TrainingStatus? _trainingStatus;

    /// <summary>
    /// State of training
    /// </summary>
    public bool Completed => _trainingStatus == null;

    /// <summary>
    /// Lab Test
    /// </summary>
    [JsonProperty(PropertyName = "lab_test_")]
    public string LabTest { get; internal set; } = string.Empty;

    /// <summary>
    /// Feature importance
    /// </summary>
    [JsonProperty(PropertyName = "feature_importance")]
    public string FeatureImportance { get; internal set; } = _emptyJson;

    /// <summary>
    /// Performance Metrics
    /// </summary>
    [JsonProperty(PropertyName = "performance_metrics")]
    public string PerformanceMetrics { get; internal set; } = _emptyJson;

    /// <summary>
    /// Result of the probability CV
    /// </summary>
    [JsonProperty(PropertyName = "predicted_prob_cv_")]
    public string PredictedProbCV { get; internal set; } = _emptyJson;

    /// <summary>
    /// Result of the probability test
    /// </summary>
    [JsonProperty(PropertyName = "predicted_prob_test_")]
    public string PredictedProbTest { get; internal set; } = _emptyJson;

    /// <summary>
    /// Result of the target CV
    /// </summary>
    [JsonProperty(PropertyName = "predicted_targets_cv_")]
    public string PredictedTargetsCV { get; internal set; } = _emptyJson;

    /// <summary>
    /// Result of the target test
    /// </summary>
    [JsonProperty(PropertyName = "predicted_targets_test_")]
    public string PredictedTargetsTest { get; internal set; } = _emptyJson;

    /// <summary>
    /// Description of the exploratory data analysis
    /// </summary>
    [JsonProperty(PropertyName = "eda_describe")]
    public string ExploratoryDataAnalysisDescribe { get; internal set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrainingResult"/> class
    /// </summary>
    protected TrainingResult() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrainingResult"/> class
    /// </summary>
    /// <param name="message">State of the result</param>
    protected TrainingResult(string message) : this(TrainingStatus.Null(message)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrainingResult"/> class
    /// </summary>
    /// <param name="trainingStatus">Status of the result</param>
    public TrainingResult(TrainingStatus trainingStatus) => _trainingStatus = trainingStatus;

    /// <summary>
    /// Represents an empty TaskResult (not associated with a valid task)
    /// </summary>
    /// <param name="message">State of the result</param>
    public static TrainingResult Null(string message) => new(message);

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => _trainingStatus?.ToString() ?? JsonConvert.SerializeObject(this);
}