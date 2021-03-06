﻿using IQ.Workflows;
using IQ.Workflows.WorkFlowParameters;
using Run32.Backend;

namespace Sipper
{
    public class SipperTargetedWorkflowParameters : TargetedWorkflowParameters
    {

        public SipperTargetedWorkflowParameters()
        {
            ChromPeakSelectorMode = Globals.PeakSelectorMode.ClosestToTarget;
            
            ResultType = Globals.ResultType.SIPPER_TARGETED_RESULT;
        }


        public override GlobalsWorkFlow.TargetedWorkflowTypes WorkflowType
        {
            get { return GlobalsWorkFlow.TargetedWorkflowTypes.SipperTargeted1; }
        }
    }
}
