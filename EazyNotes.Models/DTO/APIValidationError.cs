using System;
using System.Collections.Generic;
using System.Text;

namespace EazyNotes.Models.DTO
{
    public class APIValidationError
    {
        public string Model { get; set; }
        public string Property { get; set; }
        public string Error { get; set; }

        public APIValidationError() { /* empty ctor */ }
        public APIValidationError(string model, string prop, string error)
        {
            Model = model;
            Property = prop;
            Error = error;
        }
    }
}
