using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class ReactionIndex : MonoBehaviour
    {
        public enum Reaction
        {
            smirk, shock, mad, love, wut, joy, invalid
        }
        public Reaction reaction;

        public static Reaction GetReactionFromText(string inText)
        {
            if (!System.Enum.IsDefined(typeof(Reaction), inText))
            {
                return Reaction.invalid;
            }

            return (Reaction)System.Enum.Parse(typeof(Reaction), inText);

        }
    }
}