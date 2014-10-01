using System;
using Newtonsoft.Json;

namespace Minimod.RabbitMqTopicsMessageStream
{
    internal class RabbitMqTopicsMessageStreamSample
    {
        static void Main(string[] args)
        {
            const string mqConnectionString = "your connection string";

            Func<object, string> serializer = JsonConvert.SerializeObject;
            Func<string, Type, object> deserializer = (value, type) => JsonConvert.DeserializeObject(value, type, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var stream1 = new RabbitMqTopicsMessageStream(mqConnectionString, "queue1")
            {
                Serializer = serializer,
                Deserializer = deserializer,
                RoutingResolver = x => x.Name
            };
            var stream2 = new RabbitMqTopicsMessageStream(mqConnectionString, "queue2")
            {
                Serializer = serializer,
                Deserializer = deserializer,
                RoutingResolver = x => "queue2"
            };

            stream1.Subscribe<Test1>(Console.WriteLine, error => Console.WriteLine(error.Message));
            stream2.Subscribe<Test2>(Console.WriteLine, error => Console.WriteLine(error.Message));

            Console.ReadLine();
            stream1.Dispose();
            stream2.Dispose();
        }

    }

    internal class Test1
    {
    }

    internal class Test2
    {
    }
}