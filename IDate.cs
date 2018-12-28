using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.SqlClr
{
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Microsoft.SqlServer.Server.Format.Native, IsByteOrdered=true, IsFixedLength=true)]
    public struct IDate: INullable
    {
        private const           int                     NullValue = unchecked((int)0xFFFF8000);
        private static readonly DateTime                _epoch    = new DateTime(1970, 1, 1);
        private                 byte                    _b1;
        private                 byte                    _b2;

        private                 int                     _value
        {
            get {
                return (int)(short)((_b1<<8) | _b2);
            }
        }

        public                  SqlInt32                Int
        {
            get {
                var v = _value;
                return v == NullValue ? SqlInt32.Null : new SqlInt32(v);
            }
        }
        public                  SqlInt32                Year
        {
            get {
                var v = _value;
                return v == NullValue ? SqlInt32.Null : new SqlInt32(_epoch.AddTicks(v * TimeSpan.TicksPerDay).Year);
            }
        }
        public                  SqlInt32                Month
        {
            get {
                var v = _value;
                return v == NullValue ? SqlInt32.Null : new SqlInt32(_epoch.AddTicks(v * TimeSpan.TicksPerDay).Month);
            }
        }
        public                  SqlInt32                Day
        {
            get {
                var v = _value;
                return v == NullValue ? SqlInt32.Null : new SqlInt32(_epoch.AddTicks(v * TimeSpan.TicksPerDay).Day);
            }
        }
        public                  SqlDateTime             DateTime
        {
            get {
                var v = _value;
                return v == NullValue ? SqlDateTime.Null : new SqlDateTime(_epoch.AddTicks(v * TimeSpan.TicksPerDay));
            }
        }
        public                  bool                    IsNull
        {
            get {
                return _value == NullValue;
            }
        }

        private                                         IDate(int value)
        {
            if (value < -32768 || value > 32768)
                throw new ArgumentException("idate out of range");

            _b1 = (byte)(value >> 8);
            _b2 = (byte)(value);
        }

        public      static      IDate                   Null
        {
            get {
                return new IDate()
                            {
                                _b1 = (byte)((NullValue >> 8) & 0xff),
                                _b2 = (byte)((NullValue     ) & 0xff)
                            };
            }
        }
        public      static      IDate                   Value(SqlInt32 value)
        {
            return value.IsNull ? Null : new IDate(value.Value);
        }
        public      static      IDate                   Parse(SqlString value)
        {
                if (value.IsNull || string.IsNullOrEmpty(value.Value))
                    return Null;

                return  parse(value.Value);
        }
        public      static      IDate                   Date(Int32 year, Int32 month, Int32 day)
        {
            if (year  < 1900 || year  > 2059)   throw new ArgumentException("IDate year out of range.");
            if (month <    1 || month >   12)   throw new ArgumentException("IDate month out of range.");
            if (day   <    1 || day   >   31)   throw new ArgumentException("IDate day out of range.");

            return new IDate((int)(((new DateTime(year, month, day)) - _epoch).Ticks / TimeSpan.TicksPerDay));
        }
        public                  IDate                   AddDays(SqlInt32 n)
        {
            var v   = _value;
            return (n.IsNull || v == NullValue) ? Null : new IDate(v + n.Value);
        }

        internal    static      IDate                   parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Null;

            if (value.Length == 10 &&
                value[0] >= '0' && value[0] <= '9' &&
                value[1] >= '0' && value[1] <= '9' &&
                value[2] >= '0' && value[2] <= '9' &&
                value[3] >= '0' && value[3] <= '9' &&
                value[4] == '-'                    &&
                value[5] >= '0' && value[5] <= '9' &&
                value[6] >= '0' && value[6] <= '9' &&
                value[7] == '-'                    &&
                value[8] >= '0' && value[8] <= '9' &&
                value[9] >= '0' && value[9] <= '9')
                return Date((value[0] - '0') * 1000 + (value[1] - '0') *  100 + (value[2] - '0') *   10 + (value[3] - '0'),
                            (value[5] - '0') * 10 + (value[6] - '0'),
                            (value[8] - '0') * 10 + (value[9] - '0'));

            if (int.TryParse(value, out var i))
                return new IDate(i);

            throw new FormatException("Invalid date string.");
        }

        public      override    string                  ToString()
        {
            var v = _value;
            return (v == NullValue) ? "" : _epoch.AddTicks(v * TimeSpan.TicksPerDay).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
