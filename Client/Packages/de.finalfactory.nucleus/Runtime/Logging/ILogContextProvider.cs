using System.Diagnostics;

namespace FinalFactory.Logging
{
    public interface ILogContextProvider
    {
        /// <summary>
        /// Determines the context of the log message.
        /// </summary>
        /// <param name="stackTrace">StackTrace is only set if FINALLOG_STACKTRACE is defined otherwise it is null.</param>
        /// <returns></returns>
        string DetermineContext(StackTrace stackTrace);
    }
}