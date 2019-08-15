using Lighthouse.Core;
using Lighthouse.Core.IO;
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
                Entries.AddOrUpdate(new KvdEntry { PartitionKey = partition, Key = key}, value, (kvd, curVal) => value);
                //SyncWithFileSystem();
            });
        }

        public async Task<string> Retrieve(string partition, string key)
        {
            return await Task.Run<string>(() => Entries.TryGetValue(new KvdEntry { PartitionKey = partition, Key = key }, out var value) ? value : null);
        }

        public KVD()
        {

        }

        public ConcurrentDictionary<KvdKey, string> Entries
        {
            get; private set;
        } = new ConcurrentDictionary<KvdKey, string>();

        private IFileSystemProvider FileSystem { get; set; }

        protected override void OnStart()
        {
            // load the values from the file system
            FileSystem = Container.GetFileSystemProviders()?.FirstOrDefault();

            // TODO: load file storage location from some sort of config
            Entries = ReadFromFileSystem();
        }

        protected override void OnStop()
        {
            base.OnStop();

            // push the file to the file system
            WriteToFileSystem();
        }

        public void WriteToFileSystem()
        {
            if(FileSystem == null)
            {
                throw new ApplicationException("No file system is present.");
            }

            var fileData = JsonConvert.SerializeObject(Entries.Select(e => new KvdEntry(e.Key,e.Value)));
            FileSystem.WriteStringToFileSystem(PrimaryDBFileName, fileData);
        }

        public ConcurrentDictionary<KvdKey, string> ReadFromFileSystem()
        {
            if (FileSystem == null)
            {
                throw new ApplicationException("No file system is present.");
            }

            var serializedDbFile = FileSystem.ReadStringFromFileSystem(PrimaryDBFileName);

            if (serializedDbFile != null)
            {
                var listOfAllEntries = JsonConvert.DeserializeObject<IList<KvdEntry>>(serializedDbFile);
                return new ConcurrentDictionary<KvdKey, string>(listOfAllEntries.ToDictionary(kvd => (KvdKey)kvd, kve => kve.Value));
            }

            return null;
        }
    }

    public class KvdEntry : KvdKey
    {
        public string Value { get; set; }

        public KvdEntry() { }

        public KvdEntry(KvdKey key, string value)
            :base(key)
        {
            Value = value;
        }
    }

    public class KvdKey : IEqualityComparer<KvdKey>
    {
        public string PartitionKey { get; set; }
        public string Key { get; set; }

        public KvdKey() { }

        public KvdKey(KvdKey other)
        {
            this.PartitionKey = other.PartitionKey;
            this.Key = other.Key;
        }

        public bool Equals(KvdKey x, KvdKey y) => (x.PartitionKey?.Equals(y.PartitionKey, StringComparison.OrdinalIgnoreCase) ?? false) &&
            (x.Key?.Equals(y.Key, StringComparison.OrdinalIgnoreCase) ?? false);

        public int GetHashCode(KvdKey obj) => HashCode.Combine(obj.PartitionKey, obj.Key);
    }
}
