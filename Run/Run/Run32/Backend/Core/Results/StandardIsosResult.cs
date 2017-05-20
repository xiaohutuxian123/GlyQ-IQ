﻿using System;

namespace Run32.Backend.Core.Results
{
    [Serializable]
    public class StandardIsosResult : IsosResult
    {

        public StandardIsosResult()
        {

        }

        public StandardIsosResult(Run run, ScanSet scanset)
        {
            this.Run = run;
            this.ScanSet = scanset;
        }


    







    }
}
