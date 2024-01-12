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
/// Required parameters to deploy a live prediction.  
/// </summary>
public class LivePredictionParameters : PortfolioParameters
{
    /// <summary>
    /// The target rebalance date.
    /// </summary>
    [JsonProperty(PropertyName = "rebalance_date")]
    [JsonConverter(typeof(DateTimeJsonConverter), "yyyy-MM-dd")]
    public DateTime RebalanceDate { get; }

    /// <summary>
    /// The next rebalance date after current target date. 
    /// For example, for a weekly-rebalanced portfolio, if the `RebalanceDate` is set to '2023-10-02', the `NextRebalanceDate` would be Monday '2023-10-09'.
    /// If `NextRebalanceDate` is passed, CPO will use US market calendar to determine how many market days are there in the target rebalancing period.
    /// </summary>
    [JsonProperty(PropertyName = "next_rebalance_date", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(DateTimeJsonConverter), "yyyy-MM-dd")]
    public DateTime? NextRebalanceDate { get; }

    /// <summary>
    /// The number of market days in the incoming rebalancing period. 
    /// For a weekly rebalanced portfolio, MarketDays is usually 5 unless there is a holiday. This parameter overrides `NextRebalanceDate`.
    /// </summary>
    [JsonProperty(PropertyName = "n_days", NullValueHandling = NullValueHandling.Ignore)]
    public int? MarketDays { get; }

    /// <summary>
    /// Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.
    /// </summary>
    [JsonProperty(PropertyName = "debug", NullValueHandling = NullValueHandling.Ignore)]
    public string? Debug { get; }

    /// <summary>
    /// Creates a new instance of LivePredictionParameters
    /// </summary>
    /// <param name="portfolioParameters">Portfolio parameter</param>
    /// <param name="rebalanceDate">The target rebalance date.</param>
    /// <param name="nextRebalanceDate">The next rebalance date after current target date.</param>
    /// <param name="marketDays">The number of market days in the incoming rebalancing period. </param>
    /// <param name="debug">Will output more information in the backend when set to `debug`, and will not affect the performance or prediction.</param>
    public LivePredictionParameters(PortfolioParameters portfolioParameters, DateTime rebalanceDate, DateTime? nextRebalanceDate = null, int? marketDays = null, string? debug = null) :
        base(portfolioParameters.Name, portfolioParameters.ReturnsFile, portfolioParameters.ConstraintFile, portfolioParameters.MaxCash,
            portfolioParameters.RebalancingPeriodUnit, portfolioParameters.RebalancingPeriod, portfolioParameters.RebalanceOn,
            portfolioParameters.TrainingDataSize, portfolioParameters.EvaluationMetric, portfolioParameters.FeatureFile, portfolioParameters.SkipPNowFeature)
    {
        SetUserId(portfolioParameters.UserId);
        RebalanceDate = rebalanceDate;
        NextRebalanceDate = nextRebalanceDate;
        MarketDays = marketDays;
        Debug = debug;
    }

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => JsonConvert.SerializeObject(this);
}