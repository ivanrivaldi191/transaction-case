using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics.Metrics;

namespace Transaction.Models
{
    public class Balance
    {
        [Key, Column(Order = 0)]
        public string? szAccountId {get; set;}
        [Key, Column(Order = 1)]
        public required string szCurrencyId {get; set;}
        public required decimal decAmount { get; set; }

        public void UpdateBalance(SqlConnection connection, SqlTransaction transaction, bool isSurplus = true)
        {
            if (this.AccountAmount(connection, transaction) < 0)
            {
                this.CreateIfNotExists(connection, transaction, isSurplus);
                return;
            }
            string expr = "UPDATE BOS_Balance SET decAmount = decAmount + @amount WHERE szAccountId = @account AND szCurrencyId = @currency";
            if (!isSurplus)
            {
                expr = "UPDATE BOS_Balance SET decAmount = decAmount - @amount WHERE szAccountId = @account AND szCurrencyId = @currency";
            }
            SqlCommand cmdUpdateBalance = new SqlCommand(expr, connection, transaction);
            cmdUpdateBalance.Prepare();
            cmdUpdateBalance.Parameters.Add("@amount", SqlDbType.Decimal).Value = this.decAmount;
            cmdUpdateBalance.Parameters.Add("@account", SqlDbType.VarChar, 50).Value = this.szAccountId;
            cmdUpdateBalance.Parameters.Add("@currency", SqlDbType.VarChar, 50).Value = this.szCurrencyId;
            cmdUpdateBalance.ExecuteNonQuery();
        }

        private void CreateIfNotExists(SqlConnection connection, SqlTransaction transaction, bool isSurplus = true)
        {
            string expr = "BEGIN " +
                "IF NOT EXISTS (SELECT szAccountId FROM BOS_Balance " +
                "WHERE szAccountId = @account AND szCurrencyId = @currency) " +
                "BEGIN " +
                "INSERT INTO BOS_Balance (szAccountId, szCurrencyId, decAmount) " +
                "VALUES (@account, @currency, @amount) " +
                "END " +
                "END";
            decimal amount = isSurplus ? this.decAmount : 0;
            SqlCommand cmdCreateNewBalance = new SqlCommand(expr, connection, transaction);
            cmdCreateNewBalance.Prepare();
            cmdCreateNewBalance.Parameters.Add("@amount", SqlDbType.Decimal).Value = amount;
            cmdCreateNewBalance.Parameters.Add("@account", SqlDbType.VarChar, 50).Value = this.szAccountId;
            cmdCreateNewBalance.Parameters.Add("@currency", SqlDbType.VarChar, 50).Value = this.szCurrencyId;
            cmdCreateNewBalance.ExecuteNonQuery();
        }

        public decimal AccountAmount(SqlConnection connection, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand("SELECT decAmount FROM BOS_Balance WHERE szAccountId = @account AND szCurrencyId = @currency", connection, transaction);
            cmd.Prepare();
            cmd.Parameters.Add("@account", SqlDbType.VarChar, 50).Value = this.szAccountId;
            cmd.Parameters.Add("@currency", SqlDbType.VarChar, 50).Value = this.szCurrencyId;
            SqlDataReader dataReader = cmd.ExecuteReader();
            decimal amount = -1;
            while (dataReader.Read())
            {
                amount = (decimal)dataReader["decAmount"];
            }
            dataReader.Close();
            return amount;
        }
    }
}
