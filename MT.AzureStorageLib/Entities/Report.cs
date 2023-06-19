

using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace MT.AzureStorageLib.Entities
{
    public enum FileStatus
    {
        Creating,
        Completed,
        Cancelled
    }
    public class Report : TableEntity
    {
     
        public string ReportId { get; set; }
        public string UserId { get; set; }
        public string ReportState { get; set; }
 

        public string RawCreatedDate { get; set; }

        public string RawRequestedDate { get; set; }

        [IgnoreProperty]
        public DateTime RequestDate
        {
            get => RawRequestedDate == null ? DateTime.MinValue : JsonConvert.DeserializeObject<DateTime>(RawRequestedDate);
            set => RawRequestedDate = JsonConvert.SerializeObject(value);
        }

        [IgnoreProperty]
        public DateTime CreatedDate
        {
            get => RawCreatedDate == null ? DateTime.MinValue : JsonConvert.DeserializeObject<DateTime>(RawCreatedDate);
            set => RawCreatedDate = JsonConvert.SerializeObject(value);
        }

    }
}
