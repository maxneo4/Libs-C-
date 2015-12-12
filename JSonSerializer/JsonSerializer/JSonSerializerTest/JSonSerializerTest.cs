using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.JsonSerializer;
using System.Collections;

namespace JSonSerializerTest
{
    [TestClass]
    public class JSonSerializerTest
    {        

        [TestCleanup]
        public void CleanUp()
        {
            JSonExtent.PropertyNamesAsFirstLowerChar = false;
            JSonExtent.IgnorePropertyWhenValueIsNull = false;
        }

        [TestMethod]
        public void ObjectToJson()
        {
            //Given
            Object obj = new object();
            //When
            string json = JSonExtent.ToJson(obj);
            //Then
            Assert.AreEqual("{}", json);
        }

        [TestMethod]
        public void ObjectIntPropertyToJson()
        {
            //Given
            Point point = new Point() { X = -25, Y = 100 };
            //When
            string json = JSonExtent.ToJson(point);
            //Then
            Assert.AreEqual(FormatJson("{'X':-25,'Y':100}"), json);
        }

        [TestMethod]
        public void ObjectBooleanPropertyToJson()
        {
            //Given
            Flags flags = new Flags() { IsService = true };
            //When
            string json = JSonExtent.ToJson(flags);
            //Then
            Assert.AreEqual(FormatJson("{'IsService':true,'HasCount':false}"), json);
        }

        [TestMethod]
        public void ObjectStringPropertyToJson()
        {
            //Given
            Person person = new Person() { FirstName="Alejandro", LastName ="Benitez" };
            //When
            string json = JSonExtent.ToJson(person);
            //Then
            Assert.AreEqual(FormatJson("{'FirstName':'Alejandro','LastName':'Benitez'}"), json);
        }

        [TestMethod]
        public void ObjectNullPropertyToJson()
        {
            //Given
            Person person = new Person() { FirstName = "Alejandro", LastName = null };
            //When
            string json = JSonExtent.ToJson(person);
            //Then
            Assert.AreEqual(FormatJson("{'FirstName':'Alejandro','LastName':null}"), json);
        }

        [TestMethod]
        public void ObjectArrayPropertyToJson()
        {
            //Given
            Persons persons = new Persons() { Members = new Person[]
            {
                new Person() { FirstName="Alejandro", LastName ="Benitez" },
                new Person() { FirstName="Tomas", LastName ="Saenz" }
            } 
            };
            //When
            string json = JSonExtent.ToJson(persons);
            //Then
            Assert.AreEqual(FormatJson("{'Members':[{'FirstName':'Alejandro','LastName':'Benitez'},{'FirstName':'Tomas','LastName':'Saenz'}]}"), json);
        }

        [TestMethod]
        public void ObjectListPropertyToJson()
        {
            //Given
            PersonsList persons = new PersonsList()
            {
                Members = new List<Person>()
            {
                new Person() { FirstName="Alejandro", LastName ="Benitez" },
                new Person() { FirstName="Tomas", LastName ="Saenz" }
            }
            };
            //When
            string json = JSonExtent.ToJson(persons);
            //Then
            Assert.AreEqual(FormatJson("{'Members':[{'FirstName':'Alejandro','LastName':'Benitez'},{'FirstName':'Tomas','LastName':'Saenz'}]}"), json);
        }

        [TestMethod]
        public void ObjectDictionaryToJson()
        {
            //Given
            IDictionary<string,object> dictionary = new Dictionary<string,object>();
            dictionary["ValueA"] = 1;
            dictionary["ValueB"] = "2";
            //When
            string json = JSonExtent.ToJson(dictionary);
            //Then
            Assert.AreEqual(FormatJson("{'ValueA':1,'ValueB':'2'}"), json);
        }

        [TestMethod]
        public void ObjectQuotePropertyToJson()
        {
            //Given
            Path path = new Path() { Value = "\"Text\"=\"1\"" };
            //When
            string json = JSonExtent.ToJson(path);
            //Then
            Assert.AreEqual(FormatJson("{'Value':'\\\"Text\\\"=\\\"1\\\"'}"), json);
        } 

        [TestMethod]
        public void ObjectPathPropertyToJson()
        {
            //Given
            Path path = new Path() { Value = "C:\\\"Doc\"\\log\\" };
            //When
            string json = JSonExtent.ToJson(path);
            //Then
            Assert.AreEqual(FormatJson("{'Value':'C:\\\\\\\"Doc\\\"\\\\log\\\\'}"), json);
        }

        [TestMethod]
        public void ObjectBreakLinePropertyToJson()
        {
            //Given
            Path path = new Path() { Value = @"Linea 1
Linea 2" };
            //When
            string json = JSonExtent.ToJson(path);
            //Then
            Assert.AreEqual(FormatJson("{'Value':'Linea 1\\r\\nLinea 2'}"), json);
        }

        [TestMethod]
        public void ObjectGuidPropertyToJson()
        {
            //Given
            Catalog catalog = new Catalog() { ID = new Guid("70c45575-7001-4e73-b70b-0fc719ea7241") };
            //When
            string json = JSonExtent.ToJson(catalog);
            //Then
            Assert.AreEqual(FormatJson("{'ID':'70c45575-7001-4e73-b70b-0fc719ea7241'}"), json);
        }

        [TestMethod]
        public void ObjectEnumPropertyToJson()
        {
            //Given
            ExceptionC exception = new ExceptionC() { LevelError = Error.Medium };
            //When
            string json = JSonExtent.ToJson(exception);
            //Then
            Assert.AreEqual(FormatJson("{'LevelError':'Medium'}"), json);
        }

        [TestMethod]
        public void ObjectDatePropertyToJson()
        {
            //Given
            DateTime datetime = DateTime.Now;
            Entry entry = new Entry() { DateEntry = datetime };
            //When
            string json = JSonExtent.ToJson(entry);
            //Then
            Assert.AreEqual(FormatJson(String.Format("{{'DateEntry':'{0}'}}", datetime.ToString())), json);
        }

        [TestMethod]
        public void ObjectByteArrayPropertyToJson()
        {
            //Given
            byte[] data = new byte[]{0,1,2,3,4,5,6,7,8,9,10};
            ContentFile contentFile = new ContentFile() { Data = data  };
            //When
            string json = JSonExtent.ToJson(contentFile);
            //Then
            Assert.AreEqual(FormatJson(String.Format("{{'Data':'{0}'}}", Convert.ToBase64String(data))), json);
        }

        [TestMethod]
        public void ObjectIgnorePropertyToJson()
        {
            //Given
            Email email = new Email() { Address="maxneo4@gmail.com", Url = "http://Google.com" };
            //When
            string json = JSonExtent.ToJson(email);
            //Then
            Assert.AreEqual(FormatJson("{'Address':'maxneo4@gmail.com'}"), json);
        }

        [TestMethod]
        public void ObjectRenamePropertyToJson()
        {
            //Given
            Context context = new Context() { Level = Error.High, Impact = null };
            //When
            string json = JSonExtent.ToJson(context);
            //Then
            Assert.AreEqual(FormatJson("{'level':'High','impact':null}"), json);
        }

        [TestMethod]
        public void ObjectFormatValuePropertyToJson()
        {
            //Given
            DateTime date = new DateTime(2015,3,20);            
            Form form = new Form() { Name = "sampleForm", CreationTime = date, IdParentForm = new Guid("70c45575-7001-4e73-b70b-0fc719ea7241") };
            //When
            string json = JSonExtent.ToJson(form);
            //Then
            Assert.AreEqual(FormatJson("{'Name':'SAMPLEFORM','CreationTime':'20-03-2015','IdParentForm':{'Baref':{'Ref':'70c45575-7001-4e73-b70b-0fc719ea7241'}}}"), json);
        }

        [TestMethod]
        public void ObjectAllPropertiesFirstCharToLowerToJson()
        {
            //Given
            JSonExtent.PropertyNamesAsFirstLowerChar = true;
            DateTime date = new DateTime(2015, 3, 20);
            Form form = new Form() { Name = "sampleForm", CreationTime = date, IdParentForm = new Guid("70c45575-7001-4e73-b70b-0fc719ea7241") };
            //When
            string json = JSonExtent.ToJson(form);
            //Then
            Assert.AreEqual(FormatJson("{'name':'SAMPLEFORM','creationTime':'20-03-2015','idParentForm':{'Baref':{'Ref':'70c45575-7001-4e73-b70b-0fc719ea7241'}}}"), json);
        
        }

        [TestMethod]
        public void ObjectIgnoreNullPropertiesToJson()
        {
            //Given
            JSonExtent.IgnorePropertyWhenValueIsNull = true;
            Flags flags = new Flags() { IsService = null, HasCount = true };
            //When
            string json = JSonExtent.ToJson(flags);
            //Then
            Assert.AreEqual(FormatJson("{'HasCount':true}"), json);
        }

        

        #region Private methods

        private string FormatJson(string source)
        {
            return source.Replace("'", "\"");
        }

        #endregion

        #region Clasess

        class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        class Flags
        {
            public bool? IsService { get; set; }
            public Boolean HasCount { get; set; }
        }

        class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        class Persons
        {
            public Person[] Members { get; set; }
        }

        class PersonsList
        {
            public IList<Person> Members { get; set; }
        }

        class Path
        {
            public String Value { get; set; }
        }

        class Catalog
        {
            public Guid? ID { get; set; }
        }

        enum Error
        {
            High=3,
            Medium=2,
            Low=1
        }

        class ExceptionC
        {
            public Error LevelError { get; set; }
        }

        class Entry
        {
            public DateTime DateEntry { get; set; }
        }

        class ContentFile
        {
            public byte[] Data { get; set; }
        }

        class Email
        {
            public string Address { get; set; }
            [JSonIgnore]
            public string Url { get; set; }
        }

        class Context
        {
            [JSonPropertyName("level")]
            public Error Level { get; set; }
            [JSonPropertyName("impact")]
            public int? Impact { get; set; }
        }

        class Form
        {
            [UpperString]
            public string Name { get; set; }
            [DateFormatBizagi]
            public DateTime CreationTime { get; set; }
            [Baref]
            public Guid? IdParentForm { get; set; }            
        }

        #endregion

        #region Attributes

        class DateFormatBizagi : JSonFormatDate
        {
            public DateFormatBizagi() : base("dd-MM-yyyy")
            { 
            }            
        }

        class Baref : JSonFormatWrap
        {
            public Baref() : base("{\"Baref\":{\"Ref\":", "}}")
            {
            }            
        }

        class UpperString : JSonFormatValue
        {
            public override void FormatValue(object value, StringBuilder stringBuilder)
            {
                stringBuilder.Append(JSonExtent.ToJson(value.ToString().ToUpper()));
            }
        }

        #endregion
    }
}
