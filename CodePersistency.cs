using System.Collections.Generic;
using System.Linq;

namespace tst_otp
{
    public interface ICodePersistency
    {
        public long GetPersistency(string code, string seed);
        public void AddPersistency(string code, string seed, long timeStepMatched);
    }

    public class CodePersistency : ICodePersistency
    {
        private readonly List<(string pCode, string pSeed, long pTimeStepMatched)> _persistedValues =
            new();

        public long GetPersistency(string code, string seed)
        {
            var persistedValue = _persistedValues.FirstOrDefault(x => x.pCode == code && x.pSeed == seed);
            return persistedValue.pTimeStepMatched;
        }

        public void AddPersistency(string code, string seed, long timeStepMatched)
        {
            var persistedValue = GetPersistency(code, seed);
            if (persistedValue == 0)
                _persistedValues.Add((code, seed, timeStepMatched));
        }
    }
}
