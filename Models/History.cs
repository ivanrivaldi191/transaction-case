using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Transactions;

namespace Transaction.Models
{
    public class History
    {
        [Key, Column(Order = 0)]
        public required string szTransactionId { get; set; }
        [Key, Column(Order = 1)]
        public required string szAccountId { get; set; }
        [Key, Column(Order = 2)]
        public required string szCurrencyId { get; set; }
        public required DateTime dtmTransaction { get; set; }
        public required decimal decAmount { get; set; }
        public required string szNote { get; set; }

        public void CreateHistory(SqlConnection connection, SqlTransaction transaction)
        {
            SqlCommand cmdCreateHistory = new SqlCommand("INSERT INTO BOS_History (szTransactionId, szAccountId, szCurrencyId, dtmTransaction, decAmount, szNote)" +
                            " VALUES (@transaction, @account, @currency, @date, @amount, @type)", connection, transaction);
            cmdCreateHistory.Prepare();
            cmdCreateHistory.Parameters.Add("@transaction", SqlDbType.VarChar, 50).Value = this.szTransactionId;
            cmdCreateHistory.Parameters.Add("@account", SqlDbType.VarChar, 50).Value = this.szAccountId;
            cmdCreateHistory.Parameters.Add("@currency", SqlDbType.VarChar, 50).Value = this.szCurrencyId;
            cmdCreateHistory.Parameters.Add("@date", SqlDbType.DateTime).Value = this.dtmTransaction;
            cmdCreateHistory.Parameters.Add("@amount", SqlDbType.Decimal).Value = this.decAmount;
            cmdCreateHistory.Parameters.Add("@type", SqlDbType.VarChar).Value = this.szNote;
            cmdCreateHistory.ExecuteNonQuery();
        }
    }
}
