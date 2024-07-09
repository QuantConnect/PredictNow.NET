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
/// Model Prediction Result
/// </summary>
public class PredictResult
{
    /// <summary>
    /// True if should use exploratory data analysis
    /// </summary>
    [JsonProperty(PropertyName = "eda")]
    public string ExploratoryDataAnalysis { get; internal set; } = string.Empty;

    /// <summary>
    /// Name of the file with the prediction
    /// </summary>
    [JsonProperty(PropertyName = "filename")]
    public string Filename { get; internal set; } = string.Empty;

    /// <summary>
    /// Labels used in the prediction
    /// </summary>
    [JsonProperty(PropertyName = "labels")]
    public string Labels { get; internal set; } = string.Empty;

    /// <summary>
    /// Objective function
    /// </summary>
    [JsonProperty(PropertyName = "objective")]
    public string Objective { get; internal set; } = string.Empty;

    /// <summary>
    /// True if should refine the probability
    /// </summary>
    [JsonProperty(PropertyName = "prob_calib")]
    public string ProbabilityCalibration { get; internal set; } = string.Empty;

    /// <summary>
    /// Probabilities
    /// </summary>
    [JsonProperty(PropertyName = "probabilities")]
    public string Probabilities { get; internal set; } = string.Empty;

    /// <summary>
    /// Title
    /// </summary>
    [JsonProperty(PropertyName = "title")]
    public string Title { get; internal set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty(PropertyName = "too_many_nulls_list")]
    public string TooManyNullsList { get; internal set; } = string.Empty;

    /// <summary>
    /// Result of the Prediction
    /// </summary>
    /// <param name="title"></param>
    public PredictResult(string title)
    {
        Title = title;
    }

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => JsonConvert.SerializeObject(this);
}