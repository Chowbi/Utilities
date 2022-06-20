using System;

namespace Chowbi_Utilities.Helpers
{
    public static class DateHelpers
    {
        public static int? GetDateId(DateTime? dateTime) => dateTime == null ? null : dateTime?.Year * 10000 + dateTime?.Month * 100 + dateTime?.Day;
    }
}
