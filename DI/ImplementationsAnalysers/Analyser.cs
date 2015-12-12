using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces;
using AnotherProject;

namespace ImplementationsAnalysers
{
    public class Analyser : IAnalyzer
    {
        public Analyser()
        {
            //string result = dataService.ReadObject(String.Empty);
        }

        #region Miembros de IAnalyzer

        public string Analyze(string json)
        {
            return string.Empty;
        }

        #endregion
                
    }

    class QueryService : IQueryService
    {
        internal QueryService(IService service, IQueryBuilder queryBuilder)
        {

        }

    }
}
