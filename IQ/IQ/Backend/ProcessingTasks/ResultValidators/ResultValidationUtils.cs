﻿using System;
using System.Collections.Generic;
using Run32.Backend.Core;

namespace IQ.Backend.ProcessingTasks.ResultValidators
{
    public class ResultValidationUtils
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public static int GetFlagCode(IList<ResultFlag>resultFlags)
        {
            if (resultFlags == null || resultFlags.Count == 0) return -1;
            

            //TODO: add code later, but for now, will return 1;
            return 1;



        }


        #endregion

        #region Private Methods
        #endregion

        public static string GetStringFlagCode(IList<ResultFlag> resultFlags)
        {
            int flagCode =  GetFlagCode(resultFlags);
            if (flagCode == -1) return String.Empty;
            else
            {
                return flagCode.ToString();
            }
        }
    }
}
