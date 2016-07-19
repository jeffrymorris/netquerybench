using System;
using System.Collections.Concurrent;


namespace netquerybench.measurements
{
    public class Measurements
    {
        static Measurements singleton = null;
        private ConcurrentDictionary<string, OneMeasurement> dict = new ConcurrentDictionary<string, OneMeasurement>();
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

    }
}
