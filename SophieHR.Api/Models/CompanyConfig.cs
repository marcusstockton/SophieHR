namespace SophieHR.Api.Models
{
    public class CompanyConfig : Base
    {
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public int GdprRetentionPeriodInYears { get; set; }
        // DDMM format
        private int _yearEnd;
        public int YearEnd
        {
            get => _yearEnd;
            set
            {
                int day = value / 100; // Extract the day (first two digits)
                int month = value % 100; // Extract the month (last two digits)

                if (day < 1 || day > 31 || month < 1 || month > 12)
                {
                    throw new ArgumentOutOfRangeException(nameof(YearEnd), "Year End must be in DDMM format with valid day and month.");
                }

                _yearEnd = value;
            }
        }
    }
}