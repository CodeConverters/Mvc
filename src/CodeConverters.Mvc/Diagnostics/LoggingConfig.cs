namespace CodeConverters.Mvc.Diagnostics
{
    public static class LoggingConfig
    {
        public static string[] DefaultScrubParams =
        {
            "password", "password_confirmation", "confirm_password",
            "secret", "secret_token",
            "creditcard", "credit_card", "credit_card_number", "card_number", "ccnum", "cc_number"
        };
    }
}