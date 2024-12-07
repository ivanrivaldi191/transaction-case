using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.Data;
using Transaction.Models;

namespace Transaction.Controllers
{
    [Route("api/history")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HistoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("list")]
        [HttpGet]
        public async Task<ActionResult> GetTransactions()
        {
            List<History> histories = new List<History>();
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("SELECT * FROM BOS_History", connection, transaction);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, ex.Message);
                    }
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    History history = new History()
                    {
                        szTransactionId = dt.Rows[i]["szTransactionId"].ToString(),
                        szAccountId = dt.Rows[i]["szAccountId"].ToString(),
                        szCurrencyId = dt.Rows[i]["szCurrencyId"].ToString(),
                        dtmTransaction = Convert.ToDateTime(dt.Rows[i]["dtmTransaction"]),
                        decAmount = Convert.ToDecimal(dt.Rows[i]["decAmount"]),
                        szNote = dt.Rows[i]["szNote"].ToString(),
                    };
                    //return Ok(history);
                    histories.Add(history);
                }

                return Ok(histories);
                connection.Close();
            }
        }
    }
}
