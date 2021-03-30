using System;
using System.Collections.Generic;
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
                    DataIni = DateTime.Now.AddDays(i),
                    DataEnd = DateTime.Now.AddDays(i+1)
                });
            }

            return listToReturn;
        }
    }
}
