using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.VisualBasic;

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

            Console.WriteLine("Finished sending messages to SQS. Press enter to continue.");
            Console.ReadKey();
            Console.WriteLine("Starting to read messages from SQS");

            ReceiveMessage(queueUrl, sqsClient);

            Console.WriteLine("Finished to read messages from SQS");

            Console.ReadKey();

        }

        private static void ReceiveMessage(string queueUrl, AmazonSQSClient sqsClient)
        {
            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = queueUrl;

            var counter = 0;

            var lenght = NumbersOfMessagesInQueue(queueUrl, sqsClient);

            while (counter < lenght)
            {
                ReceiveMessageResponse receiveMessageResponse =
                    Task.Run(async () => await sqsClient.ReceiveMessageAsync(receiveMessageRequest)).Result;

                if (receiveMessageResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    Message message = receiveMessageResponse.Messages[0];

                    ReportFilters filter = JsonSerializer.Deserialize<ReportFilters>(message.Body);

                    Console.WriteLine("*********************************");
                    Console.WriteLine($"SQS Message Id: {message.MessageId}");
                    Console.WriteLine($"Message Id: {filter.Id}");
                    Console.WriteLine($"Message DataIni: {filter.DataIni}");
                    Console.WriteLine($"Message DateEnd: {filter.DataEnd}");
                    Console.WriteLine("*********************************");
                    Console.WriteLine();
                }
                counter++;
            }
        }

        private static int NumbersOfMessagesInQueue(string queueUrl, AmazonSQSClient sqsClient)
        {
            GetQueueAttributesRequest attReq = new GetQueueAttributesRequest();
            attReq.QueueUrl = queueUrl;
            attReq.AttributeNames.Add("ApproximateNumberOfMessages");

            GetQueueAttributesResponse response =
                Task.Run(async () => await sqsClient.GetQueueAttributesAsync(attReq)).Result;

            var retval = response.ApproximateNumberOfMessages;
            return retval;
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
