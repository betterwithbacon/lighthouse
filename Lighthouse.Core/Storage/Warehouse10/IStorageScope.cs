using System.Collections.Generic;

namespace Lighthouse.Core.Storage.Legacy
{
	public interface IStorageScope : IEqualityComparer<IStorageScope>
	{
		string ScopeName { get; }

		string Identifier { get; }
	}

    public class StorageOperation
    {
        public StorageAction Action { get; set; }
        public string Key { get; set; }
        public IStorageScope Scope { get; set; }
    }

    //public class StorageResponse
    //{
    //    public static StorageResponse Stored = new StorageResponse();

    //    public StorageResponse(bool wasSuccessful = true, string message = null)
    //    {
    //        WasSuccessful = wasSuccessful;
    //        Message = message;
    //    }

    //    public bool WasSuccessful { get; }
    //    public string Message { get; }
    //    public Receipt Receipt { get; set; }
    //    public byte[] Data { get; set; }
    //    public string StringData { get; set; } // TODO: is this a necessary hack?!        
    //}

    public abstract class StorageRequest
    {
        public IStorageScope Scope { get; set; }        
    }

    public class KeyValueStoreRequest : StorageRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class InspectResponse
    {
        public List<ItemDescriptor> Items { get; set; }
    }

    public enum StorageAction
    {
        Store,
        Retrieve,
        Delete,
        Inspect
    }

    public enum StoragePayloadType
    {
        String,
        Blob
    }
}