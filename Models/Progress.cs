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
using QuantConnect.Util;

namespace QuantConnect.PredictNowNET.Models;

/// <summary>
/// Tracks the progress of a <seealso cref="Job"/>.
/// </summary>
public class Progress
{
    /// <summary>
    /// Number of steps
    /// </summary>
    [JsonProperty(PropertyName = "step")]
    public int Step { get; internal set; } = -1;

    /// <summary>
    /// Information about the progress
    /// </summary>
    [JsonProperty(PropertyName = "progress")]
    public string Message { get; internal set; } = string.Empty;

    /// <summary>
    /// Represents an empty progress (not associated with a valid backtest)
    /// </summary>
    /// <param name="message">Information about the progress</param>
    static public Progress Null(string message) => new() { Message = message };
}

/// <summary>
/// Defines how Progress should be serialized to json
/// </summary>
public class ProgressJsonConverter : TypeChangeJsonConverter<Progress, string>
{
    /// <summary>
    /// Convert the input value to a value to be serialized
    /// </summary>
    /// <param name="value">The input value to be converted before serialization</param>
    /// <returns>A new instance of TResult that is to be serialized</returns>
    protected override string Convert(Progress value) => JsonConvert.SerializeObject(value);

    /// <summary>
    /// Converts the input value to be deserialized
    /// </summary>
    /// <param name="value">The deserialized value that needs to be converted to T</param>
    /// <returns>The converted value</returns>
    protected override Progress Convert(string value)
    {
        return JsonConvert.DeserializeObject<Progress>(value) ?? Progress.Null(value);
    }
}