using System;
namespace com.mobility.packer.Exceptions
{
    [Serializable]
    public class APIException : Exception
    {
        public string FileName { get; }

        public APIException() { }

        public APIException(string message) : base(message) { }

        public APIException(string message, Exception inner) : base(message, inner) { }

        public APIException(string message, string fileName) : this(message)
        {
            FileName = fileName;
        }
    }
}