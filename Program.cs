using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace TesteSQS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SQS demo!");

            var messages = CreateMessages();

            Console.WriteLine("Creating queue!");

            AmazonSQSClient sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.APSoutheast2);
            string queueUrl = CreateQueue(sqsClient, "ReportsQueue", "10");


            Console.WriteLine($"Queue created. Url: {queueUrl}");
            Console.WriteLine("Press enter to continue!");
            Console.ReadKey();

            foreach (var message in messages)
            {
                string msg = JsonSerializer.Serialize(message);
                SendMessageResponse sendMessageResponse = SendMessage(msg, queueUrl, sqsClient);
                Console.WriteLine($"Message {message.Id} sent to queue. HTTP response code {sendMessageResponse.HttpStatusCode}");
            }

        }

        private static SendMessageResponse SendMessage(string msg, string queueUrl, AmazonSQSClient sqsClient)
        {
            SendMessageRequest sendMessageRequest = new SendMessageRequest();
            sendMessageRequest.QueueUrl = queueUrl;
            sendMessageRequest.MessageBody = msg;

            SendMessageResponse sendMessageResponse =
                Task.Run(async () => await sqsClient.SendMessageAsync(sendMessageRequest)).Result;
            return sendMessageResponse;
        }

        private static string CreateQueue(AmazonSQSClient sqsClient, string reportsqueue, string visibilityTimeout)
        {
            CreateQueueRequest createQueueRequest = new CreateQueueRequest();

            createQueueRequest.QueueName = reportsqueue;

            Dictionary<string,string> attrs = new Dictionary<string, string>();
            attrs.Add(QueueAttributeName.VisibilityTimeout, visibilityTimeout);
            createQueueRequest.Attributes = attrs;

            CreateQueueResponse createQueueResponse =
                Task.Run(async () => await sqsClient.CreateQueueAsync(createQueueRequest)).Result;

            return createQueueResponse.QueueUrl;
        }

        private static IEnumerable<ReportFilters> CreateMessages()
        {
            var listToReturn = new List<ReportFilters>();
            for (var i = 0; i < 100; i++)
            {
                listToReturn.Add(new ReportFilters
                {
                    Id = i,
                    DataIni = DateTime.Now.AddDays(i),
                    DataEnd = DateTime.Now.AddDays(i+1)
                });
            }

            return listToReturn;
        }
    }
}
