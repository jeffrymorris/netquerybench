using System;
using System.Collections.Generic;
using System.Threading;
using CommandLineParser.Arguments;
using netquerybench.client;
using netquerybench.measurements;
using netquerybench.workload;

namespace netquerybench
{
  class Options {
        [ValueArgument(typeof(int), 't', DefaultValue = 1)]
        public int threads;

        [ValueArgument(typeof(int), 'd', DefaultValue = 1000)] 
        public int documentcount;

        [ValueArgument(typeof(int), 'o', DefaultValue = 10000)]
        public int operationcount;

        [ValueArgument(typeof(int), 'f', DefaultValue = 10)]
        public int fieldCount;

        [ValueArgument(typeof(int), 'l', DefaultValue = 100)]
        public int fieldLength;

        [ValueArgument(typeof(int), 'w', DefaultValue = 1000)]
        public int scanLength;

        [ValueArgument(typeof(string), 'x', DefaultValue = "default")]
        public string table;

        [ValueArgument(typeof(double), 'r', DefaultValue = 0)]
        public double readratio;

        [ValueArgument(typeof(double), 'u', DefaultValue = 0.5)] 
        public double updateratio;

        [ValueArgument(typeof(double), 's', DefaultValue = 0.5)] 
        public double scanratio;

        [ValueArgument(typeof(double), 'i',  DefaultValue = 0)]
        public double insertratio;

        [ValueArgument(typeof(string), 'm',  DefaultValue = "run")]
        public string mode;

        [ValueArgument(typeof(Boolean), 'k', DefaultValue = false)]
        public Boolean useKV;

        [ValueArgument(typeof(Boolean), 'a', DefaultValue = false)]
        public Boolean readAllFields;

        [ValueArgument(typeof(string), 'c', DefaultValue = "127.0.0.1")]
        public string hostname;

        [ValueArgument(typeof(int), 'p', DefaultValue = 8091)]
        public int port;

        [ValueArgument(typeof(string), 'b', DefaultValue = "default")]
        public string bucketName;

        [ValueArgument(typeof(string), 'g', DefaultValue = "")]
        public string bucketPassword;
  }


    class Program
    {
        private static Workload getWorkload(Options opts)
        {
            var workload = new Workload();
            workload.FieldCount = opts.fieldCount;
            workload.Insertproportion = opts.insertratio;
            workload.UpdateProportion = opts.updateratio;
            workload.ScanProportion = opts.scanratio;
            workload.ReadProportion = opts.readratio;
            workload.ReadAllFields = opts.readAllFields;
            workload.Table = opts.table;
            workload.FieldLength = opts.fieldLength;
            workload.ScanLength = opts.scanLength;
            workload.Init();
            return workload;
        }
        public static void Main(string[] args)
        {
            var parser = new CommandLineParser.CommandLineParser();
            var opts = new Options();
            parser.ExtractArgumentAttributes(opts);
            parser.ParseCommandLine(args);

            Workload workload = getWorkload(opts);
            Measurements measurements = Measurements.GetMesMeasurements();
            var clients = new List<Thread>();
            int opcount = opts.operationcount/opts.threads;

            for (int i = 0; i < opts.threads; i++)
            {
                DB db = new DB();
                try
                {
                    db.Init(opts.hostname, opts.port, opts.bucketName, opts.bucketPassword, opts.useKV, measurements);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
                Client client;
                if (opts.mode.Equals("load") == true)
                {
                    client = new Client(db, false, workload, opcount, opts.documentcount);
                }
                else
                {
                    client = new Client(db, true, workload, opcount, opts.documentcount);
                }
                Thread clientThread = new Thread(new ThreadStart(client.Run));
                clientThread.Start();
                clients.Add(clientThread);
            }

            foreach (var ct in clients)
            {
                ct.Join();
            }

            measurements.PrintSummary();
        }
    }
}
