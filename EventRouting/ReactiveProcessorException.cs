using System;
namespace EventRouting
{
    public class ReactiveProcessorException : Exception
    {
        public new Exception InnerException { get; private set; }
        public int CausedThreadId { get; private set; }
        public object Parameter { get; private set; }

        public ReactiveProcessorException(Exception orignal, int threadId, object param)
        {
            InnerException = orignal;
            CausedThreadId = threadId;
            Parameter = param;
        }
    }
}
