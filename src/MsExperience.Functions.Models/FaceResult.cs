namespace MsExperience.Models
{
    public class FaceResult
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string BlobName { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public bool HasBeard { get; set; }
        public bool HasMoustache { get; set; }
        public bool IsBald { get; set; }
        public string Emotion { get; set; }
        public string MainHairColor { get; set; }
    }
}