using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace MT.AzureStorageLib.Entities
{
    public class Person : TableEntity
    {

        public string UserId { get; set; }
        public string City { get; set; }
        public string Name { get; set; }
        public string RawPhoneNumber { get; set; }
        [IgnoreProperty]
        public List<string> PhoneNumber
        {
            get => RawPhoneNumber == null ? null : JsonConvert.DeserializeObject<List<string>>(RawPhoneNumber);
            set => RawPhoneNumber = JsonConvert.SerializeObject(value);
        }
    }
}
