using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConverter
{
    class DatabaseConverter
    {
        public string Path { get; set; }
        public List<string> Tables { get; set; }
        public OdbcConnection Connection { get; private set; }
        public List<SimpleTable> DataSet
        {
            get
            {
                if (!Connected)
                    Connect();

                if (Tables.Count < 1)
                    ReadSchema();

                return GetDataSet();
            }
        }

        public bool Connected { get; private set; }

        public DatabaseConverter(string path)
        {
            Tables = new List<string>();
            Path = path;
        }


        public void Connect()
        {
            Connection = new OdbcConnection
            {
                ConnectionString = $"Driver={{Microsoft Access Driver (*.mdb)}};Dbq={Path};Uid=Admin;Pwd=;"
            };
            Connection.Open();
            Connected = true;
        }

        public void ReadSchema()
        {
            var tableMetaData = Connection.GetSchema(OdbcMetaDataCollectionNames.Tables);

            using (var reader = tableMetaData.CreateDataReader())
            {
                object[] array;
                while (reader.Read())
                {
                    array = new object[reader.FieldCount];

                    reader.GetValues(array);

                    if (TableTypes.Table == (TableTypes)Enum.Parse(typeof(TableTypes),
                        ((string)array[3]).Replace(" ", ""), true))
                    {

                        Tables.Add((string)array[2]);
                    }
                }
            }
        }

        public List<SimpleTable> GetDataSet()
        {
            OdbcCommand command;

            var tableData = new List<SimpleTable>();

            foreach (var tableName in Tables)
            {
                command = new OdbcCommand($"select * from {tableName};", Connection);
                var simpleTable = new SimpleTable(tableName);

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        foreach (DataRow row in reader.GetSchemaTable().Rows)
                        {
                            simpleTable.Columns.Add(
                                new KeyValuePair<string, Type>(
                                    (string)row.ItemArray[0],
                                    (Type)row.ItemArray[5]));
                        };

                        object[] values;

                        while (reader.Read())
                        {
                            values = new object[reader.FieldCount];
                            reader.GetValues(values);
                            simpleTable.Data.Add(values);
                        }
                    }
                    tableData.Add(simpleTable);
                }
                catch
                {
                    continue;
                }
            }

            return tableData;
        }

        private enum TableTypes
        {
            SystemTable,
            Table
        }
    }
}
