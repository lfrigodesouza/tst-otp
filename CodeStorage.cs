using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tst_otp
{
    public interface ICodeStorage
    {
        void AddCode(string seed, string code, DateTime dateGenerated);
        DateTime? GetGeneratedTime(string seed, string code);
    }

    public class CodeStorage : ICodeStorage
    {
        private List<(string pSeed, string pCode, DateTime? pDateGenerated )> _storedCodes = new ();

        public void AddCode(string seed, string code, DateTime dateGenerated)
        {
            var tst = GetGeneratedTime(seed, code);
            if(GetGeneratedTime(seed, code) is null)
                _storedCodes.Add((seed, code, dateGenerated));
        }

        public DateTime? GetGeneratedTime(string seed, string code)
        {
            return _storedCodes.FirstOrDefault(x => x.pCode == code && x.pSeed == seed).pDateGenerated;
        }
    }
}
