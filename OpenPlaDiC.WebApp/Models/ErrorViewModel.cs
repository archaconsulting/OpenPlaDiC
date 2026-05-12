using System;

namespace OpenPlaDiC.WebApp.Models;

public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public string Path { get; set; }
    }