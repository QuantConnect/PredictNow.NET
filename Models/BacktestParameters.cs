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
/// Required parameters to deploy a in-sample and out-of-sample backtests.  
/// </summary>
public class BacktestParameters : PortfolioParameters
{
    /// <summary>
    /// Start date of the first rebalancing period to be included in the experiment. 
    /// </summary>
    [JsonProperty(PropertyName = "training_start_date")]
    [JsonConverter(typeof(DateTimeJsonConverter), "yyyy-MM-dd")]
    public DateTime TrainingStartDate { get; }

    /// <summary>
    /// The experiment terminates when the start of the period exceed the training end date.
    /// </summary>
    [JsonProperty(PropertyName = "training_end_date")]
    [JsonConverter(typeof(DateTimeJsonConverter), "yyyy-MM-dd")]
    public DateTime TrainingEndDate { get; }

    /// <summary>
    /// Float between 0 and 1, the fraction of base strategies to be kept. This parameter is usually set to 0.3 or 0.4.
    /// </summary>
    [JsonProperty(PropertyName = "sampling_proportion", NullValueHandling = NullValueHandling.Ignore)]
    public double? SamplingProportion { get; }

    /// <summary>
    /// Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.
    /// </summary>
    [JsonProperty(PropertyName = "debug", NullValueHandling = NullValueHandling.Ignore)]
    public string? Debug { get; }

    /// <summary>
    /// Creates a new instance of BacktestParameters
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameter</param>
    /// <param name="trainingStartDate">Start date of the first rebalancing period to be included in the experiment. </param>
    /// <param name="trainingEndDate">The experiment terminates when the start of the period exceed the training end date.</param>
    /// <param name="samplingProportion">Float between 0 and 1, the fraction of base strategies to be kept. This parameter is usually set to 0.3 or 0.4.</param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    public BacktestParameters(PortfolioParameters portfolioParameters, DateTime trainingStartDate, DateTime trainingEndDate, double? samplingProportion = null, string? debug = null) :
        base(portfolioParameters.Name, portfolioParameters.ReturnsFile, portfolioParameters.ConstraintFile, portfolioParameters.MaxCash,
            portfolioParameters.RebalancingPeriodUnit, portfolioParameters.RebalancingPeriod, portfolioParameters.RebalanceOn,
            portfolioParameters.TrainingDataSize, portfolioParameters.EvaluationMetric, portfolioParameters.FeatureFile, portfolioParameters.SkipPNowFeature)
    {
        SetUserId(portfolioParameters.UserId);
        TrainingStartDate = trainingStartDate;
        TrainingEndDate = trainingEndDate;
        SamplingProportion = samplingProportion;
        Debug = debug;
    }

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => JsonConvert.SerializeObject(this);
}