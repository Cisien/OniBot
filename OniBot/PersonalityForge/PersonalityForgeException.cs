using System;

namespace JamesWright.PersonalityForge
{
    public class PersonalityForgeException : Exception
    {
        public PersonalityForgeException() { }
        public PersonalityForgeException(string message) : base(message) { }
        public PersonalityForgeException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
