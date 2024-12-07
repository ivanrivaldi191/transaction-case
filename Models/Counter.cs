using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Transactions;

namespace Transaction.Models
{
    public class Counter
    {
        [Key]
        public string? szCounterId { get; set; }
        public BigInteger? iLastNumber { get; set; }

        public static int GetCounter(SqlConnection connection, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM BOS_Counter", connection, transaction);
            SqlDataReader dataReader = cmd.ExecuteReader();
            int counter = 0;
            while (dataReader.Read())
            {
                counter = (int)Convert.ToInt64(dataReader["iLastNumber"]);
            }
            dataReader.Close();
            return counter;
        }

        public static void UpdateCounter(SqlConnection connection, SqlTransaction transaction, int counter)
        {
            SqlCommand cmdWriteCounter = new SqlCommand("UPDATE BOS_Counter SET iLastNumber = @counter", connection, transaction);
            cmdWriteCounter.Prepare();
            cmdWriteCounter.Parameters.Add("@counter", SqlDbType.Int).Value = counter;
            cmdWriteCounter.ExecuteNonQuery();
        }
    }
}
