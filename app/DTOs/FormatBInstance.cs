using System;
using Dynamitey;

namespace app.DTOs
{
    public class FormatBInstance
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int First { get; set; }
        public int Second { get; set; }
        public int Third { get; set; }
        public int AdditionalField { get; set; }

        public FormatBInstance()
        {
        }

        public FormatBInstance(FormatAInstance input)
        {
            this.Id = input.Id;
            this.Name = input.Name;
            this.First = input.First;
            this.Second = input.Second;
            this.Third = input.Third;
            this.AdditionalField = input.First * input.Second * input.Third;
        }

        public override string ToString()
        {
            return
                $"ID: {this.Id} - First: {this.First}, Second: {this.Second}, Third: {this.Third} - Additional field: {this.AdditionalField}";
        }
    }
}
