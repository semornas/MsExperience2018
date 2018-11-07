using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using MsExperience.Models;
using Newtonsoft.Json;

namespace MsExperience.Functions.StorageListener
{
    public class StatEntity : TableEntity
    {
        public StatEntity() : base()
        {
        }

        public StatEntity(IEnumerable<FaceResultEntity> allData, string rk) : base("pk", rk)
        {
            CountAnger = allData.Count(d => d.Emotion.Equals("anger", StringComparison.OrdinalIgnoreCase));
            CountContempt = allData.Count(d => d.Emotion.Equals("comptent", StringComparison.OrdinalIgnoreCase));
            CountDisgust = allData.Count(d => d.Emotion.Equals("disgust", StringComparison.OrdinalIgnoreCase));
            CountFear = allData.Count(d => d.Emotion.Equals("fear", StringComparison.OrdinalIgnoreCase));
            CountHappiness = allData.Count(d => d.Emotion.Equals("happiness", StringComparison.OrdinalIgnoreCase));
            CountNeutral = allData.Count(d => d.Emotion.Equals("neutral", StringComparison.OrdinalIgnoreCase));
            CountSadness = allData.Count(d => d.Emotion.Equals("sadness", StringComparison.OrdinalIgnoreCase));
            CountSurprise = allData.Count(d => d.Emotion.Equals("surprise", StringComparison.OrdinalIgnoreCase));
            CountUpload = allData.Count();
            CountMale = allData.Count(d => d.Gender == Gender.Male);
            CountFemale = allData.Count(d => d.Gender == Gender.Female);
            CountBald = allData.Count(d => d.IsBald);
            CountMoustache = allData.Count(d => d.HasMoustache);
            CountBeard = allData.Count(d => d.HasBeard);
            AvgAge = allData.Average(d => d.Age);
            HairColor = JsonConvert.SerializeObject(allData.GroupBy(d => d.MainHairColor)
                                                           .ToDictionary(d => d.Key, d => d.Count()));
        }

        public string HairColor { get; set; }
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
        public double AvgAge { get; set; }
    }
}