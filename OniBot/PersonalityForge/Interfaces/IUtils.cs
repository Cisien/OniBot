using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamesWright.PersonalityForge.Interfaces
{
    interface IUtils
    {
        string GenerateSecret(string secret, string data);
        int GenerateTimestamp();
        string FilterJson(byte[] response);
    }
}
