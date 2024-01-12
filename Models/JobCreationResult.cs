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
/// Information on a newly created task: in-sample and out-of-sample backtest and live prediction
/// </summary>
public class JobCreationResult
{
    /// <summary>
    /// Unique identifier of a job
    /// </summary>
    [JsonProperty(PropertyName = "task_id")]
    public string Id { get; internal set; } = string.Empty;

    /// <summary>
    /// Information about the task
    /// </summary>
    [JsonProperty(PropertyName = "message")]
    public string Message { get; internal set; } = string.Empty;

    /// <summary>
    /// Represents an empty TaskResult (not associated with a valid task)
    /// </summary>
    public static JobCreationResult Null => new();

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => $"{Message}: Id {Id}";
}