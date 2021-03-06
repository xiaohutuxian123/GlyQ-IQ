﻿using System;

namespace Run64.Backend.Core
{
    [Serializable]
    public abstract class ScanResult
    {

        public ScanResult(ScanSet scanset)
        {
            ScanSet = scanset;
        }

        protected ScanResult()
        {
            ScanSet = null;
        }

        public Run Run { get; set; }
        public  int NumPeaks { get; set; }
        public  int NumIsotopicProfiles { get; set; }
        public  Peak BasePeak { get; set; }
        public  float TICValue { get; set; }
        public  ScanSet ScanSet { get; set; }
        public double ScanTime { get; set; }
        public int SpectrumType { get; set; }
        public string Description { get; set; }
    }
}
