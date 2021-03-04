using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpNet;

namespace tst_otp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MultifactorController : ControllerBase
    {
        private const int EXPIRATION_TIME_IN_SECONDS = 30;
        private readonly ICodePersistency _codePersistency;
        private readonly ICodeStorage _codeStorage;

        public MultifactorController(ICodePersistency codePersistency, ICodeStorage codeStorage)
        {
            _codePersistency = codePersistency;
            _codeStorage = codeStorage;
        }

        private static Totp GetTotp(string seed)
        {
            return new(Encoding.UTF8.GetBytes(seed), EXPIRATION_TIME_IN_SECONDS);
        }

        [HttpGet]
        public ActionResult GetOtp()
        {
            var seed = Guid.NewGuid().ToString();
            var totp = GetTotp(seed);

            var dateGenerated = DateTime.Now;
            var code = totp.ComputeTotp(dateGenerated);
            _codeStorage.AddCode(seed, code, dateGenerated);
            var remainingSeconds = totp.RemainingSeconds();

            return new OkObjectResult(
                new
                {
                    seed,
                    code,
                    remainingSeconds
                });
        }

        [HttpPut]
        public ActionResult GetOtp([FromForm] string seed)
        {
            var totp = GetTotp(seed);

            var code = totp.ComputeTotp();
            var remainingSeconds = totp.RemainingSeconds();

            return new OkObjectResult(
                new
                {
                    seed,
                    code,
                    remainingSeconds
                });
        }

        [HttpPost]
        public ActionResult ValidateOtp([FromForm] string seed, [FromForm] string code)
        {
            var totp = GetTotp(seed);

            var persistedTimeStepMatched = _codePersistency.GetPersistency(code, seed);
            if (persistedTimeStepMatched > 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "Código inválido");


            var generatedTime = _codeStorage.GetGeneratedTime(seed, code.ToString());

            var window = new VerificationWindow(0, 0);
            if (totp.VerifyTotp((DateTime)generatedTime,code.ToString(), out var timeStepMatched, window))
            {
                _codePersistency.AddPersistency(code, seed, timeStepMatched);
                return Ok("Código validado com sucesso");
            }

            return StatusCode(StatusCodes.Status400BadRequest, "Código inválido");
        }
    }
}
