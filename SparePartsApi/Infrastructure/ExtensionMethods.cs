using System;
using System.Globalization;
using System.Linq;

namespace Infrastructure
{
    public static class ExtensionMethods
    {
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotNull(this object source)
        {
            return source != null;
        }

        /// <summary>
        /// Formata a string com .PadLeft(18, '0').
        /// </summary>
        public static string ToFormatCode(this string source)
        {
            if (source.Any(x => char.IsLetter(x)))
                return source;
            else
                return source.PadLeft(18, '0');
        }

        /// <summary>
        /// Formata double em string pt-BR.
        /// </summary>
        public static string ToFormatString(this double source)
        {
            return string.Format(new CultureInfo("pt-BR"), "{0}", source);
        }

        /// <summary>
        /// Pega a última mensagem de exceção.
        /// </summary>
        public static string GetInnerExceptionMessage(this Exception ex)
        {
            string msg = string.Empty;

            if (ex.InnerException != null)
            {
                msg = ex.InnerException.GetInnerExceptionMessage();
            }
            else
                return ex.Message;

            return msg;
        }

        /// <summary>
        /// Formata a string em ####
        /// </summary>
        public static string FormatarNumeroItem(this string source)
        {
            return source.TrimStart('0').PadLeft(4, '0');
        }

        /// <summary>
        /// Formata a string em ##########
        /// </summary>
        public static string FormatarNumeroLista(this string source)
        {
            return source.TrimStart('0').PadLeft(10, '0');
        }
    }
}
