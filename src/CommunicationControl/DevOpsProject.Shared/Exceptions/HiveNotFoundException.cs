namespace DevOpsProject.Shared.Exceptions
{
    public class HiveNotFoundException : Exception
    {
        public HiveNotFoundException()
        {
        }

        public HiveNotFoundException(string message)
            : base(message)
        {
        }

        public HiveNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
