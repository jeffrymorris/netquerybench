using System;
using System.Collections.Concurrent;
using netquerybench.client;


namespace netquerybench.measurements
{
    public class Measurements
    {
        static Measurements singleton = null;
        private ConcurrentDictionary<string, OneMeasurement> dict = new ConcurrentDictionary<string, OneMeasurement>();
        private ConcurrentDictionary<string, int> opStatus = new ConcurrentDictionary<string, int>();
        private Boolean initialized = false;
        private DateTime startTime;

        public static Measurements GetMesMeasurements()
        {
            if (singleton == null)
            {
                singleton = new Measurements();
            }
            return singleton;
        }

        public void PrintSummary()
        {
            var endTime = DateTime.UtcNow;
            int total = 0;
            foreach (var entry in dict)
            {
                total += entry.Value.GetCount();
            }
            double throughput = total/ (endTime -startTime).Seconds;
            Console.WriteLine("Overall Summary");
            Console.WriteLine("Throughput ops/s " + throughput);

            int successPercent = (opStatus["Success"]/total)*100;
            Console.WriteLine("There were "+ successPercent +"% of ops successful");
            foreach (var entry in dict)
            {
                Console.WriteLine("Summary for " + entry.Key);
                Console.Write(entry.Value.GetSummary());
            }
        }

        private OneMeasurement getOneMeasurement(string operation)
        {
            OneMeasurement measurement = dict.GetOrAdd(operation,
                stats => new OneMeasurementRaw()
            );
            return measurement;

        }

        public void Measure(string operation, int latency)
        {
            if (initialized == false)
            {
                startTime = DateTime.Now;
                initialized = true;
            }
            OneMeasurement m = getOneMeasurement(operation);
            m.Measure(latency);
        }

        public void AddStatus(Status status)
        {
            switch (status)
            {
                case Status.Failure:
                    opStatus.AddOrUpdate("Failue", 1, (s, i) => i + 1);
                    break;
                case Status.Success:
                    opStatus.AddOrUpdate("Success", 1, (s, i) => i + 1);
                    break;
                case Status.IncorrectRecordCount:
                    opStatus.AddOrUpdate("Incorrect record count", 1, (s, i) => i + 1);
                    break;
                case Status.ValueMismatch:
                    opStatus.AddOrUpdate("Value mismatch", 1, (s, i) => i + 1);
                    break;
            }
        }

    }
}
