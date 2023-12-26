using Newtonsoft.Json;

namespace CURD_Sahil.DTO
{
    public class TasksModel
    {
        [JsonProperty(PropertyName = "uId", NullValueHandling = NullValueHandling.Ignore)]
        public string UId { get; set; }

        [JsonProperty(PropertyName = "taskId", NullValueHandling = NullValueHandling.Ignore)]
        public string TaskId { get; set; }

        [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
