using System.Data.Entity.Migrations;
using Galen.Ci.EntityFramework.Initialization;
using Galen.EntityFramework.Sharding;

namespace CodeCamp2015.Data.Seeding
{
    public class ShardedTestDataSeeder : ISeedData<ClinicalDbContext>
    {
        public void Seed(ClinicalDbContext context)
        {
            //
            // seed the data
            //

            var strategy = new ClinicalSeedingStrategy();
            var resolver = new PartialDatabaseNameShardKeyMapper();

            var testData = new TestData();

            foreach (var patient in testData.Patients)
            {
                var key = strategy.GetKey(patient);
                if (resolver.KeyMapsToShard(key, context))
                {
                    context.Patients.AddOrUpdate(x => x.Id, new[] { patient });
                }
            }

            foreach (var physician in testData.Physicians)
            {
                var key = strategy.GetKey(physician);
                if (resolver.KeyMapsToShard(key, context))
                {
                    context.Physicians.AddOrUpdate(x => x.Id, new[] { physician });
                }
            }

            foreach (var visit in testData.Visits)
            {
                var key = strategy.GetKey(visit);
                if (resolver.KeyMapsToShard(key, context))
                {
                    context.Visits.AddOrUpdate(x => x.Id, new[] { visit });
                }
            }

            //
            // save
            //

            context.SaveChanges();
        }
    }
}
