using Lighthouse.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Apps.Database
{
    public class KVD : LighthouseServiceBase
    {
        public const string PrimaryDBFileName = "primary.kvd";
        public KVD()
        {

        }

        public List<KvdEntry> Entries
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
                Entries = JsonConvert.DeserializeObject<List<KvdEntry>>(dictionaryString);
            }
        }
    }

    public class KvdEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
