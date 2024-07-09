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
using Newtonsoft.Json.Converters;
using QuantConnect.Util;

namespace QuantConnect.PredictNowNET.Models;

/// <summary>
/// Type of Analysis
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum Analysis
{
    /// <summary>
    /// No analysis
    /// </summary>
    None,
    /// <summary>
    /// 
    /// </summary>
    Small
}

/// <summary>
/// Type of Boost algorithms
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum Boost
{
    /// <summary>
    /// Dropouts meet Multiple Additive Regression Trees.
    /// </summary>
    Dart,
    /// <summary>
    /// Gradient-boosted decision trees
    /// </summary>
    Gbdt
}

/// <summary>
/// Feature selection
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum FeatureSelection
{
    /// <summary>
    /// No selection
    /// </summary>
    None,
    /// <summary>
    /// SHapley Additive exPlanations
    /// </summary>
    Shap,
    /// <summary>
    /// Computer-Mediated Discourse Analysis
    /// </summary>
    CMDA
}

/// <summary>
/// Model modes
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum Mode
{
    /// <summary>
    /// Train
    /// </summary>
    Train,
    /// <summary>
    /// Live
    /// </summary>
    Live
}

/// <summary>
/// Model type
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ModelType
{
    /// <summary>
    /// Classification
    /// </summary>
    Classification,
    /// <summary>
    /// Regression
    /// </summary>
    Regression
}

/// <summary>
/// Represents parameters for both backtests and live prediction
/// </summary>
public class ModelParameters
{
    /// <summary>
    /// True if use timeseries
    /// </summary>
    [JsonProperty(PropertyName = "timeseries")]
    [JsonConverter(typeof(YesNoJsonConverter))]
    public bool Timeseries { get; private set; }

    /// <summary>
    /// The type of the mode. For example regression or classification
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public ModelType Type { get; private set; }

    /// <summary>
    /// The feature selection
    /// </summary>
    [JsonProperty(PropertyName = "feature_selection")]
    public FeatureSelection FeatureSelection { get; private set; }

    /// <summary>
    /// The analysys output
    /// </summary>
    [JsonProperty(PropertyName = "analysis")]
    public Analysis Analysis { get; private set; }

    /// <summary>
    /// Select the Boost algorithm
    /// </summary>
    [JsonProperty(PropertyName = "boost")]
    public Boost Boost { get; private set; }

    /// <summary>
    /// The model mode
    /// </summary>
    [JsonProperty(PropertyName = "mode")]
    public Mode Mode { get; }

    /// <summary>
    /// The size of the test sample. If less than 1 --> ratio, otherwise --> exact # of rows
    /// </summary>
    [JsonProperty(PropertyName = "testsize")]
    public string Testsize { get; private set; }

    /// <summary>
    /// Define if should use weights: yes, no, or custom
    /// </summary>
    [JsonProperty(PropertyName = "weights")]
    public string Weights { get; private set; }

    /// <summary>
    /// True if should refine the probability
    /// </summary>
    [JsonProperty(PropertyName = "prob_calib")]
    [JsonConverter(typeof(YesNoJsonConverter))]
    public bool ProbabilityCalibration { get; private set; }

    /// <summary>
    /// True if should use exploratory data analysis
    /// </summary>
    [JsonProperty(PropertyName = "eda")]
    [JsonConverter(typeof(YesNoJsonConverter))]
    public bool ExploratoryDataAnalysis { get; private set; }

    /// <summary>
    /// Random seed for initialization
    /// </summary>
    [JsonProperty(PropertyName = "random_seed")]
    public string RandomSeed { get; private set; } = "1";

    /// <summary>
    /// Define custom weights
    /// </summary>
    [JsonProperty(PropertyName = "custom_weights")]
    public string CustomWeights { get; private set; }

    /// <summary>
    /// Create a new instance of ModelParameters
    /// </summary>
    /// <param name="mode">The model mode</param>
    /// <param name="type">The type of the mode. For example regression or classification</param>
    /// <param name="featureSelection">The feature selection</param>
    /// <param name="analysis">The analysys output</param>
    /// <param name="boost">Select the Boost algorithm</param>
    /// <param name="testsize">The size of the test sample. If less than 1 --> ratio, otherwise --> exact # of rows</param>
    /// <param name="timeseries">True if use timeseries</param>
    /// <param name="probabilityCalibration">True if should refine the probability</param>
    /// <param name="exploratoryDataAnalysis">True if should use exploratory data analysis</param>
    /// <param name="weights">Define if should use weights: yes, no, or custom</param>
    /// <param name="customWeights">Define custom weights</param>
    /// <param name="randomSeed">Random seed for initialization</param>
    public ModelParameters(Mode mode, ModelType type, FeatureSelection featureSelection, Analysis analysis, Boost boost, double testsize, bool timeseries, bool probabilityCalibration, bool exploratoryDataAnalysis, string weights, string customWeights= "", double randomSeed = 1)
    {
        Mode = mode;
        Type = type;
        FeatureSelection = featureSelection;
        Analysis = analysis;
        Boost = boost;
        Testsize = testsize.ToString();
        Timeseries = timeseries;
        ProbabilityCalibration = probabilityCalibration;
        ExploratoryDataAnalysis = exploratoryDataAnalysis;
        Weights = weights;
        CustomWeights = customWeights;
        RandomSeed = randomSeed.ToString();
    }

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => JsonConvert.SerializeObject(this);
}

/// <summary>
/// Defines how bool should be serialized to json
/// </summary>
public class YesNoJsonConverter : TypeChangeJsonConverter<bool, string>
{
    /// <summary>
    /// Convert the input value to a value to be serialized
    /// </summary>
    /// <param name="value">The input value to be converted before serialization</param>
    /// <returns>A new instance of TResult that is to be serialized</returns>
    protected override string Convert(bool value) => value ? "yes" : "no";

    /// <summary>
    /// Converts the input value to be deserialized
    /// </summary>
    /// <param name="value">The deserialized value that needs to be converted to T</param>
    /// <returns>The converted value</returns>
    protected override bool Convert(string value) => value.ToLower() switch
    {
        "yes" => true,
        "no" => false,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Not expected value: {value}")
    };
}