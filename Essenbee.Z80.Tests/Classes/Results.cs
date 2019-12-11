using System.Collections.Generic;

namespace Essenbee.Z80.Tests.Classes
{
    public class Results
    {
        public List<string> Passing { get; } = new List<string>();
        public Dictionary<string, List<string>> Failing { get; } = new Dictionary<string, List<string>>();
        public List<string> NotImplemented { get; } = new List<string>();

        public Results(List<string> passing, Dictionary<string, List<string>> failing, List<string> missing)
        {
            Passing = passing;
            Failing = failing;
            NotImplemented = missing;
        }
    }
}
