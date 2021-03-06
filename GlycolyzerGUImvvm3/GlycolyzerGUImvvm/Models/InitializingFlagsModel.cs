﻿/* Written by Myanna Harris
* for the Department of Energy (PNNL, Richland, WA)
* Battelle Memorial Institute
* E-mail: Myanna.Harris@pnnl.gov
* Website: http://omics.pnl.gov/software
* -----------------------------------------------------
* 
 * Notice: This computer software was prepared by Battelle Memorial Institute,
* hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
* Department of Energy (DOE).  All rights in the computer software are reserved
* by DOE on behalf of the United States Government and the Contractor as
* provided in the Contract.
* 
 * NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY WARRANTY, EXPRESS OR
* IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS SOFTWARE.
* 
 * This notice including this sentence must appear on any copies of this computer
* software.
* -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlycolyzerGUImvvm.Models
{
    public class InitializingFlagsModel : ObservableObject
    {
        //Initialize Page Flags
        private Boolean parameterModel_InitializeFlag = true;
        private Boolean omniFinderGMModel_InitializeFlag = true;
        private Boolean parameterRangesSave_InitializeFlag = false;
        private Boolean omniFinderGMRangesSave_InitializeFlag = false;
        private Boolean tab4_ResizeFlag = false;


        public Boolean ParameterModel_InitializeFlag
        {
            get { return parameterModel_InitializeFlag; }
            set { parameterModel_InitializeFlag = value; OnPropertyChanged("parameterModel_InitializeFlag"); }
        }

        public Boolean OmniFinderGMModel_InitializeFlag
        {
            get { return omniFinderGMModel_InitializeFlag; }
            set { omniFinderGMModel_InitializeFlag = value; OnPropertyChanged("omniFinderGMModel_InitializeFlag"); }
        }

        public Boolean ParameterRangesSave_InitializeFlag
        {
            get { return parameterRangesSave_InitializeFlag; }
            set { parameterRangesSave_InitializeFlag = value; OnPropertyChanged("saveParameterRangesModel_InitializeFlag"); }
        }

        public Boolean OmniFinderGMRangesSave_InitializeFlag
        {
            get { return omniFinderGMRangesSave_InitializeFlag; }
            set { omniFinderGMRangesSave_InitializeFlag = value; OnPropertyChanged("saveOmniFinderGMRangesModel_InitializeFlag"); }
        }

        public Boolean Tab4_ResizeFlag
        {
            get { return tab4_ResizeFlag; }
            set { tab4_ResizeFlag = value; OnPropertyChanged("tab4_ResizeFlag"); }
        }
    }
}
