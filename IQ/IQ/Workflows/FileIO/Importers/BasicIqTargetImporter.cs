﻿using System.Collections.Generic;
using IQ.Workflows.Core;

namespace IQ.Workflows.FileIO.Importers
{
    public class BasicIqTargetImporter : IqTargetImporter
    {

        public BasicIqTargetImporter(string filename)
        {
            Filename = filename;
        }


        protected override IqTarget ConvertTextToIqTarget(List<string> processedRow)
        {
            IqTarget target = new IqChargeStateTarget();
            target.ID = ParseIntField(processedRow, TargetIDHeaders, -1);

            if (target.ID == -1)
            {
                target.ID = GetAutoIncrementForTargetID();
            }


            target.EmpiricalFormula = ParseStringField(processedRow, EmpiricalFormulaHeaders, string.Empty);
            target.Code = ParseStringField(processedRow, CodeHeaders, "");
            target.ElutionTimeTheor = ParseDoubleField(processedRow, NETHeaders, 0);
            target.ScanLC = ParseIntField(processedRow, ScanHeaders, -1);
            target.QualityScore = ParseDoubleField(processedRow, QualityScoreHeaders, -1);
            target.ChargeState = ParseIntField(processedRow, ChargeStateHeaders, 0);

            return target;
        }


    }
}
