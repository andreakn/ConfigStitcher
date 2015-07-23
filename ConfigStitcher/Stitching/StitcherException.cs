using System;

namespace ConfigStitcher.Stitching
{
    public class StitcherException : Exception
    {
        public StitcherException(string message)
            : base(message)
        {
        }
        public StitcherException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}