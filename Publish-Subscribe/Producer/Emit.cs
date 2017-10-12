using System;
using RabbitMQ.Client;
using System.Text;

class Emit
{
    public static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "datdb.cphbusiness.dk" };
        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "djurLogs", type: "fanout");

            var props = channel.CreateBasicProperties();
            props.ReplyTo = "djurtest";
            props.CorrelationId = "1234";

            // var message = GetJOSN();
            var message = GetMessage(args);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "cphbusiness.bankJSON",
                                 routingKey: "",
                                 basicProperties: props,
                                 body: body);
            Console.WriteLine(" [x] Sent {0}", message);
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static string GetMessage(string[] args)
    {
        return ((args.Length > 0)
               ? string.Join(" ", args)
               : "info: Hello World!");
    }

    // private static string GetJSON(){
    //     return '{"ssn": 1602942917,"creditScore": 400,"loanAmount":10.0,"loanDuration": 360}'
    // }
}