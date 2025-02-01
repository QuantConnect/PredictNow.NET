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
/// Provides the status and results of a backtest or live prediction job
/// </summary>
public class Job
{
    /// <summary>
    /// Unique identifier of a job 
    /// </summary>
    [JsonProperty(PropertyName = "cpo_job_id")]
    public string Id { get; internal set; } = string.Empty;

    /// <summary>
    /// Status: SUCCESS or PENDING
    /// </summary>
    [JsonProperty(PropertyName = "cpo_job_status")]
    public string Status { get; internal set; } = string.Empty;

    /// <summary>
    /// Performance metrics such as returns, risk and sharpe ratio
    /// </summary>
    [JsonProperty(PropertyName = "cpo_result")]
    [JsonConverter(typeof(PerformanceJsonConverter))]
    public Performance Performance { get; internal set; } = Performance.Null(string.Empty);

    /// <summary>
    /// Tracks the progress of the job
    /// </summary>
    [JsonProperty(PropertyName = "progress")]
    [JsonConverter(typeof(ProgressJsonConverter))]
    public Progress Progress { get; internal set; } = Progress.Null(string.Empty);

    /// <summary>
    /// Represents an empty job (not associated with a valid backtest)
    /// </summary>
    /// <param name="message">Information about the progress</param>
    public static Job Null(string message) => new() { Progress = Progress.Null(message) };

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => JsonConvert.SerializeObject(this);
}