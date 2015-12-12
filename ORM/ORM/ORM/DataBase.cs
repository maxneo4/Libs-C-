using System.Collections.Generic;

namespace ORM
{
    public interface DataBase
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollBackTransaction();
        void RunMerge(string mergeStatement);
        List<T> RunQuery<T>(string queryStatement);
    }
}
