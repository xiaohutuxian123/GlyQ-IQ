﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using IQ.Workflows.FileIO.Importers;
using Run32.Backend.Core;
using Run32.Backend.Runs;
using Run32.Utilities;

namespace IQ.Backend.Utilities
{
    public class RunUtilities
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public static Run CreateAndAlignRun(string filename)
        {
            return CreateAndAlignRun(filename, null);
        }

        private static void GetPeaks(Run run, string expectedPeaksFile)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFile, bw);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

			if (!run.PrimaryLcScanNumbers.Any())
			{
				run.PrimaryLcScanNumbers = FindPrimaryLcScanNumbers(run.ResultCollection.MSPeakResultList);
			}
        }

        #endregion

        #region Private Methods

        #endregion

        public static bool AlignRunUsingAlignmentInfoInFiles(Run run, string alignmentDataFolder="")
        {
            bool alignmentSuccessful = false;


            string basePath;
            if (string.IsNullOrEmpty(alignmentDataFolder))
            {
                basePath= run.DataSetPath;
            }
            else
            {
                if (Directory.Exists(alignmentDataFolder))
                {
                    basePath = alignmentDataFolder;
                }
                else
                {
                    throw new DirectoryNotFoundException(
                        "Cannot align dataset. Source alignment folder does not exist. Alignment folder = " +
                        alignmentDataFolder);
                }
            }

            

            string expectedMZAlignmentFile = basePath + Path.DirectorySeparatorChar + run.DatasetName + "_MZAlignment.txt";
            string expectedNETAlignmentFile = basePath + Path.DirectorySeparatorChar + run.DatasetName + "_NETAlignment.txt";

            //first will try to load the multiAlign alignment info
            //if (File.Exists(expectedMZAlignmentFile))
            //{
            //    MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(expectedMZAlignmentFile);

            //    List<MassAlignmentDataItem> massAlignmentData = importer.Import();

            //    run.SetMassAlignmentData(massAlignmentData);


            //}

            //if (File.Exists(expectedNETAlignmentFile))
            //{
            //    NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(expectedNETAlignmentFile);
            //    List<ScanNETPair> scanNETList = netAlignmentInfoImporter.Import();

            //    run.SetScanToNETAlignmentData(scanNETList);

            //}

            //if still not aligned, try to get the NET alignment from UMCs file. (this is the older way to do it)
            if (run.NETIsAligned)
            {

                alignmentSuccessful = true;
            }
            else
            {
                DirectoryInfo alignmentDirInfo = new DirectoryInfo(basePath);
                FileInfo[] umcFileInfo = alignmentDirInfo.GetFiles("*_umcs.txt");

                int umcFileCount = umcFileInfo.Count();

                //if (umcFileCount == 1)
                //{
                //    string targetUmcFileName = umcFileInfo.First().FullName;

                //    UMCCollection umcs = new UMCCollection();
                //    UMCFileImporter importer = new UMCFileImporter(targetUmcFileName, '\t');
                //    umcs = importer.Import();

                //    List<ScanNETPair> scannetPairs = umcs.GetScanNETLookupTable();
                //    run.SetScanToNETAlignmentData(scannetPairs);

                   
                //    Console.WriteLine(run.DatasetName + " aligned.");
                //    alignmentSuccessful = true;
                //}
                //else if (umcFileCount>1)
                //{
                //    string expectedUMCName = basePath + Path.DirectorySeparatorChar + run.DatasetName + "_UMCs.txt";

                //    if (File.Exists(expectedUMCName))
                //    {
                //        UMCCollection umcs = new UMCCollection();
                //        UMCFileImporter importer = new UMCFileImporter(expectedUMCName, '\t');
                //        umcs = importer.Import();

                //        List<ScanNETPair> scannetPairs = umcs.GetScanNETLookupTable();
                //        run.SetScanToNETAlignmentData(scannetPairs);

                //        Console.WriteLine(run.DatasetName + " NET aligned using UMC file: " + expectedUMCName);

                //        alignmentSuccessful = true;
                //    }
                //    else
                //    {
                //        throw new FileNotFoundException("Trying to align dataset: " + run.DatasetName +
                //                                        " but UMC file not found.");
                //    }


                //}
                //else
                //{
                //    Console.WriteLine(run.DatasetName + " is NOT NET aligned.");
                //    alignmentSuccessful = false;
                //}
            }

            //mass is still not aligned... use data in viper output file: _MassAndGANETErrors_BeforeRefinement.txt
            if (run.MassIsAligned==false)
            {

                string expectedViperMassAlignmentFile = basePath + Path.DirectorySeparatorChar + run.DatasetName + "_MassAndGANETErrors_BeforeRefinement.txt";

                //if (File.Exists(expectedViperMassAlignmentFile))
                //{
                    
                //    var importer = new ViperMassCalibrationLoader(expectedViperMassAlignmentFile);
                //    var viperCalibrationData = importer.ImportMassCalibrationData();

                //    run.SetMassAlignmentData(viperCalibrationData);

                //    Console.WriteLine(run.DatasetName + "- mass aligned using file: " + expectedViperMassAlignmentFile);

                //    alignmentSuccessful = true;
                //}
                //else
                //{
                //    Console.WriteLine(run.DatasetName + " is NOT mass aligned");
                //}

            }


            return alignmentSuccessful;
        }



        public static Run CreateAndAlignRun(string filename, string peaksFile)
        {

            bool folderExists = Directory.Exists(filename);
            bool fileExists = File.Exists(filename);
            
            
            Check.Require(folderExists||fileExists, "Dataset file not found error when RunUtilites tried to create Run.");
     
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(filename);

            Check.Ensure(run != null, "RunUtilites could not create run. Run is null.");

            //Console.WriteLine(run.DatasetName + " loaded.");


            AlignRunUsingAlignmentInfoInFiles(run);





            string sourcePeaksFile;
            if (peaksFile == null || peaksFile == String.Empty)
            {
                sourcePeaksFile = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";
            }
            else
            {
                sourcePeaksFile = peaksFile;
            }

            GetPeaks(run, sourcePeaksFile);

            // Console.WriteLine("Peaks loaded = " + run.ResultCollection.MSPeakResultList.Count);
            return run;
        }

        public static Run CreateAndLoadPeaks(string rawdataFilename)
        {
            return CreateAndLoadPeaks(rawdataFilename, string.Empty);
        }


        public static Run CreateAndLoadPeaks(string rawdataFilename, string peaksTestFile)
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(rawdataFilename);

            //Console.WriteLine(run.DatasetName + " loaded.");

            string basePath = run.DataSetPath;


            string sourcePeaksFile;
            if (peaksTestFile == null || peaksTestFile == String.Empty)
            {
                sourcePeaksFile = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";
            }
            else
            {
                sourcePeaksFile = peaksTestFile;
            }

            GetPeaks(run, sourcePeaksFile);

        	// Console.WriteLine("Peaks loaded = " + run.ResultCollection.MSPeakResultList.Count);
            return run;
        }

        public static List<int> FindPrimaryLcScanNumbers(IEnumerable<Run32.Backend.DTO.MSPeakResult> msPeaks)
		{
			HashSet<int> primaryLcScanNumbers = new HashSet<int>();

			foreach (var msPeakResult in msPeaks)
			{
				int scan = msPeakResult.FrameNum > 0 ? msPeakResult.FrameNum : msPeakResult.Scan_num;
				primaryLcScanNumbers.Add(scan);
			}

			return primaryLcScanNumbers.ToList();
		}
    }
}
