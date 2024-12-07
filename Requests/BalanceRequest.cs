using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Transaction.Requests
{
    public class BalanceRequest
    {
        public required string szCurrencyId { get; set; }
        public required decimal decAmount { get; set; }
    }
}
