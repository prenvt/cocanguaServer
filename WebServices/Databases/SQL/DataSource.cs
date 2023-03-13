using System.Collections.Specialized;
using System.Configuration;

class DataSource
{
    /*public static string GameConn
    {
        get
        {
            return "data source =45.76.181.41,1433; database=CTP; uid=moo; pwd=a123456!"; // ORIGINAL
        }
    }*/

    public static string MongoConn
    {
        get
        {
#if DEBUG
            return "mongodb://sa:Abc12321@103.116.100.13/";
#else
            return "mongodb://sa:Abc12321@localhost:27017/";
#endif
            //return "mongodb://sa:Abc12321@45.76.181.41/";
        }
    }

    /*public static string GetConnectionStr(string connStr)
    {
        connStr = System.String.IsNullOrEmpty(connStr) ? GameConn : connStr;
        return connStr;
    }*/
}
