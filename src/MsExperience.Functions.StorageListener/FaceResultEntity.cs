using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MsExperience.Models;

namespace MsExperience.Functions.StorageListener
{
    public class FaceResultEntity : FaceResult, ITableEntity
    {
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            BlobName = properties["BlobName"].StringValue;
            HasBeard = properties[nameof(HasBeard)].BooleanValue.Value;
            HasMoustache = properties[nameof(HasMoustache)].BooleanValue.Value;
            IsBald = properties[nameof(IsBald)].BooleanValue.Value;
            Gender = (Gender)Enum.Parse(typeof(Gender), properties[nameof(Gender)].StringValue);
            Age = properties[nameof(Age)].Int32Value.Value;
            Emotion = properties[nameof(Emotion)].StringValue;
            MainHairColor = properties[nameof(MainHairColor)].StringValue;
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return null;
        }
    }
}
