﻿using System.Collections.Generic;
using System.Linq;
using IQ2_x64.Backend.ProcessingTasks.FitScoreCalculators;
using Run64.Backend.Data;
using Run64.Utilities;

namespace IQ_X64.Backend.ProcessingTasks.FitScoreCalculators
{
    /// <summary>
    /// Does a least-squares fit.  
    /// </summary>
    public class AreaFitter:LeastSquaresFitter
    {


        public double GetFit(XYData theorXYData, XYData observedXYData, double minIntensityForScore, double offset = 0)
        {
            Check.Require(theorXYData != null && theorXYData.Xvalues != null && theorXYData.Yvalues != null,
               "AreaFitter failed. Theoretical XY data is null");

            Check.Require(observedXYData != null & observedXYData.Xvalues != null && observedXYData.Yvalues != null,
                "AreaFitter failed. Observed XY data is null");
            //Check.Require(minIntensityForScore >= 0 && minIntensityForScore <= 100, "MinIntensityForScore should be between 0 and 100");



            double sumOfSquaredDiff = 0;
            double sumOfSquaredTheorIntens = 0;
            double xmin = theorXYData.Xvalues[0] + offset;
            double xmax = theorXYData.Xvalues.Max() + offset;


            XYData trimmedObservedXYData = observedXYData.TrimData(xmin, xmax);
            //XYData trimmedObservedXYData = observedXYData;
            
            //trimmedObservedXYData.Display();
            //theorXYData.Display();

            List<double> interpolatedValues = new List<double>();
            List<double> theoreticalValues = new List<double>();


            for (int i = 0; i < theorXYData.Xvalues.Length; i++)
            {

                if (theorXYData.Yvalues[i] >= minIntensityForScore)
                {
                    double currentTheorMZ = theorXYData.Xvalues[i] + offset;

                    int indexOfClosest = MathUtils.GetClosest(trimmedObservedXYData.Xvalues, currentTheorMZ, 0.1);

                    if (indexOfClosest == -1)
                    {
                        //Console.WriteLine(i + "\t" + currentTheorMZ);
                        return 1;
                    }

                    //findout if closest is above or below
                    bool closestIsBelow = (trimmedObservedXYData.Xvalues[indexOfClosest] < currentTheorMZ);

                    double mz1 = 0;
                    double mz2 = 0;
                    double intensity1 = 0;
                    double intensity2 = 0;

                    if (closestIsBelow)
                    {
                        mz1 = trimmedObservedXYData.Xvalues[indexOfClosest];
                        intensity1 = trimmedObservedXYData.Yvalues[indexOfClosest];
                        if (indexOfClosest == trimmedObservedXYData.Xvalues.Length - 1)   //if at the end of the XY array; this should be very rare
                        {
                            mz2 = mz1;
                            intensity2 = intensity1;
                        }
                        else
                        {
                            mz2 = trimmedObservedXYData.Xvalues[indexOfClosest + 1];
                            intensity2 = trimmedObservedXYData.Yvalues[indexOfClosest + 1];
                        }


                    }
                    else  // closest point is above the targetMZ; so get the mz of the point below
                    {
                        mz2 = trimmedObservedXYData.Xvalues[indexOfClosest];
                        intensity2 = trimmedObservedXYData.Yvalues[indexOfClosest];
                        if (indexOfClosest == 0)        //if at the beginning of the XY array  (rare)
                        {
                            mz1 = mz2;
                            intensity1 = intensity2;
                        }
                        else
                        {
                            mz1 = trimmedObservedXYData.Xvalues[indexOfClosest - 1];
                            intensity1 = trimmedObservedXYData.Yvalues[indexOfClosest - 1];
                        }
                    }


                    double interopolatedIntensity = MathUtils.getInterpolatedValue(mz1, mz2, intensity1, intensity2, currentTheorMZ);

                    interpolatedValues.Add(interopolatedIntensity);
                    theoreticalValues.Add(theorXYData.Yvalues[i]);

                    //double normalizedObservedIntensity = interopolatedIntensity / maxIntensity;

                    //double normalizedTheorIntensity = theorXYData.Yvalues[i]/maxTheorIntensity;   

                    //double diff = normalizedTheorIntensity - normalizedObservedIntensity;

                    //sumOfSquaredDiff += diff * diff;

                    //sumOfSquaredTheorIntens += normalizedTheorIntensity * normalizedTheorIntensity;


                }


            }

            double maxTheoreticalIntens = getMax(theoreticalValues);
            double maxObservIntens = getMax(interpolatedValues);


            for (int i = 0; i < theoreticalValues.Count; i++)
            {
                double normalizedObservedIntensity = interpolatedValues[i] / maxObservIntens;

                double normalizedTheorIntensity = theoreticalValues[i] / maxTheoreticalIntens;

                double diff = normalizedTheorIntensity - normalizedObservedIntensity;

                sumOfSquaredDiff += diff * diff;

                sumOfSquaredTheorIntens += normalizedTheorIntensity * normalizedTheorIntensity;



            }
            if (sumOfSquaredTheorIntens == 0) return -1;

            //StringBuilder sb = new StringBuilder();
            //TestUtilities.GetXYValuesToStringBuilder(sb, theoreticalValues.ToArray(), interpolatedValues.ToArray());
            //Console.Write(sb.ToString());


            return sumOfSquaredDiff / sumOfSquaredTheorIntens;

        }

        private double getMax(List<double> values)
        {
            double max = double.MinValue;

            foreach (double d in values)
            {
                if (d > max) max = d;
            }

            return max;
        }








    }
}
