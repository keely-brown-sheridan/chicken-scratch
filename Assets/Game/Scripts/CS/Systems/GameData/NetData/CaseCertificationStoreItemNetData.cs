using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class CaseCertificationStoreItemNetData
    {
        public string caseIdentifier;
        public string certificationIdentifier;
        public int index;

        public CaseCertificationStoreItemNetData()
        {

        }

        public CaseCertificationStoreItemNetData(CaseCertificationStoreItemData gameData)
        {
            caseIdentifier = gameData.caseChoiceIdentifier;
            certificationIdentifier = gameData.certificationIdentifier;
            index = gameData.index;
        }
    }
}
