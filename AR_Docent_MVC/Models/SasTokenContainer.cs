using System.Collections.Generic;

namespace AR_Docent_MVC.Models
{
    public class SasTokenContainer
    {
        private Dictionary<string, string> _tokens;

        public SasTokenContainer(){ _tokens = new Dictionary<string, string>(); }
        public SasTokenContainer(Dictionary<string, string> tokens) { _tokens = tokens; }
        ~SasTokenContainer()
        {
            _tokens.Clear();
            _tokens = null;
        }

        public string this[string index]
        {
            get { return _tokens[index]; }
            set
            {
                if (_tokens.ContainsKey(index))
                    _tokens[index] = value;
                else
                    _tokens.Add(index, value);
            }
        }
    }
}
