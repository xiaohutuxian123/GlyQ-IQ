﻿using System;
using IQ.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using IQ.Workflows;
using Run32.Backend.Core;
using Run32.Backend.Core.Results;
using Run32.Utilities;

namespace IQ.Backend.ProcessingTasks.Quantifiers
{
    public class N14N15QuantifierTask : IQ.Backend.Core.TaskIQ
    {

        N15IsotopeProfileGenerator _N15IsotopicProfileGenerator = new N15IsotopeProfileGenerator();


        #region Constructors
        public N14N15QuantifierTask()
            : this(3, 25)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numPeaksUsedInRatioCalc">Number of peaks used in the ratio calculation</param>
        public N14N15QuantifierTask(int numPeaksUsedInRatioCalc, double msToleranceInPPM)
        {
            this.NumPeaksUsedInRatioCalc = numPeaksUsedInRatioCalc;
            this.MSToleranceInPPM = msToleranceInPPM;
            this.RatioType = GlobalsWorkFlow.RatioType.ISO2_OVER_ISO1;
        }


        public N14N15QuantifierTask(int numPeaksUsedInRatioCalc, double msToleranceInPPM, GlobalsWorkFlow.RatioType ratioType)
            : this(numPeaksUsedInRatioCalc, msToleranceInPPM)
        {
            this.RatioType = ratioType;
        }

        #endregion

        #region Properties
        public int NumPeaksUsedInRatioCalc { get; set; }
        public double MSToleranceInPPM { get; set; }
        public GlobalsWorkFlow.RatioType RatioType { get; set; }


        #endregion

        #region Public Methods


        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultList)
        {
            TargetBase mt = resultList.Run.CurrentMassTag;
            Check.Require(mt != null, "Current mass tag is not defined.");

            TargetedResultBase currentResult = resultList.CurrentTargetedResult;

            Check.Require(currentResult != null, "Quantifier failed. Result doesn't exist for current mass tag.");
            Check.Require(currentResult is N14N15_TResult, "Quantifier failed. Result is not of the N14N15 type.");

            N14N15_TResult n14n15Result = ((N14N15_TResult)currentResult);

            BasicN14N15Quantifier quant = new BasicN14N15Quantifier(this.MSToleranceInPPM,this.RatioType);

            IsotopicProfile iso1 = ((N14N15_TResult)currentResult).IsotopicProfile;
            IsotopicProfile iso2 = ((N14N15_TResult)currentResult).IsotopicProfileLabeled;

            if (iso1 == null || iso2 == null)
            {
                n14n15Result.RatioN14N15 = -1;
            }
            else
            {
                double ratioContribIso1;
                double ratioContribIso2;
                double ratio;


                quant.GetRatioBasedOnTopPeaks(iso1, iso2, mt.IsotopicProfile, _N15IsotopicProfileGenerator.GetN15IsotopicProfile(mt, 0.005),
                    currentResult.ScanSet.BackgroundIntensity, NumPeaksUsedInRatioCalc,
                    out ratio, out ratioContribIso1, out ratioContribIso2);

                n14n15Result.RatioN14N15 = ratio;
                n14n15Result.RatioContributionN14 = ratioContribIso1;
                n14n15Result.RatioContributionN15 = ratioContribIso2;

            }

            //((N14N15_TResult)currentResult).RatioN14N15 = quant.GetRatioBasedOnAreaUnderPeaks(resultColl.Run.XYData.Xvalues, resultColl.Run.XYData.Yvalues, iso1, iso2, currentResult.ScanSet.BackgroundIntensity);


        }

        private object IsotopicProfile(N14N15_TResult n14N15_TResult)
        {
            throw new NotImplementedException();
        }


    }
}
