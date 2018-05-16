using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\bid01023\Downloads\ttfull\tracker\TAPETRAK\TAPETRAK.MDB";
            List<string> tables = new List<string>();
            using (OdbcConnection connection = new OdbcConnection())
            {
                connection.ConnectionString = $"Driver={{Microsoft Access Driver (*.mdb)}};Dbq={path};Uid=Admin;Pwd=;";
                connection.Open();

                var tableMetaData = connection.GetSchema(OdbcMetaDataCollectionNames.Tables);

                using (var reader = tableMetaData.CreateDataReader())
                {
                    object[] array = new object[reader.FieldCount];
                    while (reader.Read())
                    {
                        if (reader.FieldCount != array.Length)
                            array = new object[reader.FieldCount];

                        reader.GetValues(array);

                        if (TableTypes.Table == (TableTypes)Enum.Parse(typeof(TableTypes),
                            ((string)array[3]).Replace(" ", ""), true))
                        {

                            tables.Add((string)array[2]);
                        }
                    }
                }

                OdbcCommand command;

                var tableData = new List<SimpleTable>();

                foreach (var tableName in tables)
                {
                    command = new OdbcCommand($"select * from {tableName};", connection);
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

                            object[] values = new object[reader.FieldCount];

                            while (reader.Read())
                            {
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

            }
        }

        private enum TableTypes
        {
            SystemTable,
            Table
        }
    }
}
