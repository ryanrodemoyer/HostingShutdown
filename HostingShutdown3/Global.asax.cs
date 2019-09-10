using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Routing;

namespace HostingShutdown3
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }

    public static class Globals
    {
        public static Guid InstanceId = Guid.NewGuid();

        public static void WriteToLog(Guid instanceId, string message)
        {
            //using (var conn = new SqliteConnection("Filename=messages.db;"))
            //{
            //    conn.Open();

            //    using (var cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = "insert dbo.Messages (InstanceId,Message,Timestamp) values (@instanceId,@message,@timestamp)";
            //        cmd.Parameters.AddWithValue("@instanceId", instanceId);
            //        cmd.Parameters.AddWithValue("@message", message);
            //        cmd.Parameters.AddWithValue("@timestamp", DateTimeOffset.Now);

            //        cmd.ExecuteNonQuery();
            //    }
            //}
        }
    }

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
        public static SharedResource Instance = new SharedResource();

        private static SharedResourceLifetime Lifetime = new SharedResourceLifetime();

        private SharedResource()
        {
            Globals.WriteToLog(Globals.InstanceId, "SharedResource.ctor");
        }
    }
}
