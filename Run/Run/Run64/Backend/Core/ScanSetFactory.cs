﻿using System.Collections.Generic;
using System.Linq;
using Run64.Utilities;

namespace Run64.Backend.Core
{
    public class ScanSetFactory
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods




        public ScanSet CreateScanSet(Run run, int primaryScan, IEnumerable<int> scansToSum)
        {
            //if (run is UIMFRun)
            //{
            //    return new LCScanSetIMS(primaryScan, scansToSum);
            //}
            //else
            //{
                return new ScanSet(primaryScan, scansToSum);
            //}
        }


        public ScanSet CreateScanSet(Run run, int primaryScan, int startScan, int stopScan)
        {
            int currentLevel = run.GetMSLevel(primaryScan);

            var scansToSum = new List<int>();

            for (int i = startScan; i <= stopScan; i++)
            {
                if (run.GetMSLevel(i) == currentLevel)
                {
                    scansToSum.Add(i);
                }

            }
            return CreateScanSet(run, primaryScan, scansToSum);

            //return new ScanSet(primaryScan, scansToSum.ToArray());

        }


        public ScanSet CreateScanSet(Run run, int scan, int scansSummed)
        {
            int currentLevel = run.GetMSLevel(scan);

            List<int> lowerScansToSum = getLowerScans(run, scan, currentLevel, (scansSummed - 1) / 2);
            List<int> upperScansToSum = getUpperScans(run, scan, currentLevel, (scansSummed - 1) / 2);

            List<int> scansToSum = lowerScansToSum.OrderBy(p => p).ToList();
            scansToSum.Add(scan);
            scansToSum.AddRange(upperScansToSum);
            //scansToSum.Sort();

            return CreateScanSet(run, scan, scansToSum);
            //return new ScanSet(scan, scansToSum.ToArray());

        }


        public void TrimScans(ScanSet scanset, int maxScansAllowed)
        {
            Check.Require(maxScansAllowed > 0, "Scans cannot be trimmed to fewer than one");



            if (scanset.IndexValues.Count > maxScansAllowed)
            {
                int numScansToBeRemoved = (scanset.IndexValues.Count - maxScansAllowed + 1) / 2;

                List<int> newScans = new List<int>();

                for (int i = numScansToBeRemoved; i < (scanset.IndexValues.Count - numScansToBeRemoved); i++)    //this loop will cleave off the first n scans and the last n scans
                {
                    newScans.Add(scanset.IndexValues[i]);

                }

                scanset.IndexValues = newScans;


            }






        }


        #endregion



        private List<int> getLowerScans(Run run, int startingScan, int currentMSLevel, int numLowerScansToGet)
        {
            int currentScan = startingScan - 1;
            List<int> lowerScans = new List<int>();

            int scansCounter = 0;
            while (currentScan >= 1 && numLowerScansToGet > scansCounter)
            {
                if (run.GetMSLevel(currentScan) == currentMSLevel)
                {
                    lowerScans.Insert(0, currentScan);
                    scansCounter++;
                }
                currentScan--;


            }
            return lowerScans;

        }
        private List<int> getUpperScans(Run run, int startingScan, int currentMSLevel, int numUpperScansToGet)
        {
            int currentScan = startingScan + 1;
            List<int> scans = new List<int>();

            int scansCounter = 0;
            int scanUppperLimit = run.GetNumMSScans();

            while (currentScan <= scanUppperLimit && numUpperScansToGet > scansCounter)
            {
                if (run.GetMSLevel(currentScan) == currentMSLevel)
                {
                    scans.Add(currentScan);
                    scansCounter++;
                }
                currentScan++;


            }
            return scans;

        }


        #region Private Methods
        #endregion
    }
}
