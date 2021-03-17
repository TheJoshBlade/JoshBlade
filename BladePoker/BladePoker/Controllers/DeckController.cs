using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BladePoker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BladePoker.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeckController : ControllerBase
    {

        [HttpGet]
        public Deck GetNewDeck()
        {
            Deck deck = new Deck();
            using (StreamReader reader = new StreamReader("../BladePoker/Data/DeckOfCards.json"))
            {
                string json = reader.ReadToEnd();
                List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(json);
                deck.cards = cards;
            }

            return deck;
        }

        [HttpPost]
        public ActionResult<List<Card>> DrawCards(Deck deck, int numberToDraw)
        {
            Random random = new Random();
            List<Card> deckcards = deck.cards;
            List<Card> drawnCards = new List<Card>();
            for(int i = 1; i <= numberToDraw; i++)
            {
                Card card = deckcards.ElementAt(random.Next(deckcards.Count()));
                drawnCards.Add(card);
                deckcards.Remove(card);
            }
            deck.cards = deckcards;
            return drawnCards;
        }
    }
}