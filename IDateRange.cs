using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.SqlClr
{
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Microsoft.SqlServer.Server.Format.Native, IsByteOrdered=true, IsFixedLength=true)]
    public struct IDateRange: INullable
    {
        private                 IDate                   _begin;
        private                 IDate                   _end;

        public                  IDate                   Begin
        {
            get {
                return _begin;
            }
        }
        public                  IDate                   End
        {
            get {
                return _end;
            }
        }
        public                  bool                    IsNull
        {
            get {
                return _begin.IsNull && _end.IsNull;
            }
        }

        private                                         IDateRange(IDate begin, IDate end)
        {
            _begin = begin;
            _end   = end;
        }

        public      static      IDateRange              Null
        {
            get {
                return new IDateRange(IDate.Null, IDate.Null);
            }
        }
        public      static      IDateRange              Parse(SqlString value)
        {
            if (value.IsNull || string.IsNullOrEmpty(value.Value))
                return Null;

            var parts = value.Value.Split('~', ',');

            if (parts.Length != 2)
                throw new FormatException("Invalid daterange string.");

            return new IDateRange(IDate.parse(parts[0]), IDate.parse(parts[1]));
        }
        public      static      IDateRange              Range(IDate begin, IDate end)
        {
            return new IDateRange(begin, end);
        }

        public      override    string                  ToString()
        {
            if (IsNull)
                return "";

            return _begin.ToString() + "," + _end.ToString();
        }
    }
}
