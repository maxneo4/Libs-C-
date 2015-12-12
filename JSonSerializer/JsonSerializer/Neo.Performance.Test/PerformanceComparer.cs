using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Neo.Performance.Test
{   
    public static class PerformanceComparer
    {

        #region Constants
        private const string HEADER = "Iteration";
        private const string HEADER_DATA = "\tMiliseconds-{0}\tTicks-{0}";
        #endregion

        public static T[] GetObjectArray<T>(Func<T> createMethod, double length)
        {
            T[] arrayObjects = new T[(int)length];
            for (int i = 0; i < length; i++)
                arrayObjects[i] = createMethod();
            return arrayObjects;
        }

        public static string ComparePerformanceOver(int iterations, int measureEvery, params Action<object>[] actions)
        {
            object[] input = new object[1];
            return ComparePerformanceOver(input, iterations, measureEvery, actions);
        }

        public static string ComparePerformanceOver<I>(I[] inputObjects, int measureEvery, params Action<I>[] actions)
        {
            return ComparePerformanceOver(inputObjects, inputObjects.Length, measureEvery, actions);
        } 

        public static string ComparePerformanceOver<I>(I[] inputObjects, int iterations, int measureEvery,params Action<I>[] actions)
        {
            Stopwatch[] stopwatchs = BuildSetStopwatch(actions.Length);
            Stopwatch[] stopwatchTotals = BuildSetStopwatch(actions.Length);
            StringBuilder stringBuilder = new StringBuilder(BuildHeader(actions.Length));

            RunOverInputObjects<I>(inputObjects, iterations, measureEvery, actions, stopwatchs, stopwatchTotals, stringBuilder);

            string text = stringBuilder.ToString();
            Clipboard.SetText(text);
            return text;
        }        
        
        #region Private methods

        private static void RunOverInputObjects<I>(I[] inputObjects, int iterations, int measureEvery, Action<I>[] actions, Stopwatch[] stopwatchs, Stopwatch[] stopwatchTotals, StringBuilder stringBuilder)
        {
            for (int i = 0; i < iterations; i++)
            {
                bool measure = (i + 1) % measureEvery == 0;
                if (measure)
                    stringBuilder.AppendLine().Append(i + 1);
                MeasureActions<I>(inputObjects[i % inputObjects.Length], actions, stopwatchs, stopwatchTotals, stringBuilder, measure);
            }
        }

        private static void MeasureActions<I>(I inputObject, Action<I>[] actions, Stopwatch[] stopwatchs, Stopwatch[] stopwatchTotals, StringBuilder stringBuilder, bool measure)
        {
            for (int j = 0; j < actions.Length; j++)
            {
                stopwatchs[j].Start();
                stopwatchTotals[j].Start();

                actions[j](inputObject);

                if (measure)
                {
                    stringBuilder.Append("\t").Append(stopwatchTotals[j].ElapsedMilliseconds).Append("\t").Append(stopwatchs[j].ElapsedTicks);
                    stopwatchs[j].Restart();
                }
                stopwatchs[j].Stop();
                stopwatchTotals[j].Stop();
            }
        }

        private static Stopwatch[] BuildSetStopwatch(int count)
        {
            Stopwatch[] stopwatchs = new Stopwatch[count];
            for (int i = 0; i < stopwatchs.Length; i++)
                stopwatchs[i] = new Stopwatch();
            return stopwatchs;
        }

        private static string BuildHeader(int count)
        {
            string result = HEADER;
            for (int i = 1; i < count + 1; i++)
                result += String.Format(HEADER_DATA, i);
            return result;
        }  

        #endregion
    }
}
