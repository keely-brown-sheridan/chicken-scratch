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
        public CaseScoringData(EndgameCaseData caseData, int pointsPerCorrectWord, int bonusPoints)
        {
            caseID = caseData.identifier;
            CaseChoiceData originalCaseChoice = GameDataManager.Instance.GetCaseChoice(caseData.caseTypeName);
            GuessData guessData = caseData.guessData;
            CaseWordData prefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]);
            CaseWordData noun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]);
            Initialize(pointsPerCorrectWord, bonusPoints, caseData.scoreModifier, guessData, prefix, noun);
        }

        private void Initialize(int pointsPerCorrectWord, int bonusPoints, float currentModifier, GuessData guessData, CaseWordData prefix, CaseWordData noun)
        {
            bool isPrefixCorrect = guessData.prefix == prefix.value;
            bool isNounCorrect = guessData.noun == noun.value;
            if (isPrefixCorrect)
            {
                prefixBirdbucks = pointsPerCorrectWord + prefix.difficulty;
            }           

            if (isNounCorrect)
            {
                nounBirdbucks = pointsPerCorrectWord + noun.difficulty;
            }

            if(isPrefixCorrect && isNounCorrect)
            {
                bonusBirdbucks = bonusPoints;
            }

            scoreModifier = currentModifier;
        }

        public int GetTotalPointsForPlayer(ColourManager.BirdName bird)
        {
            EndgameCaseData caseData = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(caseData.caseTypeName);
            if(caseChoice != null && caseChoice.caseFormat == CaseTemplateData.CaseFormat.competition)
            {
                //You make no money if you aren't the winner or the guesser (it should be split in half otherwise)
                if (caseData.guessData.author != bird && !GameManager.Instance.playerFlowManager.slidesRound.IsPlayerCompetitionWinner(caseData.identifier, bird))
                {
                    return 0;
                }
            }

            return GetTotalPoints();
        }
        public int GetTotalPoints()
        {
            EndgameCaseData caseData = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            int baseBirdbucks = prefixBirdbucks + nounBirdbucks + bonusBirdbucks;

            bool caseHasSanctionsCertification = GameManager.Instance.playerFlowManager.CaseHasCertification(caseData.caseTypeName, "Sanctions");
            if (caseHasSanctionsCertification)
            {
                IntCertificationData sanctionsCertification = (IntCertificationData)GameDataManager.Instance.GetCertification("Sanctions");
                int numberOfIncorrectWords = 0;
                CaseWordData prefixWord = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]);
                CaseWordData nounWord = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]);

                if (prefixWord != null && caseData.guessData.prefix != prefixWord.value)
                {
                    numberOfIncorrectWords++;
                }
                if (nounWord != null && caseData.guessData.noun != nounWord.value)
                {
                    numberOfIncorrectWords++;
                }
                if (sanctionsCertification != null)
                {
                    baseBirdbucks -= sanctionsCertification.value * numberOfIncorrectWords;
                }
            }

            int birdbucksEarned = baseBirdbucks;
            if (birdbucksEarned > 0)
            {
                birdbucksEarned = (int)(birdbucksEarned * scoreModifier);
            }
            return birdbucksEarned;
        }
    }
}
