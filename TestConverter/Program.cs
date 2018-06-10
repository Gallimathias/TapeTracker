using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestConverter
{
    class Program
    {


        static void Main(string[] args)
        {

            var set = new DatabaseConverter(@"C:\Users\bid01023\Downloads\ttfull\tracker\TAPETRAK\TAPETRAK.MDB").DataSet;

            //var targetDatabase = TargetDatabase.GetDatabase(@".\TapeTrak.db");
            var source = @".\TapeTrak.db";
            var typeBuilder = new DbTypeBuilder();
            var list = new List<Type>();

            foreach (var table in set)
                list.Add(typeof(DbSet<>).MakeGenericType(typeBuilder.CreateType(table)));

        
            var dbType = typeBuilder.CreateType("TapeTrackDatabase", typeof(Db), list.ToArray());
            //var db = Activator.CreateInstance(dbType);
            var construct = dbType.GetConstructor(new[] { typeof(DbContextOptions) });
            var db = construct.Invoke(null);
            SQLitePCL.Batteries.Init();
            var database = dbType.GetProperty("Database").GetValue(db);
            var method = database.GetType().GetRuntimeMethod("EnsureCreated", new Type[0]);
            method.Invoke(database, null);
        }
    }
}
