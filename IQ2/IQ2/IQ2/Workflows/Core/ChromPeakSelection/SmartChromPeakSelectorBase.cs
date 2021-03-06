﻿using System;
using System.Collections.Generic;
using System.Linq;
using IQ.Backend.ProcessingTasks;
using IQ.Backend.ProcessingTasks.FitScoreCalculators;
using IQ.Backend.ProcessingTasks.MSGenerators;
using IQ.Backend.ProcessingTasks.PeakDetectors;
using IQ.Backend.ProcessingTasks.ResultValidators;
using IQ.Backend.ProcessingTasks.TargetedFeatureFinders;
using Run32.Backend;
using Run32.Backend.Core;
using Run32.Backend.Core.Results;
using Run32.Utilities;

namespace IQ.Workflows.Core.ChromPeakSelection
{



    public abstract class SmartChromPeakSelectorBase : ChromPeakSelectorBase
    {
        protected MSGenerator msgen;
        protected ResultValidatorTask resultValidator;
        protected IsotopicProfileFitScoreCalculator fitScoreCalc;
        protected InterferenceScorer InterferenceScorer;
        protected DeconToolsPeakDetectorV2 MSPeakDetector;
        protected IterativeTFF TargetedMSFeatureFinder;

        //public DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders.BasicTFF TargetedMSFeatureFinder { get; set; }

        #region Constructors

        public SmartChromPeakSelectorBase():base()
        {
            
        }

        #endregion

        #region Properties

      




        protected SmartChromPeakSelectorParameters _parameters;
        public override ChromPeakSelectorParameters Parameters
        {
            get { return _parameters; }
            set { _parameters = value as SmartChromPeakSelectorParameters; }
        }
        #endregion

        #region Public Methods




        public override Peak SelectBestPeak(List<ChromPeakQualityData> peakQualityList, bool filterOutFlaggedIsotopicProfiles)
        {

            //flagging algorithm checks for peak-to-the-left. This is ok for peptides whose first isotope
            //is most abundant, but not good for large peptides in which the mono peak is of lower intensity. 


            var filteredList1 = (from n in peakQualityList
                                 where n.IsotopicProfileFound &&
                                 n.FitScore < 1 && n.InterferenceScore < 1
                                 select n).ToList();


            if (filterOutFlaggedIsotopicProfiles)
            {
                filteredList1 = filteredList1.Where(p => p.IsIsotopicProfileFlagged == false).ToList();
            }

            ChromPeak bestpeak;

            //target.NumQualityChromPeaks = filteredList1.Count;

            if (filteredList1.Count == 0)
            {
                bestpeak = null;
                //currentResult.FailedResult = true;
                //currentResult.FailureType = Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
            }
            else if (filteredList1.Count == 1)
            {
                bestpeak = filteredList1[0].Peak;
            }
            else
            {
                filteredList1 = filteredList1.OrderBy(p => p.FitScore).ToList();

                double diffFirstAndSecondFitScores = Math.Abs(filteredList1[0].FitScore - filteredList1[1].FitScore);

                bool differenceIsSmall = (diffFirstAndSecondFitScores < 0.05);
                if (differenceIsSmall)
                {
                    if (_parameters.MultipleHighQualityMatchesAreAllowed)
                    {

                        if (filteredList1[0].Abundance >= filteredList1[1].Abundance)
                        {
                            bestpeak = filteredList1[0].Peak;
                        }
                        else
                        {
                            bestpeak = filteredList1[1].Peak;
                        }
                    }
                    else
                    {
                        bestpeak = null;
                        //currentResult.FailedResult = true;
                        //currentResult.FailureType = Globals.TargetedResultFailureType.TooManyHighQualityChrompeaks;
                    }
                }
                else
                {
                    bestpeak = filteredList1[0].Peak;
                }
            }

            return bestpeak;
        }
        
        [Obsolete("This is the old SmartChromPeakSelector")]
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed. MassTag was not defined.");

            TargetedResultBase currentResult = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);

            if (msgen == null)
            {
                msgen = MSGeneratorFactory.CreateMSGenerator(resultList.Run.MSFileType);
                msgen.IsTICRequested = false;
            }

            TargetBase mt = resultList.Run.CurrentMassTag;

            float normalizedElutionTime;

            if (currentResult.Run.CurrentMassTag.ElutionTimeUnit == Globals.ElutionTimeUnit.ScanNum)
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.ScanLCTarget / (float)currentResult.Run.GetNumMSScans();
            }
            else
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.NormalizedElutionTime;
            }

            //collect Chrom peaks that fall within the NET tolerance
            List<ChromPeak> peaksWithinTol = new List<ChromPeak>(); // 

            foreach (ChromPeak peak in resultList.Run.PeakList)
            {
                if (Math.Abs(peak.NETValue - normalizedElutionTime) <= Parameters.NETTolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }

            List<ChromPeakQualityData> peakQualityList = new List<ChromPeakQualityData>();

            //iterate over peaks within tolerance and score each peak according to MSFeature quality
#if DEBUG
            int tempMinScanWithinTol = resultList.Run.GetScanValueForNET(normalizedElutionTime - Parameters.NETTolerance);
            int tempMaxScanWithinTol = resultList.Run.GetScanValueForNET(normalizedElutionTime + Parameters.NETTolerance);
            int tempCenterTol = resultList.Run.GetScanValueForNET(normalizedElutionTime);


            Console.WriteLine("SmartPeakSelector --> NETTolerance= " + Parameters.NETTolerance + ";  chromMinCenterMax= " + tempMinScanWithinTol + "\t" + tempCenterTol + "" +
                              "\t" + tempMaxScanWithinTol);
            Console.WriteLine("MT= " + currentResult.Target.ID + ";z= " + currentResult.Target.ChargeState + "; mz= " + currentResult.Target.MZ.ToString("0.000") + ";  ------------------------- PeaksWithinTol = " + peaksWithinTol.Count);
#endif

            currentResult.NumChromPeaksWithinTolerance = peaksWithinTol.Count;
            currentResult.NumQualityChromPeaks = -1;

            ChromPeak bestChromPeak;
            if (currentResult.NumChromPeaksWithinTolerance > _parameters.NumChromPeaksAllowed)
            {
                bestChromPeak = null;
            }
            else
            {
                foreach (var chromPeak in peaksWithinTol)
                {
                    ChromPeakQualityData pq = new ChromPeakQualityData(chromPeak);
                    peakQualityList.Add(pq);

                    var lcscanSet= ChromPeakUtilities.GetLCScanSetForChromPeak(chromPeak, resultList.Run, _parameters.NumMSSummedInSmartSelector);
                    resultList.Run.CurrentScanSet = lcscanSet;

                    //This resets the flags and the scores on a given result
                    currentResult.ResetResult();

                    //generate a mass spectrum
                    msgen.Execute(resultList);

                    //find isotopic profile
                    TargetedMSFeatureFinder.Execute(resultList);

                    try
                    {
                        //get fit score
                        fitScoreCalc.Execute(resultList);

                        //get i_score
                        resultValidator.Execute(resultList);
                    }
                    catch (Exception)
                    {
                        currentResult.FailedResult = true;
                    }

                    //collect the results together
                    


                    
                    AddScoresToPeakQualityData(pq, currentResult);
#if DEBUG
                    pq.Display();
#endif
                }

                //run a algorithm that decides, based on fit score mostly. 
                bestChromPeak = determineBestChromPeak(peakQualityList, currentResult);
            }

            currentResult.ChromPeakQualityList = peakQualityList;

            if (Parameters.SummingMode==GlobalsWorkFlow.SummingModeEnum.SUMMINGMODE_STATIC)
            {
                resultList.Run.CurrentScanSet = ChromPeakUtilities.GetLCScanSetForChromPeak(bestChromPeak, resultList.Run, Parameters.NumScansToSum); 
            }
            else
            {
                resultList.Run.CurrentScanSet = ChromPeakUtilities.GetLCScanSetForChromPeakBasedOnPeakWidth(bestChromPeak, resultList.Run, 
                    Parameters.AreaOfPeakToSumInDynamicSumming, Parameters.MaxScansSummedInDynamicSumming);
               
    
            }

            UpdateResultWithChromPeakAndLCScanInfo(currentResult, bestChromPeak);


        }

        #endregion

        #region Private Methods

        [Obsolete("Use the new IQ-based method")]
        protected void AddScoresToPeakQualityData(ChromPeakQualityData pq, TargetedResultBase currentResult)
        {
            if (currentResult.IsotopicProfile == null)
            {
                pq.IsotopicProfileFound = false;
                return;
            }
            else
            {
                pq.IsotopicProfileFound = true;
                pq.Abundance = currentResult.IntensityAggregate;
                pq.FitScore = currentResult.Score;
                pq.InterferenceScore = currentResult.InterferenceScore;
                pq.IsotopicProfile = currentResult.IsotopicProfile;
                bool resultHasFlags = (currentResult.Flags != null && currentResult.Flags.Count > 0);
                pq.IsIsotopicProfileFlagged = resultHasFlags;

                pq.ScanLc = currentResult.Run.CurrentScanSet.PrimaryScanNumber;
            }
        }

        //TODO: delete this if unused
        protected void SetDefaultTargetedFeatureFinderSettings(double toleranceInPPM)
        {
            TargetedMSFeatureFinder.ToleranceInPPM = toleranceInPPM;
        }
        #endregion


        protected virtual ChromPeak determineBestChromPeak(List<ChromPeakQualityData> peakQualityList, TargetedResultBase currentResult)
        {

            //flagging algorithm checks for peak-to-the-left. This is ok for peptides whose first isotope
            //is most abundant, but not good for large peptides in which the mono peak is of lower intensity. 
            bool goodToFilterOnFlaggedIsotopicProfiles = currentResult.Target.IsotopicProfile.GetIndexOfMostIntensePeak() < 5;


            var filteredList1 = (from n in peakQualityList
                                 where n.IsotopicProfileFound &&
                                 n.FitScore < 1 && n.InterferenceScore < 1
                                 select n).ToList();


            if (goodToFilterOnFlaggedIsotopicProfiles)
            {
                filteredList1 = filteredList1.Where(p => p.IsIsotopicProfileFlagged == false).ToList();
            }

            ChromPeak bestpeak;

            currentResult.NumQualityChromPeaks = filteredList1.Count;

            if (filteredList1.Count == 0)
            {
                bestpeak = null;
                currentResult.FailedResult = true;
                currentResult.FailureType = Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
            }
            else if (filteredList1.Count == 1)
            {
                bestpeak = filteredList1[0].Peak;
            }
            else
            {
                filteredList1 = filteredList1.OrderBy(p => p.FitScore).ToList();

                int numCandidatesWithLowFitScores = filteredList1.Count(p => p.FitScore < _parameters.UpperLimitOfGoodFitScore);
                currentResult.NumQualityChromPeaks = numCandidatesWithLowFitScores;


                double diffFirstAndSecondFitScores = Math.Abs(filteredList1[0].FitScore - filteredList1[1].FitScore);

                bool differenceIsSmall = (diffFirstAndSecondFitScores < 0.05);
                if (differenceIsSmall)
                {
                    if (_parameters.MultipleHighQualityMatchesAreAllowed)
                    {

                        if (filteredList1[0].Abundance >= filteredList1[1].Abundance)
                        {
                            bestpeak = filteredList1[0].Peak;
                        }
                        else
                        {
                            bestpeak = filteredList1[1].Peak;
                        }
                    }
                    else
                    {
                        bestpeak = null;
                        currentResult.FailedResult = true;
                        currentResult.FailureType = Globals.TargetedResultFailureType.TooManyHighQualityChrompeaks;
                    }
                }
                else
                {
                    bestpeak = filteredList1[0].Peak;
                }
            }

            return bestpeak;
        }

       
    }
}
