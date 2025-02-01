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
/// Model Training Status
/// </summary>
public class TrainingStatus
{
    /// <summary>
    /// Timestamp of the result
    /// </summary>
    [JsonProperty(PropertyName = "datetime")]
    public DateTime DateTime { get; internal set; } = DateTime.UtcNow;

    /// <summary>
    /// Current task count
    /// </summary>
    [JsonProperty(PropertyName = "current")]
    public int Current { get; internal set; }

    /// <summary>
    /// Total number of tasks
    /// </summary>
    [JsonProperty(PropertyName = "total")]
    public int Total { get; internal set; }

    /// <summary>
    /// Result of the task
    /// </summary>
    [JsonProperty(PropertyName = "result")]
    public string Result { get; internal set; } = string.Empty;

    /// <summary>
    /// State of the result: COMPLETED, FAILED, or PROGRESS
    /// </summary>
    [JsonProperty(PropertyName = "state")]
    public string State { get; internal set; } = "PROGRESS";

    /// <summary>
    /// Status of the result
    /// </summary>
    [JsonProperty(PropertyName = "status")]
    public string Status { get; internal set; } = string.Empty;

    /// <summary>
    /// True if the task is completed
    /// </summary>
    public bool Completed => State?.ToUpperInvariant() == "COMPLETED";

    /// <summary>
    /// Represents an empty TaskResult (not associated with a valid task)
    /// </summary>
    /// <param name="message">State of the result</param>
    public static TrainingStatus Null(string message) => new() { Status = message };

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() =>
        $"{DateTime.ToUniversalTime():o}: {State} ({Current}/{Total}) | {Status}" +
        (string.IsNullOrWhiteSpace(Result) ? string.Empty : $" | Result: {Result}");
}