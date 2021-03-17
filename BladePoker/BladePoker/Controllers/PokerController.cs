using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BladePoker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BladePoker.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PokerController : ControllerBase
    {

        [HttpPost]
        public string CompareHands([FromBody]List<Hand> hands)
        {
            List<Hand> evaluatedHands = new List<Hand>();
            foreach(Hand hand in hands)
            {
                Hand evaluatedHand = evaluateHand(hand);
                evaluatedHands.Add(evaluatedHand);
            }

            Hand winningHand = evaluatedHands.OrderByDescending(x => x.handGrade).First();
            return winningHand.playerName + " wins with " + winningHand.handName;
        }

        public Hand evaluateHand(Hand hand)
        {
            double handGrade = 0;
            string handName = "";
            List<Card> cards = hand.cards;
            if (IsRoyalFlush(cards))
            {
                handGrade = 1000;
                handName = "Royal Flush";
            }
            else if (isStraightFlush(cards))
            {
                int highCard = getHighCard(cards);
                handGrade = 900 + highCard * 1.0 / 100;
                handName = "Straight Flush " + getCardStringFromInt(highCard) + " high";
            }
            else if (isFourOfAKind(cards) > 0)
            {
                int fourOfAKindValue = isFourOfAKind(cards);
                handGrade = 800 + fourOfAKindValue;
                handName = "Four of a Kind " + getCardStringFromInt(fourOfAKindValue) + "s";
            }
            else if (isFullHouse(cards) > 0)
            {
                double fullHouseValue = isFullHouse(cards);
                int pair = Convert.ToInt32((fullHouseValue % 1) * 100);
                int threeOfAKind = Convert.ToInt32(fullHouseValue - pair/100.0);
                handGrade = 700 + fullHouseValue;
                handName = "Full House " + getCardStringFromInt(threeOfAKind) + "s over " + getCardStringFromInt(pair) + "s";
            }
            else if (IsFlush(cards))
            {
                int highCardValue = getHighCard(cards);
                handGrade = 600 + highCardValue;
                handName = "Flush " + getCardStringFromInt(highCardValue) + " high";
            }
            else if (IsStraight(cards))
            {
                int highCardValue = getHighCard(cards);
                handGrade = 500 + highCardValue;
                handName = "Straight " + getCardStringFromInt(highCardValue) + " high";
            }
            else if (isThreeOfAKind(cards) > 0)
            {
                int threeOfAKindValue = isThreeOfAKind(cards);
                string threeOfAKindString = getCardStringFromInt(threeOfAKindValue);
                List<Card> remainingCards = cards.Where(x => x.value != threeOfAKindString).ToList(); //Remove the first pair value from the list and check for another pair
                int highCard = getHighCard(remainingCards);
                handGrade = 400 + threeOfAKindValue + highCard * 1.0 / 100;
                handName = "Three of a Kind " + threeOfAKindString + "s " + getCardStringFromInt(highCard) + " high";
            }
            else if (isTwoPair(cards) > 0)
            {
                double twoPairValue = isTwoPair(cards);
                int secondPairValue = Convert.ToInt32((twoPairValue % 1)*100);
                int firstPairValue = Convert.ToInt32(twoPairValue - secondPairValue/100.0);
                string firstPairStr = getCardStringFromInt(firstPairValue);
                string secondPairStr = getCardStringFromInt(secondPairValue);
                List<Card> cardsRemaining = cards.Where(x => x.value != firstPairStr && x.value != secondPairStr).ToList();
                int highCard = getHighCard(cardsRemaining);
                handGrade = 300 + firstPairValue + secondPairValue * 1.0 / 100 + highCard * 1.0 / 10000;
                handName = "Two Pair " + firstPairStr + "s " + secondPairStr + "s " + getCardStringFromInt(highCard) + " high";
            }
            else if (isPair(cards) > 0)
            {
                int pairValue = isPair(cards);
                string pairStr = getCardStringFromInt(pairValue);
                List<Card> cardsRemaining = cards.Where(x => x.value != pairStr).ToList();
                int highCard = getHighCard(cardsRemaining);
                handGrade = 200 + pairValue + highCard * 1.0 / 100;
                handName = "One Pair of " + pairStr + "s " + getCardStringFromInt(highCard) + " high";
            }
            else
            {
                int highCard = getHighCard(cards);
                handGrade = 100 + getHighCard(cards);
                handName = getCardStringFromInt(highCard) + " high";
            }

            hand.handGrade = handGrade;
            hand.handName = handName;
            return hand;
        }

        public bool IsRoyalFlush(List<Card> cards)
        {
            // returns true if it is a straight flush and the highest card is an Ace (value 13)    
            return isStraightFlush(cards) && (getHighCard(cards) == 13);        
        }

        public int getHighCard(List<Card> cards)
        {
            List<int> cardIntValues = getCardIntValues(cards);

            return cardIntValues.Max();
        }
        public bool isStraightFlush(List<Card> cards)
        {
            return IsFlush(cards) && IsStraight(cards);
        }
        public bool IsFlush(List<Card> cards)
        {
            string firstSuit = cards.First().suit;
            return cards.All(x => x.suit == firstSuit);
            
        }

        public bool IsStraight(List<Card> cards)
        {


            //Replace lettered values (J/Q/K/A) with numerical values (10/11/12/13)
            List<int> cardIntValues = getCardIntValues(cards);

            //Order list
            cardIntValues.OrderBy(x => x).ToList();

            //Compare the list of values we have to a created list that starts at the first element and ends at the last element
            //Values are consecutive if these are the same
            return cardIntValues.SequenceEqual(Enumerable.Range(cardIntValues.First(), cardIntValues.Last()));

        }

        public double isFullHouse(List<Card> cards)
        {
            int threeOfAKindValue = isThreeOfAKind(cards);
            string threeOfAKindString = getCardStringFromInt(threeOfAKindValue);
            if (threeOfAKindValue > 0)
            {
                List<Card> remainingCards = cards.Where(x => x.value != threeOfAKindString).ToList(); //Remove the first pair value from the list and check for another pair
                int pairValue = isPair(remainingCards);
                if (pairValue > 0)
                {
                    return 1.0 * threeOfAKindValue + 1.0 * pairValue / 100; //3 of a kind is represented by a first pair value in front of the decimal point and the pair value after
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public int isFourOfAKind(List<Card> cards)
        {
            List<int> cardIntValues = getCardIntValues(cards);
            var pair = cardIntValues.GroupBy(i => i) //group by card value
                .Select(x => new { numCards = x.Count(), cardValue = x.Key }) // select the count of cards
                .OrderByDescending(x => x.numCards).ThenBy(x => x.cardValue) // order descending so the first item has the largest count of cards
                .Take(1).First(); // take only the first item (largest count) .

            if (pair.numCards == 4)
            {
                return pair.cardValue;
            }
            else
            {
                return 0;
            }
        }

        public int isThreeOfAKind(List<Card> cards)
        {
            List<int> cardIntValues = getCardIntValues(cards);
            var pair = cardIntValues.GroupBy(i => i) //group by card value
                .Select(x => new { numCards = x.Count(), cardValue = x.Key }) // select the count of cards
                .OrderByDescending(x => x.numCards).ThenBy(x => x.cardValue) // order descending so the first item has the largest count of cards
                .Take(1).First(); // take only the first item (largest count) .

            if (pair.numCards == 3)
            {
                return pair.cardValue; 
            }
            else
            {
                return 0;
            }
        }

        public int isPair(List<Card> cards)
        {
            List<int> cardIntValues = getCardIntValues(cards);
            var pair = cardIntValues.GroupBy(i => i) //group by card value
                .Select(x => new { numCards = x.Count(), cardValue = x.Key }) // select the count of cards
                .OrderByDescending(x => x.numCards).ThenBy(x => x.cardValue) // order descending so the first item has the largest count of cards
                .Take(1).First(); // take only the first item (largest count) .

            if(pair.numCards == 2)
            {
                return pair.cardValue;
            }
            else
            {
                return 0;
            }
        }

        public double isTwoPair(List<Card> cards)
        {
            int pairValue1 = isPair(cards);
            string pairStr1 = getCardStringFromInt(pairValue1);
            if(pairValue1 > 0)
            {
                List<Card> remainingCards = cards.Where(x => x.value != pairStr1).ToList(); //Remove the first pair value from the list and check for another pair
                int pairValue2 = isPair(remainingCards);
                if(pairValue2 > 0)
                {
                    return 1.0 * pairValue1 + 1.0 * pairValue2 / 100; //2 pairs are represented by a first pair value in front of the decimal point and the 2nd pair value after
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        

        
        public List<int> getCardIntValues(List<Card> cards)
        {
            List<string> cardStrValues = cards.Select(x => x.value).ToList();

            //Replace lettered values (J/Q/K/A) with numerical values (10/11/12/13)
            List<int> cardIntValues = cardStrValues.Select(
                                    x => x.Replace("J", "11")
                                    .Replace("Q", "12")
                                    .Replace("K", "13")
                                    .Replace("A", "14")
                                  ).Select(int.Parse).ToList();
            return cardIntValues;
        }

        public List<string> getCardStringFromInt(List<int> cardIntValues)
        {
            List<string> cardStrValues = cardIntValues.Select(x => x.ToString()).ToList();
            cardStrValues = cardStrValues.Select(
                                    x => x.Replace("11", "J")
                                    .Replace("12", "Q")
                                    .Replace("13", "K")
                                    .Replace("14", "A")
                                  ).ToList();
            return cardStrValues;
        }

        public string getCardStringFromInt(int cardIntValue)
        {
            return cardIntValue.ToString().Replace("11", "J")
                                    .Replace("12", "Q")
                                    .Replace("13", "K")
                                    .Replace("14", "A");
             
        }
    }
}