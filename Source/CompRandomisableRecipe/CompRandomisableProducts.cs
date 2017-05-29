using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CompRandomisableProducts
{
    public class CompProperties_RandomisableProducts : CompProperties
    {
        public class DefLowUpperTriple
        {
            public ThingDef def;
            public int lowerLimit;
            public int upperLimit;
        }
        public List<DefLowUpperTriple> limitsForProducts = new List<DefLowUpperTriple>();

        public CompProperties_RandomisableProducts()
        {
            compClass = typeof(CompRandomisableProducts);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (base.ConfigErrors(parentDef) != null)
                foreach (var s in base.ConfigErrors(parentDef))
                    yield return s;
            if (parentDef.butcherProducts == null)
            {
                yield return "ThingDef " + parentDef.defName + " has CompProperties_RandomisableProducts, when it has no butcher products. Either remove CompProperties_RandomisableProducts or add butcher products.";
                yield break;
            }
            for (int i = 0; i < limitsForProducts.Count; i++)
            {
                var limit = limitsForProducts[i];
                if (limit.lowerLimit > limit.upperLimit)
                    yield return "CompProperties_RandomisableProducts in " + parentDef.defName + ": lowerLimit of " + limit.def + "is greater than upperLimit.";
                var butcherProduct = parentDef.butcherProducts.Find(t => t.thingDef == limit.def);
                if (butcherProduct == null)
                    yield return "CompProperties_RandomisableProducts in " + parentDef.defName + " has a defined limitsForProducts but def has no such ThingDef in butcher products.";
                else if (limit.lowerLimit > butcherProduct.count || limit.upperLimit < butcherProduct.count)
                    yield return "CompProperties_RandomisableProducts in " + parentDef.defName + " defines butcherProducts but it is outside defined range.";
                
            }
        }
    }
    public class CompRandomisableProducts : ThingComp
    {
        public CompProperties_RandomisableProducts Props => (CompProperties_RandomisableProducts)props;
        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit) Randomise();
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            Randomise();
        }

        public void Randomise()
        {
            if (parent.def.butcherProducts != null)
            {
                for (int i = 0; i < parent.def.butcherProducts.Count; i++)
                {
                    var prod = parent.def.butcherProducts[i];
                    RandomiseCount(prod);
                }
            }
        }

        public virtual void RandomiseCount(ThingCountClass tcc)
        {
            var tccProps = Props.limitsForProducts.Find(d => d.def == tcc.thingDef);
            if (tccProps != null)
            {
                var difference = tccProps.upperLimit - tccProps.lowerLimit;
                tcc.count = GenMath.RoundRandom(tccProps.lowerLimit + Rand.Value * difference);
            }
        }
    }
}
