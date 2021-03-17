using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BladePoker.Models
{
    public class Hand
    {
        public List<Card> cards { get; set; }
        public string playerName { get; set; }
        public double handGrade { get; set; }
        public string handName { get; set; }
    }
}
