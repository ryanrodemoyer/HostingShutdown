using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Routing;
using log4net;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace HostingShutdown3
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            log4net.Config.XmlConfigurator.Configure();
        }
    }

    public static class Globals
    {
        public static Guid InstanceId = Guid.NewGuid();

        private static readonly ILog log = LogManager.GetLogger(typeof(SharedResource));

        public static void WriteToLog(Guid instanceId, string message)
        {
            log.Info($"{DateTimeOffset.Now}, InstanceId={instanceId.ToString()}, Message={message}");
            //using (var conn = new SQLiteConnection("Data Source=messages.db;New=true;"))
            //{
            //    conn.Open();

            //    using (var cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = "insert Messages (InstanceId,Content,Timestamp) values (@instanceId,@message,@timestamp)";
            //        cmd.Parameters.AddWithValue("@instanceId", instanceId);
            //        cmd.Parameters.AddWithValue("@content", message);
            //        cmd.Parameters.AddWithValue("@timestamp", DateTimeOffset.Now);

            //        cmd.ExecuteNonQuery();
            //    }
            //}
        }
    }

    //public class AppContext : DbContext
    //{
    //    public DbSet<Message> Messages { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    {

    //    }

    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {

    //    }
    //}

    //public class Message
    //{
    //    public int Id { get; set; }
    //    public string InstanceId { get; set; }
    //    public string Content { get; set; }
    //    public DateTime Timestamp { get; set; }
    //}

    public class SharedResourceLifetime : IRegisteredObject
    {
        public SharedResourceLifetime()
        {
            Globals.WriteToLog(Globals.InstanceId, "SharedResourceLifetime.ctor");

            HostingEnvironment.RegisterObject(this);

            AppDomain.CurrentDomain.ProcessExit += (o, e) => { Globals.WriteToLog(Globals.InstanceId, "AppDomain.ProcessExit"); };
        }

        public void Stop(bool immediate)
        {
            Globals.WriteToLog(Globals.InstanceId, "SharedResourceLifetime.Stop");

            HostingEnvironment.UnregisterObject(this);
        }
    }

    public sealed class SharedResource
    {
        public static SharedResource _instance = new SharedResource();

        private static SharedResourceLifetime Lifetime = new SharedResourceLifetime();

        private IConnection _connection = null;

        private static object _lock = new object();

        public static SharedResource Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SharedResource();
                        }
                    }
                }

                return _instance;
            }
        }

        private SharedResource()
        {
            Globals.WriteToLog(Globals.InstanceId, "SharedResource.ctor");

            CreateConnection();
        }

        private void Shutdown()
        {
            _connection?.Dispose();

            _instance = null;
        }

        private void CreateConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            // "guest"/"guest" by default, limited to localhost connections
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "localhost";

            _connection = factory.CreateConnection();
        }
    }
}
