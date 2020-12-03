using System;

namespace app.DTOs
{
    public class FormatAInstance
    {
        public Guid Id { get; set; }
        public string N { get; set; }
        public int F { get; set; }
        public int S { get; set; }
        public int T { get; set; }

        public FormatAInstance()
        {
        }

        public FormatAInstance(InputFormat input)
        {
            this.Id = input.Id;
        }
    }
}
