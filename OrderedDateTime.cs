using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.SqlClr
{
    [Serializable]
    internal struct OrderedDateTime
    {
        private         byte                _b1;
        private         byte                _b2;
        private         byte                _b3;
        private         byte                _b4;
        private         byte                _b5;
        private         byte                _b6;
        private         byte                _b7;
        private         byte                _b8;

        public          DateTime            Value
        {
            get {
                return new DateTime(((Int64)(((UInt32)_b1 << 24) |
                                             ((UInt32)_b2 << 16) |
                                             ((UInt32)_b3 <<  8) |
                                             ((UInt32)_b4      )) << 32) |
                                    ((Int64)(((UInt32)_b5 << 24) |
                                             ((UInt32)_b6 << 16) |
                                             ((UInt32)_b7 <<  8) |
                                             ((UInt32)_b8      ))));
            }
        }
        public          SqlDateTime         SqlDateTime
        {
            get {
                return IsNull ? SqlDateTime.Null : new SqlDateTime(Value);
            }
        }
        public          bool                IsNull
        {
            get {
                return _b1 == 0x80 &&
                       _b2 == 0    &&
                       _b3 == 0    &&
                       _b4 == 0    &&
                       _b5 == 0    &&
                       _b6 == 0    &&
                       _b7 == 0    &&
                       _b8 == 0;
            }
        }

        public                              OrderedDateTime(DateTime value)
        {
            var h = (Int32)(value.Ticks >> 32);
            var l = (Int32)(value.Ticks      );
            _b1 = (byte)(h >> 24);
            _b2 = (byte)(h >> 16);
            _b3 = (byte)(h >>  8);
            _b4 = (byte)(h      );
            _b5 = (byte)(l >> 24);
            _b6 = (byte)(l >> 16);
            _b7 = (byte)(l >>  8);
            _b8 = (byte)(l      );
        }

        public  static  OrderedDateTime     Null
        {
            get {
                return new OrderedDateTime()
                            {
                                _b1 = 0x80, _b2 = 0, _b3 = 0, _b4 = 0,
                                _b5 = 0,    _b6 = 0, _b7 = 0, _b8 = 0
                            };
            }
        }
        public  static  OrderedDateTime     Parse(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return Null;

            int     fieldpos = 0;
            int[]   fields   = new int[7];
            int     factor   = 0;

            for (int pos = 0 ; pos<sValue.Length ; ++pos) {
                char    chr = sValue[pos];

                if (chr>='0' && chr <='9') {
                    if (fieldpos<6)
                        fields[fieldpos] = fields[fieldpos]*10 + (chr-'0');
                    else {
                        fields[fieldpos] += (chr-'0')*factor;
                        factor /= 10;
                    }
                }
                else {
                    switch(fieldpos) {
                    case 0: // year
                    case 1: // month
                        if (chr!='-') goto invalid_date;
                        break;

                    case 2: // day
                        if (chr!='T') goto invalid_date;
                        break;

                    case 3: // hour
                    case 4: // minute
                        if (chr!=':') goto invalid_date;
                        break;

                    case 5: // second
                        if (chr!='.') goto invalid_date;
                        factor = 100;
                        break;

                    default:
invalid_date:               throw new System.FormatException("Invalid date format.");
                    }

                    ++fieldpos;
                }
            }

            if (fields[1]<1 || fields[1]>12 ||
                fields[2]<1 || fields[2]>31 ||
                fields[3]>23 ||
                fields[4]>59 ||
                fields[5]>59)
                throw new FormatException("Invalid date format.");

            if (fields[0]<1900 || fields[0]>2999)
                throw new System.FormatException("datetime out of range.");

            return new OrderedDateTime(new DateTime(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6]));
        }

        public override string              ToString()
        {
            return IsNull ? "" : Value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
