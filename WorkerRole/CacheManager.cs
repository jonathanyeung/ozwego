using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Caching;

namespace WorkerRole
{
    public static class CacheManager
    {
        private static readonly DataCacheFactory DataCacheFactory;

        public static DataCache DataCache;

        static CacheManager()
        {
            DataCacheFactory = new DataCacheFactory();

            try
            {
                DataCache = DataCacheFactory.GetDefaultCache();
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                                              e.StackTrace));
            }
        }
    }
}
