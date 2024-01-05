using System;
using System.Collections.Generic;
using System.Text;

namespace EazyNotes.Models.DTO
{
    public enum APIResponseStatus
    {
        Success,
        ValidationError,
        InternalServerError
    }

    public class APIResponse
    {
        public bool Success { get; set; }
        public string JsonData { get; set; }
        public string Message { get; set; }

        public APIResponse() { /* empty ctor */ }
        public APIResponse(bool success, string json, string msg)
        {
            Success = success;
            JsonData = json;
            Message = msg;
        }
    }
}
