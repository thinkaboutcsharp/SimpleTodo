using System;
namespace SimpleTodo
{
    public class DataAccessException : Exception
    {
        public new Exception InnerException { get; private set; }
        public int CausedThreadId { get; private set; }

        public DataAccessException(Exception original, int threadId)
        {
            InnerException = original;
            CausedThreadId = threadId;
        }
    }
}
