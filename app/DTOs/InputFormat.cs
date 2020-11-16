using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace app.DTOs
{
    public class InputFormat
    {
        public Guid Id { get; }
        public string StringValue { get; }

        public InputFormat(string stringValue)
        {
            this.Id = Guid.NewGuid();
            this.StringValue = stringValue;
        }
    }
}
