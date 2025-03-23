namespace BankingApp.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }

        public static Result Success() => new Result { IsSuccess = true };
        public static Result Fail(string error) => new Result { IsSuccess = false, Error = error };
    }
}
