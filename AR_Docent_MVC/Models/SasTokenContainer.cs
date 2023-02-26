using System.Collections.Generic;

namespace AR_Docent_MVC.Models
{
    public class SasTokenContainer
    {
        private Dictionary<int, string> _tokens;

        public SasTokenContainer(){ _tokens = new Dictionary<int, string>(); }
        public SasTokenContainer(Dictionary<int, string> tokens) { _tokens = tokens; }
        ~SasTokenContainer()
        {
            _tokens.Clear();
            _tokens = null;
        }

        public string this[int index]
        {
            get
            {
                if (_tokens.ContainsKey(index))
                    return _tokens[index];
                return null;
            }
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
