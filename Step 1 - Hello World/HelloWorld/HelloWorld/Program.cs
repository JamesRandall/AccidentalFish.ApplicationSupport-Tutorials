using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AccidentalFish.ApplicationSupport.Core;
using AccidentalFish.ApplicationSupport.DependencyResolver;
using AccidentalFish.ApplicationSupport.Unity;
using AccidentalFish.ApplicationSupport.Azure;
using AccidentalFish.ApplicationSupport.Core.Queues;
using Microsoft.Practices.Unity;
using LoggerTypeEnum = AccidentalFish.ApplicationSupport.Core.Bootstrapper.LoggerTypeEnum;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            IUnityContainer unityContainer = new UnityContainer();
            IDependencyResolver dependencyResolver = new UnityApplicationFrameworkDependencyResolver(unityContainer);
            dependencyResolver
                .UseCore(loggerType: LoggerTypeEnum.Console)
                .UseAzure();

            IQueueFactory queueFactory = dependencyResolver.Resolve<IQueueFactory>();
            IAzureResourceManager resourceManager = dependencyResolver.Resolve<IAzureResourceManager>();
            IQueue <QueuedMessage> queue = queueFactory.CreateQueue<QueuedMessage>(
                "DefaultEndpointsProtocol=https;AccountName=<accountname>;AccountKey=<accountkey>",
                "myqueue");
            resourceManager.CreateIfNotExists(queue);

            Task.Run(() => DequeueMessages(queue));
            EnqueueMessages(queue);
        }

        private static void DequeueMessages(IQueue<QueuedMessage> queue)
        {
            while (true)
            {
                bool shouldPause = true;
                queue.Dequeue(msg =>
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(msg.Item.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                        shouldPause = false;
                        return true;
                    });
                if (shouldPause)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void EnqueueMessages(IQueue<QueuedMessage> queue)
        {
            bool shouldContinue = true;
            while (shouldContinue)
            {
                Console.Write("Message: ");
                string message = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(message))
                {
                    shouldContinue = false;
                }
                else
                {
                    queue.Enqueue(new QueuedMessage
                    {
                        Message = message
                    });
                }
            }
        }
    }
}
