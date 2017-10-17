using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Bank;
using Newtonsoft.Json;

public class RpcClient
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
    private readonly IBasicProperties props;

    public RpcClient()
    {
        var factory = new ConnectionFactory() { HostName = "datdb.cphbusiness.dk" };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        replyQueueName = channel.QueueDeclare().QueueName;
        consumer = new EventingBasicConsumer(channel);

        props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body;
            var response = Encoding.UTF8.GetString(body);

            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                respQueue.Add(response);
            }
        };
    }

    public string Call(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(
            exchange: "",
            routingKey: "rpc_queue_djur",
            basicProperties: props,
            body: messageBytes);

        channel.BasicConsume(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: true);

        return respQueue.Take(); ;
    }

    public void Close()
    {
        connection.Close();
    }
}

public class Rpc
{
    public static void Main(string[] args)
    {
        double loanAmount = 250000;
        if(args.Length > 0){
            loanAmount = Convert.ToDouble(args[0]);
        }
        
        int loanDuration = 720;
        if(args.Length > 1){
            loanDuration = Convert.ToInt32(args[1]);
        }
        
        string ssn = "1602942917";
        if(args.Length > 2){
            ssn = args[2];
        }

        int creditScore = 400;

        LoanRequest request = new LoanRequest(ssn,loanAmount,loanDuration,creditScore);
        Console.WriteLine("Requesting -  amount: {0} duration: {1})", request.LoanAmount, request.LoanDuration);
        
        var rpcClient = new RpcClient();
        var response = rpcClient.Call(JsonConvert.SerializeObject(request));
        var deserialized = JsonConvert.DeserializeObject<LoanRespond>(response);
        Console.WriteLine("Interest rate: '{0}'", deserialized.InterestRate);
        rpcClient.Close();
    }
}
