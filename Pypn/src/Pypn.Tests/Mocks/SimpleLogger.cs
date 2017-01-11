using System;
using System.Text;

namespace Pypn.Tests.Mocks
{
    public class SimpleLogger
    {
        private readonly StringBuilder _messageHistory;

        public SimpleLogger(StringBuilder messageHistory = null)
        {
            _messageHistory = messageHistory ?? new StringBuilder();
        }

        public void Debug(string message)
        {
            _messageHistory.AppendFormat("Debug: {0}", message);
            _messageHistory.AppendLine();
        }

        public void Warning(string message)
        {
            _messageHistory.AppendFormat("Warning: {0}", message);
            _messageHistory.AppendLine();
        }

        public void Error(string message)
        {
            _messageHistory.AppendFormat("Error: {0}", message);
            _messageHistory.AppendLine();
        }

        public void Error(Exception ex)
        {
            _messageHistory.AppendFormat("Error: {0}", ex.ToString());
            _messageHistory.AppendLine();
        }
    }
}