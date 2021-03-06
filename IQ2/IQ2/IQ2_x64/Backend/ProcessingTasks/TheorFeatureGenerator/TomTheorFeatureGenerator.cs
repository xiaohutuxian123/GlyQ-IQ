﻿using System;
 



using IQ_X64.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using Run64.Backend;
using Run64.Backend.Core;
using Run64.Utilities;

namespace IQ_X64.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public class TomTheorFeatureGenerator : ITheorFeatureGenerator
    {
        #region Constructors

        TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        
        N15IsotopeProfileGenerator _N15IsotopicProfileGenerator = new N15IsotopeProfileGenerator();

        public TomTheorFeatureGenerator()
            : this(Globals.LabellingType.NONE,0.005)
        {
            
        }

        public TomTheorFeatureGenerator(Globals.LabellingType labellingType, double lowPeakCutOff)
        {
            this.LabellingType = labellingType;
            this.LowPeakCutOff = lowPeakCutOff;
        }
        #endregion

        #region Properties

        public Globals.LabellingType LabellingType { get; set; }
        
        /// <summary>
        /// Peaks below the cutoff will be trimmed out from the theoretical profile
        /// </summary>
        public double LowPeakCutOff { get; set; }



        #endregion

        #region Public Methods
        public override void GenerateTheorFeature(TargetBase mt)
        {
            Check.Require(mt != null, "FeatureGenerator failed. MassTag not defined.");
            Check.Require(mt.EmpiricalFormula != null, "Theoretical feature generator failed. Can't retrieve empirical formula from Mass Tag");

            switch (LabellingType)
            {
                case Globals.LabellingType.NONE:

                    mt.IsotopicProfile = GetUnlabelledIsotopicProfile(mt);
                    break;
                case Globals.LabellingType.O18:
                    throw new NotImplementedException();
                case Globals.LabellingType.N15:
                    mt.IsotopicProfileLabelled = _N15IsotopicProfileGenerator.GetN15IsotopicProfile(mt, LowPeakCutOff);
                    
                    break;
                default:
                    break;
            }

        }

       

        private IsotopicProfile GetUnlabelledIsotopicProfile(TargetBase mt)
        {

            IsotopicProfile iso = new IsotopicProfile();

            try
            {
                iso = _tomIsotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula, _tomIsotopicPatternGenerator.aafIsos);
            }
            catch (Exception ex)
            {
                throw new Exception("Theoretical feature generator failed. Details: " + ex.Message);
            }
            
            PeakUtilities.TrimIsotopicProfile(iso, LowPeakCutOff);
            iso.ChargeState = mt.ChargeState;
            if (iso.ChargeState != 0) calculateMassesForIsotopicProfile(iso, mt);

            return iso;
        }


        #endregion

        #region Private Methods
        #endregion


        private void calculateMassesForIsotopicProfile(IsotopicProfile iso, TargetBase mt)
        {
            if (iso == null || iso.Peaklist == null) return;

          

            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                double calcMZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS + i * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / mt.ChargeState;
                iso.Peaklist[i].XValue = calcMZ;
            }

            iso.MonoPeakMZ = iso.Peaklist[0].XValue;
            iso.MonoIsotopicMass = (iso.MonoPeakMZ - Globals.PROTON_MASS) * mt.ChargeState;

        }


        public override void LoadRunRelatedInfo(ResultCollection results)
        {
            // do nothing
        }
    }
}
