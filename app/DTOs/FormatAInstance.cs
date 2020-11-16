using System;

namespace app.DTOs
{
    public class FormatAInstance
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public int First { get; set; }
        public int Second { get; set; }
        public int Third { get; set; }

        public FormatAInstance(InputFormat input)
        {
            this.Id = input.Id;
        }
    }
}
