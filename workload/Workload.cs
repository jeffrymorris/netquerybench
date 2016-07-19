using System;
using System.Collections.Generic;
using System.Text;
using netquerybench.client;

namespace netquerybench.workload
{

    public class Workload
    {
        private List<String> operationsList = new List<string>();
        DiscreteGenerator operationChooser = new DiscreteGenerator();
        private IntegerGenerator keyChooser;
        private UniformIntegerGenerator fieldChooser;
        private List<string> fieldNames = new List<string>();

        public string Table { get; set; }

        public int FieldCount { get; set; }

        public int FieldLength { get; set; }

        public bool ReadAllFields { get; set; }

        public double ReadProportion { get; set; }

        public double ScanProportion { get; set; }

        public double UpdateProportion { get; set; }

        public double Insertproportion { get; set; }

        public void Init()
        {
          
            if (ReadProportion > 0)
            {
                string operation = "READ";
                operationsList.Add(operation);
                operationChooser.AddValue(ReadProportion, operation);
            }
            if (ScanProportion > 0)
            {
                string operation = "SCAN";
                operationsList.Add(operation);
                operationChooser.AddValue(ScanProportion, operation);
            }
            if (UpdateProportion > 0)
            {
                string operation = "UPDATE";
                operationsList.Add(operation);
                operationChooser.AddValue((double)UpdateProportion, operation);
            }
            if (Insertproportion > 0)
            {
                string operation = "INSERT";
                operationsList.Add(operation);
                operationChooser.AddValue(Insertproportion, operation);
            } 
            keyChooser = new CounterGenerator(0);
            for (int i = 0; i < FieldCount; i++)
            {
                fieldNames.Add("Field" + i);
            }
            fieldChooser = new UniformIntegerGenerator(0, FieldCount-1);

        }

        private string buildKeyName(int keynum)
        {
            return "user" + keynum;
        }

        private String buildValue(string key, string fieldName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(key);
            sb.Append(":");
            sb.Append(fieldName);
            while (sb.Length < FieldLength)
            {
                sb.Append(":");
                sb.Append(sb.ToString().GetHashCode());
            }
            return sb.ToString();
        }

        public void DoTransaction(DB db)
        {
            String operation = operationChooser.NextString();
            int keyNum = keyChooser.NextInt();
            string keyName = buildKeyName(keyNum);
            HashSet<String> fields = new HashSet<string>();
            if (!ReadAllFields)
            {
                int fieldNum = fieldChooser.NextInt();
                fields.Add("Field" + fieldNum);
            }
            else
            {
                for (int i = 0; i < fieldNames.Count; i ++)
                {
                    fields.Add(fieldNames[i]);
                }
            }
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            for (int i = 0; i < fieldNames.Count - 1; i++)
            {
                fieldValues.Add(fieldNames[i], buildValue(keyName, fieldNames[i]));

            }
            
            switch (operation)
            {
                case "READ":
                    db.Read(Table, keyName, fields);
                    break;
                case "UPDATE":
                    db.Update(Table, keyName, fieldValues);
                    break;
                case "INSERT":
                    db.Update(Table, keyName, fieldValues);
                    break;
                case "SCAN":
                    db.Read(Table, keyName, fields);
                    break;
            }

        }

        public void DoInsert(DB db)
        {
            int keyNum = keyChooser.NextInt();
            string keyName = buildKeyName(keyNum);
            HashSet<String> fields = new HashSet<string>();
            for (int i = 0; i < fieldNames.Count; i ++)
            {
                fields.Add(fieldNames[i]);
            }
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            for (int i = 0; i < fieldNames.Count - 1; i++)
            {
                fieldValues.Add(fieldNames[i], buildValue(keyName, fieldNames[i]));
            }
            db.Insert(Table, keyName, fieldValues);

        }


    }
}
