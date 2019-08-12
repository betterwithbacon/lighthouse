using Lighthouse.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Apps.Database
{
    public class KVD : LighthouseServiceBase
    {
        public const string PrimaryDBFileName = "primary.kvd";

        public async Task Store(string partition, string key, string value)
        {
            await Task.Run(() => {
                Entries.Add(new KvdEntry { PartitionKey = partition, Key = key, Value = value });
            });
        }

        public async Task<string> Retrieve(string partition, string key)
        {   
            return await Task.Run<string>( () => Entries.FirstOrDefault(kvd => kvd.PartitionKey == partition && kvd.Key.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value);            
        }

        public KVD()
        {

        }

        public ConcurrentBag<KvdEntry> Entries
        {
            get; private set;
        }

        protected override void OnStart()
        {
            // load the values from the file system
            var fileSystem = Container.GetFileSystemProviders()?.FirstOrDefault();

            if(fileSystem == null)
            {
                throw new ApplicationException("no file system is present");
            }

            // TODO: load file storage location from some sort of config

            var dbFile = fileSystem.ReadFromFileSystem(PrimaryDBFileName).GetAwaiter().GetResult();

            if(dbFile != null)
            {
                var dictionaryString = Encoding.UTF8.GetString(dbFile, 0, dbFile.Length);
                Entries = new ConcurrentBag<KvdEntry>(JsonConvert.DeserializeObject<List<KvdEntry>>(dictionaryString));
            }
        }
    }

    public class KvdEntry
    {
        public string PartitionKey { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
