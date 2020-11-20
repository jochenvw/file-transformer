using System;

namespace app.DTOs
{
    public class InputFormat
    {
        public Guid Id { get; set; }
        public string StringValue { get; set; }

        public InputFormat()
        {
        }

        public InputFormat(string stringValue)
        {
            this.Id = Guid.NewGuid();
            this.StringValue = stringValue;
        }
    }
}
