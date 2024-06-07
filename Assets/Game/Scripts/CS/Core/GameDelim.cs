using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    class GameDelim
    {
        public const string BASE = "':'";
        public const string SUB = "','";
        public const string NAME = "'~'";
        public const string VISUAL = "'newv'";
        public const string SUBVISUAL = "'subV'";
        public const string POINT = "'newp'";
        public const string CONT = "'c'";
        public const string ITEM = "'item'";
        public const string RATING = "'r'";

        public static string stripGameDelims(string inString)
        {
            inString = inString.Replace(BASE, ":");
            inString = inString.Replace(SUB, ",");
            inString = inString.Replace(NAME, "~");
            inString = inString.Replace(VISUAL, "newv");
            inString = inString.Replace(SUBVISUAL, "subV");
            inString = inString.Replace(POINT, "newp");
            inString = inString.Replace(ITEM, "item");
            inString = inString.Replace(RATING, "r");
            inString = inString.Replace(CONT, "c");

            return inString;
        }
    }
}
