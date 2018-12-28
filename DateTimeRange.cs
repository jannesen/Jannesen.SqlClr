using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.SqlClr
{
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Microsoft.SqlServer.Server.Format.Native, IsByteOrdered=true, IsFixedLength=true)]
    public struct DateTimeRange: INullable
    {
        private                 OrderedDateTime         _begin;
        private                 OrderedDateTime         _end;

        public                  SqlDateTime             Begin
        {
            get {
                return _begin.SqlDateTime;
            }
        }
        public                  SqlDateTime             End
        {
            get {
                return _end.SqlDateTime;
            }
        }
        public                  bool                    IsNull
        {
            get {
                return _begin.IsNull && _end.IsNull;
            }
        }

        private                                         DateTimeRange(OrderedDateTime begin, OrderedDateTime end)
        {
            _begin = begin;
            _end   = end;
        }

        public      static      DateTimeRange           Null
        {
            get {
                return new DateTimeRange(OrderedDateTime.Null, OrderedDateTime.Null);
            }
        }
        public      static      DateTimeRange           Parse(SqlString value)
        {
            if (value.IsNull || string.IsNullOrEmpty(value.Value))
                return Null;

            var parts = value.Value.Split('~', ',');

            if (parts.Length != 2)
                throw new FormatException("Invalid datetimerange string.");

            return new DateTimeRange(OrderedDateTime.Parse(parts[0]), OrderedDateTime.Parse(parts[1]));
        }
        public      static      DateTimeRange           Range(SqlDateTime begin, SqlDateTime end)
        {
            return new DateTimeRange(begin.IsNull ? OrderedDateTime.Null : new OrderedDateTime(begin.Value),
                                      end.IsNull   ? OrderedDateTime.Null : new OrderedDateTime(end.Value));
        }

        public      override    string                  ToString()
        {
            if (IsNull)
                return "";

            return _begin.ToString() + "~" + _end.ToString();
        }
    }
}
