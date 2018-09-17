using System;
namespace EventRouting
{
    public static class EnumUtil
    {
        public static int ParseInt(this Enum namedEnumValue)
        {
            string name = Enum.GetName(namedEnumValue.GetType(), namedEnumValue);
            int value = (int)Enum.Parse(namedEnumValue.GetType(), name);
            return value;
        }
    }
}
