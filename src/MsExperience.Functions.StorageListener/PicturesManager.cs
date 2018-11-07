using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using MsExperience.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MsExperience.Functions.StorageListener
{
    public static class PicturesManager
    {
        [FunctionName("OnPictureUploaded")]
        [return: Queue("stats-calc-queue")]
        public static async Task<FaceResult> OnPictureUploaded(
            [BlobTrigger("images/{name}", Connection = "")] Stream myBlob,
            string name,
            [SignalR(HubName = "NotifyHub")] IAsyncCollector<SignalRMessage> hubClient,
            [Table("UploadedTable")] ICollector<FaceResult> uploadedPictureBinding,
            ILogger log)
        {
            //Get Cognitive Services Result from image stream
            var coginitiveResult = CallCognitiveServices(myBlob);

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            //If no data returned from Cognitive, just log and return
            if (coginitiveResult == null)
            {
                log.LogWarning($"no face result for {name}");
                return null;
            }

            //Humanize Cognitive Services response
            var faceResult = GetFaceResult(coginitiveResult, name);

            //Add data to Table storage
            uploadedPictureBinding.Add(faceResult);

            //Push message to SignalR
            await hubClient.AddAsync(new SignalRMessage
            {
                Target = "NotifyNewPerson",
                Arguments = new[] { faceResult }
            });

            //Send message in Storage Queue
            return faceResult;
        }

        [FunctionName("OnCognitiveCompleted")]
        public static async Task OnQueueMessageReceivedAsync(
            [QueueTrigger("stats-calc-queue")] FaceResult uploadedPicture,
            [Table("UploadedTable")] CloudTable uploadTable,
            [Table("stats")] CloudTable statsTable,
            [SignalR(HubName = "NotifyHub")] IAsyncCollector<SignalRMessage> hubClient)
        {
            //Load data from Table Storage
            var allData = await uploadTable.QueryEntitiesAsync<FaceResultEntity>();

            //Calculate statistics
            var globalStats = new StatEntity(allData, "global");

            //Update stats in Table Storage
            await statsTable.InsertOrReplaceAsync(globalStats);

            //Push message to SignalR
            await hubClient.AddAsync(new SignalRMessage
            {
                Target = "NotifyStatChanged",
                Arguments = new object[] { globalStats }
            });
        }

        private static FaceResult GetFaceResult(Face face, string blobName)
        {
            var result = new FaceResult
            {
                PartitionKey = DateTime.Today.AddMinutes((int)DateTime.Now.TimeOfDay.TotalMinutes).ToString("o"),
                RowKey = $"{blobName}-{Guid.NewGuid().ToString()}",
                BlobName = blobName,
                Age = (int)face.FaceAttributes.Age,
            };

            if (face.FaceAttributes.Gender != string.Empty)
            {
                if (face.FaceAttributes.Gender == "male")
                    result.Gender = Gender.Male;
                else
                    result.Gender = Gender.Female;
            }

            if (face.FaceAttributes.FacialHair != null)
            {
                result.HasBeard = face.FaceAttributes.FacialHair.Beard > 0.5;
                result.HasMoustache = face.FaceAttributes.FacialHair.Moustache > 0.5;
            }

            if (face.FaceAttributes.Hair != null)
            {
                result.IsBald = face.FaceAttributes.Hair.Bald > 0.5;
                result.MainHairColor = face.FaceAttributes.Hair.HairColor.OrderByDescending(h => h.Confidence).FirstOrDefault()?.Color ?? "none";
            }

            if (face.FaceAttributes.Emotion != null)
            {
                var emotions = new Dictionary<string, float> {
                    { "Fear", face.FaceAttributes.Emotion.Fear },
                    { "Anger", face.FaceAttributes.Emotion.Anger },
                    { "Comptent", face.FaceAttributes.Emotion.Contempt },
                    { "Disgust", face.FaceAttributes.Emotion.Disgust },
                    { "Happiness", face.FaceAttributes.Emotion.Happiness },
                    { "Neutral", face.FaceAttributes.Emotion.Neutral },
                    { "Sadness", face.FaceAttributes.Emotion.Sadness },
                    { "Surprise", face.FaceAttributes.Emotion.Surprise }
                };
                result.Emotion = emotions.OrderByDescending(e => e.Value).First().Key;
            }

            return result;
        }

        private static Face CallCognitiveServices(Stream myBlob)
        {
            var endpoint = "https://westeurope.api.cognitive.microsoft.com/face/v1.0";
            var action = "/detect?returnFaceId=false&returnFaceAttributes=age,gender,smile,glasses,facialHair,emotion,hair";

            var request = WebRequest.Create($"{endpoint}{action}");
            request.Method = "POST";
            request.Headers.Add("Ocp-Apim-Subscription-Key", "71b6a43b6d064f4391e1bcfb3e9ba0a1");
            request.ContentType = "application/octet-stream";
            var requestStream = request.GetRequestStream();
            myBlob.CopyTo(requestStream);

            string responseString;
            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var streamReader = new StreamReader(responseStream))
                responseString = streamReader.ReadToEnd();

            var result = JsonConvert.DeserializeObject<Face[]>(responseString, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            if (result.Any())
                return result[0];
            else
                return null;
        }
    }
}
