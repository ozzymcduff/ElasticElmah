using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Presentation
{
    public struct Token
    {
        public Symbols Type;
        public string Value;
        public int Position;
        public Token(Symbols type, string value, int position)
        {
            this.Type = type;
            this.Value = value;
            this.Position = position;
        }
        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + this.Value.GetHashCode() + 423;
        }
        public override bool Equals(object obj)
        {
            if (obj is Token)
            {
                var t = (Token)obj;
                return Type==t.Type && Value.Equals(t.Value) && Position.Equals(t.Position);
            }
            return false;
        }
        public override string ToString()
        {
            return String.Format("{0}: '{1}' {2}", Type, Value, Position);
        }

        public int LastPosition()
        {
            return Position+Value.Length-1;
        }
    }
}
