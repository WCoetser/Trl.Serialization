using System;

namespace Trl.Serialization.Tests.TestObjects
{
    public static class DateTimeExtensions
    {
        public static void Deconstruct(this DateTime dateTime, out int year)
        {
            year = dateTime.Year;
        }

        public static void Deconstruct(this DateTime dateTime, out int year, out int month)
        {
            year = dateTime.Year;
            month = dateTime.Month;
        }

        public static void Deconstruct(this DateTime dateTime, out int year, out int month, out int day)
        {
            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
        }
    }
}
