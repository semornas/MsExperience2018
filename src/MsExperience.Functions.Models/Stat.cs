namespace MsExperience.Models
{
    public class Stat
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int CountUpload { get; set; }
        public int CountAnger { get; set; }
        public int CountContempt { get; set; }
        public int CountDisgust { get; set; }
        public int CountFear { get; set; }
        public int CountHappiness { get; set; }
        public int CountNeutral { get; set; }
        public int CountSadness { get; set; }
        public int CountSurprise { get; set; }
        public int CountMale { get; set; }
        public int CountFemale { get; set; }
        public int CountMoustache { get; set; }
        public int CountBeard { get; set; }
        public int CountBald { get; set; }
    }
}
