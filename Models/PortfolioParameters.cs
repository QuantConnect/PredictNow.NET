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
/// Represents parameters for both backtests and live prediction
/// </summary>
public class PortfolioParameters
{
    private string _userId = string.Empty;

    /// <summary>
    /// User identification
    /// </summary>
    [JsonProperty(PropertyName = "email")]
    public string UserId => _userId;

    /// <summary>
    /// Project / portfolio identification, e.g. `Demo_Project`
    /// </summary>
    [JsonProperty(PropertyName = "project_name")]
    public string Name { get; private set; }

    /// <summary>
    /// Daily return filename, should be uploaded first, e.g. `ETF_return.csv`
    /// </summary>
    [JsonProperty(PropertyName = "returns_file")]
    public string ReturnsFile { get; private set; }

    /// <summary>
    /// Portfolio components and their min and max allocations, should be uploaded first, e.g. `ETF_constraint.csv`.
    /// Note it's the `component` column in the `ConstraintFile` that defines the current portfolio universe. 
    /// </summary>
    [JsonProperty(PropertyName = "constraint_file")]
    public string ConstraintFile { get; private set; }

    /// <summary>
    /// Maximum cash allocation allowed when risk is predicted to be large, float between 0 and 1 where 1 correspond to 100% (no market exposure).
    /// </summary>
    [JsonProperty(PropertyName = "max_cash")]
    public double MaxCash { get; private set; }

    /// <summary>
    /// 'week' or 'month', used with <see cref="RebalancingPeriod"/>>.
    /// </summary>
    [JsonProperty(PropertyName = "rebalancing_period_unit")]
    public string RebalancingPeriodUnit { get; private set; }

    /// <summary>
    /// If `RebalancingPeriod = 2` and <see cref="RebalancingPeriodUnit"/> = 'week'`, the portfolio would rebalanced every other week.
    /// </summary>
    [JsonProperty(PropertyName = "rebalancing_period")]
    public int RebalancingPeriod { get; private set; }

    /// <summary>
    /// 'first' or 'last', determine if the portfolio is rebalanced at the close of the first or last market day of the rebalancing period.
    /// </summary>
    [JsonProperty(PropertyName = "rebalance_on")]
    public string RebalanceOn { get; private set; }

    /// <summary>
    /// in the unit of years, size of rolling window that is used to make the prediction for each rebalancing.
    /// </summary>
    [JsonProperty(PropertyName = "training_data_size")]
    public int TrainingDataSize { get; private set; }

    /// <summary>
    /// Key performance metric to optimize, can be chosen from 'return', 'risk', 'sharpe', 'CAGR', 'UI', 'UPI', or 'MaxDD'.
    /// For risk related metrics, i.e. 'risk', 'UI', and 'MaxDD', `max_cash` will be overridden since Cash will always have zero and hence the minimal risk.
    /// </summary>
    [JsonProperty(PropertyName = "evaluation_metric")]
    public string EvaluationMetric { get; private set; }

    /// <summary>
    /// 'none' or 'feature_file1.csv, feature_file2.csv, ...', client feature to be included, and these files should be uploaded first.
    /// </summary>
    [JsonProperty(PropertyName = "feature_file", NullValueHandling = NullValueHandling.Ignore)]
    public string? FeatureFile { get; private set; }

    /// <summary>
    /// If set to 'yes', 'true', or 'skip', will not include predictnow features. 
    /// Note, if <see cref="FeatureFile"/> is not provided (`none` or not in the parameter dictionary) there would be no X-columns in the prediction model.
    /// </summary>
    [JsonProperty(PropertyName = "skip_PNow_feature", NullValueHandling = NullValueHandling.Ignore)]
    public string? SkipPNowFeature { get; private set; }

    /// <summary>
    /// Creates a new instance of PortfolioParameters
    /// </summary>
    /// <param name="name">Project/Portfolio identification, e.g. `Demo_Project`</param>
    /// <param name="returnsFile">Daily return filename, should be uploaded first, e.g. `ETF_return.csv`</param>
    /// <param name="constraintFile">Portfolio components and their min and max allocations, should be uploaded first, e.g. `ETF_constraint.csv`.. Note it's the `component` column in the <see cref="ConstraintFile"/> that defines the current portfolio universe.</param>
    /// <param name="maxCash">Maximum cash allocation allowed when risk is predicted to be large, float between 0 and 1 where 1 correspond to 100% (no market exposure).</param>
    /// <param name="rebalancingPeriodUnit">'week' or 'month', used with `rebalancing_period`.</param>
    /// <param name="rebalancingPeriod">If `rebalancing_period = 2` and <see cref="RebalancingPeriodUnit"/> = 'week'`, the portfolio would rebalanced every other week.</param>
    /// <param name="rebalanceOn">'first' or 'last', determine if the portfolio is rebalanced at the close of the first or last market day of the rebalancing period.</param>
    /// <param name="trainingDataSize">In the unit of years, size of rolling window that is used to make the prediction for each rebalancing.</param>
    /// <param name="evaluationMetric">Key performance metric to optimize, can be chosen from 'return', 'risk', 'sharpe', 'CAGR', 'UI', 'UPI', or 'MaxDD'. For risk related metrics, i.e. 'risk', 'UI', and 'MaxDD', `max_cash` will be overridden since Cash will always have zero and hence the minimal risk.</param>
    /// <param name="featureFile">'none' or 'feature_file1.csv, feature_file2.csv, ...', client feature to be included, and these files should be uploaded first.</param>
    /// <param name="skipPNowFeature">if set to 'yes', 'true', or 'skip', will not include predictnow features. Note, if <see cref="FeatureFile"/> is not provided (`none` or not in the parameter dictionary) there would be no X-columns in the prediction model.</param>
    public PortfolioParameters(string name, string returnsFile, string constraintFile,
        double maxCash, string rebalancingPeriodUnit, int rebalancingPeriod, string rebalanceOn,
        int trainingDataSize, string evaluationMetric, string? featureFile = null, string? skipPNowFeature = null)
    {
        Name = name;
        ReturnsFile = returnsFile;
        ConstraintFile = constraintFile;
        MaxCash = maxCash;
        RebalancingPeriodUnit = rebalancingPeriodUnit;
        RebalancingPeriod = rebalancingPeriod;
        RebalanceOn = rebalanceOn;
        TrainingDataSize = trainingDataSize;
        EvaluationMetric = evaluationMetric;
        FeatureFile = featureFile;
        SkipPNowFeature = skipPNowFeature;
    }

    /// <summary>
    /// Sets the user's Id
    /// </summary>
    /// <param name="userId"></param>
    public void SetUserId(string userId) => _userId = userId;

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString() => JsonConvert.SerializeObject(this);
}