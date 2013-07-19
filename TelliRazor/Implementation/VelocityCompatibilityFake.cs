using System;
using System.Dynamic;

namespace TelliRazor
{
    /// <summary>
    /// Fake object to return to try and maintain compatibility with velocity.
    /// Returns itself for all properties & sub methods.
    /// </summary>
    public class VelocityCompatibilityWrapper : DynamicObject, IComparable, IConvertible
    {
        private VelocityCompatibilityWrapper()
        {
        }
        public static dynamic NullInstance = new VelocityCompatibilityWrapper();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this;
            return true;
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = this;
            return true;
        }
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            result = this.ToType(binder.ReturnType, null);

            return true;
        }
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            result = this;
            return true;
        }

        public override string ToString()
        {
            return "";
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public int CompareTo(object obj)
        {
            return -1;
        }

        public static implicit operator bool(VelocityCompatibilityWrapper value)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return false;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return default(byte);
        }

        public char ToChar(IFormatProvider provider)
        {
            return default(char);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return DateTime.MinValue;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return default(decimal);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return default(double);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return default(short);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return default(int);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return default(long);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return default(sbyte);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return default(float);
        }

        public string ToString(IFormatProvider provider)
        {
            return String.Empty;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(Boolean))
                return false;
            else if (conversionType.IsValueType)
                return Activator.CreateInstance(conversionType);
            else
                return this;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return default(ushort);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return default(ushort);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return default(ulong);
        }
    }
}
