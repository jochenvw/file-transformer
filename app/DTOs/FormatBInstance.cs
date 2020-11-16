using System;
using Dynamitey;

namespace app.DTOs
{
    public class FormatBInstance
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int First { get; private set; }
        public int Second { get; private set; }
        public int Third { get; private set; }
        public int AdditionalField { get; private set; }

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
