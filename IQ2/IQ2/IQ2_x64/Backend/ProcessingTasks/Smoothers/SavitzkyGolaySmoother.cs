﻿// Written by Kevin Crowell, Spencer Prost and Gordon Slysz for the Department of Energy (PNNL, Richland, WA)
// Copyright 2012, Battelle Memorial Institute
// E-mail: gordon.slysz@pnl.gov
// Website: http://panomics.pnnl.gov/software/
// -------------------------------------------------------------------------------
// 
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0


using System;
 
using MathNet.Numerics.LinearAlgebra.Double;
using Run64.Backend.Data;

namespace IQ_X64.Backend.ProcessingTasks.Smoothers
{
    public class SavitzkyGolaySmoother : Smoother
    {
        MathNet.Numerics.LinearAlgebra.Double.DenseMatrix _smoothingFilters;
        
        public SavitzkyGolaySmoother(int pointsForSmoothing, int polynomialOrder, bool allowNegativeValues=true)
        {
            PointsForSmoothing = pointsForSmoothing;
            PolynomialOrder = polynomialOrder;


        }

        public int PointsForSmoothing { get; set; }
        public int PolynomialOrder { get; set; }
        
        /// <summary>
        /// When smoothing, smoothed values that once were positive, may become negative. This will zero-out any negative values
        /// </summary>
        public bool AllowNegativeValues { get; set; }

        /// <summary>
        /// Performs SavitzkyGolay smoothing
        /// </summary>
        /// <param name="inputValues">Input values</param>
        /// <returns></returns>
        public double[] Smooth(double[] inputValues)
        {
            if (PointsForSmoothing < 3)
                throw new ArgumentOutOfRangeException("savGolayPoints must be an odd number 3 or higher");

            if (PointsForSmoothing % 2 == 0)
                throw new ArgumentOutOfRangeException("savGolayPoints must be an odd number 3 or higher");

            int m = (PointsForSmoothing - 1) / 2;
            int colCount = inputValues.Length;
            double[] returnYValues = new double[colCount];

            
            
            if (_smoothingFilters==null)
            {
                _smoothingFilters = CalculateSmoothingFilters(PolynomialOrder, PointsForSmoothing);
            }

            var conjTransposeMatrix = _smoothingFilters.ConjugateTranspose();

            for (int i = 0; i <= m; i++)
            {
                var conjTransposeColumn = conjTransposeMatrix.Column(i);

                double multiplicationResult = 0;
                for (int z = 0; z < PointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumn[z] * inputValues[z]);
                }
                
                returnYValues[i] = multiplicationResult;
            }

            var conjTransposeColumnResult = conjTransposeMatrix.Column(m);

            for (int i = m + 1; i < colCount - m - 1; i++)
            {
                double multiplicationResult = 0;
                for (int z = 0; z < PointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumnResult[z] * inputValues[i - m + z]);
                }
                returnYValues[i] = multiplicationResult;
            }

            for (int i = 0; i <= m; i++)
            {
                var conjTransposeColumn = conjTransposeMatrix.Column(m + i);

                double multiplicationResult = 0;
                for (int z = 0; z < PointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumn[z] * inputValues[colCount - PointsForSmoothing + z]);
                }
                returnYValues[colCount - m - 1 + i] = multiplicationResult;
            }

            if (!AllowNegativeValues)
            {
                for (int i = 0; i < returnYValues.Length; i++)
                {
                    if (returnYValues[i] < 0) returnYValues[i] = 0;
                }
            }

            return returnYValues;
        }



        public override XYData Smooth(XYData xyData)
        {
            if (xyData == null || xyData.Xvalues.Length == 0) return xyData;

            var smoothedData = Smooth(xyData.Yvalues);

            XYData returnVals = new XYData();
            returnVals.Xvalues = xyData.Xvalues;
            returnVals.Yvalues = smoothedData;
           
            return returnVals;
        }



        private MathNet.Numerics.LinearAlgebra.Double.DenseMatrix CalculateSmoothingFilters(int polynomialOrder, int filterLength)
        {
            int m = (filterLength - 1) / 2;
            var denseMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(filterLength, polynomialOrder + 1);

            for (int i = -m; i <= m; i++)
            {
                for (int j = 0; j <= polynomialOrder; j++)
                {
                    denseMatrix[i + m, j] = Math.Pow(i, j);
                }
            }

            var sTranspose = (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)denseMatrix.ConjugateTranspose();
            var f = sTranspose * denseMatrix;
            var fInverse = (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)f.LU().Solve(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.Identity(f.ColumnCount));
            var smoothingFilters = denseMatrix * fInverse * sTranspose;

            return smoothingFilters;
        }


       
    }



}
