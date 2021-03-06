﻿namespace IQ_X64.Workflows.WorkFlowParameters
{
    public abstract class WorkflowExecutorBaseParameters : WorkflowParameters
    {
        

        #region Constructors
        public WorkflowExecutorBaseParameters()
        {
            CopyRawFileLocal = false;
            DeleteLocalDatasetAfterProcessing = false;
            TargetedAlignmentIsPerformed = false;
            TargetType = GlobalsWorkFlow.TargetType.DatabaseTarget;


            AlignmentInfoIsExported = true;
            AlignmentFeaturesAreSavedToTextFile = true;

            MinMzForDefiningChargeStateTargets = 400;
            MaxMzForDefiningChargeStateTargets = 1500;
            MaxNumberOfChargeStateTargetsToCreate = 100;

        }
        #endregion

      

        #region Properties

        public bool CopyRawFileLocal { get; set; }
        public bool DeleteLocalDatasetAfterProcessing { get; set; }
        public string FolderPathForCopiedRawDataset { get; set; }
        public string LoggingFolder { get; set; }
        public string TargetsUsedForAlignmentFilePath { get; set; }
        public string TargetsFilePath { get; set; }
        public string TargetsBaseFolder { get; set; }
        public GlobalsWorkFlow.TargetType TargetType { get; set; }
        public string ResultsFolder { get; set; }
        public bool TargetedAlignmentIsPerformed { get; set; }
        public string TargetedAlignmentWorkflowParameterFile { get; set; }
        public string WorkflowParameterFile { get; set; }
        public bool AlignmentInfoIsExported { get; set; }
        public bool AlignmentFeaturesAreSavedToTextFile { get; set; }
        public string AlignmentInfoFolder { get; set; }

        public string OutputFolderBase { get; set; }
        //ChromGen Peak Generator


        public double ChromGenSourceDataPeakBR { get; set; }
        public double ChromGenSourceDataSigNoise { get; set; }
        public bool ChromGenSourceDataIsThresholded { get; set; }
        public bool ChromGenSourceDataProcessMsMs { get; set; }

        
        /// <summary>
        /// Minimum m/z value used for defining the a range of IqChargeState targets
        /// </summary>
        public double MinMzForDefiningChargeStateTargets { get; set; }


        /// <summary>
        /// Maxium m/z value used for defining the a range of IqChargeState targets
        /// </summary>
        public double MaxMzForDefiningChargeStateTargets { get; set; }

        /// <summary>
        /// Maximum number of charge states to create
        /// </summary>
        public int MaxNumberOfChargeStateTargetsToCreate { get; set; }

        //public string ExportAlignmentFolder { get; set; }

        
        #endregion


    }
}
