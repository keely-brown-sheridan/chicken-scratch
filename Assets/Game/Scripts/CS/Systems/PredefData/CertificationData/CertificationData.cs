using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Certification", menuName = "GameData/Create Certification")]
    public class CertificationData : ScriptableObject
    {
        public enum CertificationType
        {
            growth, extension, synergy, trademark, sanctions, demand, shareholders, ballpark, assembly, collaboration, overhead, expiration, supply
        }

        public enum CertificationQuality
        {
            good, bad
        }

        public string identifier;
        public CertificationType type;
        public CertificationQuality quality;
        public Sprite sealSprite;
        public Sprite iconSprite;
        public Color sealColour;
        public string description;

        public void Enable(CaseChoiceData caseChoice)
        {
            switch(identifier)
            {
                case "Growth":
                    GrowthCertificationData growthData = (GrowthCertificationData)this;

                    if (growthData != null && caseChoice != null)
                    {
                        caseChoice.startingScoreModifier -= growthData.initialMultiplierDecrement;
                    }
                    break;
                case "Expiration":
                    ExpirationCertificationData expiryData = (ExpirationCertificationData)this;
                    if(expiryData != null && caseChoice != null)
                    {
                        caseChoice.startingScoreModifier += expiryData.modifierIncrement;
                        caseChoice.pointsPerCorrectWord += expiryData.correctWordBirdbucksIncrement;
                        caseChoice.bonusPoints += expiryData.bonusBirdbucksIncrement;
                    }
                    break;
                case "Overhead":
                    OverheadCertificationData overheadData = (OverheadCertificationData)this;
                    if(overheadData != null && caseChoice != null)
                    {
                        caseChoice.startingScoreModifier += overheadData.modifierIncrement;
                        caseChoice.pointsPerCorrectWord += overheadData.correctWordBirdbucksIncrement;
                        caseChoice.bonusPoints += overheadData.bonusBirdbucksIncrement;
                    }
                    break;
            }
        }
    }
}
