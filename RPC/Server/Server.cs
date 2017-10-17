using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Bank;
using Newtonsoft.Json;
using System.Threading;

class RPCServer
{
    public static string QUEUE = "rpc_queue_djur";
    public static string HOST = "datdb.cphbusiness.dk";

    public static void Main(string[] args)
    {
        if(args.Length > 0){
            HOST = args[0];
        }
        if(args.Length > 1){
            QUEUE = args[1];
        }

        var factory = new ConnectionFactory() { HostName = HOST };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(
                queue: QUEUE,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(
                queue: QUEUE,
                autoAck: false,
                consumer: consumer);

            Console.WriteLine(" [x] Awaiting RPC requests");

            consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    var deserialized = JsonConvert.DeserializeObject<LoanRequest>(message);
                    Loan loan = new Loan(deserialized);
                    LoanRespond respond = loan.getLoan();
                    Console.WriteLine("Incomming loan request - ssn:{0}, loan amount:{1}, loan duration:{2}", deserialized.Ssn, deserialized.LoanAmount, deserialized.LoanDuration);
                    Console.WriteLine("Calculated interest rate:{0}", respond.InterestRate);
                    response = JsonConvert.SerializeObject(respond);
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps,
                        body: responseBytes);

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false);
                }
            };

            Console.WriteLine(" Press [enter] to exit.");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}