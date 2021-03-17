using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BladePoker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
namespace BladePoker.Controllers
{
    [Route("[controller]/[action]")]
    public class GameController : Controller
    {
        public ActionResult FiveCardDraw()
        {
            Deck deck = getNewDeckFromAPI().Result;
            Hand hand1 = new Hand();
            hand1.playerName = "Player1";
            hand1.cards = DrawCardsFromAPI(deck, 5).Result;
            foreach(Card card in hand1.cards)
            {
                deck.cards.Remove(card);
            }
            Hand hand2 = new Hand();
            hand2.playerName = "Player2";
            hand2.cards = DrawCardsFromAPI(deck, 5).Result;
            foreach (Card card in hand2.cards)
            {
                deck.cards.Remove(card);
            }

            List<Hand> handsInGame = new List<Hand>();
            handsInGame.Add(hand1);
            handsInGame.Add(hand2);
            string WinMessage = CompareHandsAPI(handsInGame).Result;
            ViewData["Message"] = WinMessage;


            return View(handsInGame);
        }

        public static async Task<string> CompareHandsAPI(List<Hand> hands)
        {
            using (HttpClient client = new HttpClient())
            {
                
                var response = await client.PostAsJsonAsync("https://localhost:44378/api/Poker/CompareHands", hands);

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var readTask = response.Content.ReadAsStringAsync();
                    string message = readTask.Result;
                    return message;
                }
                else
                {
                    throw new Exception("Error - contact administrator");
                }

            }
        }

        private static async Task<Deck> getNewDeckFromAPI()
        {
            using (HttpClient client = new HttpClient())
            {
                
                Deck deck = await client.GetFromJsonAsync<Deck>("https://localhost:44378/api/Deck/GetNewDeck");
                
                return deck;


            }
        }

        public static async Task<List<Card>> DrawCardsFromAPI(Deck deck, int numberToDraw)
        {
            using (HttpClient client = new HttpClient())
            {

                var response = await client.PostAsJsonAsync("https://localhost:44378/api/Deck/DrawCards?numberToDraw=" + numberToDraw, deck);

                response.EnsureSuccessStatusCode();
                
                if (response.IsSuccessStatusCode)
                {
                    var readTask = response.Content.ReadAsAsync<List<Card>>();
                    List<Card> cards = readTask.Result;
                    return cards;
                }
                else
                {
                    throw new Exception("Error - contact administrator");
                }

            }
        }
    }
}