namespace app.DTOs
{
    public class BatchCommand
    {
        public int BatchSize { get; set; }
        public InputFormat[] Lines { get; set; }
    }
}