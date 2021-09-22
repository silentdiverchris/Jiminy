using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    public class ResultMessage
    {
        public ResultMessage(string text, enSeverity severity = enSeverity.Info, Exception? ex = null, string? functionName = null, bool alwaysWriteToEventLog = false)
        {
            CreatedUtc = DateTime.UtcNow;
            Text = text;
            Severity = severity;
            Exception = ex;
            FunctionName = functionName;
            AlwaysWriteToEventLog = alwaysWriteToEventLog;
        }

        public DateTime CreatedUtc { get; private set; }
        public enSeverity Severity { get; set; }
        public string Text { get; set; }
        public Exception? Exception { get; set; }
        public string? FunctionName { get; set; }
        public bool HasBeenWritten { get; set; }
        public bool AlwaysWriteToEventLog { get; set; }
    }

    public class Result
    {
        public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;

        public List<ResultMessage> Messages { get; set; } = new();
        public string? FunctionName { get; private set; }

        public List<ResultMessage> UnprocessedMessages => Messages.Where(_ => _.HasBeenWritten == false).ToList();

        public int? ReturnedInt { get; set; }
        public string? ReturnedString { get; set; }

        public int ItemsFound { get; set; }
        public int ItemsProcessed { get; set; }
        public long BytesProcessed { get; set; }

        public enSeverity HighestSeverity => Messages.Any() ? Messages.OrderBy(_ => _.Severity).First().Severity : enSeverity.Debug;

        public bool HasErrors => Messages.Any(_ => _.Severity == enSeverity.Error);
        public bool HasWarnings => Messages.Any(_ => _.Severity == enSeverity.Warning);

        public Result()
        {

        }

        public Result(string? functionName, bool addStartingItem = false, string? appendText = null)
        {
            FunctionName = functionName;

            if (addStartingItem && functionName is not null)
                AddInfo($"Running {functionName} {appendText}");
        }

        public void MarkMessagesWritten()
        {
            foreach (var msg in UnprocessedMessages)
            {
                msg.HasBeenWritten = true;
            }
        }

        public void AddError(string text)
        {
            Messages.Add(new ResultMessage(text, severity: enSeverity.Error, functionName: FunctionName));
        }

        public void AddWarning(string text)
        {
            Messages.Add(new ResultMessage(text, severity: enSeverity.Warning, functionName: FunctionName));
        }

        public void AddInfo(string text)
        {
            Messages.Add(new ResultMessage(text, severity: enSeverity.Info, functionName: FunctionName));
        }

        public void AddDebug(string text)
        {
            Messages.Add(new ResultMessage(text, severity: enSeverity.Debug, functionName: FunctionName));
        }

        public void AddSuccess(string text)
        {
            Messages.Add(new ResultMessage(text, severity: enSeverity.Success, functionName: FunctionName));
        }

        public void AddException(Exception ex)
        {
            Messages.Add(new ResultMessage(ex.Message, severity: enSeverity.Error, ex: ex, functionName: FunctionName));
        }

        public bool HasNoErrors
        {
            get
            {
                return !HasErrors;
            }
        }

        public bool HasNoErrorsOrWarnings
        {
            get
            {
                return !HasErrors & !HasWarnings;
            }
        }

        public bool HasErrorsOrWarnings
        {
            get
            {
                return HasErrors || HasWarnings;
            }
        }

        /// <summary>
        /// Copies the messages and optionally, item counts from the result into the caller
        /// </summary>
        /// <param name="result"></param>
        /// <param name="AddItemCounts">Whether to add the ItemsFound, ItemsProcessed and BytesProcessed to the ones in the caller</param>
        public void SubsumeResult(Result result, bool AddItemCounts = true)
        {
            if (result == this)
            {
                throw new Exception("Cannot subsume a result into itself, that would be very silly");
            }

            foreach (var message in result.Messages)
            {
                Messages.Add(message);
            }

            if (AddItemCounts)
            {
                ItemsFound += result.ItemsFound;
                ItemsProcessed += result.ItemsProcessed;
                BytesProcessed += result.BytesProcessed;
            }
        }

        public void ClearItemCounts()
        {
            ItemsFound += 0;
            ItemsProcessed += 0;
            BytesProcessed += 0;
        }

        public string TextSummary
        {
            get
            {
                StringBuilder sb = new(300);

                foreach (var message in Messages)
                {
                    sb.AppendLine($"{message.Severity}: {message.Text}");
                }

                return sb.ToString();
            }
        }
    }
}
