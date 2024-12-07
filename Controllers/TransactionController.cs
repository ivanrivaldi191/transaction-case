using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Transaction.Models;
using Transaction.Requests;

namespace Transaction.Controllers
{
    [Route("api/transaction")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TransactionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("deposit/{accountId}")]
        [HttpPut]
        public async Task<IActionResult> Deposit(string accountId, [FromBody] BalanceRequest balance)
        {
            if (balance.decAmount <= 0)
            {
                return StatusCode(422, "Amount must not lower than equal from zero!");
            }

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    try
                    {
                        int counter = Counter.GetCounter(connection, transaction);
                        DateTime now = DateTime.Now;
                        string transactionId = this.GenerateTransactionId(counter);

                        History history = new History(){
                            szTransactionId = transactionId,
                            szAccountId = accountId,
                            szCurrencyId = balance.szCurrencyId,
                            dtmTransaction = now,
                            decAmount = balance.decAmount,
                            szNote = "SETOR"

                        };
                        history.CreateHistory(connection, transaction);

                        Balance updateBalance = new Balance() { 
                            szAccountId = accountId,
                            szCurrencyId = balance.szCurrencyId,
                            decAmount = balance.decAmount
                        };
                        updateBalance.UpdateBalance(connection, transaction, true);

                        Counter.UpdateCounter(connection, transaction, ++counter);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, ex.Message);
                    }
                }
                connection.Close();
                return Ok("Success");
            }
        }

        [Route("withdrawal/{accountId}")]
        [HttpPut]
        public async Task<IActionResult> Withdrawal(string accountId, [FromBody] BalanceRequest balance)
        {
            if (balance.decAmount <= 0)
            {
                return StatusCode(422, "Amount must not lower than equal from zero!");
            }

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    Balance currentUserBalance = new Balance()
                    {
                        szAccountId = accountId,
                        decAmount = balance.decAmount,
                        szCurrencyId = balance.szCurrencyId
                    };
                    decimal currentUserAmount = currentUserBalance.AccountAmount(connection, transaction);
                    if (currentUserAmount <= 0 || balance.decAmount > currentUserAmount)
                    {
                        return StatusCode(422, "Amount and/or current balance must be above zero!");
                    }

                    try
                    {
                        int counter = Counter.GetCounter(connection, transaction);
                        DateTime now = DateTime.Now;
                        string transactionId = this.GenerateTransactionId(counter);

                        History history = new History()
                        {
                            szTransactionId = transactionId,
                            szAccountId = accountId,
                            szCurrencyId = balance.szCurrencyId,
                            dtmTransaction = now,
                            decAmount = balance.decAmount,
                            szNote = "TARIK"

                        };
                        history.CreateHistory(connection, transaction);

                        Balance updateBalance = new Balance()
                        {
                            szAccountId = accountId,
                            szCurrencyId = balance.szCurrencyId,
                            decAmount = balance.decAmount
                        };
                        updateBalance.UpdateBalance(connection, transaction, false);

                        Counter.UpdateCounter(connection, transaction, ++counter);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, ex.Message);
                    }
                }
                connection.Close();
                return Ok("Success");
            }
        }

        [Route("transfer/{accountId}")]
        [HttpPut]
        public async Task<IActionResult> Transfer(string accountId, [FromBody] TransferRequest balance)
        {
            if (balance.decAmount <= 0)
            {
                return StatusCode(422, "Amount must not lower than equal from zero!");
            }

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    Balance currentUserBalance = new Balance()
                    {
                        szAccountId = accountId,
                        decAmount = balance.decAmount,
                        szCurrencyId = balance.szCurrencyId
                    };
                    decimal currentUserAmount = currentUserBalance.AccountAmount(connection, transaction);
                    if (currentUserAmount <= 0 || balance.decAmount * balance.szAccountId.Length > currentUserAmount)
                    {
                        return StatusCode(422, "Amount and/or current balance must be above zero!");
                    }

                    try
                    {
                        int counter = Counter.GetCounter(connection, transaction);
                        DateTime now = DateTime.Now;
                        string transactionId = this.GenerateTransactionId(counter);

                        History historyCurrentUser = new History()
                        {
                            szTransactionId = transactionId,
                            szAccountId = accountId,
                            szCurrencyId = balance.szCurrencyId,
                            dtmTransaction = now,
                            decAmount = balance.decAmount * balance.szAccountId.Length  * -1,
                            szNote = "TRANSFER"

                        };
                        historyCurrentUser.CreateHistory(connection, transaction);

                        Balance updateCurrentBalance = new Balance()
                        {
                            szAccountId = accountId,
                            szCurrencyId = balance.szCurrencyId,
                            decAmount = balance.decAmount * balance.szAccountId.Length
                        };
                        updateCurrentBalance.UpdateBalance(connection, transaction, false);

                        foreach(string targetAccountId in balance.szAccountId)
                        {
                            History historyTargetUser = new History()
                            {
                                szTransactionId = transactionId,
                                szAccountId = targetAccountId,
                                szCurrencyId = balance.szCurrencyId,
                                dtmTransaction = now,
                                decAmount = balance.decAmount,
                                szNote = "TRANSFER"

                            };
                            historyTargetUser.CreateHistory(connection, transaction);

                            Balance updateTargetBalance = new Balance()
                            {
                                szAccountId = targetAccountId,
                                szCurrencyId = balance.szCurrencyId,
                                decAmount = balance.decAmount
                            };
                            updateTargetBalance.UpdateBalance(connection, transaction, true);
                        }

                        Counter.UpdateCounter(connection, transaction, ++counter);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, ex.Message);
                    }
                }
                connection.Close();
                return Ok("Success");
            }
        }

        private string GenerateTransactionId(int counter)
        {
            DateTime now = DateTime.Now;
            string todayString = now.ToString("yyyymmdd");
            return todayString.ToString() + "-00000." + counter.ToString("D5");
        }
    }
}
