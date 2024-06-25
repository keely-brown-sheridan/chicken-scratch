using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class CaseScoringData
    {
        public int caseID = -1;
        public int prefixBirdbucks = 0;
        public int nounBirdbucks = 0;
        public int bonusBirdbucks = 0;
        public float scoreModifier = 0;

        public CaseScoringData()
        {

        }
        public CaseScoringData(EndgameCaseData caseData)
        {
            caseID = caseData.identifier;
            CaseChoiceData originalCaseChoice = GameDataManager.Instance.GetCaseChoice(caseData.caseTypeName);
            GuessData guessData = caseData.guessData;
            CaseWordData prefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]);
            CaseWordData noun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]);
            Initialize(originalCaseChoice, caseData.scoreModifier, guessData, prefix, noun);
        }

        private void Initialize(CaseChoiceData originalCaseChoice, float currentModifier, GuessData guessData, CaseWordData prefix, CaseWordData noun)
        {
            bool isPrefixCorrect = guessData.prefix == prefix.value;
            bool isNounCorrect = guessData.noun == noun.value;
            if (isPrefixCorrect)
            {
                prefixBirdbucks = originalCaseChoice.pointsPerCorrectWord + prefix.difficulty;
            }           

            if (isNounCorrect)
            {
                nounBirdbucks = originalCaseChoice.pointsPerCorrectWord + noun.difficulty;
            }

            if(isPrefixCorrect && isNounCorrect)
            {
                bonusBirdbucks = originalCaseChoice.bonusPoints;
            }

            scoreModifier = currentModifier;
        }

        public int GetTotalPoints()
        {
            return (int)((prefixBirdbucks + nounBirdbucks + bonusBirdbucks) * scoreModifier);
        }
    }
}
