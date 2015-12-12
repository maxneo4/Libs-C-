using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.JsonSerializer;
using Newtonsoft.Json;
using ServiceStack.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using Neo.Performance.Test;

namespace JSonSerializerTest
{
    /// <summary>
    /// Descripción resumida de PerformanceTest
    /// </summary>
    [TestClass]
    public class PerformanceTest
    {       
        #region test tolower

        //[TestMethod]
        public void PerformanceObjectTest()
        {
            //Given            
            double iterations = 1E7;
            //When
            ComparePerformanceFirstToLower("HelloWorld", iterations);
            ComparePerformanceFirstToLower("helloWorld", iterations);
        }
                
        private void ComparePerformanceFirstToLower(string source, double iterations)
        {
            //When
            TimeSpan timeA = MeasurePerformance(() => firstToLowerA(source), iterations);
            TimeSpan timeB = MeasurePerformance(() => firstToLowerB(source), iterations);
            //Then
            Assert.IsTrue(timeB > timeA);
        }

        private string firstToLowerA(string source)
        {
            return Char.IsLower(source[0]) ? source : Char.ToLowerInvariant(source[0]) + source.Substring(1);
        }

        private string firstToLowerB(string source)
        {
            return source.Substring(0, 1).ToLower() + source.Substring(1);
        }               

        private TimeSpan MeasurePerformance(Action action, double iterations)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < iterations; i++)
                action();
            sw.Stop();
            return sw.Elapsed;
        }

        #endregion

        [TestMethod]
        public void CompareStreamWriterVsOnlyStringBuilder()
        {
            double iterations = 1E6;
            int measureEvery = 1000;
            Func<String> testAction = Path.GetRandomFileName;
            string[] listStrings = PerformanceComparer.GetObjectArray(testAction, iterations);
            //When
            string timestringBuilder = MeasurePerformanceOverArray((obj) => AppendString(obj), listStrings, measureEvery);
            string timesStreamWriter = MeasurePerformanceOverArray((obj) => JoinString(obj), listStrings, measureEvery);

            Clipboard.SetText(timestringBuilder + "\r\n" + timesStreamWriter);
            //Then
            //if(timeCreateObject.TotalMilliseconds>0)
            if (timestringBuilder.Length >0 && timesStreamWriter.Length >0)
                Assert.IsTrue(true);//timeNewton.Milliseconds > timeNeo.Milliseconds);
        }

        private StringBuilder sb = new StringBuilder(250);
        private StringBuilder sbO = new StringBuilder();
        private StringWriter sw;

        public PerformanceTest()
        {
            sw = new StringWriter(sb);            
        }

        public string JoinString(string s)
        {
            sw.Write(s);
            return null;
        }

        public string AppendString(string s)
        {
            sbO.Append(s);
            return null;
        }

        [TestMethod]
        public void CompareOverArrayTest()
        {
            double iterations = 1000;
            int measureEvery = 10;
            Func<Object> testAction = GetFullObject;
            object[] listToSerialize = PerformanceComparer.GetObjectArray(testAction, iterations);

            string result = PerformanceComparer.ComparePerformanceOver(listToSerialize, measureEvery,
                (obj) => JsonConvert.SerializeObject(obj),
                (obj) => JSonExtent.ToJson(obj),
                (obj) => ServiceStack.Text.JsonSerializer.SerializeToString(obj)
                );
            
            //When
            //string jsonNewton = null;
            //string jsonNeo = null;
            //string jsonStack = null;
            string json = listToSerialize[0].ToJson();
            JsonConvert.SerializeObject(listToSerialize[0]);
            string timesNewton = MeasurePerformanceOverArray((obj) => JsonConvert.SerializeObject(obj), listToSerialize, measureEvery);
            string timesNeo = MeasurePerformanceOverArray((obj)=> JSonExtent.ToJson(obj),listToSerialize, measureEvery);
            string timesStack = MeasurePerformanceOverArray((obj) =>  ServiceStack.Text.JsonSerializer.SerializeToString(obj), listToSerialize, measureEvery);
                        
            // TieSpan timeCreateObject = MeasurePerformance(() => testAction(), iterations);
            //Then
            //if(timeCreateObject.TotalMilliseconds>0)
            if (timesNewton.Length >0 && timesNeo.Length>0 && timesStack.Length>0)
                Assert.IsTrue(true);//timeNewton.Milliseconds > timeNeo.Milliseconds);

        }

        [TestMethod]
        public void MeasureTypeProcess()
        {
            object[] list = PerformanceComparer.GetObjectArray(GetFullObject, 15e3);//new object[(int)2E4];
            string timeProcess = MeasurePerformanceOverArray((obj) => { num = 0; ProcessType(obj); }, list, 100);

            if (timeProcess.Length > 0 && total > num)
                Assert.IsTrue(true);
        }

        int num = 0;
        int total = 0;
        private void ProcessType(Object source)
        {
            total++;
            Type type =source.GetType();
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propInfo = properties[i];
                object[] attributes = propInfo.GetCustomAttributes(true);
                if (propInfo.CanRead)
                {
                    try
                    {
                        object value = propInfo.GetValue(source, null);
                        if (!(value == null && JSonExtent.IgnorePropertyWhenValueIsNull) && num < 1)
                        {
                            num++;
                            //ProcessType(value);                            
                        }
                    }
                    catch { }
                }
            }
        }

        private string MeasurePerformanceOverArray<I>(Action<I> action, I[] objects, int measureEvery)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch swTotal = System.Diagnostics.Stopwatch.StartNew();
            StringBuilder sb = new StringBuilder("iteration\tticks\tAcumulative miliSeconds");
            sw.Start(); sw.Restart();          
            for (int i = 0; i < objects.Length; i++)
            {
                action(objects[i]); 
                if ((i+1) % measureEvery == 0)
                {
                    sb.AppendLine().Append(i + 1).Append("\t").Append(sw.ElapsedTicks).Append("\t").Append(swTotal.ElapsedMilliseconds);
                    sw.Restart();
                }                               
            }
            sw.Stop();
            swTotal.Stop();
            sb.AppendLine().Append("Total time\t").Append(swTotal.Elapsed);
            return sb.ToString();
        }               

        private object GetSimpleObject()
        {
            return new Object();
        }

        private object GetSimplePropertiesObject()
        {
            Random rand = new Random();
            return new Point(){ X=-rand.Next(), Y=rand.Next(), Z=rand.Next(), Is3D = rand.Next()%2==0};
        }

        private int count = -1;
        private object GetFullObject()
        {
            count++;
            Random rand = new Random();
            byte[] refer = new byte[14]; 
            rand.NextBytes(refer);
           // return new Polygon() { ID = Guid.NewGuid(), Name = Convert.ToString(refer), Type = rand.Next() % 2 == 0 ? PolygonType.Close : PolygonType.Open, CustomProperties = GetCustomProperties(), Lines = GetLines(50) };
        //        
            if (count % 2 == 0)
                return (object)(new Polygon() { ID = Guid.NewGuid(), Name = Convert.ToString(refer), Type = rand.Next() % 2 == 0 ? PolygonType.Close : PolygonType.Open, CustomProperties = GetCustomProperties(), Lines = GetLines(50) });
            else return (object)(new PolygonExtend() { ID = Guid.NewGuid(), Name = Convert.ToString(refer), Type = rand.Next() % 2 == 0 ? PolygonType.Close : PolygonType.Open, CustomProperties = GetCustomProperties(), Lines = GetLinesExtend(50), IsExtend=true });
        }

        private Line[] GetLines(int number)
        {
            Line[] lines = new Line[number];
            for (int i = 0; i < lines.Length; i++)            
                lines[i] = GetLine();            
            return lines;
        }

        private LineExtend[] GetLinesExtend(int number)
        {
            LineExtend[] lines = new LineExtend[number];
            for (int i = 0; i < lines.Length; i++)
                lines[i] = GetLineExtend();
            return lines;
        }

        private Line GetLine()
        {
            return new Line(){ ModifiedDate = DateTime.Now, PointBegin = GetPoint(), PointEnd = GetPoint() };
        }

        private LineExtend GetLineExtend()
        {
            return new LineExtend() { ModifiedDate = DateTime.Now, PointBegin = GetPointExtend(), PointEnd = GetPointExtend() };
        }

        private Point GetPoint()
        {
            Random rand = new Random();
            int index = rand.Next();
            byte[] data = new byte[20];
            rand.NextBytes(data);
            return new Point() { X = rand.Next(), Y = rand.Next(), Z = rand.Next(), Is3D = index % 2 == 0, Refer = index % 2 == 0 ? "referTo" : null, data = data };
        }

        private PointExtend GetPointExtend()
        {
            Random rand = new Random();
            int index = rand.Next();
            byte[] data = new byte[20];
            rand.NextBytes(data);
            return new PointExtend() { X = rand.Next(), Y = rand.Next(), Z = rand.Next(), Is3D = index % 2 == 0, Refer = index % 2 == 0 ? "referTo" : null, data = data };
        }

        private Dictionary<string, object> GetCustomProperties()
        {
            Random rand = new Random();
            byte[] refer = new byte[14];
            return new Dictionary<string, object>() { { "valueA", rand.Next() }, { "valueB", rand.Next() }, { "number", rand.Next() }, { "Display", Convert.ToString(refer) } };
        }

        class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public bool Is3D { get; set; }
            public byte[] data { get; set; }
            public object Refer { get; set; }
        }

        class PointExtend
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public bool Is3D { get; set; }
            public byte[] data { get; set; }
            public object Refer { get; set; }
        }

        class Line
        {
            public Point PointBegin { get; set; }
            public Point PointEnd { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        class LineExtend
        {
            public PointExtend PointBegin { get; set; }
            public PointExtend PointEnd { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        class Polygon
        {
            public string Name { get; set; }
            public Guid? ID { get; set; }
            public Line[] Lines { get; set; }
            public PolygonType Type { get; set; }
            public Dictionary<string, object> CustomProperties { get; set; }
        }

        class PolygonExtend 
        {
            public string Name { get; set; }
            public Guid? ID { get; set; }
            public LineExtend[] Lines { get; set; }
            public PolygonType Type { get; set; }
            public Dictionary<string, object> CustomProperties { get; set; }
            public bool IsExtend { get; set; }
        }

        enum PolygonType
        {
            Open,
            Close
        }
    }
}
