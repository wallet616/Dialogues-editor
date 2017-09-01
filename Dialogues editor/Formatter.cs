using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Dialogues_editor
{
    public static class Formatter
    {
        public static string get_formatted_or_null_id(string id)
        {
            // If null return null.
            if (id == null) return null;

            // If empty return null.
            int counter = Regex.Replace(id, "[\\s]+", "").Length;
            if (counter == 0) return null;


            // Else, return formated output.
            string output = id.ToUpper();
            output = Regex.Replace(output, "[\\s]+", "_");
            output = Regex.Replace(output, "[_]+", "_");

            return output;
        }


        public static string get_formatted_or_null_text(string message)
        {
            // If null return null.
            if (message == null) return null;

            // If empty return null.
            int counter = Regex.Replace(message, "[\\s]+", "").Length;
            if (counter == 0) return null;

            // Else, return formated output.
            string output = null;
            output = Regex.Replace(message, "[\\n]+", "↨NEW_LINE↨"); // ↨ = alt + 23
            output = Regex.Replace(output, "[\\s]+", " ");
            output = Regex.Replace(output, "↨NEW_LINE↨", "\n");

            counter = 0;
            while (output.Length != counter)
            {
                counter = output.Length;
                output = Regex.Replace(output, "( \n)+|(\n )+", "\n");
            }

            if (output[0].Equals(' ') || output[0].Equals('\n'))
                output = output.Substring(1);
            if (output[output.Length - 1].Equals('\n') || output[output.Length - 1].Equals(' '))
                output = output.Substring(0, output.Length - 1);


            //ada [aaa] das, dfaa [bb]! 
            MatchCollection matching_ids = Regex.Matches(output, "\\[(.*?)\\]");
            foreach (Match m in matching_ids)
            {
                //Console.WriteLine(m.Groups[1]);
                string to_replace = m.Groups[1].ToString();
                output = Regex.Replace(output, to_replace, get_formatted_or_null_id(to_replace));
            }

            return output;
        }
    }
}
