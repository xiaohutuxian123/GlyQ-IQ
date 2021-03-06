﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PNNLOmics.Data;
using Run64.Backend.Data;
using Run64.DeconEngine.PeakDetection;
using Peak = Run64.Backend.Core.Peak;
using XYData = Run64.Backend.Data.XYData;
//using DeconTools.Backend.Data;

namespace Run64.Backend.Core
{
    [Serializable]
    public abstract class Run : IDisposable
    {
        public Run()
        {
            ScanSetCollection = new ScanSetCollection();
            ResultCollection = new ResultCollection(this);
            XYData = new XYData();
            MSLevelList = new SortedDictionary<int, byte>();
            ScanToNETAlignmentData = new SortedDictionary<int, float>();
            PrimaryLcScanNumbers = new List<int>();

            IsMsAbundanceReportedAsAverage = false;

        }

        #region Properties

        [field: NonSerialized]
        private clsPeak2[] deconToolsPeakList;
        public clsPeak2[] DeconToolsPeakList        //I need to change this later; don't want anything connected to DeconEngine in this class
        {
            get { return deconToolsPeakList; }
            set { deconToolsPeakList = value; }
        }

        private string filename;
        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        public string DatasetName { get; set; }

        public string DataSetPath { get; set; }

        private Globals.MSFileType mSFileType;
        public Globals.MSFileType MSFileType
        {
            get { return mSFileType; }
            set { mSFileType = value; }
        }

        public int MinLCScan { get; set; }
        public int MaxLCScan { get; set; }

        private bool areRunResultsSerialized;   //this is a flag to indicate whether or not Run's results were written to disk
        public bool AreRunResultsSerialized
        {
            get { return areRunResultsSerialized; }
            set { areRunResultsSerialized = value; }
        }

        [field: NonSerialized]
        private List<Peak> peakList;
        public List<Peak> PeakList
        {
            get { return peakList; }
            set { peakList = value; }
        }

        public ScanSetCollection ScanSetCollection { get; set; }

        private ResultCollection resultCollection;
        public ResultCollection ResultCollection
        {
            get { return resultCollection; }
            set { resultCollection = value; }
        }

        private ScanSet currentScanSet;
        public virtual ScanSet CurrentScanSet
        {
            get { return currentScanSet; }
            set { currentScanSet = value; }
        }

        private bool isDataThresholded;
        public bool IsDataThresholded          //not crazy about having this here; may want to change later
        {
            get { return isDataThresholded; }
            set { isDataThresholded = value; }
        }

        private SortedDictionary<int, float> scanToNETAlignmentData;
        
        internal SortedDictionary<int, float> ScanToNETAlignmentData
        {
            get
            {
                return scanToNETAlignmentData;
            }
            set
            {
                scanToNETAlignmentData = value;
            }
        }

        public virtual bool ContainsMSMSData { get; set; }

        private List<int> msLevelScanIndexList { get; set; }

        public IList<int> MSLevelMappings { get; set; }

        public List<int> PrimaryLcScanNumbers { get; set; }

        public bool MassIsAligned
        {
            get
            {
                return false;
            }
        }

        public bool NETIsAligned { get; set; }

        public double CurrentBackgroundIntensity { get; set; }


        #endregion

        public abstract XYData XYData { get; set; }
        public abstract int GetNumMSScans();
        public abstract double GetTime(int scanNum);
        public abstract int GetMSLevelFromRawData(int scanNum);



        /// <summary>
        /// Returns Scan information.
        /// </summary>
        /// <param name="scanNum"></param>
        /// <returns></returns>
        public virtual string GetScanInfo(int scanNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(scanNum.ToString());
            return sb.ToString();
        }

        public virtual int GetMSLevel(int scanNum)
        {
            // check to see if we have a value already stored
            if (this.MSLevelList.ContainsKey(scanNum))
            {
                return this.MSLevelList[scanNum];
            }
            // if not, look up MSLevel from Raw data
            else
            {
                int mslevel = GetMSLevelFromRawData(scanNum);

                this.MSLevelList.Add(scanNum, (byte)mslevel);

                return mslevel;
            }
        }

        public virtual PrecursorInfo GetPrecursorInfo(int scanNum)
        {
            throw new System.NotImplementedException("GetPrecursorInfo is not available for this Run type.");
        }

        public virtual bool IsDataCentroided(int scanNum)
        {
            throw new System.NotImplementedException("'IsDataCentroided' method is not available for this Run type.");
        }


        public virtual XYData GetMassSpectrum(ScanSet scanset)
        {
            //set a wide mz range so we get everything
            return GetMassSpectrum(scanset, 0, 100000);
        }

        public abstract XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ);





        public virtual double GetTICFromInstrumentInfo(int scanNum)
        {
            return 0;
        }



        public TargetBase CurrentMassTag { get; set; }

        public virtual XYData GetMassSpectrum(ScanSet lcScanset, ScanSet imsScanset, double minMZ, double maxMZ)
        {
            throw new NotSupportedException("This overload of GetMassSpectrum is only supported in IMS dataset types");
        }

        #region Methods

        public abstract int GetMinPossibleLCScanNum();

        public abstract int GetMaxPossibleLCScanNum();
        //{
        ////default:  scan is zero-based
        //int maxpossibleScanIndex = GetNumMSScans() - 1;           
        //if (maxpossibleScanIndex < 0) maxpossibleScanIndex = 0;

        //return maxpossibleScanIndex;
        //}

        internal void ResetXYPoints()
        {
            this.XYData = new XYData();
            this.XYData.Xvalues = new double[1];   //TODO:  re-examine this. I do this to allow
            this.XYData.Yvalues = new double[1];
        }

        internal void FilterXYPointsByMZRange(double minMZ, double maxMZ)
        {
            List<double> filteredXValues = new List<double>();
            List<double> filteredYValues = new List<double>();

            int numPointsToAdd = 0;

            for (int i = 0; i < XYData.Xvalues.Length; i++)
            {
                if (XYData.Xvalues[i] >= minMZ)
                {
                    if (XYData.Xvalues[i] <= maxMZ)
                    {
                        //filteredXValues.Add(XYData.Xvalues[i]);
                        //filteredYValues.Add(XYData.Yvalues[i]);
                        numPointsToAdd++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            double[] xvals = new double[numPointsToAdd];
            double[] yvals = new double[numPointsToAdd];

            int counter = 0;
            for (int i = 0; i < XYData.Xvalues.Length; i++)
            {

                if (XYData.Xvalues[i] >= minMZ)
                {
                    if (XYData.Xvalues[i] <= maxMZ)
                    {
                        xvals[counter] = XYData.Xvalues[i];
                        yvals[counter] = XYData.Yvalues[i];
                        counter++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //XYData.Xvalues = filteredXValues.ToArray();
            //XYData.Yvalues = filteredYValues.ToArray();

            XYData.Xvalues = xvals;
            XYData.Yvalues = yvals;
        }

        #endregion

        //public void CreateScanSetCollection()
        //{
        //    Check.Require(GetNumMSScans() > 0, "Cannot create set of scans to be analyzed; Reason: number of MS scans is 0");
        //    CreateScanSetCollection(0, GetNumMSScans() - 1);
        //}

        //public void CreateScanSetCollection(int minScan, int maxScan)
        //{
        //    for (int i = minScan; i <= maxScan; i++)
        //    {
        //        this.ScanSetCollection.ScanSetList.Add(new ScanSet(i));
        //    }
        //}

        public virtual int GetCurrentScanOrFrame()
        {
            return this.CurrentScanSet.PrimaryScanNumber;
        }

        public virtual int GetClosestMSScan(int inputScan, Globals.ScanSelectionMode scanSelectionMode)
        {


            switch (scanSelectionMode)
            {
                case Globals.ScanSelectionMode.ASCENDING:
                    for (int i = inputScan; i <= this.MaxLCScan; i++)     // MaxScan is an index value
                    {
                        if (this.GetMSLevel(i) == 1) return i;
                    }
                    // reached end of scans. Don't want to throw a nasty error, so return what was inputted.
                    return inputScan;

                case Globals.ScanSelectionMode.DESCENDING:
                    for (int i = inputScan; i >= this.MinLCScan; i--)
                    {
                        if (this.GetMSLevel(i) == 1) return i;
                    }
                    //reached starting scan. No MS-level scan was found.  Simply return what was inputted. 
                    return inputScan;

                case Globals.ScanSelectionMode.CLOSEST:
                    int upperScan = -1;
                    int lowerScan = -1;
                    if (this.GetMSLevel(inputScan) == 1) return inputScan;
                    for (int i = inputScan; i <= this.MaxLCScan; i++)
                    {
                        if (this.GetMSLevel(i) == 1)
                        {
                            upperScan = i;
                            break;
                        }
                    }
                    for (int i = inputScan; i >= this.MinLCScan; i--)
                    {
                        if (this.GetMSLevel(i) == 1)
                        {
                            lowerScan = i;
                            break;
                        }
                    }

                    if (upperScan == -1 && lowerScan == -1) return inputScan;
                    if (upperScan == -1 && lowerScan != -1) return lowerScan;
                    if (lowerScan == -1 && upperScan != -1) return upperScan;

                    if (Math.Abs(upperScan - inputScan) < Math.Abs(lowerScan - inputScan))
                    {
                        return upperScan;
                    }
                    else
                    {
                        return lowerScan;
                    }

                default:

                    return inputScan;
            }

        }

        /// <summary>
        /// Purpose is to return an array of MS1-level Scan values for the entire Run. 
        /// </summary>
        /// <returns>
        /// List of Scan numbers pertaining to MS1-level scans only. 
        /// </returns>
        public List<int> GetMSLevelScanValues()
        {
            return GetMSLevelScanValues(this.MinLCScan, this.MaxLCScan);
        }

        public List<int> GetMSLevelScanValues(int minScan, int maxScan)
        {
            if (this.msLevelScanIndexList == null)
            {
                msLevelScanIndexList = new List<int>();

                for (int i = minScan; i <= maxScan; i++)
                {
                    if (this.ContainsMSMSData)   // contains MS1 and MS2
                    {
                        if (this.GetMSLevel(i) == 1)
                        {
                            msLevelScanIndexList.Add(i);
                        }
                    }
                    else      // all MS are MS1
                    {
                        msLevelScanIndexList.Add(i);
                    }
                }
            }
            else
            {

            }
            return this.msLevelScanIndexList;
        }

        protected void addToMSLevelData(int scanNum, int mslevel)
        {
            if (this.MSLevelList.ContainsKey(scanNum))
            {
                // do nothing
            }
            else
            {
                this.MSLevelList.Add(scanNum, (byte)mslevel);
            }

        }

        protected SortedDictionary<int, byte> MSLevelList { get; set; }

        protected SortedDictionary<int, int> ParentScanList { get; set; }

        /// <summary>
        /// This indicates whether or not the intensity values from a summed/averaged mass spectrum is reported as an average or not.
        /// e.g. Thermo .raw reports values as an average
        /// </summary>
        public bool IsMsAbundanceReportedAsAverage { get; set; }

        public SortedDictionary<int, byte> GetMSLevels(int minScan, int maxScan)
        {
            return null;
        }

        public virtual void Close()
        {
            if (this.PeakList != null)
            {
                this.PeakList.Clear();
            }
            this.ResultCollection.ClearAllResults();
            this.XYData = null;

            GC.Collect();
        }

        public virtual string GetCurrentScanOrFrameInfo()
        {
            if (this.CurrentScanSet != null)
            {
                return "Scan = " + this.CurrentScanSet.PrimaryScanNumber.ToString();
            }
            else
            {
                return "Scan = NULL";
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            this.Close();
        }

        #endregion

        #region Mass and NET Alignment
        public virtual void CreateDefaultScanToNETAlignmentData()
        {
            scanToNETAlignmentData = new SortedDictionary<int, float>();

            List<ScanNETPair> scanNETList = new List<ScanNETPair>();

            for (int i = this.MinLCScan; i <= this.MaxLCScan; i++)
            {
                ScanNETPair snp = new ScanNETPair((float)i, i / (float)this.MaxLCScan);
                scanNETList.Add(snp);
            }

            SetScanToNETAlignmentData(scanNETList);
            this.NETIsAligned = false;

        }

        public virtual float GetNETValueForScan(int scanNum)
        {
            if (this.ScanToNETAlignmentData == null || this.ScanToNETAlignmentData.Count == 0)
            {
                CreateDefaultScanToNETAlignmentData();

                bool scanToNETTableIsStillEmpty = this.ScanToNETAlignmentData == null || this.ScanToNETAlignmentData.Count == 0;
                if (scanToNETTableIsStillEmpty)
                {
                    throw new ArgumentException("Scan-to-NET table is empty. Tried to create it from Dataset but failed.");
                }

            }


            if (this.ScanToNETAlignmentData.ContainsKey(scanNum))
            {
                return (float)this.ScanToNETAlignmentData[scanNum];
            }
            else
            {
                return calculateNET(scanNum);
            }

        }


        public int GetScanValueForNET(float netVal)
        {
            if (this.ScanToNETAlignmentData == null || this.ScanToNETAlignmentData.Count == 0)
            {
                CreateDefaultScanToNETAlignmentData();

                bool scanToNETTableIsStillEmpty = this.ScanToNETAlignmentData == null || this.ScanToNETAlignmentData.Count == 0;
                if (scanToNETTableIsStillEmpty)
                {
                    throw new ArgumentException("Scan-to-NET table is empty. Tried to create it from Dataset but failed.");
                }
            }

            return (int)CalculateScanForNET(netVal);



        }


        //GORD: look carefully here for any scan/NET skewing 
        protected virtual float CalculateScanForNET(float net)
        {
            //need to find the two (scan,net) pairs that are the lower and upper boundaries of the input NET
            //then do an intersect

            KeyValuePair<int, float> closestNETPair = new KeyValuePair<int, float>();


            int lowerScan = this.MinLCScan;
            int upperScan = this.MaxLCScan;

            float lowerNET = 0;
            float upperNET = 1;


            //first find the closest ScanNET pair
            float diff = float.MaxValue;
            foreach (var item in this.ScanToNETAlignmentData)
            {
                float currentDiff = Math.Abs(item.Value - net);
                if (currentDiff < diff)
                {
                    closestNETPair = item;
                    diff = currentDiff;
                }

            }

            //we found either the point above the inputted NET or below. Need to fill the appropriate lower and upper scan/NET
            bool isLowerThanInputNET = closestNETPair.Value <= net;

            if (isLowerThanInputNET)
            {
                lowerScan = closestNETPair.Key;
                lowerNET = closestNETPair.Value;

                bool found = false;
                int currentScan = lowerScan + 1; //add one and then start looking for next higher scan
                while (!found && currentScan <= MaxLCScan)
                {
                    currentScan++;
                    if (this.ScanToNETAlignmentData.ContainsKey(currentScan))
                    {
                        upperScan = currentScan;
                        upperNET = this.ScanToNETAlignmentData[upperScan];
                        found = true;
                    }

                }


            }
            else
            {
                upperScan = closestNETPair.Key;
                upperNET = closestNETPair.Value;

                bool found = false;
                int currentScan = upperScan - 1;

                while (!found && currentScan > this.MinLCScan)
                {
                    currentScan--;
                    if (this.ScanToNETAlignmentData.ContainsKey(currentScan))
                    {
                        lowerScan = currentScan;
                        lowerNET = this.ScanToNETAlignmentData[lowerScan];
                        found = true;
                    }
                }


            }


            if (upperScan <= lowerScan)    //this happens at the MinScan
            {

                return lowerScan;
            }

            float slope = (upperNET - lowerNET) / (upperScan - lowerScan);
            float yintercept = (upperNET - slope * upperScan);

            float xvalue = (net - yintercept) / slope;

            if (xvalue < this.MinLCScan)
            {
                xvalue = this.MinLCScan;
            }

            if (xvalue > this.MaxLCScan)
            {
                xvalue = this.MaxLCScan;
            }

            return xvalue;




        }

        private float calculateNET(int scanNum)
        {
            if (scanNum < 2) return 0;
            int maxScan = this.MaxLCScan;


            double lowerNET = 0;
            double upperNET = 1;
            int lowerScan = 1;
            int upperScan = maxScan;


            bool found = false;
            int currentScan = scanNum;

            while (!found && currentScan > 0)
            {
                currentScan--;
                if (this.ScanToNETAlignmentData.ContainsKey(currentScan))
                {
                    lowerScan = currentScan;
                    lowerNET = this.ScanToNETAlignmentData[lowerScan];
                    found = true;
                }
            }

            found = false;
            currentScan = scanNum;
            while (!found && currentScan <= maxScan)
            {
                currentScan++;
                if (this.ScanToNETAlignmentData.ContainsKey(currentScan))
                {
                    upperScan = currentScan;
                    upperNET = this.ScanToNETAlignmentData[upperScan];
                    found = true;
                }

            }

            double slope = (upperNET - lowerNET) / (upperScan - lowerScan);
            double yintercept = (upperNET - slope * upperScan);

            return (float)(scanNum * slope + yintercept);

        }




        public virtual int GetNearestScanValueForNET(double minNetVal)
        {
            if (this.ScanToNETAlignmentData == null || this.ScanToNETAlignmentData.Count == 0) return -1;

            double diff = double.MaxValue;
            int closestScan = -1;


            foreach (var keyValuePair in this.ScanToNETAlignmentData)
            {
                double currentDiff = Math.Abs(keyValuePair.Value - minNetVal);
                if (currentDiff < diff)
                {
                    closestScan = keyValuePair.Key;
                    diff = currentDiff;
                }
            }
            return closestScan;


        }


        public void SetScanToNETAlignmentData(List<ScanNETPair> scanNETList)
        {
            float[] scanVals = scanNETList.Select(p => p.Scan).ToArray();
            float[] netVals = scanNETList.Select(p => p.NET).ToArray();

            SetScanToNETAlignmentData(scanVals, netVals);

        }

        public virtual void SetScanToNETAlignmentData(float[] scanVals, float[] netVals)
        {
            this.ScanToNETAlignmentData.Clear();


            for (int i = 0; i < scanVals.Length; i++)
            {
                int scanToAdd = (int)(Math.Round(scanVals[i]));

                if (!this.ScanToNETAlignmentData.ContainsKey(scanToAdd))
                {
                    this.ScanToNETAlignmentData.Add(scanToAdd, netVals[i]);
                }
            }

            this.NETIsAligned = true;   //Keep an eye on this!

            UpdateNETValuesInScanSetCollection();

        }

        

        public virtual void UpdateNETValuesInScanSetCollection()
        {

            if (ScanSetCollection == null) return;

            foreach (ScanSet scan in ScanSetCollection.ScanSetList)
            {
                if (this.ScanToNETAlignmentData.ContainsKey(scan.PrimaryScanNumber))
                {
                    scan.NETValue = (float)this.ScanToNETAlignmentData[scan.PrimaryScanNumber];
                }
                else
                {
                    scan.NETValue = calculateNET(scan.PrimaryScanNumber);
                }

            }
        }



        ///// <summary>
        ///// Returns the adjusted m/z after alignment
        ///// </summary>
        ///// <param name="observedMZ"></param>
        ///// <param name="scan"></param>
        ///// <returns></returns>
        //public double GetAlignedMZ(double observedMZ, double scan=-1)
        //{
        //    if (this.AlignmentInfo == null) return observedMZ;

        //    float ppmShift = GetPPMShift(observedMZ, scan);


        //    double alignedMZ = observedMZ - (ppmShift * observedMZ / 1e6);
        //    return alignedMZ;
        //}
    
        ///// <summary>
        ///// The method returns the m/z that you should look for, when m/z alignment is considered
        ///// </summary>
        ///// <param name="theorMZ"></param>
        ///// <param name="scan"> </param>
        ///// <returns></returns>
        //public double GetTargetMZAligned(double theorMZ, double scan=-1)
        //{
        //    if (AlignmentInfo == null) return theorMZ;

        //    float ppmShift = GetPPMShift(theorMZ, scan);

        //    double alignedMZ = theorMZ + (ppmShift * theorMZ / 1e6);
        //    return alignedMZ;
        //}

        ///// <summary>
        ///// Return the calibration information (mass shift) for a given m/z value and (optionally) at a given  scan
        ///// </summary>
        ///// <param name="inputMz"></param>
        ///// <param name="scan">Optional</param>
        ///// <returns></returns>
        //public float GetPPMShift(double inputMz, double scan=-1)
        //{
        //    if (AlignmentInfo == null) return 0;

        //    bool alignmentInfoContainsScanInfo = (this.AlignmentInfo.marrMassFncTimeInput != null && this.AlignmentInfo.marrMassFncTimeInput.Length > 0);
        //    bool alignmentInfoContainsMZInfo = (this.AlignmentInfo.marrMassFncMZInput != null && this.AlignmentInfo.marrMassFncMZInput.Length > 0);
            
        //    bool canUseScanWhenGettingPPMShift = alignmentInfoContainsScanInfo && alignmentInfoContainsMZInfo && scan >= 0;
            
        //    float mzForGettingAlignmentInfo;
        //    if (alignmentInfoContainsMZInfo)
        //    {
        //        //check if mz is less than lower limit
        //        if (inputMz < AlignmentInfo.marrMassFncMZInput[0])
        //        {
        //            mzForGettingAlignmentInfo = AlignmentInfo.marrMassFncMZInput[0];
        //        }
        //        else if (inputMz > AlignmentInfo.marrMassFncMZInput[AlignmentInfo.marrMassFncMZInput.Length - 1])   //check if mz is greater than upper limit
        //        {
        //            mzForGettingAlignmentInfo = AlignmentInfo.marrMassFncMZInput[AlignmentInfo.marrMassFncMZInput.Length - 1];
        //        }
        //        else
        //        {
        //            mzForGettingAlignmentInfo = (float)inputMz;
        //        }

        //    }
        //    else
        //    {
        //        return 0;
        //    }


        //    if (canUseScanWhenGettingPPMShift)
        //    {
        //        var scanForGettingAlignmentInfo = (float)scan;

        //        if (scanForGettingAlignmentInfo < AlignmentInfo.marrMassFncTimeInput[0])
        //        {
        //            scanForGettingAlignmentInfo = AlignmentInfo.marrMassFncTimeInput[0];
        //        }
        //        else if (scanForGettingAlignmentInfo > AlignmentInfo.marrMassFncTimeInput[AlignmentInfo.marrMassFncTimeInput.Length - 1])
        //        {
        //            scanForGettingAlignmentInfo = AlignmentInfo.marrMassFncTimeInput[AlignmentInfo.marrMassFncTimeInput.Length - 1];
        //        }
        //        else
        //        {
        //            scanForGettingAlignmentInfo = (float)scan;
        //        }

        //        var ppmShift = this.AlignmentInfo.GetPPMShiftFromTimeMZ(scanForGettingAlignmentInfo, mzForGettingAlignmentInfo);
        //        return ppmShift;
        //    }
        //    else
        //    {
        //        var ppmShift = this.AlignmentInfo.GetPPMShiftFromMZ(mzForGettingAlignmentInfo);
        //        return ppmShift;
        //    }


        //}

  


        #endregion

        public virtual int GetParentScan(int scanLC)
        {
            throw new NotImplementedException();
        }

        public virtual double GetIonInjectionTimeInMilliseconds(int scanNum)
        {
            return 0;
        }


    }
}
