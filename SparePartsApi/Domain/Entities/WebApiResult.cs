namespace Domain
{
    public class WebApiResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public object Result { get; set; }
    }
}