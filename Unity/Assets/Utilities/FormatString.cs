#nullable enable

using JetBrains.Annotations;

namespace NeuroSdk.Utilities
{
    [PublicAPI]
    public sealed class FormatString
    {
        private readonly string _str;

        public FormatString(string str)
        {
            _str = str;
        }

        public string Format(params object[] args)
        {
            return string.Format(_str, args);
        }

        public static implicit operator FormatString(string str)
        {
            return new FormatString(str);
        }
    }
}
