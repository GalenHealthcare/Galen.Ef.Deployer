using CodeCamp2015.Domain;
using Galen.EntityFramework.Sharding;

namespace CodeCamp2015.Data.Seeding
{
    public class ClinicalSeedingStrategy : IShardedSeedingStrategy<IClinicalData, string>
    {
        public string GetKey(IClinicalData data)
        {
            return data.ClinicName;
        }
    }
}