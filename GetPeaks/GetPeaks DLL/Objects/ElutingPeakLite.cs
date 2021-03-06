﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetPeaks_DLL.Objects
{
    public class ElutingPeakLite
    {
        #region Constructors

        public ElutingPeakLite()
        {
            this.ID = -1;
            this.ScanStart = -1;
            this.ScanEnd = -1;
            this.ScanMaxIntensity = -1;

            this.RetentionTime = 0;
            this.Intensity = 0;
            this.SummedIntensity = 0;
            this.AggregateIntensity = 0;
            this.Mass = 0;
            this.ChargeState = 0;

            this.NumberOfPeaks = 0;
            this.NumberOfPeaksFlag = 0;
            this.NumberOfPeaksMode = "Current"; //"Current" for current peak or "NewPeak" for possible next peak after this one
            
        }

        #endregion

        #region Properties

        public int ID { get; set; }

        public float RetentionTime { get; set; }

        public double Mass { get; set; }

        public float Intensity { get; set; }

        /// <summary>
        /// Summed intensity across time
        /// </summary>
        public float SummedIntensity { get; set; }

        public int ScanStart { get; set; }

        public int ScanEnd { get; set; }

        public int ScanMaxIntensity { get; set; }

        public int NumberOfPeaks { get; set; }

        public int NumberOfPeaksFlag { get; set; }

        public string NumberOfPeaksMode { get; set; }

        public int ChargeState { get; set; }

        /// <summary>
        /// Aggregate intensity across mass
        /// </summary>
        public double AggregateIntensity { get; set; }

        #endregion
    }
}
