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
/// Provides information on the performance of the in-sample and out-of-sample backtests
/// </summary>
public class Performance
{
    private string _message;

    /// <summary>
    /// Backtest return
    /// </summary>
    [JsonProperty(PropertyName = "return")]
    public double Return { get; internal set; }

    /// <summary>
    /// Backtest risk
    /// </summary>
    [JsonProperty(PropertyName = "risk")]
    public double Risk { get; internal set; }

    /// <summary>
    /// Backtest Sharpe Ratio
    /// </summary>
    [JsonProperty(PropertyName = "sharpe")]
    public double SharpeRatio { get; internal set; }

    /// <summary>
    /// Backtest Compound Annual Growth Rate 
    /// </summary>
    [JsonProperty(PropertyName = "CAGR")]
    public double CAGR { get; internal set; }

    /// <summary>
    /// Backtest Ulcer Index, measure downside risk
    /// </summary>
    [JsonProperty(PropertyName = "UI")]
    public double UI { get; internal set; }

    /// <summary>
    /// Backtest Ulcer Performance Index, uses the magnitude of drawdowns (all drawdowns, not just the maximum drawdown) and their duration as the measure of risk.
    /// </summary>
    [JsonProperty(PropertyName = "UPI")]
    public double UPI { get; internal set; }

    /// <summary>
    /// Backtest maximum drawdown 
    /// </summary>
    [JsonProperty(PropertyName = "MaxDD")]
    public double MaximumDrawdown { get; internal set; }

    /// <summary>
    /// Creates a new instance of the Performance class
    /// </summary>
    protected Performance() : this(string.Empty) { }

    /// <summary>
    /// Creates a new instance of the Performance class
    /// </summary>
    /// <param name="message">Information about the progress</param>
    protected Performance(string message) => _message = message;

    /// <summary>
    /// Represents an empty Performance object
    /// </summary>
    /// <param name="message">Information about the progress</param>
    public static Performance Null(string message) => new(message);

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => string.IsNullOrWhiteSpace(_message) ? JsonConvert.SerializeObject(this) : _message;
}

/// <summary>
/// Defines how Performance should be serialized to json
/// </summary>
public class PerformanceJsonConverter : TypeChangeJsonConverter<Performance, string>
{
    /// <summary>
    /// Convert the input value to a value to be serialized
    /// </summary>
    /// <param name="value">The input value to be converted before serialization</param>
    /// <returns>A new instance of TResult that is to be serialized</returns>
    protected override string Convert(Performance value) => JsonConvert.SerializeObject(value);
  
    /// <summary>
    /// Converts the input value to be deserialized
    /// </summary>
    /// <param name="value">The deserialized value that needs to be converted to T</param>
    /// <returns>The converted value</returns>
    protected override Performance Convert(string value)
    {
        if (value == "None" || value == "Expected object or value")
        {
            return Performance.Null(value);
        }

        return JsonConvert.DeserializeObject<Performance>(value) ?? Performance.Null(value);
    }
}