using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Transaction.Requests
{
    public class TransferRequest
    {
        public required string[] szAccountId { get; set; }
        public required string szCurrencyId { get; set; }
        public required decimal decAmount { get; set; }
    }
}
